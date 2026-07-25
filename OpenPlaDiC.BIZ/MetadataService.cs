using System;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;

namespace OpenPlaDiC.BIZ;

public interface IMetadataService
{
    Task<Entity> GetEntityByNameAsync(string entityName);
    Task<Entity> GetEntityWithPropertiesAsync(string entityName);
    Task<IEnumerable<Entity>> GetAllEntitiesAsync(bool includeAll = false);
    Task<Entity?> GetEntityMetadataAsync(string entityName);
    Task RefreshEntityListQueryAsync(string entityName, AppDbContext context);
    string BuildDynamicListQuery(Entity entity);
}

public class MetadataService : IMetadataService
    {
        private readonly AppDbContext _context;

        public MetadataService(AppDbContext context)
        {
            _context = context;
        }

        // Obtiene la metadata básica de la entidad
        public async Task<Entity> GetEntityByNameAsync(string entityName)
        {
            return await _context.Entities
                .FirstAsync(e => e.Name == entityName && e.IsAvailable);
        }

        // Obtiene la entidad incluyendo todos sus campos ordenados por fila y columna
        public async Task<Entity> GetEntityWithPropertiesAsync(string entityName)
        {
            // Verifica si en tu base de datos la columna es 'IsAvailable' o 'IsActive'
            // Si la tabla es nueva, asegúrate de que IsAvailable sea 1 (true)
            return await _context.Entities
                .Include(e => e.Properties)
                .FirstOrDefaultAsync(e => e.Name == entityName); // Quita el filtro de IsAvailable temporalmente para probar
        }

        public async Task<IEnumerable<Entity>> GetAllEntitiesAsync(bool includeAll = false)
        {
            var list = includeAll ? _context.Entities.AsQueryable() : _context.Entities.Where(e => e.IsAvailable);//.OrderBy(e => e.Label);

            if(list.AsEnumerable != null )
            {
                
                return await list.ToListAsync();
            }
            else
            {
                return null;

            }

            
        }

        public async Task<Entity?> GetEntityMetadataAsync(string entityName)
        {
            // Aquí va tu lógica actual para recuperar la definición de la entidad desde SQL Server.
            // Un ejemplo rápido usando EF Core sobre las tablas del Kernel:
            return await _context.Entities
                .Include(e => e.Properties)
                .FirstOrDefaultAsync(e => e.Name == entityName);
        }


        public string BuildDynamicListQuery(Entity entity)
        {
            // Si la entidad no tiene mapeadas propiedades de listado, hacemos una caída segura
            var listProperties = entity.Properties?.Where(p => p.OnList).ToList();
            if (listProperties == null || !listProperties.Any())
            {
                return $"SELECT t0.[Id] AS [Id], t0.[Folio] AS [Folio], t0.[IsDeleted] as [IsDeleted] FROM [{entity.Name}] t0 WHERE t0.[IsDeleted] = 0";
            }

            var selectFields = new List<string>();
            var joinClauses = new List<string>();
            
            // El ID de la fila base es obligatorio para acciones en UI
            selectFields.Add("t0.[Id] AS [Id]");
            selectFields.Add("t0.[Folio] AS [Folio]"); // 👈 ⚡ INYECTAR ESTA LÍNEA AQUÍ
            selectFields.Add("t0.[IsDeleted] AS [IsDeleted]"); // 👈 ⚡ INYECTAR ESTA LÍNEA AQUÍ

            int joinCounter = 1;

            foreach (var prop in listProperties)
            {
                if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) continue;
                if (prop.Name.Equals("Folio", StringComparison.OrdinalIgnoreCase)) continue; // Evita duplicar si el usuario también la marcó como OnList
                
                // Caso Relacional (Tipo 10) -> Inyección de LEFT JOIN dinámico
                if (prop.DataTypeId == 10 && !string.IsNullOrEmpty(prop.SourceDefinition))
                {
                    string aliasRelacionado = $"t{joinCounter}";
                    string tablaRelacionada = prop.SourceDefinition;

                    // COALESCE nativo en T-SQL para evaluar prioridades en el catálogo relacionado
                    string coalesceDisplay = $"COALESCE({aliasRelacionado}.[Name], {aliasRelacionado}.[Name], {aliasRelacionado}.[Folio], CAST({aliasRelacionado}.[Id] AS VARCHAR(36)))";
                    
                    selectFields.Add($"{coalesceDisplay} AS [{prop.Name}]");
                    joinClauses.Add($"LEFT JOIN [{tablaRelacionada}] {aliasRelacionado} ON t0.[{prop.Name}] = {aliasRelacionado}.[Id] AND {aliasRelacionado}.[IsDeleted] = 0");

                    joinCounter++;
                }
                else
                {
                    // Campos primitivos directos (Texto, Números, Fechas, etc.)
                    selectFields.Add($"t0.[{prop.Name}] AS [{prop.Name}]");
                }
            }

            return $"SELECT {string.Join(", ", selectFields)} FROM [{entity.Name}] t0 {string.Join(" ", joinClauses)} WHERE t0.[IsDeleted] = 0";
        }

        // Sub-método orquestador para refrescar el campo ListQuery en SQL Server
        public async Task RefreshEntityListQueryAsync(string entityName, AppDbContext context)
        {
            try
            {
                // 1. Recuperar la fotografía actual de la entidad con todas sus propiedades mutadas
                var entity = await GetEntityWithPropertiesAsync(entityName);
                if (entity != null)
                {
                    // 2. Compilar el string T-SQL dinámico
                    string nuevoQuery = BuildDynamicListQuery(entity);

                    // 3. Guardar el query precalculado directamente en la columna de la metadata
                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE [Entity] SET [ListQuery] = @p0 WHERE [Name] = @p1", 
                        nuevoQuery, entityName
                    );
                }
            }
            catch
            {
                // Fail-safe silencioso para prevenir que un error de string rompa el flujo visual del panel
            }
        }

    }