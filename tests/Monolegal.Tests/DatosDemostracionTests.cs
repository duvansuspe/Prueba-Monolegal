using Monolegal.Web.Dominio;
using Monolegal.Web.Models;

namespace Monolegal.Tests;

public class DatosDemostracionTests
{
    [Fact]
    public void La_demostracion_incluye_tres_clientes_distintos()
    {
        var (clientes, facturas) = DatosDemostracion.Crear();

        Assert.Equal(3, clientes.Count);
        Assert.Equal(3, clientes.Select(cliente => cliente.Email).Distinct().Count());
        Assert.Equal(9, facturas.Count);
        Assert.Contains(facturas, item => item.Factura.Estado == EstadosFactura.PrimerRecordatorio);
        Assert.Contains(facturas, item => item.Factura.Estado == EstadosFactura.SegundoRecordatorio);
        Assert.Contains(facturas, item => item.Factura.Estado == EstadosFactura.Pagada);
        Assert.Contains(facturas, item => item.Factura.Estado == EstadosFactura.Desactivado);
    }
}
