using Monolegal.Web.Data;
using Monolegal.Web.Dominio;

namespace Monolegal.Web.Servicios;

public class RecordatorioService : IRecordatorioService
{
    private readonly IFacturaRepositorio _repositorio;
    private readonly IEmailSender _correo;
    private readonly ILogger<RecordatorioService> _logger;

    public RecordatorioService(
        IFacturaRepositorio repositorio,
        IEmailSender correo,
        ILogger<RecordatorioService> logger)
    {
        _repositorio = repositorio;
        _correo = correo;
        _logger = logger;
    }

    public async Task<ResumenFacturas> ObtenerResumenAsync(CancellationToken cancellationToken)
    {
        var clientes = await _repositorio.ListarClientesAsync(cancellationToken);
        var facturas = await _repositorio.ListarFacturasAsync(null, cancellationToken);
        var porCliente = clientes.ToDictionary(cliente => cliente.Id ?? string.Empty);

        var detalle = facturas
            .Select(factura =>
            {
                porCliente.TryGetValue(factura.ClienteId, out var cliente);
                return new FacturaDetalle(
                    factura.Id ?? string.Empty,
                    factura.Numero,
                    factura.Concepto,
                    factura.Valor,
                    factura.FechaEmision,
                    factura.FechaVencimiento,
                    factura.Estado,
                    TransicionEstado.EsGestionable(factura.Estado),
                    factura.ClienteId,
                    cliente?.Nombre ?? "Cliente no encontrado",
                    cliente?.Email ?? string.Empty,
                    cliente?.Documento ?? string.Empty);
            })
            .ToList();

        var resumenClientes = clientes
            .Select(cliente =>
            {
                var propias = detalle.Where(factura => factura.ClienteId == cliente.Id).ToList();
                var gestionables = propias.Where(factura => factura.Gestionable).ToList();
                return new ClienteResumen(
                    cliente.Id ?? string.Empty,
                    cliente.Nombre,
                    cliente.Email,
                    cliente.Documento,
                    propias.Count,
                    gestionables.Count,
                    gestionables.Sum(factura => factura.Valor));
            })
            .ToList();

        return new ResumenFacturas(
            detalle.Count,
            detalle.Count(factura => factura.Estado == EstadosFactura.PrimerRecordatorio),
            detalle.Count(factura => factura.Estado == EstadosFactura.SegundoRecordatorio),
            detalle.Count(factura => factura.Estado == EstadosFactura.Desactivado),
            detalle.Count(factura => !TransicionEstado.EsGestionable(factura.Estado) && factura.Estado != EstadosFactura.Desactivado),
            detalle.Where(factura => factura.Gestionable).Sum(factura => factura.Valor),
            resumenClientes,
            detalle);
    }

    public async Task<IReadOnlyList<CorreoEnviado>> ListarCorreosAsync(CancellationToken cancellationToken)
    {
        var notificaciones = await _repositorio.ListarNotificacionesAsync(cancellationToken);
        return notificaciones
            .Select(notificacion => new CorreoEnviado(
                notificacion.Id ?? string.Empty,
                notificacion.Destinatario,
                notificacion.Asunto,
                notificacion.Mensaje,
                notificacion.NumeroFactura,
                notificacion.ClienteNombre,
                notificacion.EstadoAnterior,
                notificacion.EstadoNuevo,
                notificacion.FechaEnvio))
            .ToList();
    }

    public async Task<ResultadoProceso> ProcesarAsync(string? clienteId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Cliente> clientes;
        if (string.IsNullOrWhiteSpace(clienteId))
        {
            clientes = await _repositorio.ListarClientesAsync(cancellationToken);
        }
        else
        {
            var cliente = await _repositorio.ObtenerClienteAsync(clienteId, cancellationToken);
            if (cliente is null)
            {
                throw new ClienteNoEncontradoException(clienteId);
            }

            clientes = new List<Cliente> { cliente };
        }

        var detalles = new List<DetalleProceso>();
        var omitidas = 0;
        var enviados = 0;
        var actualizadas = 0;
        var errores = 0;

        foreach (var cliente in clientes)
        {
            var id = cliente.Id ?? string.Empty;
            var todas = await _repositorio.ListarFacturasAsync(id, cancellationToken);
            var gestionables = await _repositorio.ListarGestionablesAsync(id, cancellationToken);
            omitidas += todas.Count - gestionables.Count;

            if (gestionables.Count == 0)
            {
                detalles.Add(new DetalleProceso(
                    id,
                    cliente.Nombre,
                    cliente.Email,
                    null,
                    null,
                    null,
                    null,
                    "Sin facturas en primerrecordatorio o segundorecordatorio.",
                    true));
                continue;
            }

            foreach (var factura in gestionables)
            {
                var detalle = await ProcesarUnaFacturaAsync(cliente, factura, cancellationToken);
                if (detalle.Exitoso && detalle.EstadoNuevo is not null)
                {
                    enviados++;
                    actualizadas++;
                }
                else if (!detalle.Exitoso)
                {
                    errores++;
                }

                detalles.Add(detalle);
            }
        }

        return new ResultadoProceso(
            clientes.Count,
            omitidas,
            enviados,
            actualizadas,
            errores,
            detalles);
    }

