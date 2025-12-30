using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using OpenPlaDiC.BIZ.Services;
using OpenPlaDiC.WebApp.Models; // Donde esté tu LoginViewModel
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _authService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IAuthService authService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Guardamos la URL de retorno para redirigir tras un login exitoso
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {

                var user = await _signInManager.UserManager.FindByNameAsync(model.Username);

                if(user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                    return View(model);
                }

                var respLogin = await _authService.LoginAsync(model.Username, model.Password);

                if (!respLogin.IsSuccess || respLogin.Data is null)
                {
                    ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
                    return View(model);
                }


                var customClaims = new[] { 
                    new Claim("FullName", user.NombreCompleto),
                    new Claim("UserId",user.Id),
                    new Claim("PhoneNumber",respLogin.Data.MobilePhone ?? ""),
                    new Claim("Email",respLogin.Data.Email)

                };
                var res = _signInManager.SignInWithClaimsAsync(
                    user, model.RememberMe,
                    customClaims);

                if (res.IsCompletedSuccessfully)
                {
                    return LocalRedirect(returnUrl);

                }


                if (false)
                {

                    #region Password SignIn con ExternalSignInManager
                    // Este método dispara la cadena: 
                    // ExternalSignInManager -> BIZ (AuthService) -> DAL (Repository) -> API
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Username,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {

                        _logger.LogInformation("Usuario {User} autenticado vía API externa.", model.Username);
                        return LocalRedirect(returnUrl);
                    }


                    #endregion

                    // Manejo de errores específicos
                    if (result.IsLockedOut)
                    {
                        return View("Lockout");
                    }
                }
                ModelState.AddModelError(string.Empty, "Credenciales inválidas en el sistema externo.");
            }

            // Si llegamos aquí, algo falló, volvemos a mostrar el formulario
            return View(model);
        }

        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var response = await _authService.RequestPasswordResetAsync( email );
            if (response.IsSuccess)
            {
                ViewBag.Message = "Si el correo existe, recibirás un enlace de recuperación.";
                return View("ForgotPasswordConfirmation");
            }
            return View();
        }

        public IActionResult ForgotPasswordConfirmation() => View();
    }
}
