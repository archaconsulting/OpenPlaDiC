using System.Text.Json;
using OpenPlaDiC.Core.Models; // Aquí debe residir tu clase genérica Response<T> y GlobalItem
using OpenPlaDiC.Core.DTOs;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.BIZ.Services;

public interface IEntitySchemaService
{
    Task<Response<string>> ImportSchemaAsync(string jsonContent, Guid userId);
}

public class EntitySchemaService(AppDbContext context, IMetadataService metadataService) : IEntitySchemaService
{
    private readonly AppDbContext _context = context;
    private readonly IMetadataService _metadataService = metadataService;

    public async Task<Response<string>> ImportSchemaAsync(string jsonContent, Guid userId)
    {
        try
        {
            // 1. Deserializar el archivo JSON de intercambio
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var schema = JsonSerializer.Deserialize<EntitySchemaDto>(jsonContent, options);
            
            if (schema == null)
            {
                return Response<string>.Fail("El formato del archivo JSON de esquema no es válido o está vacío.");
            }

            // 2. Control de colisiones: Validar que no exista la entidad en la metadata del destino
            var existingEntity = await _metadataService.GetEntityMetadataAsync(schema.Name);
            if (existingEntity != null)
            {
                return Response<string>.Fail($"Error de replicación: La entidad '{schema.Name}' ya se encuentra registrada en este ambiente.");
            }

            // 3. Orquestación Atómica: Llamada al creador de la tabla física del Kernel
            var entityParams = new[] {
                new GlobalItem { Name = "Name", Value = schema.Name },
                new GlobalItem { Name = "Label", Value = schema.Label },
                new GlobalItem { Name = "PageSize", Value = schema.PageSize },
                new GlobalItem { Name = "Prefix", Value = schema.Prefix },
                new GlobalItem { Name = "Icon", Value = schema.Icon },
                new GlobalItem { Name = "CreatedById", Value = userId },
                new GlobalItem { Name = "UseNameField", Value = schema.UseNameField ? 1 : 0 },
                new GlobalItem { Name = "NameLabel", Value = schema.NameLabel }
            };
            
            // Invocamos el SP base de tu Kernel
            await _context.ExecProcAsync("sp_Core_CreateEntity", entityParams);

            // 4. Inyección secuencial de propiedades físicas en SQL Server
            foreach (var prop in schema.Properties)
            {
                var propParams = new[] {
                    new GlobalItem { Name = "EntityName", Value = schema.Name },
                    new GlobalItem { Name = "PropertyName", Value = prop.Name },
                    new GlobalItem { Name = "Label", Value = prop.Label },
                    new GlobalItem { Name = "DataTypeId", Value = prop.DataTypeId },
                    new GlobalItem { Name = "RelatedEntityName", Value = (object?)prop.SourceDefinition ?? DBNull.Value },                    
                    new GlobalItem { Name = "IsRequired", Value = prop.IsRequired ? 1 : 0 },
                    new GlobalItem { Name = "IsUnique", Value = prop.IsUnique ? 1 : 0 },
                    new GlobalItem { Name = "GridRow", Value = prop.GridRow },
                    new GlobalItem { Name = "GridColumn", Value = prop.GridColumn },
                    new GlobalItem { Name = "OnList", Value = prop.OnList ? 1 : 0 },
                    new GlobalItem { Name = "IsIndexed", Value = prop.IsIndexed ? 1 : 0 },
                    new GlobalItem { Name = "CreatedById", Value = userId }
                };

                // Invocamos sp_Core_AddProperty para esculpir la columna física
                
                var respExec =  await _context.ExecProcAsync("sp_Core_AddProperty", propParams);
                if (!respExec.IsSuccess)
                {
                    return Response<string>.Fail($"Hubo un error al crear la propiedad '{schema.Label}' con el siguiente mensaje: {respExec.Message} ");                  

                }
            }

            // Retorno exitoso homologado pasando el nombre de la entidad creada como el tipo <T>
            return Response<string>.Success(
                data: schema.Name, 
                message: $"La estructura de la entidad '{schema.Label}' y sus {schema.Properties.Count} propiedades se importaron y esculpieron con éxito en la base de datos."
            );
        }
        catch (JsonException ex)
        {
            // Captura de errores específicos de sintaxis en el archivo JSON
            return Response<string>.Fail($"Error de sintaxis en el archivo: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Captura de errores físicos de SQL Server (tipos de datos inexistentes, fallos de FK, etc.)
            // Evitamos que truene la app y empaquetamos el log forense
            return Response<string>.Fail($"Fallo crítico durante la mutación física del esquema en SQL Server: {ex.Message}");
        }
    }
}

