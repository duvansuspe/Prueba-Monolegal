using Monolegal.Web.Data;

namespace Monolegal.Web.Dominio;

public static class DatosDemostracion
{
    public static (IReadOnlyList<Cliente> Clientes, IReadOnlyList<(string DocumentoCliente, Factura Factura)> Facturas) Crear()
    {
        var hoy = DateTime.UtcNow.Date;

        var clientes = new List<Cliente>
        {
            new()
            {
                Nombre = "Ana María López",
                Email = "duvansuspe17@gmail.com",
                Documento = "1023456789"
            },
            new()
            {
                Nombre = "Carlos Gómez Ruiz",
                Email = "carlos.gomez@cliente.test",
                Documento = "80123456"
            },
            new()
            {
                Nombre = "Inversiones Delta S.A.S.",
                Email = "cartera@inversionesdelta.test",
                Documento = "900123456"
            }
        };

        var facturas = new List<(string DocumentoCliente, Factura Factura)>
        {
            FacturaDe("1023456789", "FAC-1001", "Honorarios de asesoría contractual", 1_500_000, hoy.AddDays(-40), hoy.AddDays(-10), EstadosFactura.PrimerRecordatorio),
            FacturaDe("1023456789", "FAC-1002", "Revisión de minuta de compraventa", 850_000, hoy.AddDays(-25), hoy.AddDays(-5), EstadosFactura.PrimerRecordatorio),
            FacturaDe("1023456789", "FAC-1003", "Consulta societaria", 400_000, hoy.AddDays(-60), hoy.AddDays(-30), EstadosFactura.Pagada),

            FacturaDe("80123456", "FAC-2001", "Representación en audiencia", 2_200_000, hoy.AddDays(-50), hoy.AddDays(-20), EstadosFactura.SegundoRecordatorio),
            FacturaDe("80123456", "FAC-2002", "Estudio de títulos", 960_000, hoy.AddDays(-18), hoy.AddDays(-2), EstadosFactura.PrimerRecordatorio),
            FacturaDe("80123456", "FAC-2003", "Concepto jurídico laboral", 300_000, hoy.AddDays(-90), hoy.AddDays(-60), EstadosFactura.Desactivado),

            FacturaDe("900123456", "FAC-3001", "Acompañamiento en due diligence", 5_400_000, hoy.AddDays(-45), hoy.AddDays(-15), EstadosFactura.SegundoRecordatorio),
            FacturaDe("900123456", "FAC-3002", "Elaboración de reformas estatutarias", 1_100_000, hoy.AddDays(-35), hoy.AddDays(-8), EstadosFactura.SegundoRecordatorio),
            FacturaDe("900123456", "FAC-3003", "Honorarios de secretaría jurídica", 750_000, hoy.AddDays(-12), hoy.AddDays(3), EstadosFactura.PrimerRecordatorio)
        };

        return (clientes, facturas);
    }

    private static (string DocumentoCliente, Factura Factura) FacturaDe(
        string documento,
        string numero,
        string concepto,
        decimal valor,
        DateTime emision,
        DateTime vencimiento,
        string estado) =>
        (documento, new Factura
        {
            Numero = numero,
            Concepto = concepto,
            Valor = valor,
            FechaEmision = DateTime.SpecifyKind(emision, DateTimeKind.Utc),
            FechaVencimiento = DateTime.SpecifyKind(vencimiento, DateTimeKind.Utc),
            Estado = estado
        });
}
