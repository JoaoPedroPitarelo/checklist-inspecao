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
    private readonly InspecaoRepository _inspecaoRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public PreencherModel(InspecaoRepository inspecaoRepository, UsuarioAtual usuarioAtual)
    {
        _inspecaoRepository = inspecaoRepository;
        _usuarioAtual = usuarioAtual;
    }

    public InspecaoDetalhe Inspecao { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
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
        return Page();
    }
}
