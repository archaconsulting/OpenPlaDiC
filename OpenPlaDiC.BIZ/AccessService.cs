using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using System.Security.Claims;
namespace OpenPlaDiC.BIZ
{
    public interface IAccessService
    {
        // Obtiene el permiso consolidado para una vista dinámica
        Task<AccessControl> GetViewAccessAsync(Guid userId, Guid viewId);
        
        // Obtiene el permiso consolidado para una entidad (CRUD)
        Task<AccessControl> GetEntityAccessAsync(Guid userId, Guid entityId);
    }

    public class AccessService : IAccessService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AccessControl> GetViewAccessAsync(Guid userId, Guid viewId)
        {
            // REGLA DE ORO: Si es Master, tiene acceso total inmediato
            if (IsCurrentUserMaster()) return GetMasterAccessControl();

            // Lógica anterior de búsqueda de permisos...
            var userAccess = await _context.AccessControls
                .FirstOrDefaultAsync(a => a.UserId == userId && a.DynamicViewId == viewId);

            if (userAccess != null) return userAccess;

            var profileIds = await _context.UserProfiles
                .Where(up => up.UserId == userId)
                .Select(up => up.ProfileId)
                .ToListAsync();

            var profileAccessList = await _context.AccessControls
                .Where(a => profileIds.Contains(a.ProfileId ?? Guid.Empty) && a.DynamicViewId == viewId)
                .ToListAsync();

            return ConsolidateAccess(profileAccessList);
        }

        public async Task<AccessControl> GetEntityAccessAsync(Guid userId, Guid entityId)
        {
            // REGLA DE ORO: Si es Master, tiene acceso total inmediato
            if (IsCurrentUserMaster()) return GetMasterAccessControl();

            // Lógica anterior de búsqueda de permisos...
            var userAccess = await _context.AccessControls
                .FirstOrDefaultAsync(a => a.UserId == userId && a.EntityId == entityId);

            if (userAccess != null) return userAccess;

            var profileIds = await _context.UserProfiles
                .Where(up => up.UserId == userId)
                .Select(up => up.ProfileId)
                .ToListAsync();

            var profileAccessList = await _context.AccessControls
                .Where(a => profileIds.Contains(a.ProfileId ?? Guid.Empty) && a.EntityId == entityId)
                .ToListAsync();

            return ConsolidateAccess(profileAccessList);
        }

        // Helper para validar el Claim de Master del usuario logueado
        private bool IsCurrentUserMaster()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.HasClaim("IsMaster", "True") ?? false;
        }

        // Helper para generar un objeto de acceso total (Nivel 3 / CRUD Completo)
        private AccessControl GetMasterAccessControl()
        {
            return new AccessControl
            {
                AccessLevel = 3, // Control total de lectura/escritura/configuración
                CanRead = true,
                CanCreate = true,
                CanUpdate = true,
                CanDelete = true,
                CanExecute = true
            };
        }

        private AccessControl ConsolidateAccess(List<AccessControl> accessList)
        {
            if (!accessList.Any()) 
                return new AccessControl { AccessLevel = 0 };

            return new AccessControl
            {
                AccessLevel = accessList.Max(a => a.AccessLevel),
                CanRead = accessList.Any(a => a.CanRead),
                CanCreate = accessList.Any(a => a.CanCreate),
                CanUpdate = accessList.Any(a => a.CanUpdate),
                CanDelete = accessList.Any(a => a.CanDelete),
                CanExecute = accessList.Any(a => a.CanExecute)
            };
        }
    }
    
}
