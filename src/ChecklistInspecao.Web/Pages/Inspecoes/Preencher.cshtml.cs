using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChecklistInspecao.Web.Pages.Inspecoes;

[Authorize(Roles = Perfis.Supervisor)]
public class PreencherModel : PageModel
{
    private static readonly string[] RespostasValidas = [
        RespostaItem.Ok, 
        RespostaItem.Pendente, 
        RespostaItem.NaoSeAplica
    ];

    private readonly InspecaoRepository _inspecaoRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public PreencherModel(InspecaoRepository inspecaoRepository, UsuarioAtual usuarioAtual)
    {
        _inspecaoRepository = inspecaoRepository;
        _usuarioAtual = usuarioAtual;
    }

    public InspecaoDetalhe Inspecao { get; private set; } = null!;
    public decimal? PercentualAtual => PercentualRealizacao.Calcular(Inspecao.Itens.Select(i => i.Resposta));

    [BindProperty]
    public List<ItemRespostaInput> Itens { get; set; } = [];

    [TempData]
    public string? Mensagem { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var erro = await CarregarAsync(id);
        return erro ?? Page();
    }

    public async Task<IActionResult> OnPostSalvarAsync(int id)
    {
        var erro = await CarregarAsync(id);
        if (erro is not null)
        {
            return erro;
        }

        if (Itens.Any(i => i.Resposta is not null && !RespostasValidas.Contains(i.Resposta)))
        {
            return BadRequest();
        }

        foreach (var item in Itens)
        {
            item.Observacao = string.IsNullOrWhiteSpace(item.Observacao) ? null : item.Observacao.Trim();
        }

        var salvou = await _inspecaoRepository.SalvarRespostasAsync(id, _usuarioAtual.Id, Itens);
        if (!salvou)
        {
            return Forbid();
        }

        Mensagem = "Rascunho salvo.";
        return RedirectToPage(new { id });
    }

    private async Task<IActionResult?> CarregarAsync(int id)
    {
        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();
        var inspecao = await _inspecaoRepository.ObterAsync(id, regiaoIds);

        if (inspecao is null)
        {
            return NotFound();
        }

        if (inspecao.AutorId != _usuarioAtual.Id || !inspecao.EhRascunho)
        {
            return Forbid();
        }

        Inspecao = inspecao;
        return null;
    }
}
