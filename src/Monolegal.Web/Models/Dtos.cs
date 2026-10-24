namespace Monolegal.Web.Models;

public record ResumenFacturas(
    int TotalFacturas,
    int PrimerRecordatorio,
    int SegundoRecordatorio,
    int Desactivadas,
    int Otras,
    decimal ValorEnGestion,
    IReadOnlyList<ClienteResumen> Clientes,
    IReadOnlyList<FacturaDetalle> Facturas);

public record ClienteResumen(
    string Id,
    string Nombre,
    string Email,
    string Documento,
    int TotalFacturas,
    int Gestionables,
    decimal ValorGestionable);

public record FacturaDetalle(
    string Id,
    string Numero,
    string Concepto,
    decimal Valor,
    DateTime FechaEmision,
    DateTime FechaVencimiento,
    string Estado,
    bool Gestionable,
    string ClienteId,
    string ClienteNombre,
    string ClienteEmail,
    string ClienteDocumento);

public record CorreoEnviado(
    string Id,
    string Destinatario,
    string Asunto,
    string Mensaje,
    string NumeroFactura,
    string ClienteNombre,
    string EstadoAnterior,
    string EstadoNuevo,
    DateTime FechaEnvio);

public record ResultadoProceso(
    int ClientesRevisados,
    int FacturasOmitidas,
    int CorreosEnviados,
    int FacturasActualizadas,
    int Errores,
    IReadOnlyList<DetalleProceso> Detalles);

public record DetalleProceso(
    string ClienteId,
    string ClienteNombre,
    string ClienteEmail,
    string? FacturaId,
    string? NumeroFactura,
    string? EstadoAnterior,
    string? EstadoNuevo,
    string Mensaje,
    bool Exitoso);
