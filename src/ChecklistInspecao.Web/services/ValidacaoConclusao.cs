using ChecklistInspecao.Web.Models;

namespace ChecklistInspecao.Web.services;

public record ProblemaConclusao(int ItemId, string Mensagem);

public static class ValidacaoConclusao
{
    public static List<ProblemaConclusao> Validar(IEnumerable<InspecaoItem> itens)
    {
        var problemas = new List<ProblemaConclusao>();

        foreach (var item in itens)
        {
            if (item.Resposta is null)
            {
                problemas.Add(new(item.Id, $"Item {item.ItemOrdem} - {item.ItemNome}: sem resposta."));
            }
            else if (item.Resposta == RespostaItem.Pendente && string.IsNullOrWhiteSpace(item.Observacao))
            {
                problemas.Add(new(item.Id, $"Item {item.ItemOrdem} - {item.ItemNome}: item Pendente exige observação."));
            }
        }

        return problemas;
    }
}
