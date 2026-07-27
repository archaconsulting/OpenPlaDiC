using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.WebApp.Extensions;
using OpenPlaDiC.WebApp.Models;
using System.Diagnostics;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRedirectService _homeRedirectService;
        

        public HomeController(ILogger<HomeController> logger, IHomeRedirectService homeRedirectService)
        {
            _logger = logger;
            _homeRedirectService = homeRedirectService;
        }

        public async Task<IActionResult> IndexAsync()
        {

            // 1. Evaluar la URL destino basada en la jerarquía: Usuario -> Perfil -> General -> Anónimo -> Home
            string redirectUrl = await _homeRedirectService.GetRedirectUrlAsync(User);

            // 2. Si la ruta evaluada es diferente a "/Home/Index", redirigimos de inmediato
            if (!redirectUrl.Equals("/Home/Index", StringComparison.OrdinalIgnoreCase) && 
                !redirectUrl.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(redirectUrl);
            }

            // 3. De lo contrario, renderizamos la vista Home estándar por defecto
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
