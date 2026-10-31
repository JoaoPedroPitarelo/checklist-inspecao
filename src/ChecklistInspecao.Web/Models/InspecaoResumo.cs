
namespace ChecklistInspecao.Web.Models;

public class InspecaoResumo
{
    public int Id { get; set; }
    public DateTime DataVisita { get; set; }
    public int LojaNumero { get; set; }
    public string LojaNome { get; set; } = "";
    public string RegiaoNome { get; set; } = "";
    public string AutorNome { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal? Percentual { get; set; }
}
