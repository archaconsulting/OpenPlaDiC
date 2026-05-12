using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using Microsoft.EntityFrameworkCore;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IMetadataService _metadataService;
        private readonly IAccessService _accessService;
        private readonly AppDbContext _context; // Para ejecutar los SPs del Kernel

        // GET: DashboardController
        public DashboardController(IMetadataService metadataService, IAccessService accessService,  AppDbContext contex)
        {
            _metadataService = metadataService;
            _accessService = accessService;
            _context = contex;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Challenge();

            var userId = Guid.Parse(userIdClaim);
            
            // Obtenemos todas las entidades registradas
            var allEntities = await _metadataService.GetAllEntitiesAsync();
            var allowedEntities = new List<Entity>();

            // Filtramos las que el usuario tiene permitido leer
            foreach (var entity in allEntities)
            {
                var access = await _accessService.GetEntityAccessAsync(userId, entity.Id);
                if (access.CanRead)
                {
                    allowedEntities.Add(entity);
                }
            }


            // Datos para la gráfica (Últimos 7 días)
            var lastWeek = DateTime.Now.AddDays(-7);
            var logData = await _context.LoginLogs
                .Where(l => l.LoginDate >= lastWeek)
                .GroupBy(l => new { Date = l.LoginDate.Date, l.Status })
                .Select(g => new { 
                    g.Key.Date, 
                    g.Key.Status, 
                    Count = g.Count() 
                })
                .ToListAsync();

            ViewBag.ChartData = logData;


            return View(allowedEntities);
        }

        [Authorize]
        public IActionResult KeepAlive()
        {
            // No hace nada, pero el simple hecho de ser llamado
            // refresca la SlidingExpiration de la cookie.
            return Ok();
        }

    }
}
