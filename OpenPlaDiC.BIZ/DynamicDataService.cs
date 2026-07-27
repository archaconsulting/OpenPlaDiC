using System;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;
using ClosedXML.Excel;
using System.Text.Json;
using System.Text;
using OpenPlaDiC.Core.Models.DynamicQuery;
using OpenPlaDiC.DAL.Extensions;

namespace OpenPlaDiC.BIZ;

// =========================================================================
// Soportes y Estructuras de Datos del Kernel Evolucionados
// =========================================================================
public enum FilterOperator
{
    Equals,
    Contains,
    Between,
    In,
    // --- Nuevos Operadores Avanzados ---
    StartsWith,
    EndsWith,
    NotContains,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Today,
    Yesterday,
    ThisMonth,
    Active,
    Inactive
}

// Usamos C# 10+ Primary Constructors para un record inmutable y limpio
public record FilterCriterion(
    string PropertyName, 
    FilterOperator Operator, 
    object? Value1, 
    object? Value2 = null
);

// Componente estático encargado del parseo atómico a T-SQL parametrizado
public static class DynamicQueryBuilder
{
    public static (string SqlClause, List<GlobalItem> DbParameters) BuildAdvancedWhereClause(
        HashSet<FilterCriterion> criteria, 
        Dictionary<string, int> propertyTypes)
    {
        var sqlBuilder = new StringBuilder();
        var dbParameters = new List<GlobalItem>();
        int paramCounter = 0;

        if (criteria == null || !criteria.Any())
            return (string.Empty, dbParameters);

        foreach (var filter in criteria)
        {
            paramCounter++;
            string p1 = $"@p_adv_{paramCounter}_v1";
            string p2 = $"@p_adv_{paramCounter}_v2";

            string columnName = $"[{filter.PropertyName}]";
            var val1Str = filter.Value1?.ToString() ?? string.Empty;
            var val2Str = filter.Value2?.ToString() ?? string.Empty;

            switch (filter.Operator)
            {
                // === OPERADORES DE STRING ===
                case FilterOperator.Contains:
                    sqlBuilder.Append($" AND {columnName} LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1, $"%{val1Str}%"));
                    break;
                case FilterOperator.StartsWith:
                    sqlBuilder.Append($" AND {columnName} LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1, $"{val1Str}%"));
                    break;
                case FilterOperator.EndsWith:
                    sqlBuilder.Append($" AND {columnName} LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1, $"%{val1Str}"));
                    break;
                case FilterOperator.NotContains:
                    sqlBuilder.Append($" AND {columnName} NOT LIKE {p1}");
                    dbParameters.Add(new GlobalItem(p1, $"%{val1Str}%"));
                    break;

                // === OPERADORES COMUNES (NUMÉRICOS, FECHAS, IGUALDAD) ===
                case FilterOperator.Equals:
                    sqlBuilder.Append($" AND {columnName} = {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;
                case FilterOperator.NotEquals:
                    sqlBuilder.Append($" AND {columnName} <> {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;
                case FilterOperator.GreaterThan:
                    sqlBuilder.Append($" AND {columnName} > {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;
                case FilterOperator.LessThan:
                    sqlBuilder.Append($" AND {columnName} < {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;
                case FilterOperator.GreaterThanOrEqual:
                    sqlBuilder.Append($" AND {columnName} >= {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;
                case FilterOperator.LessThanOrEqual:
                    sqlBuilder.Append($" AND {columnName} <= {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;

                // === OPERADORES DE RANGOS Y FECHAS ESPECIALES ===
                case FilterOperator.Between:
                    sqlBuilder.Append($" AND {columnName} BETWEEN {p1} AND {p2}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    dbParameters.Add(new GlobalItem(p2, val2Str));
                    break;

                case FilterOperator.Today:
                    sqlBuilder.Append($" AND CAST({columnName} AS DATE) = CAST(GETDATE() AS DATE)");
                    break;
                case FilterOperator.Yesterday:
                    sqlBuilder.Append($" AND CAST({columnName} AS DATE) = CAST(DATEADD(day, -1, GETDATE()) AS DATE)");
                    break;
                case FilterOperator.ThisMonth:
                    sqlBuilder.Append($" AND YEAR({columnName}) = YEAR(GETDATE()) AND MONTH({columnName}) = MONTH(GETDATE())");
                    break;

                // === BOOLEANOS ===
                case FilterOperator.Active:
                    sqlBuilder.Append($" AND {columnName} = 1");
                    break;
                case FilterOperator.Inactive:
                    sqlBuilder.Append($" AND ({columnName} = 0 OR {columnName} IS NULL)");
                    break;
                
                case FilterOperator.In:
                    // Fallback de compatibilidad por si se llega a ocupar el operador IN nativo
                    sqlBuilder.Append($" AND {columnName} = {p1}");
                    dbParameters.Add(new GlobalItem(p1, val1Str));
                    break;
            }
        }

        return (sqlBuilder.ToString(), dbParameters);
    }
}

// =========================================================================
// Interfaz de Servicio
// =========================================================================
public interface IDynamicDataService
{
    Task<Response<DataTable>> GetAllAsync(string entityName, bool allFields = true);
    Task<Response<Dictionary<string, object>>> GetByIdAsync(string entityName, Guid id);
    Response<Dictionary<string, object>> CreateEmptyDictionary(Entity entity);
    Task<Response<bool>> SaveAsync(string entityName, Guid id, IFormCollection form, Entity entity, Guid userId);
    Task<Response<byte[]>> ExportToExcelAsync(string entityName);
    Task<Response<bool>> DeleteLogicalAsync(string entityName, Guid id, Guid userId);
    Task<Response<Dictionary<string, object>>> CreateEmptyDictionaryAsync(Entity entity, IQueryCollection query);
    Task<IEnumerable<Dictionary<string, object>>> GetPagedDataAsync(
        string entityName, 
        HashSet<FilterCriterion> criteria,
        int page,
        bool isMaster);
}

// =========================================================================
// Implementación del Servicio de Negocio
// =========================================================================
public class DynamicDataService : IDynamicDataService
{
    private readonly AppDbContext _context;
    private readonly IMetadataService _metadataService;
    private readonly IRazorRenderService _razorService;
    private readonly string _triggerPath;

    public DynamicDataService(AppDbContext context, IWebHostEnvironment env, IRazorRenderService razorService, IMetadataService metadataService)
    {
        _context = context;
        _razorService = razorService;
        _metadataService = metadataService;
        _triggerPath = Path.Combine(env.ContentRootPath, "Views", "Custom", "Triggers");

        if (!Directory.Exists(_triggerPath))
            Directory.CreateDirectory(_triggerPath);
    }

    public async Task<Response<DataTable>> GetAllAsync(string entityName, bool allFields = true)
    {
        if (allFields)
        {
            return await _context.GetQueryAsync($"SELECT * FROM {entityName} e WHERE e.IsDeleted = 0 ORDER BY e.CreatedAt DESC");
        }

        var respE = await _metadataService.GetEntityWithPropertiesAsync(entityName);

        if (respE != null)
        {
            if (string.IsNullOrEmpty(respE.ListQuery))
            {
                string sql = " select e.Id, e.Folio, e.Number " + (respE.UseNameField ? ", e.Name " : "");
                var list = respE.Properties.Where(x => x.OnList).ToList();

                if (list.Count > 0)
                {
                    sql = " select e.Id, e.Folio, e.Number ";
                    foreach (var prop in list)
                    {
                        sql += " ,e." + prop.Name + " [" + prop.Label +"] ";
                    }
                }

                sql += $" FROM {entityName} e WHERE e.IsDeleted = 0 ORDER BY e.CreatedAt DESC";
                return await _context.GetQueryAsync(sql);
            }
            else
            {
                return await _context.GetQueryAsync(respE.ListQuery);
            }
        }
        else
        {
            return await _context.GetQueryAsync($"SELECT * FROM {entityName} e WHERE e.IsDeleted = 0 ORDER BY e.CreatedAt DESC");
        }
    }

    public async Task<Response<Dictionary<string, object>>> GetByIdAsync(string entityName, Guid id)
    {
        var response = new Response<Dictionary<string, object>>();
        try
        {
            var entityMetadata = await _context.Entities
                .Include(e => e.Properties)
                .FirstOrDefaultAsync(e => e.Name == entityName);

            if (entityMetadata == null)
                return new Response<Dictionary<string, object>> { IsSuccess = false, Message = "Metadata no encontrada." };

            string sql = $"SELECT * FROM {entityName} WHERE Id = @p0";
            var dbResponse = await _context.GetQueryAsync(sql, new GlobalItem { Name = "@p0", Value = id.ToString() });

            if (dbResponse.IsSuccess && dbResponse.Data.Rows.Count > 0)
            {
                var dict = new Dictionary<string, object>();
                DataRow row = dbResponse.Data.Rows[0];

                foreach (DataColumn col in dbResponse.Data.Columns)
                {
                    dict[col.ColumnName] = row[col.ColumnName] == DBNull.Value ? null : row[col.ColumnName];
                }

                foreach (var prop in entityMetadata.Properties.Where(p => p.DataTypeId == 10))
                {
                    var relatedId = dict[prop.Name]?.ToString();
                    if (!string.IsNullOrEmpty(relatedId) && Guid.TryParse(relatedId, out Guid gId))
                    {
                        string lookupSql = $"SELECT Name FROM {prop.SourceDefinition} WHERE Id = @p0";
                        var lookupRes = await _context.GetQueryAsync(lookupSql, new GlobalItem { Name = "@p0", Value = relatedId });

                        if (lookupRes.IsSuccess && lookupRes.Data.Rows.Count > 0)
                        {
                            dict[prop.Name + "_Text"] = lookupRes.Data.Rows[0]["Name"].ToString();
                        }
                    }
                }

                response.Data = dict;
                response.IsSuccess = true;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Registro no encontrado.";
                response.Code = 404;
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.IsException = true;
            response.Message = "Error en GetByIdAsync: " + ex.Message;
        }
        return response;
    }

    private Dictionary<string, object> ConvertFormToDictionary(IFormCollection form, Entity entity)
    {
        var dict = new Dictionary<string, object>();

        if (entity.UseNameField && form.ContainsKey("Name"))
            dict["Name"] = form["Name"].ToString();

        foreach (var prop in entity.Properties)
        {


            // Tratamiento de archivos subidos (Tipo de dato Image - 18)
            if (prop.DataTypeId == 18)
            {
                var file = form.Files.GetFile(prop.Name);
                if (file != null && file.Length > 0)
                {
                    // Regla de Negocio / Configuración:
                    // Si prop.SourceDefinition es "STORAGE" o "FILE", guarda el archivo físico en /wwwroot/uploads y almacena el nombre/GUID.
                    // De lo contrario (o si es "BASE64"), convierte la imagen a cadena Base64 Data URI.
                    string storageMode = (prop.SourceDefinition ?? "").Trim().ToUpper();

                    if (storageMode == "STORAGE" || storageMode == "FILE")
                    {
                        string extension = Path.GetExtension(file.FileName);
                        string fileName = $"{Guid.NewGuid()}{extension}";
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        dict[prop.Name] = fileName; // Guarda la referencia/GUID en la BD
                    }
                    else // Modo por defecto: BASE64
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            byte[] fileBytes = ms.ToArray();
                            string base64String = Convert.ToBase64String(fileBytes);
                            string contentType = file.ContentType;
                            dict[prop.Name] = $"data:{contentType};base64,{base64String}";
                        }
                    }
                }
                else if (form.ContainsKey($"{prop.Name}_KeepCurrent") && form[$"{prop.Name}_KeepCurrent"] == "true")
                {
                    // Mantiene el valor actual previamente almacenado si no se sube un archivo nuevo
                    if (form.ContainsKey($"{prop.Name}_Existing"))
                        dict[prop.Name] = form[$"{prop.Name}_Existing"].ToString();
                }
                else
                {
                    dict[prop.Name] = DBNull.Value;
                }

                continue;
            }


            if (!form.ContainsKey(prop.Name))
            {
                if (prop.DataTypeId == 11) 
                {
                    dict[prop.Name] = false;
                }
                continue; 
            }

            string rawValue = form[prop.Name].ToString();

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                dict[prop.Name] = DBNull.Value;
                continue;
            }

            switch (prop.DataTypeId)
            {
                case 11: 
                    dict[prop.Name] = rawValue == "true" || rawValue == "on" ? 1 : 0;
                    break;
                    
                case 4: 
                    if (DateTime.TryParse(rawValue, out DateTime dt))
                        dict[prop.Name] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        dict[prop.Name] = DBNull.Value;
                    break;

                case 10: 
                    if (form.ContainsKey(prop.Name) && !string.IsNullOrWhiteSpace(form[prop.Name].ToString()))
                    {
                        if (Guid.TryParse(form[prop.Name].ToString(), out Guid gId))
                            dict[prop.Name] = gId.ToString();
                        else
                            dict[prop.Name] = DBNull.Value;
                    }
                    else
                    {
                        dict[prop.Name] = DBNull.Value; 
                    }
                    break;

                case 19: // Url (NVARCHAR)
                    dict[prop.Name] = rawValue.Trim();
                    break;

                default: 
                    dict[prop.Name] = rawValue.Trim();
                    break;
            }
        }            

        return dict;
    }

    public async Task<Response<bool>> SaveAsync(string entityName, Guid id, IFormCollection form, Entity entity, Guid userId)
    {
        var response = new Response<bool>();
        try
        {
            bool isUpdate = id != Guid.Empty;
            var dataToSave = ConvertFormToDictionary(form, entity);
            var newData = ConvertFormToDictionary(form, entity);
            var oldData = new Dictionary<string, object>();

            if (isUpdate)
            {
                var currentRecordRes = await GetByIdAsync(entityName, id);
                if (currentRecordRes.IsSuccess) oldData = currentRecordRes.Data;
            }

            string triggerCode = isUpdate ? entity.OnBeforeUpdate : entity.OnBeforeInsert;
            if (!string.IsNullOrWhiteSpace(triggerCode))
            {
                string triggerType = isUpdate ? "BeforeUpdate" : "BeforeInsert";
                await SyncTriggerFile(entity.Name, triggerType, triggerCode);
                string viewPath = $"~/Views/Custom/Triggers/_{entity.Name}_{triggerType}.cshtml";
                await _razorService.RenderToStringAsync(viewPath, dataToSave);
            }

            string sql = "";
            var parameters = new List<GlobalItem>();
            var fieldsToSave = entity.Properties.Where(p => p.IsEditable).Select(p => p.Name).ToList();

            if (isUpdate)
            {
                var setClauses = new List<string>();
                foreach (var field in fieldsToSave)
                {
                    if (form.ContainsKey(field))
                    {
                        setClauses.Add($"{field} = @{field}");
                        parameters.Add(new GlobalItem(field, form[field].ToString()));
                    }
                    else
                    {
                        setClauses.Add($"{field} = @{field}");
                        parameters.Add(new GlobalItem(field, newData[field].ToString()));
                        
                    }
                }
                setClauses.Add("UpdatedAt = GETDATE()");
                sql = $"UPDATE {entityName} SET {string.Join(", ", setClauses)} WHERE Id = @Id";
                parameters.Add(new GlobalItem("Id", id.ToString()));
            }
            else
            {
                id = Guid.NewGuid();
                var columns = new List<string> { "Id", "CreatedById" };
                var values = new List<string> { "@Id", "@CreatedById" };
                parameters.Add(new GlobalItem("Id", id.ToString()));
                parameters.Add(new GlobalItem("CreatedById", userId.ToString()));

                foreach (var field in fieldsToSave)
                {
                    if (form.ContainsKey(field))
                    {
                        columns.Add(field);
                        values.Add($"@{field}");
                        parameters.Add(new GlobalItem(field, form[field].ToString()));
                    }
                    else
                    {
                        columns.Add(field);
                        values.Add($"@{field}");
                        parameters.Add(new GlobalItem(field, newData[field].ToString()));                        

                    }
                }
                sql = $"INSERT INTO {entityName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            }

            var execResponse = await _context.ExecQueryAsync(sql, parameters.ToArray());

            if (execResponse.IsSuccess)
            {
                string afterCode = isUpdate ? entity.OnAfterUpdate : entity.OnAfterInsert;
                if (!string.IsNullOrWhiteSpace(afterCode))
                {
                    string triggerType = isUpdate ? "AfterUpdate" : "AfterInsert";
                    await SyncTriggerFile(entity.Name, triggerType, afterCode);
                    string viewPath = $"~/Views/Custom/Triggers/_{entity.Name}_{triggerType}.cshtml";
                    await _razorService.RenderToStringAsync(viewPath, dataToSave);
                }

                await WriteAuditLog(entityName, id, isUpdate ? "UPDATE" : "INSERT", oldData, newData, userId);
            
                response.IsSuccess = true;
                response.Data = true;
                response.Code = 200;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = execResponse.Message;
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = "Error en el Kernel o Trigger: " + ex.Message;
            response.Code = 500;
        }
        return response;
    }

    private async Task WriteAuditLog(string entity, Guid recordId, string action, Dictionary<string, object> oldVal, Dictionary<string, object> newVal, Guid userId)
    {
        var changesOld = new Dictionary<string, object>();
        var changesNew = new Dictionary<string, object>();

        foreach (var key in newVal.Keys)
        {
            var oldV = oldVal.ContainsKey(key) ? oldVal[key]?.ToString() : null;
            var newV = newVal[key]?.ToString();

            if (oldV != newV)
            {
                changesOld[key] = oldV;
                changesNew[key] = newV;
            }
        }

        if (changesNew.Count > 0 || action == "INSERT")
        {
            string sql = "INSERT INTO AuditLog (EntityName, RecordId, Action, OldValues, NewValues, UserId) VALUES (@e, @r, @a, @o, @n, @u)";
            await _context.ExecQueryAsync(sql, 
                new GlobalItem("e", entity), 
                new GlobalItem("r", recordId.ToString()),
                new GlobalItem("a", action),
                new GlobalItem("o", JsonSerializer.Serialize(changesOld)),
                new GlobalItem("n", JsonSerializer.Serialize(changesNew)),
                new GlobalItem("u", userId.ToString()));
        }
    }

    private async Task SyncTriggerFile(string entityName, string triggerType, string content)
    {
        if (string.IsNullOrEmpty(content)) return;

        string fileName = $"_{entityName}_{triggerType}.cshtml";
        string filePath = Path.Combine(_triggerPath, fileName);

        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<Response<byte[]>> ExportToExcelAsync(string entityName)
    {
        var response = new Response<byte[]>();
        try
        {
            var dataResponse = await GetAllAsync(entityName);
            if (!dataResponse.IsSuccess) return new Response<byte[]> { IsSuccess = false, Message = "Error al obtener datos" };

            var table = dataResponse.Data;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(entityName);
                worksheet.Cell(1, 1).InsertTable(table);
                worksheet.Columns().AdjustToContents();
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    response.Data = stream.ToArray();
                    response.IsSuccess = true;
                }
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = "Error en Excel: " + ex.Message;
        }
        return response;
    }

    public async Task<Response<bool>> DeleteLogicalAsync(string entityName, Guid id, Guid userId)
    {
        var response = new Response<bool>();
        try
        {
            string sql = $"UPDATE {entityName} SET IsDeleted = 1, UpdatedAt = GETDATE(), UpdatedById = @p1 WHERE Id = @p0";
            
            var execResponse = await _context.ExecQueryAsync(sql, 
                new GlobalItem { Name = "@p0", Value = id.ToString() },
                new GlobalItem { Name = "@p1", Value = userId.ToString() }
            );

            if (execResponse.IsSuccess)
            {
                response.IsSuccess = true;
                response.Data = true;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = execResponse.Message;
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = "Error al intentar eliminar: " + ex.Message;
        }
        return response;
    }

    public Response<Dictionary<string, object>> CreateEmptyDictionary(Entity entity)
    {
        var dict = new Dictionary<string, object>
        {
            ["Id"] = Guid.Empty,
            ["Folio"] = "NUEVO",
            ["CreatedAt"] = DateTime.Now
        };

        if (entity.UseNameField)
        {
            dict["Name"] = string.Empty;
        }

        foreach (var prop in entity.Properties)
        {
            dict[prop.Name] = null;
            if (prop.DataTypeId == 10)
            {
                dict[prop.Name + "_Text"] = string.Empty;
            }
        }

        return new Response<Dictionary<string, object>> { IsSuccess = true, Data = dict };
    }

    public async Task<Response<Dictionary<string, object>>> CreateEmptyDictionaryAsync(Entity entity, IQueryCollection query)
    {
        var response = new Response<Dictionary<string, object>>();
        try
        {
            var dict = new Dictionary<string, object>
            {
                ["Id"] = Guid.Empty,
                ["Folio"] = "NUEVO",
                ["CreatedAt"] = DateTime.Now
            };

            if (entity.UseNameField) dict["Name"] = string.Empty;

            foreach (var prop in entity.Properties)
            {
                dict[prop.Name] = null;
                if (prop.DataTypeId == 10) dict[prop.Name + "_Text"] = string.Empty;

                if(prop.DataTypeId == 3) dict[prop.Name] = DateTime.Now.ToString("yyyy-MM-dd");

            }

            foreach (var param in query)
            {
                var matchingProp = entity.Properties.FirstOrDefault(p => p.Name.Equals(param.Key, StringComparison.OrdinalIgnoreCase));
                
                if (matchingProp != null && Guid.TryParse(param.Value, out Guid parentGuid))
                {
                    dict[matchingProp.Name] = parentGuid;

                    if (matchingProp.DataTypeId == 10)
                    {
                        string lookupSql = $"SELECT Name FROM {matchingProp.SourceDefinition} WHERE Id = @p0";
                        var lookupRes = await _context.GetQueryAsync(lookupSql, new GlobalItem { Name = "@p0", Value = parentGuid.ToString() });

                        if (lookupRes.IsSuccess && lookupRes.Data.Rows.Count > 0)
                        {
                            dict[matchingProp.Name + "_Text"] = lookupRes.Data.Rows[0]["Name"].ToString();
                        }
                    }
                }
            }

            response.Data = dict;
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = "Error al inicializar contexto de registro: " + ex.Message;
        }
        return response;
    }

    // =========================================================================
    // 🚀 MÉTODO PRINCIPAL DE ALTO RENDIMIENTO REESTRUCTURADO Y BLINDADO
    // =========================================================================
    public async Task<IEnumerable<Dictionary<string, object>>> GetPagedDataAsync(
        string entityName, 
        HashSet<FilterCriterion> criteria,
        int page,
        bool isMaster) 
    {
        // 1. Obtener la metadata completa de la entidad
        var entityMetadata = await _metadataService.GetEntityWithPropertiesAsync(entityName);
        if (entityMetadata == null) 
            throw new ArgumentException($"La entidad '{entityName}' no existe.");

        int configuredPageSize = entityMetadata.PageSize; 
        
        Dictionary<string, int> propertyTypes = ((IEnumerable<EntityProperty>)entityMetadata.Properties)
            .ToDictionary(p => p.Name, p => p.DataTypeId);

        // 2. Extraer las cláusulas de filtros avanzados usando el nuevo motor de parseo atómico
        var (sqlFilters, dbParameters) = DynamicQueryBuilder.BuildAdvancedWhereClause(criteria, propertyTypes);
        
        // 3. Recuperación segura de la consulta optimizada (ListQuery precalculado)
        string baseQuery = "";
        if (!string.IsNullOrEmpty(entityMetadata.ListQuery))
        {
            baseQuery = entityMetadata.ListQuery;
        }
        else
        {
            baseQuery = _metadataService.BuildDynamicListQuery(entityMetadata);
        }

        // 4. Envolver el query base en una Expresión de Tabla Común (CTE)
        string finalSql = $"WITH MainResult AS ({baseQuery}) SELECT * FROM MainResult";
        
        // 5. Concatenar de forma segura los filtros avanzados sin duplicar cláusulas WHERE
        if (!string.IsNullOrWhiteSpace(sqlFilters))
        {
            string filtrosLimpios = sqlFilters.Trim();
            
            if (filtrosLimpios.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                finalSql += " " + sqlFilters;
            }
            else if (filtrosLimpios.StartsWith("AND", StringComparison.OrdinalIgnoreCase))
            {
                finalSql += " WHERE 1=1 " + sqlFilters;
            }
            else
            {
                finalSql += " WHERE " + sqlFilters;
            }
        }
        else
        {
            finalSql += " WHERE 1=1";
        }

        // 6. Configurar el ordenamiento estructural requerido por SQL Server para paginar
        finalSql += " ORDER BY [Id] DESC";

        if (configuredPageSize > 0)
        {
            int rowsToSkip = (page - 1) * configuredPageSize;
            finalSql += $" OFFSET {rowsToSkip} ROWS FETCH NEXT {configuredPageSize} ROWS ONLY;";
        }

        // 7. Conversión nativa de la lista de parámetros al arreglo plano esperado por AppDbContext
        var parametersArray = dbParameters.ToArray();

        // 8. Ejecución asíncrona de la consulta unificada en la base de datos
        var response = await _context.GetQueryAsync(finalSql, parametersArray);

        if (response?.Data == null)
            return Enumerable.Empty<Dictionary<string, object>>();

        // 9. Retorno compatible en colecciones iterables de diccionarios planos
        return ConvertDataTableToDictionaries(response.Data);
    }

    private static IEnumerable<Dictionary<string, object>> ConvertDataTableToDictionaries(DataTable table)
    {
        var rows = new List<Dictionary<string, object>>();
        
        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                dict[col.ColumnName] = row[col] == DBNull.Value ? null! : row[col];
            }
            rows.Add(dict);
        }
        
        return rows;
    }
}