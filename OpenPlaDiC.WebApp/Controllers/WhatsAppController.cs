using Microsoft.AspNetCore.Mvc;
using OpenPlaDiC.BIZ;
using OpenPlaDiC.Core.DTOs;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppController(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook()
        {
            var form = await Request.ReadFormAsync();
            var dict = form.ToDictionary(k => k.Key, v => v.Value.ToString());

            var result = await _whatsAppService.ProcessIncomingWebhookAsync(dict);

            return Content("<Response></Response>", "text/xml");
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] WhatsAppNotificationDto dto)
        {
            var response = await _whatsAppService.SendMediaMessageAsync(dto.To, dto.Message, dto.MediaUrl);

            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}