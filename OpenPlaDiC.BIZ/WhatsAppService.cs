using System.Net.Http.Headers;
using System.Text;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.BIZ
{

    public interface IWhatsAppService
    {
        Task<Response<bool>> SendMessageAsync(string to, string messageBody);
        Task<Response<bool>> SendMediaMessageAsync(string to, string messageBody, string? mediaUrl);
        Task<Response<bool>> ProcessIncomingWebhookAsync(IDictionary<string, string> formData);
    }


    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly ISystemParameterService _paramService;

        public WhatsAppService(HttpClient httpClient, ISystemParameterService paramService)
        {
            _httpClient = httpClient;
            _paramService = paramService;
        }

        public async Task<Response<bool>> SendMessageAsync(string to, string messageBody)
        {
            return await SendMediaMessageAsync(to, messageBody, null);
        }

        public async Task<Response<bool>> SendMediaMessageAsync(string to, string messageBody, string? mediaUrl)
        {
            try
            {
                var sidParam = await _paramService.GetValueAsync("TWILIO_ACCOUNT_SID");
                var tokenParam = await _paramService.GetValueAsync("TWILIO_AUTH_TOKEN_PASS");
                var fromParam = await _paramService.GetValueAsync("TWILIO_PHONE_NUMBER");

                var sid = sidParam;
                var token = tokenParam;
                var fromNumber = fromParam;

                if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(fromNumber))
                {
                    return Response<bool>.Fail("Las credenciales de Twilio WhatsApp no están configuradas correctamente en SystemParameter.");
                }

                var requestUrl = $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json";
                var formattedTo = to.StartsWith("whatsapp:") ? to : $"whatsapp:{to}";

                var values = new Dictionary<string, string>
                {
                    { "From", fromNumber },
                    { "To", formattedTo },
                    { "Body", messageBody }
                };

                if (!string.IsNullOrEmpty(mediaUrl))
                {
                    values.Add("MediaUrl", mediaUrl);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new FormUrlEncodedContent(values)
                };

                var byteArray = Encoding.ASCII.GetBytes($"{sid}:{token}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return Response<bool>.Success(true, "Mensaje de WhatsApp enviado correctamente.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return Response<bool>.Fail($"Error al enviar WhatsApp vía Twilio API: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Exception(ex, "Ocurrió una excepción al intentar enviar el mensaje de WhatsApp.");
            }
        }

        public async Task<Response<bool>> ProcessIncomingWebhookAsync(IDictionary<string, string> formData)
        {
            try
            {
                formData.TryGetValue("From", out var fromPhone);
                formData.TryGetValue("Body", out var messageBody);
                formData.TryGetValue("MessageSid", out var messageSid);

                // Lógica de registro en bitácora / disparo de Roslyn
                return await Task.FromResult(Response<bool>.Success(true, "Webhook procesado exitosamente."));
            }
            catch (Exception ex)
            {
                return Response<bool>.Exception(ex, "Error al procesar el Webhook de WhatsApp.");
            }
        }
    }
}