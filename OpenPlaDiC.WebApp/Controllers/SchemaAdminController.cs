using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ.Services;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.Framework;
using System.Security.Claims;

namespace OpenPlaDiC.Web.Controllers;

public class SchemaAdminController(IEntitySchemaService entitySchemaService) : Controller
{
    private readonly IEntitySchemaService _entitySchemaService = entitySchemaService;

    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile schemaFile)
    {
        if (schemaFile == null || schemaFile.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Por favor, selecciona un archivo JSON de esquema válido.");
            return View();
        }

        // Validación estricta de extensión para evitar vulnerabilidades de ejecución
        var extension = Path.GetExtension(schemaFile.FileName).ToLowerInvariant();
        if (extension != ".json")
        {
            ModelState.AddModelError(string.Empty, "El archivo debe tener la extensión .json obligatoriamente.");
            return View();
        }

        try
        {
            // Leer el contenido del flujo de forma asíncrona y segura
            using var reader = new StreamReader(schemaFile.OpenReadStream());
            string jsonContent = await reader.ReadToEndAsync();

            // Extraer el ID del usuario en sesión desde los Claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "00000000-0000-0000-0000-000000000000";
            Guid currentUserId = Guid.Parse(userIdString);

            // Invocación del servicio homologado
            Response<string> response = await _entitySchemaService.ImportSchemaAsync(jsonContent, currentUserId);

            if (response.IsSuccess)
            {
                // Inyectamos un mensaje estético de éxito para que la siguiente vista lo renderice
                TempData["SuccessMessage"] = response.Message;
                
                // Redirección dinámica: enviamos al administrador directamente al listado (Index) 
                // de la nueva entidad que acaba de esculpirse en caliente en SQL Server.
                return RedirectToAction("Index", "Dynamic", new { entityName = response.Data });
            }

            // Si el servicio falló de forma controlada (ej: la entidad ya existía en la metadata), rellenamos el ModelState
            ModelState.AddModelError(string.Empty, response.Message);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Fallo inesperado en el pipeline de importación: {ex.Message}");
        }

        return View();
    }
}