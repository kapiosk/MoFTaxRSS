using System.Net;
using System.Net.Mail;
using MoFTaxRSS.Configs;

namespace MoFTaxRSS.Services;

public sealed class EmailService(EmailServiceOptions options)
{
    private readonly EmailServiceOptions options = options;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await SendEmailAsync([email], subject, htmlMessage);
    }

    public async Task SendEmailAsync(
        string[] toEmails,
        string subject,
        string htmlMessage,
        KeyValuePair<Stream, string>[]? attachments = null,
        string[]? ccEmails = null,
        string[]? bccEmails = null)
    {
        using var smtpClient = new SmtpClient()
        {
            Host = options.Host,
            Port = options.Port,
            EnableSsl = options.EnableSSL,
            Credentials = new NetworkCredential(options.Username, options.Password),
            UseDefaultCredentials = false
        };
        var mailMessage = new MailMessage()
        {
            From = new(options.FromAddress, options.DisplayName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        foreach (var email in toEmails)
            mailMessage.To.Add(email);
        if (ccEmails?.Length > 0)
            foreach (var email in ccEmails)
                mailMessage.CC.Add(email);
        if (bccEmails?.Length > 0)
            foreach (var email in bccEmails)
                mailMessage.Bcc.Add(email);
        if (attachments?.Length > 0)
            foreach (var attachment in attachments)
                mailMessage.Attachments.Add(new(attachment.Key, attachment.Value));

        await smtpClient.SendMailAsync(mailMessage);
    }
}
