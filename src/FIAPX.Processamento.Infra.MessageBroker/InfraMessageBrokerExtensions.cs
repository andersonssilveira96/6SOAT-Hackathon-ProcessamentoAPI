using FIAPX.Processamento.Domain.Consumer;
using FIAPX.Processamento.Domain.Producer;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FIAPX.Processamento.Infra.MessageBroker
{
    [ExcludeFromCodeCoverage]
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
