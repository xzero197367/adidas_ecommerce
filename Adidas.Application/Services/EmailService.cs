using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Adidas.Application.Contracts.ServicesContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Adidas.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // Validate configuration first
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortStr = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                // Log configuration values (without password)
                _logger.LogInformation("Email Configuration - Server: {Server}, Port: {Port}, Username: {Username}, FromEmail: {FromEmail}",
                    smtpServer, smtpPortStr, smtpUsername, fromEmail);

                // Validate required settings
                if (string.IsNullOrEmpty(smtpServer))
                    throw new InvalidOperationException("SMTP Server is not configured");
                if (string.IsNullOrEmpty(smtpUsername))
                    throw new InvalidOperationException("SMTP Username is not configured");
                if (string.IsNullOrEmpty(smtpPassword))
                    throw new InvalidOperationException("SMTP Password is not configured");
                if (string.IsNullOrEmpty(fromEmail))
                    throw new InvalidOperationException("From Email is not configured");

                if (!int.TryParse(smtpPortStr, out int smtpPort))
                {
                    smtpPort = 587; // Default port
                    _logger.LogWarning("SMTP Port not configured or invalid, using default: {Port}", smtpPort);
                }

                _logger.LogInformation("Attempting to send email to: {ToEmail}", toEmail);

                using var client = new SmtpClient(smtpServer, smtpPort);

                // Configure SMTP client
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 30000; // 30 seconds timeout

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName ?? "Adidas Clone"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                // Add some headers for better deliverability
                mailMessage.Headers.Add("X-Mailer", "Adidas Clone Application");
                mailMessage.Headers.Add("X-Priority", "3");

                _logger.LogInformation("Sending email with subject: {Subject}", subject);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP Error sending email to {Email}. StatusCode: {StatusCode}",
                    toEmail, smtpEx.StatusCode);
                throw new InvalidOperationException($"SMTP Error: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error sending email to {Email}", toEmail);
                throw;
            }
        }
    }
}