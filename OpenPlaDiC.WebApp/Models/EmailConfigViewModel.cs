using System;

namespace OpenPlaDiC.WebApp.Models;

public class EmailConfigViewModel
{
    public string SenderName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string ImapServer { get; set; }
    public int ImapPort { get; set; }
    public bool UseSsl { get; set; }
    
    // Para la prueba de envío
    public string TestEmailRecipient { get; set; }
}
