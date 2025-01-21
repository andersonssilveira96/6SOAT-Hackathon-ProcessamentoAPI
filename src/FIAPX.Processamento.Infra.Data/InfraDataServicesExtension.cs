using FIAPX.Processamento.Domain.Interfaces.Repositories;
using FIAPX.Processamento.Infra.Data.Repositories;
using FIAPX.Processamento.Infra.Data.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace FIAPX.Processamento.Infra.Data
{
    public static class InfraDataServicesExtensions
    {
        public static IServiceCollection AddInfraDataServices(this IServiceCollection services)
        {
            services.AddScoped<IArquivoRepository, ArquivoRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}