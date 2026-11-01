using Npgsql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using ChecklistInspecao.Web.Data;
using ChecklistInspecao.Web.services;

var builder = WebApplication.CreateBuilder(args);

// ----- Adição de serviços -----
builder.Services.AddRazorPages(options =>
{
    // Páginas que podem ser acessadas sem autentição
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
});

// "Pool" de conexo, singleton registrado somente uma vez
builder.Services.AddSingleton(NpgsqlDataSource.Create(
    builder.Configuration.GetConnectionString("Default")!));

// mapea colunas em snake_case para PascalCase senha_hash -> SenhaHash
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// ---- Repositórios ----
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<LojaRepository>();
builder.Services.AddScoped<InspecaoRepository>();
builder.Services.AddScoped<RegiaoRepository>();
builder.Services.AddScoped<DashboardRepository>();

// Serviço do usuário logodo
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UsuarioAtual>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ----- Fim da adição de serviços -----

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
