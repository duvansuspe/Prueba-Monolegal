using System.Globalization;
using System.Net;

namespace Monolegal.Web.Dominio;

public static class TransicionEstado
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-CO");

    public static bool EsGestionable(string estado) =>
        estado is EstadosFactura.PrimerRecordatorio or EstadosFactura.SegundoRecordatorio;

    public static string Siguiente(string estado) => estado switch
    {
        EstadosFactura.PrimerRecordatorio => EstadosFactura.SegundoRecordatorio,
        EstadosFactura.SegundoRecordatorio => EstadosFactura.Desactivado,
        _ => throw new InvalidOperationException(
            $"El estado '{estado}' no se procesa. Solo aplican primerrecordatorio y segundorecordatorio.")
    };

    public static string Asunto(string numeroFactura, string estadoActual) => estadoActual switch
    {
        EstadosFactura.PrimerRecordatorio =>
            $"Segundo recordatorio de la factura {numeroFactura}",
        EstadosFactura.SegundoRecordatorio =>
            $"Aviso de desactivación de la factura {numeroFactura}",
        _ => throw new InvalidOperationException(
            $"No hay asunto de correo para el estado '{estadoActual}'.")
    };

    public static string CuerpoTexto(string nombreCliente, string numeroFactura, decimal valor, string estadoActual)
    {
        var valorTexto = valor.ToString("C0", Cultura);
        return estadoActual switch
        {
            EstadosFactura.PrimerRecordatorio =>
                $"Estimado(a) {nombreCliente}, le informamos que la factura {numeroFactura} por valor de {valorTexto} ha pasado a segundo recordatorio. " +
                "El estado de la factura quedará en segundorecordatorio. Le solicitamos regularizar el pago.",
            EstadosFactura.SegundoRecordatorio =>
                $"Estimado(a) {nombreCliente}, le informamos que la factura {numeroFactura} por valor de {valorTexto} va a ser desactivada " +
                "por no registrar pago después del segundo recordatorio. El estado de la factura quedará en desactivado.",
            _ => throw new InvalidOperationException(
                $"No hay mensaje de correo para el estado '{estadoActual}'.")
        };
    }

    public static string CuerpoHtml(string nombreCliente, string numeroFactura, decimal valor, string estadoActual)
    {
        var texto = CuerpoTexto(nombreCliente, numeroFactura, valor, estadoActual);
        var titulo = estadoActual == EstadosFactura.PrimerRecordatorio
            ? "Paso a segundo recordatorio"
            : "La factura va a ser desactivada";

        return $"""
            <div style="font-family:Georgia,serif;color:#142033;line-height:1.5">
              <p style="margin:0 0 8px;letter-spacing:.08em;text-transform:uppercase;font-size:12px;color:#8a6a32">Monolegal</p>
              <h1 style="font-size:22px;margin:0 0 16px">{WebUtility.HtmlEncode(titulo)}</h1>
              <p style="margin:0 0 12px">{WebUtility.HtmlEncode(texto)}</p>
              <p style="margin:0;color:#5c6778;font-size:13px">Estado encontrado: {WebUtility.HtmlEncode(estadoActual)}. Nuevo estado: {WebUtility.HtmlEncode(Siguiente(estadoActual))}.</p>
            </div>
            """;
    }
}
