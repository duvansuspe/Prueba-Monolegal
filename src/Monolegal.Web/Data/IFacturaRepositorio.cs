namespace Monolegal.Web.Data;

public interface IFacturaRepositorio
{
    Task<bool> HayClientesAsync(CancellationToken cancellationToken);
    Task ReemplazarDatosAsync(
        IReadOnlyList<Cliente> clientes,
        IReadOnlyList<(string DocumentoCliente, Factura Factura)> facturas,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Cliente>> ListarClientesAsync(CancellationToken cancellationToken);
    Task<Cliente?> ObtenerClienteAsync(string clienteId, CancellationToken cancellationToken);
    Task<Factura?> ObtenerFacturaAsync(string facturaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Factura>> ListarFacturasAsync(string? clienteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Factura>> ListarGestionablesAsync(string clienteId, CancellationToken cancellationToken);
    Task<bool> ActualizarEstadoSiContinuaAsync(
        string facturaId,
        string estadoEsperado,
        string estadoNuevo,
        CancellationToken cancellationToken);
    Task GuardarNotificacionAsync(NotificacionCorreo notificacion, CancellationToken cancellationToken);
    Task<IReadOnlyList<NotificacionCorreo>> ListarNotificacionesAsync(CancellationToken cancellationToken);
    Task PingAsync(CancellationToken cancellationToken);
}
