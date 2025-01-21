using FIAPX.Processamento.Domain.Consumer;
using FIAPX.Processamento.Domain.Producer;
using Microsoft.Extensions.DependencyInjection;

namespace FIAPX.Processamento.Infra.MessageBroker
{
    public static class InfraMessageBrokerExtensions
    {
        public static IServiceCollection AddInfraMessageBrokerServices(this IServiceCollection services)
        {
            services.AddScoped<IMessageBrokerConsumer, MessageBrokerConsumer>();
            services.AddScoped<IMessageBrokerProducer, MessageBrokerProducer>();
            return services;
        }
    }
}
