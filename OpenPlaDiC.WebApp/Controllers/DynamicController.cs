using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using System.Security.Claims;
using System.Data;
using OpenPlaDiC.WebApp.Models;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using OpenPlaDiC.Core.Models.DynamicQuery;
using FilterCriterion = OpenPlaDiC.BIZ.FilterCriterion;


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
            [FromQuery] int page = 1) // Removimos el Dictionary plano de los parámetros de firma
        {
            if (page < 1) page = 1;

            var user = HttpContext?.User;
            bool isMaster = user?.HasClaim("IsMaster", "True") ?? false;

            var entityMetadata = await _metadataService.GetEntityMetadataAsync(entityName);
            if (entityMetadata == null) return NotFound();

            var userId = GetCurrentUserId();
            var access = await _accessService.GetEntityAccessAsync(userId, entityMetadata.Id);
            if (!access.CanRead) return Forbid();

            // 🚀 NUEVA ESTRUCTURA: Lista de GlobalItem para transportar los operadores avanzados
            var advancedFilters = new List<GlobalItem>();

            // Leemos la Query String de la petición actual
            var queryDict = HttpContext.Request.Query;

            // Procesamos cada propiedad que tenga permitido filtrar (IsFilterfdz == true)
            foreach (EntityProperty prop in entityMetadata.Properties.Where(p => p.IsFilter))
            {
                // Esperamos en la URL estructuras como: ?prop_Op=Contains&prop_Val=Juan
                string opKey = $"{prop.Name}_Op";
                string valKey = $"{prop.Name}_Val";
                string textKey = $"{prop.Name}_Text"; // Para el segundo valor de rangos (Between)

                if (queryDict.TryGetValue(opKey, out var opValue) && !string.IsNullOrWhiteSpace(opValue))
                {
                    queryDict.TryGetValue(valKey, out var val1);
                    queryDict.TryGetValue(textKey, out var val2);

                    // Solo agregamos el filtro si el operador es especial (ej: "Hoy", "Ayer") o si tiene un valor capturado
                    if (!string.IsNullOrWhiteSpace(val1) || opValue == "Today" || opValue == "Yesterday" || opValue == "ThisMonth")
                    {
                        advancedFilters.Add(new GlobalItem
                        {
                            Name = prop.Name,          // Campo de la Base de Datos
                            Opt = opValue.ToString(), // Operador (e.g., "StartsWith", "GreaterThan", "Between")
                            Value = val1.ToString(),   // Valor principal o Fecha Inicial
                            Text = val2.ToString()     // Valor secundario o Fecha Final (si aplica)
                        });
                    }
                }
            }


            // Mapeo dinámico avanzado de las estructuras HTTP a los criterios del Kernel
            HashSet<FilterCriterion> filterCriteria = MapGlobalItemsToCriteria(advancedFilters);

            var records = await _dynamicDataService.GetPagedDataAsync(entityName, filterCriteria, page, isMaster);

            // Mantenemos el diccionario en el ViewModel solo para repoblar los inputs de la UI en la Vista
            var currentFiltersPlain = queryDict.ToDictionary(k => k.Key, v => v.Value.ToString());

            var viewModel = new DynamicIndexViewModel {
                EntityMetadata = entityMetadata,
                Data = records,
                CurrentFilters = currentFiltersPlain, 
                CurrentPage = page 
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


        private HashSet<FilterCriterion> MapGlobalItemsToCriteria(List<GlobalItem> advancedFilters)
        {
            var criteriaSet = new HashSet<FilterCriterion>();
            if (advancedFilters == null || !advancedFilters.Any()) return criteriaSet;

            // Agrupamos por el nombre del campo real (asumiendo formato "Campo_Propiedad")
            // O si mandas objetos serializados, adaptas este agrupador.
            var groupedByField = advancedFilters
                .Where(x => x.Name.Contains("_"))
                .GroupBy(x => x.Name.Split('_')[0]);

            foreach (var group in groupedByField)
            {
                string fieldName = group.Key;
                
                string opStr = group.FirstOrDefault(x => x.Name.EndsWith("_operator"))?.Value?.ToString();
                var val1 = group.FirstOrDefault(x => x.Name.EndsWith("_value1"))?.Value;
                var val2 = group.FirstOrDefault(x => x.Name.EndsWith("_value2"))?.Value;

                // Intentar parsear el operador string al Enum correspondiente
                if (Enum.TryParse<BIZ.FilterOperator>(opStr, true, out var parsedOperator))
                {
                    criteriaSet.Add(new FilterCriterion(fieldName, parsedOperator, val1, val2));
                }
                else
                {
                    // Fallback por defecto si no se reconoce el operador
                    criteriaSet.Add(new FilterCriterion(fieldName, BIZ.FilterOperator.Contains, val1));
                }
            }

            return criteriaSet;
        }


    }
}
