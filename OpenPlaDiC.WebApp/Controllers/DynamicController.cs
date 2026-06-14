using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using System.Security.Claims;
using System.Data;
using OpenPlaDiC.WebApp.Models;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using OpenPlaDiC.Core.Models.DynamicQuery;


namespace OpenPlaDiC.WebApp.Controllers
{

    [ApiController]
    [Route("Data/{entityName}")] // <-- Define la ruta base para TODOS los endpoints de este controlador    
    public class DynamicController : Controller
    {
        private readonly IMetadataService _metadataService;
        private readonly IDynamicDataService _dynamicDataService;
        private readonly IAccessService _accessService;

        private readonly AppDbContext _context;

        public DynamicController(
            IMetadataService metadataService, 
            IDynamicDataService dynamicDataService, 
            IAccessService accessService,
            AppDbContext appDbContext)
        {
            _metadataService = metadataService;
            _dynamicDataService = dynamicDataService;
            _accessService = accessService;
            _context = appDbContext;
        }



        // En OpenPlaDiC.WebApp/Controllers/DynamicController.cs

        [HttpGet]
        public async Task<IActionResult> Index(
            string entityName, 
            [FromQuery] Dictionary<string, string> search,
            [FromQuery] int page = 1) // ASP.NET Core bindea esto automáticamente de la URL
        {
            if (page < 1) page = 1;

            var user = HttpContext?.User;
            bool isMaster = user?.HasClaim("IsMaster", "True") ?? false;

            var entityMetadata = await _metadataService.GetEntityMetadataAsync(entityName);
            if (entityMetadata == null) return NotFound();

            var userId = GetCurrentUserId();
            var access = await _accessService.GetEntityAccessAsync(userId, entityMetadata.Id);
            if (!access.CanRead) return Forbid();

            var criteria = new HashSet<FilterCriterion>();

            // Interceptamos la Query String basándonos en la metadata de la entidad
            foreach (EntityProperty prop in entityMetadata.Properties)
            {
                // Ejemplo para texto: search[Field_Text]=Juan
                if (search.TryGetValue(prop.Name, out string? value) && !string.IsNullOrWhiteSpace(value))
                {
                    if (prop.DataTypeId == 10) // RelatedEntity
                    {
                        criteria.Add(new FilterCriterion(prop.Name, FilterOperator.Equals, value));
                    }
                    else if (prop.DataTypeId == 0)
                    {
                        criteria.Add(new FilterCriterion(prop.Name, FilterOperator.Contains, value));
                    }
                }
                // Rangos para fechas/números: search[Field_Date_From] y search[Field_Date_To]
                else if (search.TryGetValue($"{prop.Name}_From", out var fromVal) && 
                        search.TryGetValue($"{prop.Name}_To", out var toVal))
                {
                    if (!string.IsNullOrWhiteSpace(fromVal) && !string.IsNullOrWhiteSpace(toVal))
                    {
                        criteria.Add(new FilterCriterion(prop.Name, FilterOperator.Between, fromVal, toVal));
                    }
                }
            }

            // Consumimos el servicio de negocio pasándole los criterios reconstruidos
            var dataGrid = await _dynamicDataService.GetPagedDataAsync(entityName, criteria,page, isMaster);

            var viewModel = new DynamicIndexViewModel {
                EntityMetadata = entityMetadata,
                Data = dataGrid,
                CurrentFilters = search,
                CurrentPage = page // Añade esta propiedad a tu ViewModel para controlar los botones Sig/Ant
            };

            ViewBag.Access = access;


            return View("DynamicIndex", viewModel);
        }


        // Formulario de Edición / Creación
        [HttpGet("Edit/{id?}")]
        public async Task<IActionResult> Edit(string entityName, Guid? id)
        {
            var entity = await _metadataService.GetEntityWithPropertiesAsync(entityName);
            if (entity == null) return NotFound();

            var userId = GetCurrentUserId();
            var access = await _accessService.GetEntityAccessAsync(userId, entity.Id);
            
            if (id.HasValue && !access.CanRead) return Forbid();
            if (!id.HasValue && !access.CanCreate) return Forbid();


            Dictionary<string, object> recordData;


            if (id.HasValue)
            {
                var recordDataResponse = await _dynamicDataService.GetByIdAsync(entityName, id.Value);
                recordData = recordDataResponse.Data;
            }
            else
            {
                // Pasamos la colección de la Query String (ej: ?ClienteId=GUID) al creador de diccionarios
                var emptyDictResponse = await _dynamicDataService.CreateEmptyDictionaryAsync(entity, Request.Query);
                recordData = emptyDictResponse.Data;
            }



            // var recordDataResponse = id.HasValue 
            //     ? await _dataService.GetByIdAsync(entityName, id.Value) 
            //     : _dataService.CreateEmptyDictionary(entity);

            // var viewModel = new DynamicFormViewModel
            // {
            //     EntityMetadata = entity,
            //     RecordData = recordDataResponse.Data,
            //     AccessLevel = access.AccessLevel
            // };


            var viewModel = new DynamicFormViewModel
            {
                EntityMetadata = entity,
                RecordData = recordData,
                AccessLevel = access.AccessLevel
            };

            return View("DynamicForm", viewModel);


        }

