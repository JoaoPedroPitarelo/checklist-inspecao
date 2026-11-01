using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.Services;

namespace ChecklistInspecao.Tests;

public class ValidacaoConclusaoTests
{
    private static InspecaoItem Item(int id, string? resposta, string? observacao = null) => new()
    {
        Id = id,
        ItemOrdem = id,
        ItemNome = $"Item {id}",
        Resposta = resposta,
        Observacao = observacao
    };

    [Fact]
    public void Validar_TudoRespondidoECorreto_NaoRetornaProblemas()
    {
        var itens = new[]
        {
            Item(1, RespostaItem.Ok),
            Item(2, RespostaItem.Pendente, "Vitrine desalinhada."),
            Item(3, RespostaItem.NaoSeAplica)
        };

        var problemas = ValidacaoConclusao.Validar(itens);

        Assert.Empty(problemas);
    }

    [Fact]
    public void Validar_ItemSemResposta_RetornaProblema()
    {
        var itens = new[] { Item(1, RespostaItem.Ok), Item(2, null) };

        var problema = Assert.Single(ValidacaoConclusao.Validar(itens));

        Assert.Equal(2, problema.ItemId);
        Assert.Contains("sem resposta", problema.Mensagem);
    }

    [Fact]
    public void Validar_PendenteSemObservacao_RetornaProblema()
    {
        var problema = Assert.Single(ValidacaoConclusao.Validar([Item(1, RespostaItem.Pendente)]));

        Assert.Equal(1, problema.ItemId);
        Assert.Contains("exige observação", problema.Mensagem);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Validar_PendenteComObservacaoEmBranco_RetornaProblema(string observacao)
    {
        var problemas = ValidacaoConclusao.Validar([Item(1, RespostaItem.Pendente, observacao)]);

        Assert.Single(problemas);
    }

    [Theory]
    [InlineData(RespostaItem.Ok)]
    [InlineData(RespostaItem.NaoSeAplica)]
    public void Validar_OkOuNASemObservacao_NaoRetornaProblema(string resposta)
    {
        Assert.Empty(ValidacaoConclusao.Validar([Item(1, resposta)]));
    }

    [Fact]
    public void Validar_VariosProblemas_ListaTodos()
    {
        var itens = new[]
        {
            Item(1, null),
            Item(2, RespostaItem.Ok),
            Item(3, RespostaItem.Pendente),
            Item(4, null)
        };

        var problemas = ValidacaoConclusao.Validar(itens);

        Assert.Equal(3, problemas.Count);
        Assert.Equal([1, 3, 4], problemas.Select(p => p.ItemId));
    }
}
