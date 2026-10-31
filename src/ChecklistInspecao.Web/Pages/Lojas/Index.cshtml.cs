using Microsoft.AspNetCore.Mvc.RazorPages;
using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.services;
using ChecklistInspecao.Web.Models;

namespace ChecklistInspecao.Web.Pages.Lojas;

public class IndexModel : PageModel
{
    private readonly LojaRepository _lojaRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public IndexModel(LojaRepository lojaRepository, UsuarioAtual usuarioAtual)
    {
        _lojaRepository = lojaRepository;
        _usuarioAtual = usuarioAtual;
    }

    public IReadOnlyList<LojaResumo> Lojas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();
        Lojas = await _lojaRepository.ListarAsync(regiaoIds);
    }
}