        // Acción de Guardado (Procesa el Formulario y ejecuta Triggers)
        [HttpPost("Save/{recordId:guid?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRecord(string entityName, Guid recordId, [FromForm] IFormCollection form)
        {
            var entity = await _metadataService.GetEntityWithPropertiesAsync(entityName);
            if (entity == null) return NotFound();

            var userId = GetCurrentUserId();
            var access = await _accessService.GetEntityAccessAsync(userId, entity.Id);

            // Validar permiso de escritura
            bool isUpdate = recordId != Guid.Empty;
            if (isUpdate && !access.CanUpdate) return Forbid();
            if (!isUpdate && !access.CanCreate) return Forbid();

            // Llamada al servicio que construye SQL y ejecuta los Triggers Razor
            var response = await _dynamicDataService.SaveAsync(entityName, recordId, form, entity, userId);

            if (response.IsSuccess)
            {

                TempData["Message"] = isUpdate 
                    ? "Registro actualizado con éxito." 
                    : "Nuevo registro creado correctamente.";

                return RedirectToAction(nameof(Index), new { entityName });
            }

            // En caso de error (o error en Trigger Razor), recargar el formulario con el mensaje
            ModelState.AddModelError("", response.Message);
            
            var recordData = form.ToDictionary(k => k.Key, v => (object)v.Value.ToString());
            var viewModel = new DynamicFormViewModel {
                EntityMetadata = entity,
                RecordData = recordData,
                AccessLevel = access.AccessLevel
            };

            return View("DynamicForm", viewModel);
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;
            return Guid.TryParse(claim, out Guid id) ? id : Guid.Empty;
        }


        [HttpGet("Export")]
        public async Task<IActionResult> Export(string entityName)
        {
            var response = await _dynamicDataService.ExportToExcelAsync(entityName);
            
            if (response.IsSuccess)
            {
                string fileName = $"{entityName}_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(response.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

            return RedirectToAction(nameof(Index), new { entityName });
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string entityName, Guid id)
        {
            var entity = await _metadataService.GetEntityByNameAsync(entityName);
            if (entity == null) return NotFound();

            var userId = GetCurrentUserId();
            var access = await _accessService.GetEntityAccessAsync(userId, entity.Id);

            // Validar permiso de borrado
            if (!access.CanDelete) return Forbid();

            var response = await _dynamicDataService.DeleteLogicalAsync(entityName, id, userId);

            if (response.IsSuccess)
            {
                TempData["Message"] = "Registro eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = response.Message;
            }

            return RedirectToAction(nameof(Index), new { entityName });
        }


        // El {entityName} ya viene implícito por la ruta del controlador principal
        [HttpGet("AuditTrail/{id}")] 
        public async Task<IActionResult> GetAuditTrail(string entityName, Guid id)
        {
            // 1. Obtener los logs de la tabla AuditLog
            string sql = "SELECT a.*, u.Name as UserName FROM AuditLog a " +
                        "INNER JOIN [User] u ON a.UserId = u.Id " +
                        "WHERE a.EntityName = @e AND a.RecordId = @r ORDER BY a.ChangeDate DESC";
            
            var response = await _context.GetQueryAsync(sql, 
                new GlobalItem("e", entityName), 
                new GlobalItem("r", id.ToString()));

            // 2. Obtener etiquetas de las propiedades para "traducir" el JSON
            var entity = await _metadataService.GetEntityWithPropertiesAsync(entityName);
            ViewBag.PropertyLabels = entity.Properties.ToDictionary(p => p.Name, p => p.Label);

            return PartialView("_AuditTrailList", response.Data);
        }





    }
}
