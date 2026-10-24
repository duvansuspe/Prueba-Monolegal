using Monolegal.Web.Data;
using Monolegal.Web.Dominio;

namespace Monolegal.Web.Servicios;

public interface IDatosIniciales
{
    Task AsegurarAsync(CancellationToken cancellationToken);
    Task ReiniciarAsync(CancellationToken cancellationToken);
}

public class DatosInicialesService : IDatosIniciales
{
    private readonly IFacturaRepositorio _repositorio;

    public DatosInicialesService(IFacturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task AsegurarAsync(CancellationToken cancellationToken)
    {
        if (await _repositorio.HayClientesAsync(cancellationToken))
        {
            return;
        }

        await CargarAsync(cancellationToken);
    }

    public Task ReiniciarAsync(CancellationToken cancellationToken) => CargarAsync(cancellationToken);

    private Task CargarAsync(CancellationToken cancellationToken)
    {
        var (clientes, facturas) = DatosDemostracion.Crear();
        return _repositorio.ReemplazarDatosAsync(clientes, facturas, cancellationToken);
    }
}
