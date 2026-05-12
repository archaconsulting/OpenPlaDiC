using System;
using OpenPlaDiC.Framework;
using MailKit.Net.Smtp;
using MailKit.Net.Imap;
using MimeKit;
using MailKit;

namespace OpenPlaDiC.BIZ;

public class EmailAttachment
{
    public string FileName { get; set; }
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
}

public class EmailMessage
{
    public string From { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime Date { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new();
}

public interface IEmailService
{
    Task<Response<bool>> SendEmailAsync(string to, string subject, string body, List<EmailAttachment> attachments = null);
    Task<Response<List<EmailMessage>>> ReadRecentEmailsAsync(int count = 10);
}

public class EmailService : IEmailService
{
     private readonly ISystemParameterService _paramService;

    public EmailService(ISystemParameterService paramService)
    {
        _paramService = paramService;
    }

    public async Task<Response<bool>> SendEmailAsync(string to, string subject, string body, List<EmailAttachment> attachments = null)
    {
        var response = new Response<bool>();
        try
        {


            // Recuperamos la configuración desde la DB
            string host = await _paramService.GetValueAsync("SMTP_HOST");
            int port = int.Parse(await _paramService.GetValueAsync("SMTP_PORT") ?? "587");
            string user = await _paramService.GetValueAsync("SMTP_USER");
            string pass = await _paramService.GetValueAsync("SMTP_PASS");
            string senderName = await _paramService.GetValueAsync("SMTP_SENDER_NAME");



            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, user));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            // AGREGAR ADJUNTOS AL ENVÍO
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    builder.Attachments.Add(file.FileName, file.Content, ContentType.Parse(file.ContentType));
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, true);
            await smtp.AuthenticateAsync(user, pass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            response.IsSuccess = true;
        }
        catch (Exception ex) { response.SetErrorResponse(ex.Message); }
        return response;
    }

    public async Task<Response<List<EmailMessage>>> ReadRecentEmailsAsync(int count = 10)
    {
        var response = new Response<List<EmailMessage>>();
        try
        {

            // Recuperamos la configuración desde la DB
            string host = await _paramService.GetValueAsync("SMTP_HOST");
            int port = int.Parse(await _paramService.GetValueAsync("SMTP_PORT") ?? "587");
            string user = await _paramService.GetValueAsync("SMTP_USER");
            string pass = await _paramService.GetValueAsync("SMTP_PASS");
            string senderName = await _paramService.GetValueAsync("SMTP_SENDER_NAME");



            using var client = new ImapClient();
            await client.ConnectAsync(host, 993, true);
            await client.AuthenticateAsync(user, pass);

            await client.Inbox.OpenAsync(FolderAccess.ReadOnly);
            var messages = new List<EmailMessage>();

            for (int i = client.Inbox.Count - 1; i >= Math.Max(0, client.Inbox.Count - count); i--)
            {
                var msg = await client.Inbox.GetMessageAsync(i);
                var emailMsg = new EmailMessage {
                    From = msg.From.ToString(),
                    Subject = msg.Subject,
                    Body = msg.HtmlBody ?? msg.TextBody,
                    Date = msg.Date.DateTime
                };

                // LEER ADJUNTOS DEL CORREO RECIBIDO
                foreach (var attachment in msg.Attachments)
                {
                    if (attachment is MimePart part)
                    {
                        using var ms = new MemoryStream();
                        await part.Content.DecodeToAsync(ms);
                        emailMsg.Attachments.Add(new EmailAttachment {
                            FileName = part.FileName,
                            Content = ms.ToArray(),
                            ContentType = part.ContentType.MimeType
                        });
                    }
                }
                messages.Add(emailMsg);
            }
            response.Data = messages;
            response.IsSuccess = true;
        }
        catch (Exception ex) { response.SetErrorResponse(ex.Message); }
        return response;
    }


}