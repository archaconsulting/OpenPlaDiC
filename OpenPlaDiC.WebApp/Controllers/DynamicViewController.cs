using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.BIZ;

namespace OpenPlaDiC.WebApp.Controllers
{
    // Solo permitir acceso a usuarios Master (Seguridad Kernel)
    // [Authorize(Roles = "Administrator")] 
    public class DynamicViewController : Controller
    {
        private readonly IDynamicViewService _viewService;
        private readonly IWebHostEnvironment _env; // Definir la variable

        public DynamicViewController(IDynamicViewService viewService, IWebHostEnvironment env)
        {
            _viewService = viewService;
            _env = env; // Inyectar el entorno
        }

        // Listado de vistas
        public async Task<IActionResult> Index()
        {
            var views = await _viewService.GetAllAsync();
            // Ahora _env ya estará disponible
            var customViewsPath = Path.Combine(_env.ContentRootPath, "Views", "Custom");
            
            var model = views.Select(v => new {
                Data = v,
                FileExists = System.IO.File.Exists(Path.Combine(customViewsPath, $"{v.Name}.cshtml"))
            });

            return View(views);
        }


        // Formulario de Creación/Edición
        public async Task<IActionResult> Editor(Guid? id)
        {
            if (id == null) return View(new DynamicView { ViewType = "VIEW" });

            var response = await _viewService.GetByIdAsync(id.Value);
            if (!response.IsSuccess) return NotFound();

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(DynamicView view)
        {

            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("CreatedById");
            
            if (!ModelState.IsValid) return View("Editor", view);

            var response = await _viewService.SaveViewAsync(view);
            if (response.IsSuccess)
            {
                TempData["Message"] = "View saved and synchronized successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", response.Message);
            return View("Editor", view);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _viewService.DeleteViewAsync(id);
            return Json(response);
        }
    }
}
