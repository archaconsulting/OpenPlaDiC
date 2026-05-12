using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using System.Security.Claims;

namespace OpenPlaDiC.WebApp.Controllers
{
    [Authorize(Policy = "MasterOnly")]
    public class SystemParameterController : Controller
    {
        private readonly ISystemParameterService _paramService;

        public SystemParameterController(ISystemParameterService paramService)
        {
            _paramService = paramService;
        }

        // Listado agrupado por categorías (EMAIL, GENERAL, etc.)
        public async Task<IActionResult> Index()
        {
            var parameters = await _paramService.GetAllParametersAsync();
            return View(parameters);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string key, string value)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());
            var response = await _paramService.UpdateValueAsync(key, value, userId);
            
            return Json(response);
        }

        [HttpPost]
        public IActionResult ReloadCache()
        {
            // Casteamos al servicio para llamar al método de recarga
            ((SystemParameterService)_paramService).ReloadAll();
            return Json(new { isSuccess = true, message = "Kernel Cache Refreshed" });
        }

    }
}
