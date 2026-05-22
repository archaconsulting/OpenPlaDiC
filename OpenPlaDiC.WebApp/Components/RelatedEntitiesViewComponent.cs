using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.DAL;
using System.Data;

namespace OpenPlaDiC.Web.Components
{
    public class RelatedEntitiesViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public RelatedEntitiesViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string parentEntityName, Guid parentRecordId)
        {
            // 1. Buscar qué propiedades en TODO el Kernel apuntan a esta entidad padre
            var relations = await _context.EntityProperties
                .Include(p => p.Entity)
                .Where(p => p.DataTypeId == 10 && p.SourceDefinition == parentEntityName)
                .ToListAsync();

            var model = new List<RelatedTableViewModel>();

            // 2. Para cada relación encontrada, hacer un query rápido para traer los últimos 5 registros
            foreach (var rel in relations)
            {
                // Ejemplo: SELECT TOP 5 Folio, Name FROM Pedido WHERE ClienteId = @p0 AND IsDeleted = 0
                string sql = $"SELECT TOP 5 Id, Folio, Name, CreatedAt FROM {rel.Entity.Name} WHERE {rel.Name} = @p0 AND IsDeleted = 0 ORDER BY CreatedAt DESC";
                
                // Ejecutamos a través de tu DbContext genérico
                var dbRes = await _context.GetQueryAsync(sql, new Framework.GlobalItem { Name = "@p0", Value = parentRecordId.ToString() });

                model.Add(new RelatedTableViewModel
                {
                    EntityLabel = rel.Entity.Label,
                    EntityName = rel.Entity.Name,
                    ForeignKeyName = rel.Name,
                    Data = dbRes.Data ?? new DataTable()
                });
            }

            return View(model);
        }
    }

    public class RelatedTableViewModel
    {
        public string EntityLabel { get; set; }
        public string EntityName { get; set; }
        public string ForeignKeyName { get; set; }
        public DataTable Data { get; set; }
    }
}
