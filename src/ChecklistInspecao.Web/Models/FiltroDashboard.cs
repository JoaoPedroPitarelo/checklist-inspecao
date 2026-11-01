namespace ChecklistInspecao.Web.Models;

public record FiltroDashboard(
    DateTime DataInicio,
    DateTime DataFim,
    int? RegiaoId,
    int? LojaId
);
