using System;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;

namespace OpenPlaDiC.BIZ;

public interface IMetadataService
{
    Task<Entity> GetEntityByNameAsync(string entityName);
    Task<Entity> GetEntityWithPropertiesAsync(string entityName);
    Task<IEnumerable<Entity>> GetAllEntitiesAsync();
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

        public async Task<IEnumerable<Entity>> GetAllEntitiesAsync()
        {
            var list = _context.Entities.Where(e => e.IsAvailable);//.OrderBy(e => e.Label);

            if(list.AsEnumerable != null )
            {
                
                return await list.ToListAsync();
            }
            else
            {
                return null;

            }

            
        }
    }