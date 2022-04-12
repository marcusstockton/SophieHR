using System.Diagnostics;
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
            _logger.LogInformation("Sending email to {EmailAddress}", email);
            MailMessage mail = new MailMessage();
            //set the addresses
            mail.From = new MailAddress("admin@eezeeWeb.com");
            mail.To.Add(email);

            //set the content
            mail.Subject = subject;
            mail.Body = htmlMessage;

            if (_environment.IsDevelopment())
            {
                await StartSMTP4Dev();

                //send the message
                SmtpClient smtp = new SmtpClient("localhost");
                smtp.UseDefaultCredentials = true;

                await smtp.SendMailAsync(mail);
            }

            // TODO: Implement a live smtp server...
        }

        private async Task StartSMTP4Dev()
        {
            await Task.Run(() =>
            {
                // Fire up smtp4dev:
                var desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                Process p = new Process();
                var fileName = Path.Combine(desktop, "SMTP4Dev.bat");
                p.StartInfo = new ProcessStartInfo(fileName);
                p.StartInfo.UseShellExecute = true;
                p.Start();
            });
        }
    }
}