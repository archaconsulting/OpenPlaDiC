using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.WebApp.Models;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusRes = statusCode switch
            {
                404 => "Lo sentimos, el recurso o entidad no existe en el Kernel.",
                403 => "No tienes permisos suficientes para acceder a este módulo.",
                _ => "Ocurrió un error inesperado en la plataforma."
            };

            ViewBag.ErrorMessage = statusRes;
            ViewBag.StatusCode = statusCode;

            return View("NotFound");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            // Recuperamos el error detallado de ASP.NET Core
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
            var model = new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ExceptionMessage = exceptionDetails?.Error.Message,
                StackTrace = exceptionDetails?.Error.StackTrace,
                Path = exceptionDetails?.Path
            };

            // Log opcional en EventLog (puedes inyectar tu servicio de logs aquí)
            return View(model);
        }
    }
}
