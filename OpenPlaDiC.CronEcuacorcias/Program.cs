using Microsoft.Extensions.Configuration;

namespace OpenPlaDiC.CronEcuacorcias
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fechaCargaPreventa = args.Length > 0 ? args[0] : string.Empty;

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            // 2. Retrieve the connection string
            // Note: GetConnectionString("Name") is a shorthand for GetSection("ConnectionStrings")["Name"]
            string? connectionString = config.GetConnectionString("DefaultConnection");


            Console.WriteLine("-- Procesos Ecuacorcias --");

            var p = new Procesos();
            p.CargaPreventaAsync(connectionString, fechaCargaPreventa).Wait();

            Console.WriteLine("Proceso terminado");

        }
    }
}
