using MongoDB.Bson;
using MongoDB.Driver;

namespace Monolegal.Web.Data;

public class FacturaRepositorio : IFacturaRepositorio
{
    private readonly MongoDbContext _contexto;

    public FacturaRepositorio(MongoDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<bool> HayClientesAsync(CancellationToken cancellationToken) =>
        _contexto.Clientes.Find(FilterDefinition<Cliente>.Empty).AnyAsync(cancellationToken);

    public async Task ReemplazarDatosAsync(
        IReadOnlyList<Cliente> clientes,
        IReadOnlyList<(string DocumentoCliente, Factura Factura)> facturas,
        CancellationToken cancellationToken)
    {
        await _contexto.Clientes.DeleteManyAsync(FilterDefinition<Cliente>.Empty, cancellationToken);
        await _contexto.Facturas.DeleteManyAsync(FilterDefinition<Factura>.Empty, cancellationToken);
        await _contexto.Notificaciones.DeleteManyAsync(FilterDefinition<NotificacionCorreo>.Empty, cancellationToken);

        if (clientes.Count == 0)
        {
            return;
        }

        await _contexto.Clientes.InsertManyAsync(clientes, cancellationToken: cancellationToken);
        var porDocumento = clientes.ToDictionary(cliente => cliente.Documento);

        foreach (var (documento, factura) in facturas)
        {
            factura.ClienteId = porDocumento[documento].Id
                ?? throw new InvalidOperationException($"El cliente {documento} no recibió identificador.");
        }

        await _contexto.Facturas.InsertManyAsync(
            facturas.Select(item => item.Factura),
            cancellationToken: cancellationToken);

        var indice = new CreateIndexModel<Factura>(
            Builders<Factura>.IndexKeys.Ascending(factura => factura.ClienteId).Ascending(factura => factura.Estado));
        await _contexto.Facturas.Indexes.CreateOneAsync(indice, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ListarClientesAsync(CancellationToken cancellationToken)
    {
        var clientes = await _contexto.Clientes
            .Find(FilterDefinition<Cliente>.Empty)
            .SortBy(cliente => cliente.Nombre)
            .ToListAsync(cancellationToken);
        return clientes;
    }

    public async Task<Cliente?> ObtenerClienteAsync(string clienteId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(clienteId, out _))
        {
            return null;
        }

        return await _contexto.Clientes
            .Find(cliente => cliente.Id == clienteId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Factura?> ObtenerFacturaAsync(string facturaId, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(facturaId, out _))
        {
            return null;
        }

        return await _contexto.Facturas
            .Find(factura => factura.Id == facturaId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Factura>> ListarFacturasAsync(string? clienteId, CancellationToken cancellationToken)
    {
        var filtro = string.IsNullOrWhiteSpace(clienteId)
            ? FilterDefinition<Factura>.Empty
            : Builders<Factura>.Filter.Eq(factura => factura.ClienteId, clienteId);

        var facturas = await _contexto.Facturas
            .Find(filtro)
            .SortBy(factura => factura.Numero)
            .ToListAsync(cancellationToken);
        return facturas;
    }

    public async Task<IReadOnlyList<Factura>> ListarGestionablesAsync(string clienteId, CancellationToken cancellationToken)
    {
        var filtro = Builders<Factura>.Filter.Eq(factura => factura.ClienteId, clienteId)
            & Builders<Factura>.Filter.In(
                factura => factura.Estado,
                new[] { EstadosFactura.PrimerRecordatorio, EstadosFactura.SegundoRecordatorio });

        var facturas = await _contexto.Facturas
            .Find(filtro)
            .SortBy(factura => factura.Numero)
            .ToListAsync(cancellationToken);
        return facturas;
    }

    public async Task<bool> ActualizarEstadoSiContinuaAsync(
        string facturaId,
        string estadoEsperado,
        string estadoNuevo,
        CancellationToken cancellationToken)
    {
        var filtro = Builders<Factura>.Filter.Eq(factura => factura.Id, facturaId)
            & Builders<Factura>.Filter.Eq(factura => factura.Estado, estadoEsperado);
        var actualizacion = Builders<Factura>.Update.Set(factura => factura.Estado, estadoNuevo);
        var resultado = await _contexto.Facturas.UpdateOneAsync(filtro, actualizacion, cancellationToken: cancellationToken);
        return resultado.ModifiedCount == 1;
    }

    public Task GuardarNotificacionAsync(NotificacionCorreo notificacion, CancellationToken cancellationToken) =>
        _contexto.Notificaciones.InsertOneAsync(notificacion, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<NotificacionCorreo>> ListarNotificacionesAsync(CancellationToken cancellationToken)
    {
        var notificaciones = await _contexto.Notificaciones
            .Find(FilterDefinition<NotificacionCorreo>.Empty)
            .SortByDescending(notificacion => notificacion.FechaEnvio)
            .Limit(200)
            .ToListAsync(cancellationToken);
        return notificaciones;
    }

    public async Task PingAsync(CancellationToken cancellationToken)
    {
        await _contexto.Database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: cancellationToken);
    }
}
