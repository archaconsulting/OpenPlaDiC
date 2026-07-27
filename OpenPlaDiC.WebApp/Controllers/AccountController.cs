using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.WebApp.Extensions;
using OpenPlaDiC.WebApp.Models;
using System.Security.Claims;

namespace OpenPlaDiC.WebApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuthService _authService;
        private readonly IHomeRedirectService _homeRedirect;
        
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IAuthService authService,
            IHomeRedirectService homeRedirect)
        {
            _signInManager = signInManager;
            _logger = logger;
            _authService = authService;
            _homeRedirect = homeRedirect;
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
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            string ua = Request.Headers["User-Agent"].ToString();

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByNameAsync(model.Username);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                    return View(model);
                }

                var respLogin = await _authService.LoginAsync(model.Username, model.Password, ip, ua);

                if (!respLogin.IsSuccess || respLogin.Data is null)
                {
                    ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
                    return View(model);
                }

                // 1. Agregar NameIdentifier explícitamente para que HomeRedirectService lo pueda leer como GUID
                var customClaims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 👈 Clave fundamental para el Evaluador
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("FullName", user.FullName ?? ""),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("PhoneNumber", ""),
                    new Claim("Email", respLogin.Data.Text ?? ""),
                    new Claim("IsMaster", user.IsMaster.ToString())
                };

                // 2. Iniciar sesión con claims
                await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, customClaims);

                // 3. Evaluar la redirección:
                // Si el usuario venía intentando acceder a una URL específica previa (que no sea la raíz "~/"), respetamos su intención.
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != Url.Content("~/"))
                {
                    return LocalRedirect(returnUrl);
                }

                // 4. Si venía a la raíz o no tenía returnUrl, construimos el ClaimsPrincipal dinámico para evaluar la Jerarquía
                var identity = new ClaimsIdentity(customClaims, "Identity.Application");
                var userPrincipal = new ClaimsPrincipal(identity);

                // 5. Obtenemos la URL calculada por el Kernel (Usuario -> Perfil Principal -> Registrado -> Anónimo -> Home/Index)
                string redirectUrl = await _homeRedirect.GetRedirectUrlAsync(userPrincipal);

                return LocalRedirect(redirectUrl);
            }

            // Si llegamos aquí, algo falló en la validación del modelo
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
            var response = await _authService.RequestPasswordResetAsync(email);
            if (response.IsSuccess)
            {
                ViewBag.Message = "Si el correo existe, recibirás un enlace de recuperación. "+ response.Data;
                return View("ForgotPasswordConfirmation");
            }
            return View();
        }

        public IActionResult ForgotPasswordConfirmation() => View();


    }
}
