using Microsoft.AspNetCore.Identity;
using OpenPlaDiC.BIZ.Services;

namespace OpenPlaDiC.WebApp.Models
{
    public class ExternalUserStore : IUserStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>
    {
        private readonly IAuthService _authService;

        public ExternalUserStore(IAuthService authService)
        {
            _authService = authService;
        }

        // Busca al usuario en la API externa por nombre
        public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {

            var response = await _authService.FindByNameAsync(normalizedUserName);

            if (!response.IsSuccess) return null;

            return new ApplicationUser() { Id = response.Data.Id.ToString(), UserName = response.Data.Username, Email = response.Data.Email, NombreCompleto = response.Data.Name, NormalizedEmail = response.Data.Email };
        }

        // Este método es crucial: aquí es donde la API externa valida la contraseña
        public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            // Retornamos un valor dummy porque la validación real la haremos en el SignInManager
            // o la API externa manejará el hash.
            return Task.FromResult("EXTERNAL_AUTH");
        }

        // Implementaciones obligatorias (pueden quedar vacías si la API es de solo lectura)
        public Task CreateAsync(ApplicationUser user, CancellationToken ct) => Task.CompletedTask;
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.UserName);

        Task IUserStore<ApplicationUser>.SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string?> IUserStore<ApplicationUser>.GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<ApplicationUser>.SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IdentityResult> IUserStore<ApplicationUser>.DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<ApplicationUser?> IUserStore<ApplicationUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
        }

        Task IUserPasswordStore<ApplicationUser>.SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserPasswordStore<ApplicationUser>.HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        /* Implementar otros métodos de la interfaz lanzando NotImplementedException o vacíos */

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct)
        {
            //var client = _clientFactory.CreateClient();
            //// Llamada a la API para obtener los roles del usuario
            //var roles = await client.GetFromJsonAsync<List<string>>($"api-externa.com{user.Id}/roles");
            //return roles ?? new List<string>();

            return new List<string>() { "Usuario" }; // Rol por defecto

        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken ct)
        {
            var roles = await GetRolesAsync(user, ct);
            return roles.Contains(roleName);
        }

        // Los métodos de escritura pueden quedar vacíos si la API es de solo lectura
        public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) => Task.CompletedTask;
        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken ct) => Task.CompletedTask;

        Task<IList<ApplicationUser>> IUserRoleStore<ApplicationUser>.GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
