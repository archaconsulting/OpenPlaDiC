using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.DAL;
using OpenPlaDiC.EXT;
using OpenPlaDiC.SF;
using System;
using static System.Net.Mime.MediaTypeNames;


if (false) // (args.Length == 0)
{
    Console.WriteLine("No se ha proporcionado el nombre de la tarea");

}
else
{


    string _viewName = "test01"; // args[0];
    Console.WriteLine($"Ejecutando la tarea {_viewName} ");

    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);


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



    using IHost host = builder.Build();

    ServiceExecViewAsync(host.Services, _viewName);


    await host.RunAsync();

    static async Task ServiceExecViewAsync(IServiceProvider hostProvider, string viewName)
    {
        using IServiceScope serviceScope = hostProvider.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;

        string _conventionFolderPath = @"C:\Vistas";


        RazorRenderService _renderer = provider.GetService<RazorRenderService>();


        string controller = "Custom";
        string action = viewName;
        var actionController = $"{controller}/{action}";

        string viewPath = System.IO.Path.Combine(_conventionFolderPath, actionController);

        var html = await _renderer.RenderToStringAsync(actionController, null);


        Console.WriteLine();
    }



}
