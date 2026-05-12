using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.WebApp.Models;
using System.Security.Claims;

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

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            string ua = Request.Headers["User-Agent"].ToString();
            
            returnUrl ??= Url.Content("~/");

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


                var customClaims = new[] {
                    new Claim(ClaimTypes.Name, user.UserName),

                    new Claim("FullName", user.FullName),
                    new Claim("UserId",user.Id),
                    new Claim("PhoneNumber", ""),
                    new Claim("Email",respLogin.Data.Text),
                    new Claim("IsMaster",user.IsMaster.ToString())

                };
                var res = _signInManager.SignInWithClaimsAsync(
                    user, model.RememberMe,
                    customClaims);

                if (res.IsCompletedSuccessfully)
                {
                    return LocalRedirect(returnUrl);

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
            var response = await _authService.RequestPasswordResetAsync(email);
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