    public async Task<ResultadoProceso> ProcesarFacturaAsync(string facturaId, CancellationToken cancellationToken)
    {
        var factura = await _repositorio.ObtenerFacturaAsync(facturaId, cancellationToken);
        if (factura is null)
        {
            return new ResultadoProceso(0, 0, 0, 0, 1, new[]
            {
                new DetalleProceso(string.Empty, string.Empty, string.Empty, facturaId, null, null, null, "No se encontró la factura.", false)
            });
        }

        var cliente = await _repositorio.ObtenerClienteAsync(factura.ClienteId, cancellationToken);
        if (cliente is null)
        {
            return new ResultadoProceso(0, 0, 0, 0, 1, new[]
            {
                new DetalleProceso(factura.ClienteId, string.Empty, string.Empty, factura.Id, factura.Numero, factura.Estado, null, "No se encontró el cliente de la factura.", false)
            });
        }

        if (!TransicionEstado.EsGestionable(factura.Estado))
        {
            return new ResultadoProceso(1, 1, 0, 0, 0, new[]
            {
                new DetalleProceso(
                    cliente.Id ?? string.Empty,
                    cliente.Nombre,
                    cliente.Email,
                    factura.Id,
                    factura.Numero,
                    factura.Estado,
                    null,
                    $"La factura {factura.Numero} está en {factura.Estado}. Solo se envía correo si el estado es primerrecordatorio o segundorecordatorio.",
                    true)
            });
        }

        var detalle = await ProcesarUnaFacturaAsync(cliente, factura, cancellationToken);
        var enviados = detalle.Exitoso && detalle.EstadoNuevo is not null ? 1 : 0;
        return new ResultadoProceso(
            1,
            0,
            enviados,
            enviados,
            detalle.Exitoso ? 0 : 1,
            new[] { detalle });
    }

    private async Task<DetalleProceso> ProcesarUnaFacturaAsync(Cliente cliente, Factura factura, CancellationToken cancellationToken)
    {
        var id = cliente.Id ?? string.Empty;
        var estadoAnterior = factura.Estado;
        var estadoNuevo = TransicionEstado.Siguiente(estadoAnterior);
        var asunto = TransicionEstado.Asunto(factura.Numero, estadoAnterior);
        var mensaje = TransicionEstado.CuerpoTexto(cliente.Nombre, factura.Numero, factura.Valor, estadoAnterior);

        try
        {
            var html = TransicionEstado.CuerpoHtml(cliente.Nombre, factura.Numero, factura.Valor, estadoAnterior);
            await _correo.EnviarAsync(cliente.Email, asunto, html, cancellationToken);

            var actualizo = await _repositorio.ActualizarEstadoSiContinuaAsync(
                factura.Id ?? string.Empty,
                estadoAnterior,
                estadoNuevo,
                cancellationToken);

            if (!actualizo)
            {
                return new DetalleProceso(
                    id,
                    cliente.Nombre,
                    cliente.Email,
                    factura.Id,
                    factura.Numero,
                    estadoAnterior,
                    estadoNuevo,
                    "El correo se envió, pero la factura ya no estaba en el estado esperado y no se actualizó.",
                    false);
            }

            await _repositorio.GuardarNotificacionAsync(new NotificacionCorreo
            {
                ClienteId = id,
                ClienteNombre = cliente.Nombre,
                FacturaId = factura.Id ?? string.Empty,
                NumeroFactura = factura.Numero,
                Destinatario = cliente.Email,
                Asunto = asunto,
                Mensaje = mensaje,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                FechaEnvio = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation(
                "Factura {Numero} de {Cliente}: {Anterior} -> {Nuevo}. Correo enviado a {Email}.",
                factura.Numero,
                cliente.Nombre,
                estadoAnterior,
                estadoNuevo,
                cliente.Email);

            return new DetalleProceso(
                id,
                cliente.Nombre,
                cliente.Email,
                factura.Id,
                factura.Numero,
                estadoAnterior,
                estadoNuevo,
                mensaje,
                true);
        }
        catch (Exception excepcion) when (excepcion is not OperationCanceledException)
        {
            _logger.LogError(
                excepcion,
                "No se pudo procesar la factura {Numero} de {Cliente}. El estado no se modificó.",
                factura.Numero,
                cliente.Nombre);

            return new DetalleProceso(
                id,
                cliente.Nombre,
                cliente.Email,
                factura.Id,
                factura.Numero,
                estadoAnterior,
                null,
                $"No se envió el correo y el estado se conservó en {estadoAnterior}. {excepcion.Message}",
                false);
        }
    }
}
