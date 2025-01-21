namespace FIAPX.Processamento.Domain.Consumer
{
    public interface IMessageBrokerConsumer
    {
        public Task ReceiveMessageAsync();
    }
}
