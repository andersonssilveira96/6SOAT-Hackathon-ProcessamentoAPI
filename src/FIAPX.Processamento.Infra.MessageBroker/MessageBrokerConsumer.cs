using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.UseCase;
using FIAPX.Processamento.Domain.Consumer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FIAPX.Processamento.Infra.MessageBroker
{
    public class MessageBrokerConsumer : IMessageBrokerConsumer
    {
        private readonly IArquivoUseCase _arquivoUseCase;
        private IConnection _connection;
        private IChannel _channel;

        public MessageBrokerConsumer(IArquivoUseCase arquivoUseCase)
        {
            _arquivoUseCase = arquivoUseCase;
        }

        public async Task ReceiveMessageAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync("arquivos-novos", exclusive: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, eventArgs) => {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var arquivo = JsonSerializer.Deserialize<ArquivoDto>(message)!;
                await _arquivoUseCase.ProcessFile(arquivo);
            };

            await _channel.BasicConsumeAsync(queue: "arquivos-novos", autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
        }
    }
}
