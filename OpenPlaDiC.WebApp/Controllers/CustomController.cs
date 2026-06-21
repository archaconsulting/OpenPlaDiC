using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using System.Text.Json;
using System.IO;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.WebApp.Controllers
{
    [Route("[controller]")]
    [AllowAnonymous]
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
            var viewResponse = await _viewService.GetByNameAsync(viewName);
            if (!viewResponse.IsSuccess || viewResponse.Data == null) 
                return NotFound("The requested dynamic view does not exist.");

            var dynamicView = viewResponse.Data;

            if (!dynamicView.IsPublic && !User.Identity.IsAuthenticated) return Challenge();

            int currentAccessLevel = 0;
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (Guid.TryParse(userIdClaim, out Guid userId))
                {
                    var access = await _accessService.GetViewAccessAsync(userId, dynamicView.Id);
                    currentAccessLevel = access.AccessLevel;
                    if (!dynamicView.IsPublic && currentAccessLevel <= 0) return Forbid();
                }
            }

            var parameters = Request.Query.Select(k => new GlobalItem(k.Key, k.Value.ToString() ?? "")).ToList();

            ViewBag.AccessLevel = currentAccessLevel;
            ViewBag.ViewTitle = dynamicView.Label;

            // ⚡ EVOLUCIÓN: Si la vista es una API, no devolvemos HTML, procesamos de forma especial
            if (dynamicView.ViewType == "API")
            {
                // Para las APIs GET dinámicas, puedes procesar lógica especializada o redirigir
                // De momento, si es GET, asumimos render normal o un formato específico
            }

            return View($"~/Views/Custom/{viewName}.cshtml", parameters);
        }

        [HttpPost("{viewName}")]
        public async Task<IActionResult> HandlePost(string viewName)
        {
            var viewResponse = await _viewService.GetByNameAsync(viewName);
            if (!viewResponse.IsSuccess || viewResponse.Data == null) return NotFound();

            var dynamicView = viewResponse.Data;
            if (!dynamicView.IsPublic && !User.Identity.IsAuthenticated) return Challenge();

            int currentAccessLevel = 0;
            if (User.Identity.IsAuthenticated)
            {
                var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());
                var access = await _accessService.GetViewAccessAsync(userId, dynamicView.Id);
                currentAccessLevel = access.AccessLevel;
                if (!dynamicView.IsPublic && !access.CanExecute) return Forbid();
            }

            // ⚡ BLINDAJE: Captura polimórfica de parámetros (Form o JSON Body)
            var parameters = new List<GlobalItem>();
            
            if (Request.HasFormContentType)
            {
                foreach (var item in Request.Form)
                {
                    parameters.Add(new GlobalItem(item.Key, item.Value.ToString()));
                }
            }
            else if (Request.ContentType != null && Request.ContentType.Contains("application/json"))
            {
                // Si el front nos manda un fetch JSON, leemos el stream del body de forma segura
                using var reader = new StreamReader(Request.Body);
                var bodyString = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(bodyString))
                {
                    // Convertimos el cuerpo JSON en un diccionario plano para el pipeline del Kernel
                    var jsonDoc = JsonDocument.Parse(bodyString);
                    foreach (var element in jsonDoc.RootElement.EnumerateObject())
                    {
                        parameters.Add(new GlobalItem(element.Name, element.Value.ToString()));
                    }
                }
            }

            ViewBag.AccessLevel = currentAccessLevel;

            // ⚡ EVOLUCIÓN CRÍTICA: Despacho de API de retorno estructurado JSON
            if (dynamicView.ViewType == "API")
            {
                // Invocamos la vista Razor como un procesador de comandos.
                // Guardamos los datos en el TempData o ejecutamos la vista de forma interna,
                // Pero para mantener la pureza, le permitimos a la vista Custom/Nombre.cshtml 
                // escribir un JSON que capturaremos, o pasamos el control a un evaluador de scripts.
                
                // Opción ultra-limpia para OpenPlaDiC: La vista .cshtml de tipo API contendrá
                // código de C# puro que interactúa con el DAL y escribe la respuesta.
                return View($"~/Views/Custom/{viewName}.cshtml", parameters);
            }

            return View($"~/Views/Custom/{viewName}.cshtml", parameters);
        }
    }
}