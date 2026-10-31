using System.Security.Claims;
using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.Models;

namespace ChecklistInspecao.Web.services;

public class UsuarioAtual
{
    private readonly ClaimsPrincipal _user;
    private readonly UsuarioRepository _usuarioRepository;
    private int[]? _regiaoIds;

    public UsuarioAtual(IHttpContextAccessor httpContextAccessor, UsuarioRepository usuarioRepository)
    {
        _user = httpContextAccessor.HttpContext!.User;
        _usuarioRepository = usuarioRepository;
    }

    public int Id => int.Parse(_user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public string Nome => _user.FindFirstValue(ClaimTypes.Name)!;
    public string Perfil => _user.FindFirstValue(ClaimTypes.Role)!;
    public bool EhSupervisor => Perfil == Perfis.Supervisor;

    public async Task<int[]> ObterRegiaoIdsAsync()
    {
        _regiaoIds ??= await _usuarioRepository.ObterIDsRegioesVisiveisAsync(Id, Perfil); // ??= atribui o valor somente se a variável for nula
        return _regiaoIds;
    }
}
