using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.Services;
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

    public List<ProblemaConclusao> Problemas { get; private set; } = [];

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

        if (!EntradaValida())
        {
            return BadRequest();
        }

        NormalizarObservacoes();

        var salvou = await _inspecaoRepository.SalvarRespostasAsync(id, _usuarioAtual.Id, Itens);
        if (!salvou)
        {
            return Forbid();
        }

        Mensagem = "Rascunho salvo.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostConcluirAsync(int id)
    {
        var erro = await CarregarAsync(id);
        if (erro is not null)
        {
            return erro;
        }

        if (!EntradaValida())
        {
            return BadRequest();
        }

        NormalizarObservacoes();

        var enviados = Itens.ToDictionary(i => i.Id);
        foreach (var item in Inspecao.Itens)
        {
            if (enviados.TryGetValue(item.Id, out var enviado))
            {
                item.Resposta = enviado.Resposta;
                item.Observacao = enviado.Observacao;
            }
        }

        Problemas = ValidacaoConclusao.Validar(Inspecao.Itens);
        if (Problemas.Count > 0)
        {
            return Page();
        }

        var percentual = PercentualRealizacao.Calcular(Inspecao.Itens.Select(i => i.Resposta));

        var concluiu = await _inspecaoRepository.ConcluirAsync(id, _usuarioAtual.Id, Itens, percentual);
        if (!concluiu)
        {
            return Forbid();
        }

        Mensagem = "Inspeção concluída.";
        return RedirectToPage("/Inspecoes/Detalhe", new { id });
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
            return RedirectToPage("/Inspecoes/Detalhe", new { id });
        }

        Inspecao = inspecao;
        return null;
    }

    private bool EntradaValida()
    {
        var respostasOk = Itens.All(i => i.Resposta is null || RespostasValidas.Contains(i.Resposta));
        var semRepetidos = Itens.Select(i => i.Id).Distinct().Count() == Itens.Count;
        return respostasOk && semRepetidos;
    }

    private void NormalizarObservacoes()
    {
        foreach (var item in Itens)
        {
            item.Observacao = string.IsNullOrWhiteSpace(item.Observacao) ? null : item.Observacao.Trim();
        }
    }
}
