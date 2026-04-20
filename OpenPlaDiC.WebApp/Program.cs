using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.DAL;
using OpenPlaDiC.SF;
using OpenPlaDiC.WebApp.Models;

using Microsoft.AspNetCore.HttpOverrides;



using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options; // For IOptions<RequestLocalizationOptions>

var builder = WebApplication.CreateBuilder(args);

// 1. Add Localization Services
builder.Services.AddLocalization();

// 2. Configure Request Localization Options from AppSettings
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = builder.Configuration.GetSection("Localization:SupportedCultures")
                           .AsEnumerable()
                           .Where(c => c.Value != null)
                           .Select(c => c.Value)
                           .ToList();

    // Convert the list of strings to a list of CultureInfo objects using LINQ
    List<CultureInfo> supportedCultures = cultures
        .Select(name => new CultureInfo(name))
        .ToList();


    var defaultCulture = builder.Configuration["Localization:DefaultCulture"] ?? "en-US";

    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Add providers (optional, defaults are good)
    // Cookie provider is great for remembering user's choice
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
    // Query string provider (e.g., ?culture=es-ES)
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

// Obtener cadena de conexión
string connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped(sp => new AppDbContext(connString));


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connString));


// Registro de la BIZ (Lógica de Negocio)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDataService, DataService>();

// Registro del servicio Salesforce
builder.Services.AddScoped<ISFService, SFService>();



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


// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();




//// Configurar servicios de localización
//builder.Services.Configure<RequestLocalizationOptions>(options =>
//{
//    var supportedCultures = new[] { "es-MX", "en-US" };
//    options.DefaultRequestCulture = new RequestCulture("es-MX");
//    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
//    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
//});


var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Usar el middleware de localización
app.UseRequestLocalization();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();

    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
