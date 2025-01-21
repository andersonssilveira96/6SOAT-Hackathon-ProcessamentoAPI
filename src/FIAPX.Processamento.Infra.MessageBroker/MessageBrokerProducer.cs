using FIAPX.Processamento.Domain.Producer;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FIAPX.Processamento.Infra.MessageBroker
{
    public class MessageBrokerProducer : IMessageBrokerProducer
    {
        public async Task SendMessageAsync<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            var connection = await factory.CreateConnectionAsync();

            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("arquivos-novos", exclusive: false);

            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(message, options);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: "", routingKey: "arquivos-novos", body: body);
        }
    }
}
