using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly AppDbContext _context;


        public SearchController(ISearchService searchService,  AppDbContext context)
        {
            _searchService = searchService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Results(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return RedirectToAction("Index", "Home");

            var response = await _searchService.GlobalSearchAsync(term);
            
            ViewBag.Term = term;
            return View(response.Data); // Retorna la lista de Records
        }

        // Método para redirigir dinámicamente según la entidad encontrada
        public IActionResult GoToRecord(string entityName, Guid id)
        {
            // Redirige al controlador dinámico que maneja el CRUD
            return RedirectToAction("Details", "Dynamic", new { entity = entityName, id = id });
        }

        [HttpGet]
        public async Task<IActionResult> Lookup(string entity, string term)
        {
            // Usamos el DataService para buscar en la tabla física de forma dinámica
            string sql = $"SELECT Id, Folio, Name FROM {entity} WHERE Name LIKE @p0 OR Folio LIKE @p0";
            var response = await _context.GetQueryAsync(sql, new GlobalItem { Name = "@p0", Value = $"%{term}%" });
            
            var results = new List<object>();
            foreach (System.Data.DataRow row in response.Data.Rows)
            {
                results.Add(new { 
                    id = row["Id"], 
                    folio = row["Folio"], 
                    text = row["Name"] 
                });
            }
            return Json(results);
        }

    }
}
