using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace VizsgaremekApp.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendPasswordEmail(string toEmail, string toName, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            var testemail = "nyitribusz.r@gmail.com";
            message.To.Add(new MailboxAddress(toName, testemail));
            message.Subject = "Oktatói jelentkezés – ideiglenes jelszó";

            message.Body = new TextPart("plain")
            {
                Text = $@"Kedves {toName}!

Köszönjük, hogy jelentkeztél oktatónak!

Ideiglenes jelszavad: {password}

Kérjük, lépj be és módosítsd a jelszavad mielőbb.

Üdvözlettel:
Lőtér csapata"
            };

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Jelszó email elküldve: {Email}", testemail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a jelszó email küldése közben.");
                throw;
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }
}
