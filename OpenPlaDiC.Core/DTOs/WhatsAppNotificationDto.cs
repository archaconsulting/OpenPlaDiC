namespace OpenPlaDiC.Core.DTOs
{
    public class WhatsAppNotificationDto
    {
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
    }
}