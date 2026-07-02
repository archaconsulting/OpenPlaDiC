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
using OpenPlaDiC.Core.Models.DynamicQuery;
using OpenPlaDiC.DAL.Extensions;


namespace OpenPlaDiC.BIZ;

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


public class DynamicDataService : IDynamicDataService
{
    private readonly AppDbContext _context;
    private readonly IMetadataService _metadataService;

    private readonly IRazorRenderService _razorService; // Inyectar esto
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
        if(allFields)
        {
            
            return await _context.GetQueryAsync($"SELECT * FROM {entityName} e WHERE e.IsDeleted = 0 ORDER BY e.CreatedAt DESC");

        }



        var respE = await _metadataService.GetEntityWithPropertiesAsync(entityName);

        if (respE != null)
        {

            if( string.IsNullOrEmpty( respE.ListQuery))
            {
            
                string sql = " select e.Id, e.Folio, e.Number " + (respE.UseNameField ? ", e.Name " : "")  ;

                var list = respE.Properties.Where(x => x.OnList).ToList();

                if(list.Count > 0)
                {
                    sql = " select e.Id, e.Folio, e.Number ";

                    foreach(var prop in list)
                    {
                        
                        sql += " ,e."+prop.Name +" ["+prop.Label+"] ";

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

        // Reutilizamos la respuesta estandarizada que ya devuelve el DbContext
    }

    public async Task<Response<Dictionary<string, object>>> GetById2Async(string entityName, Guid id)
    {
        var response = new Response<Dictionary<string, object>>();
        try
        {
            var dbResponse = await _context.GetQueryAsync($"SELECT * FROM {entityName} WHERE Id = @p0", 
                new GlobalItem { Name = "@p0", Value = id.ToString() });

            if (dbResponse.IsSuccess && dbResponse.Data.Rows.Count > 0)
            {
                var dict = new Dictionary<string, object>();
                DataRow row = dbResponse.Data.Rows[0];
                foreach (DataColumn col in dbResponse.Data.Columns)
                {
                    dict[col.ColumnName] = row[col.ColumnName] == DBNull.Value ? null : row[col.ColumnName];
                }
                response.Data = dict;
                response.IsSuccess = true;
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Record not found.";
                response.Code = 404;
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.IsException = true;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<Dictionary<string, object>>> GetByIdAsync(string entityName, Guid id)
    {
        var response = new Response<Dictionary<string, object>>();
        try
        {
            // 1. Obtener la metadata de la entidad para saber qué campos son relaciones
            var entityMetadata = await _context.Entities
                .Include(e => e.Properties)
                .FirstOrDefaultAsync(e => e.Name == entityName);

            if (entityMetadata == null)
                return new Response<Dictionary<string, object>> { IsSuccess = false, Message = "Metadata no encontrada." };

            // 2. Obtener el registro principal de la tabla física
            string sql = $"SELECT * FROM {entityName} WHERE Id = @p0";
            var dbResponse = await _context.GetQueryAsync(sql, new GlobalItem { Name = "@p0", Value = id.ToString() });

            if (dbResponse.IsSuccess && dbResponse.Data.Rows.Count > 0)
            {
                var dict = new Dictionary<string, object>();
                DataRow row = dbResponse.Data.Rows[0];

                // 3. Mapear columnas físicas al diccionario
                foreach (DataColumn col in dbResponse.Data.Columns)
                {
                    dict[col.ColumnName] = row[col.ColumnName] == DBNull.Value ? null : row[col.ColumnName];
                }

                // 4. Lógica de Lookup: Buscar textos para campos de relación (DataTypeId = 10)
                foreach (var prop in entityMetadata.Properties.Where(p => p.DataTypeId == 10))
                {
                    var relatedId = dict[prop.Name]?.ToString();
                    if (!string.IsNullOrEmpty(relatedId) && Guid.TryParse(relatedId, out Guid gId))
                    {
                        // Buscamos el nombre legible en la tabla origen (SourceDefinition)
                        string lookupSql = $"SELECT Name FROM {prop.SourceDefinition} WHERE Id = @p0";
                        var lookupRes = await _context.GetQueryAsync(lookupSql, new GlobalItem { Name = "@p0", Value = relatedId });

                        if (lookupRes.IsSuccess && lookupRes.Data.Rows.Count > 0)
                        {
                            // Guardamos el texto en una llave especial que el DynamicForm buscará
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


        // Recorrer las propiedades y validar tipos básicos antes de compilar el SQL
        foreach (var prop in entity.Properties)
        {
            
            //// Implementación previa
            //if (!form.ContainsKey(prop.Name)) continue;

            // ⚡ MODIFICACIÓN CRÍTICA PARA BOOLEANOS APAGADOS:
            // Si el campo NO viene en el formulario, pero en la metadata sabemos que es Booleano (DataTypeId == 3)
            // entonces forzamos su inserción en el diccionario como 0 (false).
            if (!form.ContainsKey(prop.Name))
            {
                if (prop.DataTypeId == 11) // Supongamos que 3 es Boolean/Switch en tu Kernel
                {
                    dict[prop.Name] = false; // Forzamos el false en SQL Server
                }
                continue; // Para cualquier otro tipo de campo, sí continuamos el ciclo normalmente
            }


            string rawValue = form[prop.Name].ToString();

            // Si el campo está vacío y no es requerido, lo mandamos como NULL
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                dict[prop.Name] = DBNull.Value;
                continue;
            }

            // Conversión según DataTypeId (Enum PropertyDataType)
            switch (prop.DataTypeId)
            {
                case 3: // Supongamos 3 = Boolean/Switch
                    dict[prop.Name] = rawValue == "true" || rawValue == "on" ? 1 : 0;
                    break;
                    
                case 4: // Supongamos 4 = DateTime
                    if (DateTime.TryParse(rawValue, out DateTime dt))
                        dict[prop.Name] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        dict[prop.Name] = DBNull.Value;
                    break;


                case 10: // RelatedEntity
                    if (form.ContainsKey(prop.Name) && !string.IsNullOrWhiteSpace(form[prop.Name].ToString()))
                    {
                        if (Guid.TryParse(form[prop.Name].ToString(), out Guid gId))
                            dict[prop.Name] = gId.ToString();
                        else
                            dict[prop.Name] = DBNull.Value;
                    }
                    else
                    {
                        dict[prop.Name] = DBNull.Value; // Si el usuario limpia el campo, se guarda NULL en SQL
                    }
                    break;

                default: // Textos o Strings estándar
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
                // 1. Obtener los valores actuales de la base de datos antes de actualizar
                var currentRecordRes = await GetByIdAsync(entityName, id);
                if (currentRecordRes.IsSuccess) oldData = currentRecordRes.Data;
            }

            // 1. --- TRIGGER BEFORE ---
            string triggerCode = isUpdate ? entity.OnBeforeUpdate : entity.OnBeforeInsert;
            if (!string.IsNullOrWhiteSpace(triggerCode))
            {
                string triggerType = isUpdate ? "BeforeUpdate" : "BeforeInsert";
                await SyncTriggerFile(entity.Name, triggerType, triggerCode);
                string viewPath = $"~/Views/Custom/Triggers/_{entity.Name}_{triggerType}.cshtml";
                // Si el código Razor lanza una excepción, se detiene el flujo y va al catch
                await _razorService.RenderToStringAsync(viewPath, dataToSave);
            }

            // 2. --- CONSTRUCCIÓN DINÁMICA DE SQL ---
            string sql = "";
            var parameters = new List<GlobalItem>();
            var fieldsToSave = entity.Properties.Where(p => p.IsEditable).Select(p => p.Name).ToList();
            //if (entity.UseNameField) fieldsToSave.Add("Name");

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
                }
                sql = $"INSERT INTO {entityName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
            }

            // 3. --- EJECUCIÓN EN BASE DE DATOS ---
            var execResponse = await _context.ExecQueryAsync(sql, parameters.ToArray());

            if (execResponse.IsSuccess)
            {
                // 4. --- TRIGGER AFTER ---
                string afterCode = isUpdate ? entity.OnAfterUpdate : entity.OnAfterInsert;
                if (!string.IsNullOrWhiteSpace(afterCode))
                {
                    string triggerType = isUpdate ? "AfterUpdate" : "AfterInsert";
                    await SyncTriggerFile(entity.Name, triggerType, afterCode);
                    string viewPath = $"~/Views/Custom/Triggers/_{entity.Name}_{triggerType}.cshtml";
                    await _razorService.RenderToStringAsync(viewPath, dataToSave);
                }

                // 2. Registrar Auditoría
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
        // Solo guardamos lo que realmente cambió para ahorrar espacio
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

        // Nombre de archivo: _Product_BeforeInsert.cshtml
        string fileName = $"_{entityName}_{triggerType}.cshtml";
        string filePath = Path.Combine(_triggerPath, fileName);

        // Solo escribimos si el contenido cambió o el archivo no existe (Caché simple)
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<Response<byte[]>> ExportToExcelAsync(string entityName)
    {
        var response = new Response<byte[]>();
        try
        {
            // 1. Obtener los datos actuales
            var dataResponse = await GetAllAsync(entityName);
            if (!dataResponse.IsSuccess) return new Response<byte[]> { IsSuccess = false, Message = "Error al obtener datos" };

            var table = dataResponse.Data;

            // 2. Crear el libro de Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(entityName);
                
                // Insertar la tabla (incluye encabezados automáticamente)
                worksheet.Cell(1, 1).InsertTable(table);
                
                // Estética básica: Ajustar columnas y poner encabezado en negrita
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
            // El borrado lógico solo cambia el flag y registra quién y cuándo
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
            // Campos estructurales requeridos por el Kernel
            ["Id"] = Guid.Empty,
            ["Folio"] = "NUEVO",
            ["CreatedAt"] = DateTime.Now
        };

        // Agregar el campo Name si la entidad lo utiliza
        if (entity.UseNameField)
        {
            dict["Name"] = string.Empty;
        }

        // Inicializar cada propiedad configurada en nulo para el formulario
        foreach (var prop in entity.Properties)
        {
            dict[prop.Name] = null;
            
            // Si es una relación, inicializamos también su campo de texto descriptivo
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

            // 1. Inicializar todas las propiedades configuradas de la entidad en nulo
            foreach (var prop in entity.Properties)
            {
                dict[prop.Name] = null;
                if (prop.DataTypeId == 10) dict[prop.Name + "_Text"] = string.Empty;
            }

            // 2. Evaluar si la Query String contiene parámetros que coincidan con las propiedades de la entidad
            foreach (var param in query)
            {
                // Buscamos si la tabla tiene una propiedad con el nombre que viene en la URL
                var matchingProp = entity.Properties.FirstOrDefault(p => p.Name.Equals(param.Key, StringComparison.OrdinalIgnoreCase));
                
                if (matchingProp != null && Guid.TryParse(param.Value, out Guid parentGuid))
                {
                    // Asignamos el ID del padre al campo de la tabla física
                    dict[matchingProp.Name] = parentGuid;

                    // 3. Inteligencia de Contexto: Si es una relación (Tipo 10), traemos su texto amigable de inmediato
                    if (matchingProp.DataTypeId == 10)
                    {
                        string lookupSql = $"SELECT Name FROM {matchingProp.SourceDefinition} WHERE Id = @p0";
                        var lookupRes = await _context.GetQueryAsync(lookupSql, new Framework.GlobalItem { Name = "@p0", Value = parentGuid.ToString() });

                        if (lookupRes.IsSuccess && lookupRes.Data.Rows.Count > 0)
                        {
                            // Llenamos el valor descriptivo para el input de la interfaz
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


    public async Task<IEnumerable<Dictionary<string, object>>> GetPagedDataAsync0(
        string entityName, 
        HashSet<FilterCriterion> criteria, bool includeDeleted = false)
    {
        // 1. Recuperamos la metadata de la entidad
        var entityMetadata = await _metadataService.GetEntityMetadataAsync(entityName);
        if (entityMetadata == null) 
            throw new ArgumentException($"La entidad '{entityName}' no existe en la metadata del Kernel.");

        
        Dictionary<string, int> propertyTypes = ((IEnumerable<EntityProperty>)entityMetadata.Properties)
        .ToDictionary(p => p.Name, p => p.DataTypeId);            

        // 2. Construimos la cláusula WHERE usando nuestro Builder
        var (sqlWhere, dynamicParams) = DynamicQueryBuilder.BuildWhereClause(criteria, propertyTypes);

        if(string.IsNullOrEmpty(sqlWhere) && !includeDeleted)
        {
            sqlWhere = " WHERE IsDeleted = 0 ";
        }

        // 3. Sanitizamos estrictamente el nombre de la tabla
        string safeTableName = $"[{entityName.Replace("]", "]]")}]";
        string finalSql = $"SELECT * FROM {safeTableName} {sqlWhere} ORDER BY CreatedAt DESC";

        // 4. Mapeo de Dapper (DynamicParameters) al formato nativo del Kernel (GlobalItem[])
        // Asumiendo que GlobalItem es un key-value pair estructurado de tu Kernel (ej: Name/Value o Key/Value)
        var globalParams = dynamicParams.ParameterNames
            .Select(pName => new GlobalItem 
            { 
                Name = pName, // O la propiedad correspondiente en tu estructura GlobalItem
                Value = dynamicParams.Get<object>(pName).ToString() ?? "" //DBNull.Value 
            })
            .ToArray();

        // 5. Consumimos el método oficial del Kernel sobre tu AppDbContext
        var response = await _context.GetQueryAsync(finalSql, globalParams);

        // Verificamos el estado de la respuesta del Kernel (suponiendo que sigue el patrón Result/Response estándar)
        if (response == null || response.Data == null)
            return Enumerable.Empty<Dictionary<string, object>>();

        // 6. Traducimos el DataTable devuelto a la colección de diccionarios que espera la UI asimétrica
        return ConvertDataTableToDictionaries(response.Data);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> GetPagedDataAsync(
        string entityName, 
        HashSet<FilterCriterion> criteria,
        int page,
        bool isMaster) // 🛡️ Inyectamos el flag de Súper Usuario desde el controlador
    {
        var entityMetadata = await _metadataService.GetEntityMetadataAsync(entityName);
        if (entityMetadata == null) 
            throw new ArgumentException($"La entidad '{entityName}' no existe.");

        int configuredPageSize = entityMetadata.PageSize; 
        
        Dictionary<string, int> propertyTypes = ((IEnumerable<EntityProperty>)entityMetadata.Properties)
            .ToDictionary(p => p.Name, p => p.DataTypeId);

        // 1. Obtenemos la cláusula de los filtros del usuario. 
        // NOTA: Debes remover el 'WHERE IsDeleted = 0' de adentro del DynamicQueryBuilder para que solo arme los ANDs de los inputs
        var (sqlFilters, globalParams) = DynamicQueryBuilder.BuildWhereClause(criteria, propertyTypes);
        
        string safeTableName = $"[{entityName.Replace("]", "]]")}]";

        // 2. Establecemos la Cláusula Base de la Plataforma
        // Si es IsMaster, el WHERE base es '1=1' (trae todo). Si no, forzamos 'IsDeleted = 0'
        string baseWhere = isMaster ? " WHERE 1=1 " : " WHERE IsDeleted = 0 ";

        // 3. Concatenamos de forma segura la base con los filtros avanzados si existen
        if (!string.IsNullOrWhiteSpace(sqlFilters))
        {
            // Como sqlFilters ya viene sanitizado con sus "AND ...", simplemente lo adjuntamos
            baseWhere += sqlFilters;
        }

        // Construimos el query unificado final
        string finalSql = $"SELECT * FROM {safeTableName} {baseWhere} ORDER BY CreatedAt DESC";

        if (configuredPageSize > 0)
        {
            int rowsToSkip = (page - 1) * configuredPageSize;
            finalSql += $" OFFSET {rowsToSkip} ROWS FETCH NEXT {configuredPageSize} ROWS ONLY;";
        }

        var parameters = globalParams.ParameterNames
            .Select(pName => new GlobalItem 
            { 
                Name = pName, 
                Value = globalParams.Get<object>(pName).ToString() ?? "" 
            })
            .ToArray();

        var response = await _context.GetQueryAsync(finalSql, parameters);

        if (response?.Data == null)
            return Enumerable.Empty<Dictionary<string, object>>();

        return ConvertDataTableToDictionaries(response.Data);
    }

    /// <summary>
    /// Helper privado para transformar el DataTable a Diccionarios respetando el blindaje contra nulos
    /// </summary>
    private static IEnumerable<Dictionary<string, object>> ConvertDataTableToDictionaries(DataTable table)
    {
        var rows = new List<Dictionary<string, object>>();
        
        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                // Mantenemos el blindaje estricto contra DBNull exigido en tus reglas estabilizadas
                dict[col.ColumnName] = row[col] == DBNull.Value ? null! : row[col];
            }
            rows.Add(dict);
        }
        
        return rows;
    }

}

