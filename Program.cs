using Everkeep.Data;
using Everkeep.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Loads DefaultConnection from application configuration; User Secrets are used in Development.
// Caso não exista, lança uma exceção.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configures the application DbContext to use PostgreSQL via Npgsql.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configura o ASP.NET Identity utilizando a classe ApplicationUser.
//
// O ApplicationUser representa o utilizador da aplicação e herda de IdentityUser,
// permitindo adicionar propriedades personalizadas como Nome, Bio e EstadoEmocional.
//
// O Identity é responsável por:
// - autenticação
// - gestão de sessões
// - hash de passwords
// - login/logout
// - segurança de utilizadores
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Adiciona suporte a controllers e views MVC.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuração do pipeline HTTP da aplicação.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Ativa o sistema de routing.
app.UseRouting();

// Ativa autenticação e autorização.
app.UseAuthentication();
app.UseAuthorization();

// Permite servir ficheiros estáticos (CSS, JS, imagens, etc.).
app.MapStaticAssets();

// Define a rota padrão da aplicação MVC.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Ativa suporte às Razor Pages do ASP.NET Identity.
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
