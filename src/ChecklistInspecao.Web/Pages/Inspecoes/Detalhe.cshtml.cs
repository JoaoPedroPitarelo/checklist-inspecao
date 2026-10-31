using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChecklistInspecao.Web.Pages.Inspecoes;

public class DetalheModel : PageModel
{
    private readonly InspecaoRepository _inspecaoRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public DetalheModel(InspecaoRepository inspecaoRepository, UsuarioAtual usuarioAtual)
    {
        _inspecaoRepository = inspecaoRepository;
        _usuarioAtual = usuarioAtual;
    }

    public InspecaoDetalhe Inspecao { get; private set; } = null!;

    [TempData]
    public string? Mensagem { get; set; }

    public bool PodeEditar => Inspecao.EhRascunho && Inspecao.AutorId == _usuarioAtual.Id;

    public decimal? Percentual => Inspecao.EhRascunho
        ? PercentualRealizacao.Calcular(Inspecao.Itens.Select(i => i.Resposta))
        : Inspecao.Percentual;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();
        var inspecao = await _inspecaoRepository.ObterAsync(id, regiaoIds);

        if (inspecao is null)
        {
            return NotFound();
        }

        Inspecao = inspecao;
        return Page();
    }
}
