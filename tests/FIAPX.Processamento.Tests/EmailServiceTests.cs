namespace FIAPX.Processamento.Tests
{
    using FIAPX.Processamento.Application.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly EmailService _emailService;
        private readonly Mock<ISendGridClient> _sendGridClientMock;

        public EmailServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["SendGridApiKey"]).Returns("fake-api-key");
            _sendGridClientMock = new Mock<ISendGridClient>();
            _sendGridClientMock
                .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.OK, null, null));
            _emailService = new EmailService(_sendGridClientMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ValidParameters_ShouldSendEmail()
        {
            // Arrange
            var senderEmail = "sender@example.com";
            var recipientEmail = "recipient@example.com";
            var subject = "Test Email";
            var body = "This is a test email.";            

            // Act
            await _emailService.SendEmailAsync(recipientEmail, body);

            // Assert
            _sendGridClientMock.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task SendEmailAsync_WhenSendGridThrowsException_ShouldHandleException()
        {
            // Arrange
            var senderEmail = "sender@example.com";
            var recipientEmail = "recipient@example.com";
            var subject = "Test Email";
            var body = "This is a test email.";

            _sendGridClientMock
                .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SendGrid failure"));
            var emailService = new EmailService(_sendGridClientMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => emailService.SendEmailAsync(recipientEmail, body));
        }
    }
}
