
namespace ChecklistInspecao.Web.Models;

public class ConsolidadoRegiao
{
    public string RegiaoNome { get; set; } = "";
    public int Lojas { get; set; }
    public int LojasInspecionadas { get; set; }
    public int Inspecoes { get; set; }
    public decimal? PercentualMedio { get; set; }
    public int Pendencias { get; set; }
}
