using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.WebApp.Models;


namespace OpenPlaDiC.WebApp.Controllers
{
    
    [Authorize(Policy = "MasterOnly")]
    [Route("EntityManager")] // Fuerza la ruta base del controlador
    public class EntityManagerController : Controller
    {
        private readonly IMetadataService _metadataService;
        private readonly AppDbContext _context; // Para ejecutar los SPs del Kernel

        public EntityManagerController(IMetadataService metadataService, AppDbContext context)
        {
            _metadataService = metadataService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entities = await _metadataService.GetAllEntitiesAsync(true);
            return View(entities);
        }

        [HttpPost("CreateEntity")]
        public async Task<IActionResult> CreateEntity(Entity model)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value);
            
            // Invocamos el Stored Procedure del Kernel que crea la tabla física y la metadata
            var response = await _context.ExecProcAsync("sp_Core_CreateEntity", 
                new GlobalItem("Name", model.Name),
                new GlobalItem("Label", model.Label),
                new GlobalItem("PageSize", model.PageSize.ToString()),
                new GlobalItem("Prefix", model.Prefix),
                new GlobalItem("Icon", model.Icon ?? "bi-table"),
                new GlobalItem("CreatedById", userId.ToString()),
                new GlobalItem("UseNameField", model.UseNameField ? "1":"0")
            );

