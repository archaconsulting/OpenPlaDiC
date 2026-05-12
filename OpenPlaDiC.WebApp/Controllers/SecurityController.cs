using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.WebApp.Controllers
{

    [Authorize(Policy = "MasterOnly")]
    public class SecurityController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public SecurityController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // --- SECCIÓN USUARIOS ---
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.OrderBy(u => u.Name).ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // REGLA DE ORO: No permitir desactivar al Super Usuario (Master)
            if (user.IsMaster && user.IsActive)
            {
                return Json(new { 
                    isSuccess = false, 
                    message = "Por seguridad, un usuario con privilegios Master no puede ser desactivado." 
                });
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            return Json(new { isSuccess = true, newState = user.IsActive });
            
        }


        // Acción para cargar los roles de un usuario (para el modal)
        [HttpGet]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var allProfiles = await _context.Profiles.ToListAsync();
            var userProfileIds = await _context.UserProfiles
                .Where(up => up.UserId == userId)
                .Select(up => up.ProfileId)
                .ToListAsync();

            var model = allProfiles.Select(p => new {
                p.Id,
                p.Name,
                HasRole = userProfileIds.Contains(p.Id)
            });

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRole(Guid userId, Guid profileId)
        {
            var existing = await _context.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ProfileId == profileId);

            if (existing != null)
            {
                _context.UserProfiles.Remove(existing);
            }
            else
            {
                _context.UserProfiles.Add(new UserProfile { UserId = userId, ProfileId = profileId });
            }

            await _context.SaveChangesAsync();
            return Json(new { isSuccess = true });
        }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(User model, string rawPassword)
    {
        try
        {
            // 1. Validaciones básicas
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return Json(new { isSuccess = false, message = "El nombre de usuario ya existe." });

            // 2. Generar Seguridad (Salt + Hash)
            string salt = Helper.GenerateSalt();
            string hashedPass = Helper.EncodePassword(rawPassword, salt);

            // 3. Configurar objeto final
            model.Id = Guid.NewGuid();
            model.Password = hashedPass;
            model.PasswordSalt = salt;
            model.CreatedAt = DateTime.Now;
            model.IsActive = true;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { isSuccess = true, message = "Usuario creado con éxito." });
        }
        catch (Exception ex)
        {
            return Json(new { isSuccess = false, message = ex.Message });
        }
    }


    public async Task<IActionResult> AccessLog()
    {
        // Obtenemos los últimos 100 intentos de acceso
        var logs = await _context.LoginLogs
            .OrderByDescending(l => l.LoginDate)
            .Take(100)
            .ToListAsync();
            
        return View(logs);
    }




        // --- SECCIÓN PERFILES ---
        public async Task<IActionResult> Profiles()
        {
            var profiles = await _context.Profiles
                .Include(p => p.UserProfiles)
                .OrderBy(p => p.Name).ToListAsync();
            return View(profiles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile(string name)
        {
            var profile = new Profile { 
                Id = Guid.NewGuid(), 
                Name = name, 
                CreatedAt = DateTime.Now,
                CreatedById = Guid.Parse(User.FindFirst("UserId")?.Value ?? Guid.Empty.ToString())
            };
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Profiles));
        }
    }


}
