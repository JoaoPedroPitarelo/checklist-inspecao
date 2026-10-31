
namespace ChecklistInspecao.Web.Models;

public class InspecaoDetalhe
{
    public int Id { get; set; }
    public int LojaNumero { get; set; }
    public string LojaNome { get; set; } = "";
    public string RegiaoNome { get; set; } = "";
    public string GerenteNome { get; set; } = "";
    public int AutorId { get; set; }
    public string AutorNome { get; set; } = "";
    public DateTime DataVisita { get; set; }
    public string? Observacao { get; set; }
    public string Status { get; set; } = "";
    public decimal? Percentual { get; set; }

    // Auditoria (RF08)
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
    public string? ModificadoPorNome { get; set; }

    public List<InspecaoItem> Itens { get; set; } = [];

    public bool EhRascunho => Status == StatusInspecao.Rascunho;
}
