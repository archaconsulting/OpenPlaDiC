using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.WebApp.Models;

namespace OpenPlaDiC.WebApp.Controllers
{
        
    [Authorize(Policy = "MasterOnly")]
    public class EmailSettingsController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public EmailSettingsController(IEmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _config = config;
        }

        public IActionResult Index()
        {
            // Cargamos los valores actuales (puedes migrarlos a SystemParameters después)
            var model = new EmailConfigViewModel {
                SenderName = _config["EmailConfig:SenderName"],
                Email = _config["EmailConfig:Email"],
                SmtpServer = _config["EmailConfig:SmtpServer"],
                SmtpPort = int.Parse(_config["EmailConfig:SmtpPort"] ?? "587")
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TestConnection(EmailConfigViewModel model)
        {
            var result = await _emailService.SendEmailAsync(
                model.TestEmailRecipient, 
                "OpenPlaDiC - Test Connection", 
                "<h1>Success!</h1><p>The email service is working correctly with attachments support.</p>");

            return Json(result);
        }
    }


}
