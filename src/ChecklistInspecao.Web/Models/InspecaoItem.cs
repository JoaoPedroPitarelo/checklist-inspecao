
namespace ChecklistInspecao.Web.Models;

public class InspecaoItem
{
    public int Id { get; set; }
    public string CategoriaNome { get; set; } = "";
    public int CategoriaOrdem { get; set; }
    public string ItemNome { get; set; } = "";
    public string ItemDescricao { get; set; } = "";
    public int ItemOrdem { get; set; }
    public string? Resposta { get; set; }
    public string? Observacao { get; set; }
}
