using Monolegal.Web.Dominio;
using Monolegal.Web.Models;

namespace Monolegal.Tests;

public class TransicionEstadoTests
{
    [Theory]
    [InlineData(EstadosFactura.PrimerRecordatorio, EstadosFactura.SegundoRecordatorio)]
    [InlineData(EstadosFactura.SegundoRecordatorio, EstadosFactura.Desactivado)]
    public void El_estado_avanza_segun_el_recordatorio(string actual, string esperado)
    {
        Assert.Equal(esperado, TransicionEstado.Siguiente(actual));
    }

    [Theory]
    [InlineData(EstadosFactura.PrimerRecordatorio)]
    [InlineData(EstadosFactura.SegundoRecordatorio)]
    public void Solo_los_dos_recordatorios_se_gestionan(string estado)
    {
        Assert.True(TransicionEstado.EsGestionable(estado));
    }

    [Theory]
    [InlineData(EstadosFactura.Pagada)]
    [InlineData(EstadosFactura.Desactivado)]
    [InlineData("PrimerRecordatorio")]
    public void Otros_estados_quedan_por_fuera(string estado)
    {
        Assert.False(TransicionEstado.EsGestionable(estado));
    }

    [Fact]
    public void El_primer_recordatorio_anuncia_el_paso_al_segundo()
    {
        var texto = TransicionEstado.CuerpoTexto("Ana María López", "FAC-1001", 1500000, EstadosFactura.PrimerRecordatorio);

        Assert.Contains("ha pasado a segundo recordatorio", texto);
        Assert.Contains("FAC-1001", texto);
    }

    [Fact]
    public void El_segundo_recordatorio_anuncia_la_desactivacion()
    {
        var texto = TransicionEstado.CuerpoTexto("Carlos Gómez Ruiz", "FAC-2001", 2200000, EstadosFactura.SegundoRecordatorio);

        Assert.Contains("va a ser desactivada", texto);
        Assert.Contains("desactivado", texto);
    }
}
