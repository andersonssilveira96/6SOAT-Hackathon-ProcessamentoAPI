using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FIAPX.Processamento.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ISendGridClient _sendGridClient;

        public EmailService(ISendGridClient sendGridClient)
        {
            _sendGridClient = sendGridClient;
        }

        public async Task SendEmailAsync(string recipientEmail, string body)
        {
            try
            {
                var from = new EmailAddress("hackathonfiapx@gmail.com", "FIAPX");
                var to = new EmailAddress(recipientEmail);
                var plainTextContent = body;
                var htmlContent = body;
                var msg = MailHelper.CreateSingleEmail(from, to, "FIAPX - Erro de processamento", plainTextContent, htmlContent);

                var response = await _sendGridClient.SendEmailAsync(msg);

                Console.WriteLine($"E-mail enviado! Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                throw;
            }
        }
    }

}
