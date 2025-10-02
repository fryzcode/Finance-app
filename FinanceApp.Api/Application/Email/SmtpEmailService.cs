using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FinanceApp.Api.Application.Email
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default);
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken ct = default)
        {
            var host = _configuration["Smtp:Host"];
            var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
            var user = _configuration["Smtp:User"];
            var pass = _configuration["Smtp:Pass"];
            var from = _configuration["Smtp:From"] ?? user;
            var enableSsl = bool.TryParse(_configuration["Smtp:Ssl"], out var ssl) ? ssl : true;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = enableSsl
            };
            using var message = new MailMessage(from!, to, subject, body) { IsBodyHtml = isHtml };
            await client.SendMailAsync(message, ct);
        }
    }
}
