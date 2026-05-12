using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;

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

        public AccessService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AccessControl> GetViewAccessAsync(Guid userId, Guid viewId)
        {
            // 1. Intentar obtener permiso directo del usuario
            var userAccess = await _context.AccessControls
                .FirstAsync(a => a.UserId == userId && a.DynamicViewId == viewId);

            if (userAccess != null) return userAccess;

            // 2. Si no hay directo, buscar en sus perfiles
            var profileIds = await _context.UserProfiles
                .Where(up => up.UserId == userId)
                .Select(up => up.ProfileId)
                .ToListAsync();

            var profileAccessList = await _context.AccessControls
                .Where(a => profileIds.Contains(a.ProfileId ?? Guid.Empty) && a.DynamicViewId == viewId)
                .ToListAsync();

            // Consolidar: Tomamos el máximo nivel de acceso y los permisos más altos
            return ConsolidateAccess(profileAccessList);
        }

        public async Task<AccessControl> GetEntityAccessAsync(Guid userId, Guid entityId)
        {
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
