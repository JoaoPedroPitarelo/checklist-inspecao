using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ChecklistInspecao.Web.Data;

namespace ChecklistInspecao.Web.Pages;
public class LoginModel : PageModel
{
    private readonly UsuarioRepository _usuarioRepository;

    public LoginModel(UsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    // Classe que "mapea" o formulário
    public class LoginInput
    {
        [Required(ErrorMessage = "Informe o usuário.")]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Informe a senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // Verifica se os campos obrigatórios foram preenchdidos
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Busca o usuário, verifica se está ativo e senha a senha bate com armazenado no bd
        var usuario = await _usuarioRepository.ObterPorLoginAsync(Input.Login.Trim());

        if (usuario is null || !usuario.Ativo || !BCrypt.Net.BCrypt.Verify(Input.Senha, usuario.SenhaHash))
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return Page();
        }

        // monta a indentidade (claim) e registra o cookie
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Perfil)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        // se não autenticado retorna o usuário aonda estava no site
        if (Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        // Caso login, redireciona para página inicial
        return RedirectToPage("/Index");
    }
}
