using System.ComponentModel.DataAnnotations;
using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChecklistInspecao.Web.Pages.Inspecoes;

[Authorize(Roles = Perfis.Supervisor)]
public class CriarModel : PageModel
{
    private readonly InspecaoRepository _inspecaoRepository;
    private readonly LojaRepository _lojaRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public CriarModel(
        InspecaoRepository inspecaoRepository,
        LojaRepository lojaRepository,
        UsuarioAtual usuarioAtual)
    {
        _inspecaoRepository = inspecaoRepository;
        _lojaRepository = lojaRepository;
        _usuarioAtual = usuarioAtual;
    }

    [BindProperty]
    public CriarInput Input { get; set; } = new();

    // Lojas "criaveis", ativas e dentro da abrangência do usuário ativo
    public IReadOnlyList<LojaResumo> Lojas { get; private set; } = [];

    public class CriarInput
    {
        [Required(ErrorMessage = "Selecione a loja.")]
        public int? LojaId { get; set; }

        [Required(ErrorMessage = "Informe a data da visita.")]
        [DataType(DataType.Date)]
        public DateTime? DataVisita { get; set; }

        [StringLength(2000, ErrorMessage = "A observação pode ter no máximo 2000 caracteres.")]
        public string? Observacao { get; set; }
    }

    public async Task OnGetAsync()
    {
        Input.DataVisita = DateTime.Today;
        await CarregarLojasAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.DataVisita > DateTime.Today)
        {
            ModelState.AddModelError("Input.DataVisita", "A data da visita não pode ser futura.");
        }

        if (!ModelState.IsValid)
        {
            await CarregarLojasAsync();
            return Page();
        }

        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();

        var observacao = string.IsNullOrWhiteSpace(Input.Observacao)
            ? null
            : Input.Observacao.Trim();

        var inspecaoId = await _inspecaoRepository.CriarAsync(
            Input.LojaId!.Value,
            Input.DataVisita!.Value,
            observacao,
            _usuarioAtual.Id,
            regiaoIds
        );

        if (inspecaoId is null)
        {
            ModelState.AddModelError("Input.LojaId", "Loja inválida.");
            await CarregarLojasAsync();
            return Page();
        }

        return RedirectToPage("/Inspecoes/Preencher", new { id = inspecaoId });
    }

    private async Task CarregarLojasAsync()
    {
        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();
        Lojas = await _lojaRepository.ListarAsync(regiaoIds, somenteAtivas: true);
    }
}
