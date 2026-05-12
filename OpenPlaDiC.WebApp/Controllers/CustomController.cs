using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPlaDiC.WebApp.Controllers
{
    [Route("[controller]")]
    [AllowAnonymous] // Permitimos que el atributo maneje la entrada, la lógica interna filtrará
    public class CustomController : Controller
    {
        private readonly IDynamicViewService _viewService;
        private readonly IAccessService _accessService;

        public CustomController(IDynamicViewService viewService, IAccessService accessService)
        {
            _viewService = viewService;
            _accessService = accessService;
        }

        [HttpGet("{viewName}")]
        public async Task<IActionResult> HandleGet(string viewName)
        {
            // 1. Validar existencia de la vista en el Kernel
            var viewResponse = await _viewService.GetByNameAsync(viewName);
            if (!viewResponse.IsSuccess || viewResponse.Data == null)
            {
                return NotFound("The requested dynamic view does not exist.");
            }

            var dynamicView = viewResponse.Data;

            // 2. Validar Seguridad y Acceso Anónimo
            if (!dynamicView.IsPublic && !User.Identity.IsAuthenticated)
            {
                return Challenge(); // Redirige al Login
            }

            int currentAccessLevel = 0;

            // 3. Si el usuario está autenticado, recuperamos su nivel de acceso real
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (Guid.TryParse(userIdClaim, out Guid userId))
                {
                    var access = await _accessService.GetViewAccessAsync(userId, dynamicView.Id);
                    currentAccessLevel = access.AccessLevel;

                    // Si no es pública y el nivel de acceso es 0, denegar
                    if (!dynamicView.IsPublic && currentAccessLevel <= 0)
                    {
                        return Forbid();
                    }
                }
            }

            // 4. Mapear parámetros de la URL (QueryString)
            var parameters = Request.Query.Select(k => new GlobalItem(k.Key, k.Value.ToString() ?? "")).ToList();

            // Pasamos metadata útil a la vista Razor
            ViewBag.AccessLevel = currentAccessLevel;
            ViewBag.ViewTitle = dynamicView.Label;

            // Renderizado desde la ruta física predefinida en el Kernel
            return View($"~/Views/Custom/{viewName}.cshtml", parameters);
        }

        [HttpPost("{viewName}")]
        public async Task<IActionResult> HandlePost(string viewName)
        {
            // 1. Validar existencia
            var viewResponse = await _viewService.GetByNameAsync(viewName);
            if (!viewResponse.IsSuccess || viewResponse.Data == null) return NotFound();

            var dynamicView = viewResponse.Data;

            // 2. Seguridad para POST (Acciones)
            // Por seguridad, las acciones POST suelen requerir autenticación a menos que sea explícitamente pública
            if (!dynamicView.IsPublic && !User.Identity.IsAuthenticated) return Challenge();

            int currentAccessLevel = 0;
            if (User.Identity.IsAuthenticated)
            {
                var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());
                var access = await _accessService.GetViewAccessAsync(userId, dynamicView.Id);
                currentAccessLevel = access.AccessLevel;

                // Validar si tiene permiso de ejecución (CanExecute)
                if (!dynamicView.IsPublic && !access.CanExecute) return Forbid();
            }

            // 3. Mapear parámetros del Formulario (Body)
            var parameters = new List<GlobalItem>();
            if (Request.HasFormContentType)
            {
                foreach (var item in Request.Form)
                {
                    parameters.Add(new GlobalItem(item.Key, item.Value.ToString()));
                }
            }

            ViewBag.AccessLevel = currentAccessLevel;
            return View($"~/Views/Custom/{viewName}.cshtml", parameters);
        }
    }
}
