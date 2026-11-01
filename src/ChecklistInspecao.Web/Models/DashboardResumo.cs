namespace ChecklistInspecao.Web.Models;

// Blocos 1 e 2 do dashboard
public class DashboardResumo
{
    public int TotalInspecoes { get; set; }
    public int Concluidas { get; set; }
    public int Rascunhos { get; set; }
    public decimal? PercentualMedio { get; set; }
    public int LojasAtivas { get; set; }
    public int LojasInspecionadas { get; set; }
    public int ItensOk { get; set; }
    public int ItensPendentes { get; set; }
    public int ItensNa { get; set; }
}
