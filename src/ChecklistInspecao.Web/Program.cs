using Npgsql;
using ChecklistInspecao.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// ----- Adição de serviços -----
builder.Services.AddRazorPages();

// "Pool" de conexo, singleton registrado somente uma vez
builder.Services.AddSingleton(NpgsqlDataSource.Create(
    builder.Configuration.GetConnectionString("Default")!));

// mapea colunas em snake_case para PascalCase senha_hash -> SenhaHash
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Repositórios: ----
builder.Services.AddScoped<UsuarioRepository>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
