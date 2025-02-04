namespace FIAPX.Processamento.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientEmail, string body);
    }
}
