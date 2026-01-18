using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Shared.Data;

namespace FinanceApp.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;

        public EmailService(IConfiguration configuration, Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
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
                await CreateNotificationAsync(to, subject, htmlBody);
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

                // Create Notification
                await CreateNotificationAsync(to, subject, htmlBody);
            }
        }

        private async Task CreateNotificationAsync(string to, string subject, string body)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    // Convert to lower case for comparison to avoid casing issues
                    var appUser = await context.users.FirstOrDefaultAsync(u => u.nm_email.ToLower() == to.ToLower());
                    
                    if (appUser != null)
                    {
                        Console.WriteLine($"[EmailService] Creating notification for user {appUser.id_usuario} ({appUser.nm_email})");
                        context.notifications.Add(new Shared.Models.Notification
                        {
                            id_usuario = appUser.id_usuario,
                            nm_titulo = subject,
                            ds_mensagem = body, // Store full body
                            nm_tipo = "EMAIL",
                            dt_criacao = DateTime.UtcNow,
                            fl_lida = false
                        });
                        await context.SaveChangesAsync();
                        Console.WriteLine($"[EmailService] Notification created successfully.");
                    }
                    else
                    {
                         Console.WriteLine($"[EmailService] User not found for email: {to}. Notification skipped.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Failed to create notification: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
