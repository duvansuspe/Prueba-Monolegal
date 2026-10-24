using Monolegal.Web.Models;
using Monolegal.Web.Servicios;

namespace Monolegal.Web.Controllers;

public class FacturasController : Controller
{
    private readonly IRecordatorioService _recordatorios;
    private readonly IDatosIniciales _datos;

    public FacturasController(IRecordatorioService recordatorios, IDatosIniciales datos)
    {
        _recordatorios = recordatorios;
        _datos = datos;
    }

    public async Task<IActionResult> Index(string? estado, string? clienteId, CancellationToken cancellationToken)
    {
        var resumen = await _recordatorios.ObtenerResumenAsync(cancellationToken);
        var correos = await _recordatorios.ListarCorreosAsync(cancellationToken);
        var visibles = resumen.Facturas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(clienteId))
        {
            visibles = visibles.Where(factura => factura.ClienteId == clienteId);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            visibles = visibles.Where(factura => factura.Estado == estado);
        }

        var pagina = new PaginaFacturas
        {
            Resumen = resumen,
            FacturasVisibles = visibles.ToList(),
            Correos = correos,
            Aviso = TempData["Aviso"] as string,
            Estado = estado,
            ClienteId = clienteId
        };

        return View(pagina);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar(CancellationToken cancellationToken)
    {
        var resultado = await _recordatorios.ProcesarAsync(null, cancellationToken);
        TempData["Aviso"] = Mensaje(resultado);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarCorreo(string id, string? clienteId, CancellationToken cancellationToken)
    {
        var resultado = await _recordatorios.ProcesarFacturaAsync(id, cancellationToken);
        TempData["Aviso"] = Mensaje(resultado);
        var destino = Url.Action(nameof(Index), new { clienteId }) ?? "/";
        return Redirect(destino + "#facturas");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarCliente(string id, CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _recordatorios.ProcesarAsync(id, cancellationToken);
            TempData["Aviso"] = Mensaje(resultado);
        }
        catch (ClienteNoEncontradoException excepcion)
        {
            TempData["Aviso"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reiniciar(CancellationToken cancellationToken)
    {
        await _datos.ReiniciarAsync(cancellationToken);
        TempData["Aviso"] = "Se restablecieron los tres clientes y sus facturas de demostración.";
        return RedirectToAction(nameof(Index));
    }

    private static string Mensaje(ResultadoProceso resultado)
    {
        if (resultado.Detalles.Count == 1)
        {
            return resultado.Detalles[0].Mensaje;
        }

        if (resultado.Errores > 0)
        {
            return $"Se revisaron {resultado.ClientesRevisados} clientes. {resultado.Errores} factura(s) no cambiaron porque el correo no se envió.";
        }

        return $"Se revisaron {resultado.ClientesRevisados} clientes, se enviaron {resultado.CorreosEnviados} correos y se actualizaron {resultado.FacturasActualizadas} facturas.";
    }
}
