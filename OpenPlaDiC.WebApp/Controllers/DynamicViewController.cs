using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.WebApp.Models;

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
            var customViewsPath = Path.Combine(_env.ContentRootPath, "Views", "Custom");
            
            // Mapeo explícito a nuestro ViewModel dedicado
            var model = views.Select(v => new DynamicViewRowViewModel
            {
                Data = v,
                FileExists = System.IO.File.Exists(Path.Combine(customViewsPath, $"{v.Name}.cshtml"))
            }).ToList(); // Genera una List<DynamicViewRowViewModel> real y física

            return View(model);
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
    
    
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            try
            {
                // 1. Recuperar la vista actual desde el Kernel
                var response = await _viewService.GetByIdAsync(id);
                if (!response.IsSuccess || response.Data == null)
                {
                    return Json(new { isSuccess = false, message = "View not found." });
                }

                var view = response.Data;
                
                // 2. Invertir el estado lógico
                view.IsActive = !view.IsActive;

                // 3. Persistir el cambio en la base de datos a través del servicio homologado
                var saveResponse = await _viewService.SaveViewAsync(view);
                if (!saveResponse.IsSuccess)
                {
                    return Json(new { isSuccess = false, message = saveResponse.Message });
                }

                // 4. Sincronización física reactiva en el disco de Ubuntu
                var customViewsPath = Path.Combine(_env.ContentRootPath, "Views", "Custom");
                var filePath = Path.Combine(customViewsPath, $"{view.Name}.cshtml");

                if (!view.IsActive)
                {
                    // Opcional: Si se inactiva, eliminamos o renombramos el archivo físico 
                    // para que CustomController arroje un 404/NotFound real de inmediato
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                else
                {
                    // Si se activa y por alguna razón no existía el archivo en disco, 
                    // forzamos la regeneración física usando el contenido de la base de datos
                    if (!System.IO.File.Exists(filePath))
                    {
                        await System.IO.File.WriteAllTextAsync(filePath, view.Content);
                    }
                }

                return Json(new { 
                    isSuccess = true, 
                    isActive = view.IsActive, 
                    message = $"View status updated to {(view.IsActive ? "Active" : "Inactive")}." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = $"Sychronization failure: {ex.Message}" });
            }
        }    
    
    }
}
