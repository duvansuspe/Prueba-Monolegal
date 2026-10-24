namespace Monolegal.Web.Servicios;

public interface IRecordatorioService
{
    Task<ResumenFacturas> ObtenerResumenAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CorreoEnviado>> ListarCorreosAsync(CancellationToken cancellationToken);
    Task<ResultadoProceso> ProcesarAsync(string? clienteId, CancellationToken cancellationToken);
    Task<ResultadoProceso> ProcesarFacturaAsync(string facturaId, CancellationToken cancellationToken);
}
