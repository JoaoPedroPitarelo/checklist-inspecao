using ChecklistInspecao.Web.Models;

namespace ChecklistInspecao.Web.services;

public static class Exibicao
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

  
    public static string DataHora(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, Brasilia).ToString("dd/MM/yyyy HH:mm");

   
    public static (string Texto, string Classe) Status(string status) => status switch
    {
        StatusInspecao.Rascunho => ("Rascunho", "bg-warning text-dark"),
        StatusInspecao.Concluida => ("Concluída", "bg-success"),
        StatusInspecao.Encerrada => ("Encerrada", "bg-dark"),
        _ => (status, "bg-secondary")
    };

    public static (string Texto, string Classe) Resposta(string? resposta) => resposta switch
    {
        RespostaItem.Ok => ("OK", "bg-success"),
        RespostaItem.Pendente => ("Pendente", "bg-danger"),
        RespostaItem.NaoSeAplica => ("N/A", "bg-secondary"),
        _ => ("Sem resposta", "bg-light text-dark border")
    };

    public static (string Icone, string Nome) Perfil(string? perfil) => perfil switch
    {
        Perfis.Supervisor => ("bi-clipboard-check", "Supervisor"),
        Perfis.Diretoria => ("bi-graph-up", "Diretoria"),
        Perfis.Administrador => ("bi-gear", "Administrador"),
        _ => ("bi-person-circle", "Usuário")
    };
}
