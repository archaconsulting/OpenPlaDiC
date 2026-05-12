using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using System.Security.Claims;

namespace OpenPlaDiC.WebApp.Components
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IMetadataService _metadataService;
        private readonly IAccessService _accessService;

        public SidebarViewComponent(IMetadataService metadataService, IAccessService accessService)
        {
            _metadataService = metadataService;
            _accessService = accessService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdClaim = ((ClaimsPrincipal)User).FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Content("");

            var userId = Guid.Parse(userIdClaim);
            var allEntities = await _metadataService.GetAllEntitiesAsync();
            var allowedEntities = new List<Entity>();

            foreach (var entity in allEntities)
            {
                // Solo incluimos en el menú las que el usuario puede leer
                var access = await _accessService.GetEntityAccessAsync(userId, entity.Id);
                if (access.CanRead) allowedEntities.Add(entity);
            }

            return View(allowedEntities);
        }
    }
}
