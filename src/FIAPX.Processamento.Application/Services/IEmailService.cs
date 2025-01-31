namespace FIAPX.Processamento.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string senderEmail, string recipientEmail, string subject, string body);
    }
}
