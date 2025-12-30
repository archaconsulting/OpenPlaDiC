using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using OpenPlaDiC.BIZ.Services;

namespace OpenPlaDiC.WebApp.Models
{
    public class ExternalSignInManager : SignInManager<ApplicationUser>
    {
        private readonly IAuthService _authService;

        public ExternalSignInManager(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation,
            IAuthService authService)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _authService = authService;

        }

        public override async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
        {

            var response = await _authService.LoginAsync(user.UserName, password);

            if (response.IsSuccess)
            {

                return SignInResult.Success;

            }
            return SignInResult.Failed;
        }
    }
}
