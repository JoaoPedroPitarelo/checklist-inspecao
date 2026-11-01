using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.Services;

namespace ChecklistInspecao.Tests;

public class PercentualRealizacaoTests
{
    private static IEnumerable<string?> Repetir(string? resposta, int quantidade) 
        => Enumerable.Repeat(resposta, quantidade);

    [Fact]
    public void Calcular_ExemploDoEnunciado_Retorna85v71()
    {
        var respostas = Repetir(RespostaItem.Ok, 12)
            .Concat(Repetir(RespostaItem.Pendente, 2))
            .Concat(Repetir(RespostaItem.NaoSeAplica, 1));

        var percentual = PercentualRealizacao.Calcular(respostas);

        Assert.Equal<decimal?>(85.71m, percentual);
    }

    [Fact]
    public void Calcular_ItensNA_NaoAlteramOResultado()
    {
        var semNA = Repetir(RespostaItem.Ok, 1).Concat(Repetir(RespostaItem.Pendente, 1));
        var comNA = semNA.Concat(Repetir(RespostaItem.NaoSeAplica, 5));

        Assert.Equal<decimal?>(50m, PercentualRealizacao.Calcular(semNA));
        Assert.Equal<decimal?>(50m, PercentualRealizacao.Calcular(comNA));
    }

    [Fact]
    public void Calcular_SomenteNA_RetornaNulo()
    {
        var percentual = PercentualRealizacao.Calcular(Repetir(RespostaItem.NaoSeAplica, 15));

        Assert.Null(percentual);
    }

    [Fact]
    public void Calcular_ListaVazia_RetornaNulo()
    {
        Assert.Null(PercentualRealizacao.Calcular([]));
    }

    [Fact]
    public void Calcular_TodosOk_Retorna100()
    {
        Assert.Equal<decimal?>(100m, PercentualRealizacao.Calcular(Repetir(RespostaItem.Ok, 15)));
    }

    [Fact]
    public void Calcular_TodosPendentes_RetornaZero()
    {
        Assert.Equal<decimal?>(0m, PercentualRealizacao.Calcular(Repetir(RespostaItem.Pendente, 15)));
    }

    [Fact]
    public void Calcular_ItensSemResposta_SaoIgnorados()
    {
        var respostas = Repetir(RespostaItem.Ok, 1).Concat(Repetir(null, 14));

        Assert.Equal<decimal?>(100m, PercentualRealizacao.Calcular(respostas));
    }

    [Fact]
    public void Calcular_DizimaPeriodica_ArredondaPara2Casas()
    {
        var respostas = Repetir(RespostaItem.Ok, 2).Concat(Repetir(RespostaItem.Pendente, 1));

        Assert.Equal<decimal?>(66.67m, PercentualRealizacao.Calcular(respostas));
    }

    [Fact]
    public void Calcular_MeioExato_ArredondaParaCima()
    {
        var respostas = Repetir(RespostaItem.Ok, 1).Concat(Repetir(RespostaItem.Pendente, 799));

        Assert.Equal<decimal?>(0.13m, PercentualRealizacao.Calcular(respostas));
    }

    [Theory]
    [InlineData("85.71", "85,7%")]
    [InlineData("100", "100,0%")]
    [InlineData("0", "0,0%")]
    [InlineData("66.67", "66,7%")]
    public void Formatar_ExibeUmaCasaComVirgula(string valor, string esperado)
    {
        var percentual = decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(esperado, PercentualRealizacao.Formatar(percentual));
    }

    [Fact]
    public void Formatar_Nulo_ExibeTravessao()
    {
        Assert.Equal("—", PercentualRealizacao.Formatar(null));
    }
}
