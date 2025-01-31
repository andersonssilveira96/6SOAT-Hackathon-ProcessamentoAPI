using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FIAPX.Processamento.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _sendGridApiKey;

        public EmailService(IConfiguration configuration)
        {
            _sendGridApiKey = configuration["SendGridApiKey"];
        }

        public async Task SendEmailAsync(string senderEmail, string recipientEmail, string subject, string body)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey); // Autentica com a API Key
                var from = new EmailAddress(senderEmail, "FIAP X");
                var to = new EmailAddress(recipientEmail);
                var plainTextContent = body; 
                var htmlContent = body;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
               
                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"E-mail enviado! Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        }
    }
}
