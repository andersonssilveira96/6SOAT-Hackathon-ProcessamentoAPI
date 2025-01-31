using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.Services;
using FIAPX.Processamento.Application.UseCase;
using FIAPX.Processamento.Domain.Entities;
using FIAPX.Processamento.Domain.Enum;
using FIAPX.Processamento.Domain.Interfaces.Repositories;
using FIAPX.Processamento.Domain.Producer;
using Moq;

namespace FIAPX.Processamento.Domain.Tests
{
    public class ArquivoUseCaseTests
    {
        private readonly Mock<IArquivoRepository> _arquivoRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IMessageBrokerProducer> _messageBrokerProducerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IAmazonS3> _s3ClientMock;
        private readonly ArquivoUseCase _arquivoUseCase;

        public ArquivoUseCaseTests()
        {
            _arquivoRepositoryMock = new Mock<IArquivoRepository>();
            _mapperMock = new Mock<IMapper>();
            _messageBrokerProducerMock = new Mock<IMessageBrokerProducer>();
            _emailServiceMock = new Mock<IEmailService>();
            _s3ClientMock = new Mock<IAmazonS3>();

            _arquivoUseCase = new ArquivoUseCase(
                _arquivoRepositoryMock.Object,
                _mapperMock.Object,
                _messageBrokerProducerMock.Object,
                _emailServiceMock.Object,
                _s3ClientMock.Object
            );
        }

        [Fact]
        public async Task ProcessFile_ShouldCallDependencies_WhenSuccess()
        {
            // Arrange
            var arquivoDto = new ArquivoDto { Id = Guid.NewGuid(), ContentType = "video/mp4", FileName = "teste.mp4" };
            var arquivo = new Arquivo(arquivoDto.Id, "teste.mp4", "video/mp4", StatusEnum.Cadastrado, 0);
            var basePath = AppDomain.CurrentDomain.BaseDirectory; // Diretório base da aplicação
            var filePath = Path.Combine(basePath, "assets", "SmallVideo.mp4");
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileStream = new MemoryStream(fileBytes);

            _arquivoRepositoryMock.Setup(repo => repo.CreateFile(It.IsAny<Arquivo>())).Returns(Task.FromResult(arquivo));
            _arquivoRepositoryMock.Setup(repo => repo.Update(It.IsAny<Arquivo>())).Returns(Task.FromResult(arquivo));
            _messageBrokerProducerMock.Setup(producer => producer.SendMessageAsync(It.IsAny<Arquivo>())).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(email => email.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Mockando S3 para retornar um objeto simulado
            _s3ClientMock
                .Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ReturnsAsync(new GetObjectResponse { ResponseStream = fileStream });

            _s3ClientMock
                .Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                .ReturnsAsync(new PutObjectResponse());

            // Act
            await _arquivoUseCase.ProcessFile(arquivoDto);

            // Assert
            _arquivoRepositoryMock.Verify(repo => repo.CreateFile(It.IsAny<Arquivo>()), Times.Once);
            _arquivoRepositoryMock.Verify(repo => repo.Update(It.IsAny<Arquivo>()), Times.Once);
            _messageBrokerProducerMock.Verify(producer => producer.SendMessageAsync(It.IsAny<Arquivo>()), Times.Exactly(2));
            _s3ClientMock.Verify(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), default), Times.Once);
            _s3ClientMock.Verify(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Once);
        }

        [Fact]
        public async Task ProcessFile_ShouldHandleException_WhenErrorOccurs()
        {
            // Arrange
            var arquivoDto = new ArquivoDto { Id = Guid.NewGuid(), ContentType = "video/mp4", FileName = "teste.mp4" };

            _arquivoRepositoryMock.Setup(repo => repo.CreateFile(It.IsAny<Arquivo>())).ThrowsAsync(new Exception("Erro no repositório"));

            // Mockando o S3 para lançar uma exceção de token expirado
            _s3ClientMock
                .Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
                .ThrowsAsync(new AmazonS3Exception("The provided token has expired."));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _arquivoUseCase.ProcessFile(arquivoDto));
            _arquivoRepositoryMock.Verify(repo => repo.CreateFile(It.IsAny<Arquivo>()), Times.Once);
            _arquivoRepositoryMock.Verify(repo => repo.Update(It.IsAny<Arquivo>()), Times.Once);
            _messageBrokerProducerMock.Verify(producer => producer.SendMessageAsync(It.IsAny<Arquivo>()), Times.Once);
            _emailServiceMock.Verify(email => email.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}