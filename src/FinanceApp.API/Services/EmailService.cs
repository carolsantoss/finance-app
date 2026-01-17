using MimeKit;
using MailKit.Net.Smtp;

namespace FinanceApp.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var server = _configuration["SMTP_SERVER"];
            var portString = _configuration["SMTP_PORT"];
            var user = _configuration["SMTP_USER"];
            var password = _configuration["SMTP_PASSWORD"];

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(portString))
            {
                // In development, maybe just log?
                Console.WriteLine($"[EmailService] Mock Send to {to}: {subject}");
                return;
            }

            int port = int.Parse(portString);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Finance App", user));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlBody
            };

            using (var client = new SmtpClient())
            {
                // Accept all SSL certificates (for development or self-signed)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.StartTls);
                
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(user, password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