            return RedirectToAction("Details", new { id = model.Name });
        }

        [HttpGet("Details/{entityName}")] // Ruta final: /EntityManager/Details/MiTabla
        public async Task<IActionResult> Details(string entityName)
        {
            // Agrega un breakpoint aquí para confirmar entrada
            if (string.IsNullOrEmpty(entityName)) return RedirectToAction(nameof(Index));
            
            var entity = await _metadataService.GetEntityWithPropertiesAsync(entityName);
            if (entity == null) return NotFound();

            ViewBag.DataTypes = await _context.DataTypes.ToListAsync();

            // Cargamos todas las entidades para el dropdown de relaciones
            ViewBag.AllEntities = await _metadataService.GetAllEntitiesAsync();


            return View(entity);
        }



        [HttpPost("UpdatePropertyMetadata")]
        public async Task<IActionResult> UpdatePropertyMetadata(EntityProperty model, string entityName)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());

            var response = await _context.ExecProcAsync("sp_Core_UpdatePropertyMetadata",
                new GlobalItem("PropertyId", model.Id.ToString()),
                new GlobalItem("Label", model.Label),
                new GlobalItem("GridRow", model.GridRow.ToString()),
                new GlobalItem("GridColumn", model.GridColumn.ToString()),
                new GlobalItem("OnList", model.OnList ? "1" : "0"),
                new GlobalItem("IsRequired", model.IsRequired ? "1" : "0"),
                new GlobalItem("UpdatedById", userId.ToString())
            );

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Message;
            }
            else 
            {
                // ⚡ RECALCULO AUTOMÁTICO: Si cambiaron el flag 'OnList', el ListQuery debe mutar
                await _metadataService.RefreshEntityListQueryAsync(entityName, _context);
            }
            
            return RedirectToAction("Details", new { entityName = entityName });
            //return RedirectToAction($"Details/{entityName}" );
        }



        [HttpPost("AddProperty")] // Esto generará la ruta /EntityManager/AddProperty
        public async Task<IActionResult> AddProperty(EntityProperty prop, string entityName)
        {

            if (string.IsNullOrEmpty(entityName)) {
                return BadRequest("El nombre de la entidad es obligatorio.");
            }

            var userId = Guid.Parse(User.FindFirst("UserId")?.Value);

            // Invocamos el SP que hace el ALTER TABLE ADD COLUMN
            var response = await _context.ExecProcAsync("sp_Core_AddProperty", 
                new GlobalItem("EntityName", entityName),
                new GlobalItem("PropertyName", prop.Name),
                new GlobalItem("Label", prop.Label),
                new GlobalItem("GridRow", prop.GridRow.ToString()),
                new GlobalItem("GridColumn", prop.GridColumn.ToString()),
                new GlobalItem("DataTypeId", prop.DataTypeId.ToString()),
                new GlobalItem("IsRequired", prop.IsRequired ? "1" : "0"),
                new GlobalItem("OnList", prop.OnList ? "1" : "0"),
                new GlobalItem("RelatedEntityName", prop.SourceDefinition),                
                new GlobalItem("CreatedById", userId.ToString())
            );

            if (response.IsSuccess)
            {
                TempData["Message"] = $"Campo '{prop.Label}' creado correctamente";
                // ⚡ RECALCULO AUTOMÁTICO: La columna física ya existe en base de datos, refrescamos el query
                await _metadataService.RefreshEntityListQueryAsync(entityName, _context);
            }
            else
            {
                TempData["Error"] = $"El campo '{prop.Label}' no se ha podido generar: {response.Message}.";
            }
            return RedirectToAction("Details", new { entityName });
        }

        // Acción para eliminar una propiedad físicamente
        [HttpPost("DropProperty")] // Esto generará la ruta /EntityManager/AddProperty
        public async Task<IActionResult> DropProperty(string entityName, string propertyName)
        {
            // Invocamos el SP que limpia metadata, FKs, índices y la columna física
            var response = await _context.ExecProcAsync("sp_Core_DropProperty", 
                new GlobalItem("EntityName", entityName),
                new GlobalItem("PropertyName", propertyName)
            );

            if (response.IsSuccess)
            {
                TempData["Message"] = $"Campo '{propertyName}' eliminado correctamente.";
                // ⚡ RECALCULO AUTOMÁTICO: El campo ya no existe, lo quitamos del SELECT
                await _metadataService.RefreshEntityListQueryAsync(entityName, _context);
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction("Details", new { entityName });
        }

        [HttpGet("Preview/{entityName}")]
        public async Task<IActionResult> Preview(string entityName)
        {
                    
            var entity = await _metadataService.GetEntityWithPropertiesAsync(entityName);
            if (entity == null) return NotFound();

            var mockData = new Dictionary<string, object> { ["Id"] = Guid.Empty, ["Folio"] = "PREVIEW-001" };
            foreach (var prop in entity.Properties) { mockData[prop.Name] = null; }

            var viewModel = new DynamicFormViewModel {
                EntityMetadata = entity,
                RecordData = mockData,
                AccessLevel = 1
            };

            ViewBag.IsPreview = true;

            var pv = PartialView("~/Views/Dynamic/DynamicForm.cshtml", viewModel);
            // Usamos PartialView explicitamente
            return pv;             

        }


        // Acción para actualizar el orden y diseño (Grid)
        [HttpPost("UpdateLayout")]
        public async Task<IActionResult> UpdateLayout(string entityName, List<PropertyLayoutModel> layout)
        {
            try 
            {
                foreach (var item in layout)
                {
                    // Actualizamos directamente en la tabla de metadata
                    await _context.ExecQueryAsync(
                        "UPDATE EntityProperty SET GridRow = @r, GridColumn = @c WHERE Id = @id",
                        new GlobalItem("r", item.Row.ToString()),
                        new GlobalItem("c", item.Column.ToString()),
                        new GlobalItem("id", item.PropertyId.ToString())
                    );
                }

                // ⚡ RECALCULO AUTOMÁTICO: Si el orden en el grid define el orden de aparición de las columnas,
                // refrescar aquí asegura que el T-SQL se ordene de forma idéntica a la UI.
                await _metadataService.RefreshEntityListQueryAsync(entityName, _context);

                return Json(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }


        // Acción para actualizar metadata de la entidad
        [HttpPost("UpdateEntity")]
        public async Task<IActionResult> UpdateEntity(Entity model)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString());

            // Actualizamos campos de la metadata (Label, Icon, IsAvailable)
            // No permitimos cambiar el Name (SQL) aquí para evitar romper la integridad física
            var respUpdate = await _context.ExecQueryAsync(
                "UPDATE Entity SET Label = @l, Icon = @i, IsAvailable = @a, UpdatedAt = GETDATE(), UpdatedById = @u, PageSize = @p WHERE Name = @n",
                new GlobalItem("l", model.Label),
                new GlobalItem("i", model.Icon ?? "bi-table"),
                new GlobalItem("p", model.PageSize.ToString()),
                new GlobalItem("a", model.IsAvailable ? "1" : "0"),
                new GlobalItem("u", userId.ToString()),
                new GlobalItem("n", model.Name)
            );

            TempData["Message"] = "Configuración de la entidad actualizada.";
            return RedirectToAction("Index");
        }

        // Acción para eliminar físicamente la entidad
        [HttpGet("DropEntity")]
        public async Task<IActionResult> DropEntity(string entityName)
        {
            // Invocamos el SP del Kernel que borra la tabla física y toda su metadata
            var response = await _context.ExecProcAsync("sp_Core_DropEntity", 
                new GlobalItem("EntityName", entityName)
            );

            if (response.IsSuccess)
                TempData["Message"] = $"Entidad {entityName} eliminada correctamente.";
            else
                TempData["Error"] = response.Message;

            return RedirectToAction("Index");
        }


        // Clase auxiliar para el reordenamiento
        public class PropertyLayoutModel {
            public Guid PropertyId { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
        }


    }


}
