namespace Monolegal.Web.Models;

public class PaginaFacturas
{
    public required ResumenFacturas Resumen { get; init; }
    public required IReadOnlyList<FacturaDetalle> FacturasVisibles { get; init; }
    public required IReadOnlyList<CorreoEnviado> Correos { get; init; }
    public string? Aviso { get; init; }
    public string? Estado { get; init; }
    public string? ClienteId { get; init; }
}
