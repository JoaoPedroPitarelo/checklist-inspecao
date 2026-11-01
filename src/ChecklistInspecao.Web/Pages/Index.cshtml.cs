using System.ComponentModel.DataAnnotations;
using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChecklistInspecao.Web.Pages;

public class IndexModel : PageModel
{
    private readonly DashboardRepository _dashboardRepository;
    private readonly LojaRepository _lojaRepository;
    private readonly RegiaoRepository _regiaoRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public IndexModel(
        DashboardRepository dashboardRepository,
        LojaRepository lojaRepository,
        RegiaoRepository regiaoRepository,
        UsuarioAtual usuarioAtual)
    {
        _dashboardRepository = dashboardRepository;
        _lojaRepository = lojaRepository;
        _regiaoRepository = regiaoRepository;
        _usuarioAtual = usuarioAtual;
    }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? DataInicio { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? DataFim { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? RegiaoId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LojaId { get; set; }

    public DashboardResumo Resumo { get; private set; } = new();
    public IReadOnlyList<LojaNaoInspecionada> NaoInspecionadas { get; private set; } = [];
    public IReadOnlyList<ItemPendencia> TopPendencias { get; private set; } = [];
    public IReadOnlyList<ConsolidadoRegiao> PorRegiao { get; private set; } = [];

    public IReadOnlyList<Opcao> Regioes { get; private set; } = [];
    public IReadOnlyList<LojaResumo> Lojas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        DataFim ??= DateTime.Today;
        DataInicio ??= DataFim.Value.AddDays(-29);

        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();
        Regioes = await _regiaoRepository.ListarAsync(regiaoIds);
        Lojas = await _lojaRepository.ListarAsync(regiaoIds);

        if (DataInicio > DataFim)
        {
            ModelState.AddModelError(nameof(DataFim), "A data final deve ser igual ou posterior à data inicial.");
            return;
        }

        var filtro = new FiltroDashboard(DataInicio.Value, DataFim.Value, RegiaoId, LojaId);
        Resumo = await _dashboardRepository.ObterResumoAsync(filtro, regiaoIds);
        NaoInspecionadas = await _dashboardRepository.ListarNaoInspecionadasAsync(filtro, regiaoIds);
        TopPendencias = await _dashboardRepository.ListarTopPendenciasAsync(filtro, regiaoIds);
        PorRegiao = await _dashboardRepository.ListarPorRegiaoAsync(filtro, regiaoIds);
    }
}
