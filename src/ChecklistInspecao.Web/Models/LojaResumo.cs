namespace ChecklistInspecao.Web.Models;

public class LojaResumo
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string Nome { get; set; } = "";
    public string RegiaoNome { get; set; } = "";
    public string GerenteNome { get; set; } = "";
    public bool Ativo { get; set; }
}
