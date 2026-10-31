using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChecklistInspecao.Web.Pages;

public class LogoutModel : PageModel
{
    // Para evitar que outros sites ou requisições consigam deslogar o CheckList inspecao
    // por fora o GET só redireciona para o index
    public void OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync()
    {   
        // apaga "desassina" o cookie de autenticação
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}
