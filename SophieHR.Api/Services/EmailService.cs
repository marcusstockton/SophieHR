using System.Net;
using System.Net.Mail;

namespace SophieHR.Api.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailService : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _environment;

        public EmailService(ILogger<EmailService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _environment = env;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation($"{nameof(SendEmailAsync)} Sending email to {{EmailAddress}}", email);
            MailMessage mail = new MailMessage();
            //set the addresses
            mail.From = new MailAddress("admin@eezeeWeb.com");
            mail.To.Add(email);

            //set the content
            mail.Subject = subject;
            mail.Body = htmlMessage;

            // Determine SMTP host/port. Allow overrides via environment variables.
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            var smtpPortEnv = Environment.GetEnvironmentVariable("SMTP_PORT");

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                // If running inside a container, use the smtp4dev service name. Otherwise use localhost mapped port.
                var inContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
                smtpHost = inContainer ? "smtp4dev" : "localhost";
            }

            int smtpPort = 0;
            if (!int.TryParse(smtpPortEnv, out smtpPort))
            {
                var inContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
                smtpPort = inContainer ? 25 : 5025;
            }

            _logger.LogInformation("Using SMTP host {SmtpHost} and port {SmtpPort}", smtpHost, smtpPort);

            try
            {
                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;

                    // If you need credentials, set them via env vars (not required for smtp4dev)
                    var user = Environment.GetEnvironmentVariable("SMTP_USER");
                    var pass = Environment.GetEnvironmentVariable("SMTP_PASS");
                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
                    {
                        smtp.Credentials = new NetworkCredential(user, pass);
                    }

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }

            // TODO: Implement a live smtp server configuration for production
        }
    }
}