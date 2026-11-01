using System.ComponentModel.DataAnnotations;
using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;
using ChecklistInspecao.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChecklistInspecao.Web.Pages.Inspecoes;

public class IndexModel : PageModel
{
    private const int TamanhoPagina = 10;

    private readonly InspecaoRepository _inspecaoRepository;
    private readonly LojaRepository _lojaRepository;
    private readonly RegiaoRepository _regiaoRepository;
    private readonly UsuarioAtual _usuarioAtual;

    public IndexModel(
        InspecaoRepository inspecaoRepository,
        LojaRepository lojaRepository,
        RegiaoRepository regiaoRepository,
        UsuarioAtual usuarioAtual)
    {
        _inspecaoRepository = inspecaoRepository;
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

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;


    public IReadOnlyList<InspecaoResumo> Inspecoes { get; private set; } = [];
    public int Total { get; private set; }
    public int TotalPaginas => (int)Math.Ceiling(Total / (double)TamanhoPagina);

    public IReadOnlyList<Opcao> Regioes { get; private set; } = [];
    public IReadOnlyList<LojaResumo> Lojas { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var regiaoIds = await _usuarioAtual.ObterRegiaoIdsAsync();

        Regioes = await _regiaoRepository.ListarAsync(regiaoIds);
        Lojas = await _lojaRepository.ListarAsync(regiaoIds);

        if (DataInicio > DataFim)
        {
            ModelState.AddModelError(nameof(DataFim), "A data final deve ser igual ou posterior à data inicial.");
            return;
        }

        if (Pagina < 1)
        {
            Pagina = 1;
        }

        var filtro = new FiltroHistorico(DataInicio, DataFim, RegiaoId, LojaId, Status);
        (Inspecoes, Total) = await _inspecaoRepository.ListarHistoricoAsync(filtro, regiaoIds, Pagina, TamanhoPagina);
    }

    public Dictionary<string, string> RotaDaPagina(int pagina)
    {
        var rota = new Dictionary<string, string> { ["Pagina"] = pagina.ToString() };

        if (DataInicio is not null) rota["DataInicio"] = DataInicio.Value.ToString("yyyy-MM-dd");
        if (DataFim is not null) rota["DataFim"] = DataFim.Value.ToString("yyyy-MM-dd");
        if (RegiaoId is not null) rota["RegiaoId"] = RegiaoId.Value.ToString();
        if (LojaId is not null) rota["LojaId"] = LojaId.Value.ToString();
        if (Status is not null) rota["Status"] = Status;

        return rota;
    }
}
