using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.BIZ.Services;
using OpenPlaDiC.DAL;
using OpenPlaDiC.DAL.Repositories;
using OpenPlaDiC.WebApp.Models;


var builder = WebApplication.CreateBuilder(args);


// Obtener cadena de conexión
string connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

// Registrar DAL como Singleton o Scoped
builder.Services.AddScoped(sp => new DataAccessRepository(connString));

builder.Services.AddScoped(sp => new AppDbContext(connString));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



// Registro de la BIZ (Lógica de Negocio)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDataService, DataService>();

// Add services to the container.
//builder.Services.AddControllersWithViews(); // Habilita soporte para Vistas
//builder.Services.AddControllers();          // Habilita soporte específico para APIs

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// Registramos nuestro servicio de renderizado
builder.Services.AddScoped<IRazorRenderService, RazorRenderService>();

// Registrar tu clase de usuario (sin base de datos)
builder.Services.AddIdentityCore<ApplicationUser>() // Especificar los tipos explícitamente
    .AddUserStore<ExternalUserStore>()
    // NO registramos AddRoleStore ni usamos ApplicationRole personalizado
    .AddSignInManager<ExternalSignInManager>()
    .AddDefaultTokenProviders();

// Registrar explícitamente el cookie con el esquema que usa Identity
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "OpenPlaDiCCookie";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Configurar el sistema de Cookies estándar
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OpenPlaDiCCookie";
    options.LoginPath = "/Account/Login"; // Coincide con Controller/Action
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});




builder.Services.AddHttpClient();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseAuthentication(); // ← imprescindible

app.MapStaticAssets();


// Mapear rutas de MVC (vistas)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// Mapear rutas de API (basadas en atributos)
app.MapControllers();

app.MapRazorPages(); // para usar Razor Pages


app.Run();
