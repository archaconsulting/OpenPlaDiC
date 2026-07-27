using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.Models;
using OpenPlaDiC.DAL;
using System.Security.Claims;

namespace OpenPlaDiC.WebApp.Extensions;
public interface IHomeRedirectService
{
    Task<string> GetRedirectUrlAsync(ClaimsPrincipal userPrincipal);
}

public class HomeRedirectService : IHomeRedirectService
{
    private readonly AppDbContext _context;
    private readonly IDynamicViewService _viewService;

    public HomeRedirectService(AppDbContext context, IDynamicViewService viewService)
    {
        _context = context;
        _viewService = viewService;
    }

    public async Task<string> GetRedirectUrlAsync(ClaimsPrincipal userPrincipal)
    {
        string? targetViewName = null;

        // ==========================================
        // 1. USUARIO AUTENTICADO
        // ==========================================
        if (userPrincipal.Identity != null && userPrincipal.Identity.IsAuthenticated)
        {
            var userIdStr = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Parseo seguro a Guid
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                // Consulta LINQ directa a AppDbContext con llaves GUID
                var user = await _context.Set<User>()
                    .Include(u => u.UserProfiles)
                        .ThenInclude(up => up.Profile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    // ---------------------------------------------------------
                    // JERARQUÍA 1: Página configurada directamente en el Usuario
                    // ---------------------------------------------------------
                    if (!string.IsNullOrWhiteSpace(user.HomePageView))
                    {
                        targetViewName = user.HomePageView;
                    }

                    // ---------------------------------------------------------
                    // JERARQUÍA 2: Página del Perfil Principal (IsPrimary == true)
                    // ---------------------------------------------------------
                    if (string.IsNullOrEmpty(targetViewName) && user.UserProfiles.Any())
                    {
                        // A) Perfil Principal marcado
                        var primaryProfile = user.UserProfiles
                            .Where(up => up.IsPrimary)
                            .Select(up => up.Profile)
                            .FirstOrDefault();

                        if (primaryProfile != null && !string.IsNullOrWhiteSpace(primaryProfile.HomePageView))
                        {
                            targetViewName = primaryProfile.HomePageView;
                        }
                        else
                        {
                            // B) Fallback: Primer perfil con HomePageView no nulo
                            targetViewName = user.UserProfiles
                                .Select(up => up.Profile)
                                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.HomePageView))
                                .Select(p => p.HomePageView)
                                .FirstOrDefault();
                        }
                    }
                }
            }

            // ---------------------------------------------------------
            // JERARQUÍA 3: Parámetro General para Usuarios Registrados
            // ---------------------------------------------------------
            if (string.IsNullOrEmpty(targetViewName))
            {
                targetViewName = await _context.Set<SystemParameter>()
                    .AsNoTracking()
                    .Where(p => p.Key == "DEFAULT_HOME_LOGGED_IN")
                    .Select(p => p.Value)
                    .FirstOrDefaultAsync();
            }
        }
        // ==========================================
        // 2. USUARIO ANÓNIMO
        // ==========================================
        else
        {
            // ---------------------------------------------------------
            // JERARQUÍA 4: Parámetro General para Usuarios Anónimos
            // ---------------------------------------------------------
            targetViewName = await _context.Set<SystemParameter>()
                .AsNoTracking()
                .Where(p => p.Key == "DEFAULT_HOME_ANONYMOUS")
                .Select(p => p.Value)
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // VALIDACIÓN DE VISTA EN KERNEL Y REDIRECCIÓN
        // ==========================================
        if (!string.IsNullOrWhiteSpace(targetViewName))
        {
            var viewResp = await _viewService.GetByNameAsync(targetViewName);
            if (viewResp != null && viewResp.IsSuccess && viewResp.Data != null)
            {
                return $"/Custom/{targetViewName}";
            }
        }

        // ---------------------------------------------------------
        // JERARQUÍA 5: Fallback final por defecto
        // ---------------------------------------------------------
        return "/Home/Index";
    }
}