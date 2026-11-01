
namespace ChecklistInspecao.Web.Models;

public class LojaNaoInspecionada
{
    public int Numero { get; set; }
    public string Nome { get; set; } = "";
    public string RegiaoNome { get; set; } = "";
    public DateTime? UltimaVisita { get; set; }
}
