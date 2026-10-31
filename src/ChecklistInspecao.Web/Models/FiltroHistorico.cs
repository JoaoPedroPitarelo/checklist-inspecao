
namespace ChecklistInspecao.Web.Models;

public record FiltroHistorico(
    DateTime? DataInicio,
    DateTime? DataFim,
    int? RegiaoId,
    int? LojaId,
    string? Status
);
