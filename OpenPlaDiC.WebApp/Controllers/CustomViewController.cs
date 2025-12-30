using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ.Services;
using OpenPlaDiC.Entities.Models;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class CustomViewController : Controller
    {
        private readonly IDataService _dataService;

        public CustomViewController(IDataService dataService)
        {
            _dataService = dataService;
        }

        // LISTAR: GET /CustomView
        public IActionResult Index()
        {
            var response = _dataService.GetQuery("SELECT * FROM CustomView ORDER BY Name");
            return View(response.Data); // Enviamos el DataTable directamente
        }

        // CREAR: GET /CustomView/Create
        public IActionResult Create()
        {
            return View();
        }

        // CREAR: POST /CustomView/Create
        [HttpPost]
        public IActionResult Create(CustomView model)
        {
            string sql = @"INSERT INTO CustomView (Name, Label, Icon, Content, IsAvailable, ControlledAccess, OwnerId) 
                       VALUES (@Name, @Label, @Icon, @Content, @IsAvailable, @ControlledAccess, @OwnerId)";

            var parameters = new[] {
            new GlobalItem("@Name", model.Name),
            new GlobalItem("@Label", model.Label),
            new GlobalItem("@Icon", model.Icon??""),
            new GlobalItem("@Content", model.Content??""),
            new GlobalItem("@IsAvailable", model.IsAvailable ? "1" : "0"),
            new GlobalItem("@ControlledAccess", model.ControlledAccess  ? "1" : "0"),
            new GlobalItem("@OwnerId", model.OwnerId.ToString()) // Asegúrate de obtener el ID del usuario actual
        };

            var result = _dataService.ExecQuery(sql, parameters);
            if (result.IsSuccess) return RedirectToAction(nameof(Index));

            return View(model);
        }


        // ELIMINAR: POST /CustomView/Delete/{id}
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            _dataService.ExecQuery("DELETE FROM CustomView WHERE Id = @Id", new GlobalItem("@Id", id.ToString()));
            return RedirectToAction(nameof(Index));
        }


        // GET: CustomView/Edit/{id}
        public IActionResult Edit(Guid id)
        {
            // Consultamos el registro por su Id
            var response = _dataService.GetQuery(
                "SELECT * FROM CustomView WHERE Id = @Id",
                new GlobalItem("@Id", id.ToString())
            );

            if (response.Data == null || response.Data.Rows.Count == 0)
            {
                return NotFound();
            }

            // Pasamos el DataRow directamente a la vista
            var row = response.Data.Rows[0];
            return View(row);
        }

        // POST: CustomView/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, IFormCollection form)
        {
            // Construimos el comando SQL de actualización basado en la normalización al inglés
            string sql = @"UPDATE CustomView SET 
                    Name = @Name, 
                    Label = @Label, 
                    Icon = @Icon, 
                    Content = @Content, 
                    IsAvailable = @IsAvailable, 
                    ControlledAccess = @ControlledAccess,
                    ModificationDate = GETDATE()
                   WHERE Id = @Id";

            // Mapeo de parámetros desde el formulario
            var parameters = new[] {
        new GlobalItem("@Id", id.ToString()),
        new GlobalItem("@Name", form["Name"].ToString()),
        new GlobalItem("@Label", form["Label"].ToString()),
        new GlobalItem("@Icon", form["Icon"].ToString()),
        new GlobalItem("@Content", form["Content"].ToString()),
        // Manejo de checkboxes de Bootstrap (si no vienen en el form, son false)
        new GlobalItem("@IsAvailable", form["IsAvailable"].Contains("true") ? "1" : "0"),
        new GlobalItem("@ControlledAccess", form["ControlledAccess"].Contains("true") ? "1" : "0")
    };

            var result = _dataService.ExecQuery(sql, parameters);

            if (result.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "Error al actualizar el registro: " + result.Message;
            return View(form);
        }


    }
}
