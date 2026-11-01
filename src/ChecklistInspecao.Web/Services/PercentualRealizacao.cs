using System.Globalization;
using ChecklistInspecao.Web.Models;

namespace ChecklistInspecao.Web.Services;

public static class PercentualRealizacao
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public static decimal? Calcular(IEnumerable<string?> respostas)
    {
        var lista = respostas.ToList();
        var ok = lista.Count(r => r == RespostaItem.Ok);
        var pendentes = lista.Count(r => r == RespostaItem.Pendente);

        if (ok + pendentes == 0)
        {
            return null;
        }

        return Math.Round(ok * 100m / (ok + pendentes), 2, MidpointRounding.AwayFromZero);
    }

    public static string Formatar(decimal? percentual) => percentual is null ? "—" : percentual.Value.ToString("0.0", PtBr) + "%";
}
