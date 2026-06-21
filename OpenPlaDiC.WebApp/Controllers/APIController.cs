using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Framework;
using OpenPlaDiC.Core.Models;
using System.Net;
using System.Text.Json;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class APIController : Controller
    {
        private readonly IDataService _dataService;
        private readonly IDynamicViewService _viewService; // ⚡ Inyectado para validar metadatos
        private readonly IRazorRenderService _renderer;     // ⚡ Inyectado para renderizar Razor nativo
        private readonly IWebHostEnvironment _env;

        public APIController(IDataService dataService, IDynamicViewService viewService, IRazorRenderService renderer, IWebHostEnvironment env)
        {
            _dataService = dataService;
            _viewService = viewService;
            _renderer = renderer;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // 🟢 ENDPOINTS TRANSACCIONALES REUTILIZADOS
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult GetProcData([FromBody] ProcDataModel model)
        {
            try
            {
                if (model.ProcName == null)
                {
                    return Json(new Response { Message = "Procedure name is required." });
                }
                
                var resp = model.Parameters == null 
                    ? _dataService.ExecProc(model.ProcName)
                    : _dataService.ExecProc(model.ProcName, model.Parameters.ToArray());

                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message, InnerException = ex.InnerException?.Message ?? "" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> GetSqlDataAsync([FromBody] QueryDataModel model)
        {
            try
            {
                if (model.SQLQuery == null)
                {
                    return Json(new Response { Message = "Query is required." });
                }

                var resp = model.Parameters == null
                    ? await _dataService.GetQueryAsync(model.SQLQuery)
                    : await _dataService.GetQueryAsync(model.SQLQuery, model.Parameters.ToArray());

                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message, InnerException = ex.InnerException?.Message ?? "" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> ExecSqlAsync([FromBody] QueryDataModel model)
        {
            try
            {
                if (model.SQLQuery == null)
                {
                    return Json(new Response { Message = "Query is required." });
                }

                var resp = model.Parameters == null
                    ? await _dataService.ExecQueryAsync(model.SQLQuery)
                    : await _dataService.ExecQueryAsync(model.SQLQuery, model.Parameters.ToArray());

                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message, InnerException = ex.InnerException?.Message ?? "" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetQueryAsync([FromBody] QueryDataModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.SQLQuery))
                    return Json(new Response { Message = "Query is required." });

                var resp = await _dataService.GetQueryAsync(model.SQLQuery, model.Parameters?.ToArray() ?? Array.Empty<GlobalItem>());
                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(resp));
            }
            catch (Exception ex)
            {
                return Json(new Response { Message = ex.Message });
            }
        }

        // ==========================================
        // ⚡ NUEVA IMPLEMENTACIÓN TOTALMENTE TRADUCIDA
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ExecAPI([FromBody] OpenPlaDiC.Framework.Request request)
        {
        try
        {
                if (string.IsNullOrEmpty(request.View))
                {
                    return Ok(new Response { IsException = true, Message = "El parámetro 'View' es requerido por el despachador unificado." });
                }

                // 1. Validación de Metadata en el Kernel
                var viewResponse = await _viewService.GetByNameAsync(request.View);
                if (!viewResponse.IsSuccess || viewResponse.Data == null)
                {
                    return Ok(new Response { IsException = true, Message = $"La API dinámica '{request.View}' no está dada de alta." });
                }

                var dynamicView = viewResponse.Data;
                if (dynamicView.ViewType != "API")
                {
                    return Ok(new Response { IsException = true, Message = $"El componente '{request.View}' no está configurado como tipo API." });
                }

                // 2. SOLUCIÓN A LA RUTA NULA: 
                // Intentaremos primero con la ruta convencional que el RazorViewEngine de tu IRazorRenderService comprende.
                // Si tu servicio requiere la ruta desde la raíz del motor de vistas, suele ser sin la tilde "~" o usando la ruta absoluta del sistema.
                
                //string relativeViewPath = $"Views/Custom/{dynamicView.Name}.cshtml"; 
                string relativeViewPath = $"Custom/{dynamicView.Name}";
                
                // Nota alternativa: Si tu IRazorRenderService fue programado para buscar directamente en la carpeta Views, 
                // la ruta podría ser simplemente: $"Custom/{dynamicView.Name}" (sin extensión .cshtml).
                // Si el error persiste, prueba cambiando la línea de arriba por: string relativeViewPath = $"Custom/{dynamicView.Name}";

                // 3. Extraer los parámetros (GlobalItems) de tu lista nativa y pasarlos como @model
                var parametersPayload = request.Parameters ?? new List<GlobalItem>();

                // 4. Ejecución nativa del motor Razor de ASP.NET Core
                var respH = await _renderer.RenderToStringAsync(relativeViewPath, parametersPayload);

                string html = respH.Data;

                // 5. Formateo y limpieza idéntica al estándar heredado
                html = html.Replace("\n", "").Replace("\r", "").Trim();

                return Content(html, "application/json");
        }
        catch (Exception ex)
        {
            return Ok(new Response { IsException = true, Message = $"Error crítico en ExecAPI: {ex.Message}. Interno: {ex.InnerException?.Message}" });
        }
        }



        // ==========================================
        // DTOs INTERNOS DE CONTROL DE MODELOS
        // ==========================================
        public class ProcDataModel 
        {
            public string? ProcName { get; set; }
            public List<GlobalItem>? Parameters { get; set; }
        }

        public class QueryDataModel
        {
            public string? SQLQuery { get; set; }
            public List<GlobalItem>? Parameters { get; set; }
        }
    }
}