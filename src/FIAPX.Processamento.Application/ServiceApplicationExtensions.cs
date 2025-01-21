using AutoMapper;
using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.UseCase;
using FIAPX.Processamento.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FIAPX.Processamento.Application
{
    public static class ServiceApplicationExtensions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IArquivoUseCase, ArquivoUseCase>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ArquivoDto, Arquivo>().ReverseMap();            
            });

            IMapper mapper = config.CreateMapper();

            services.AddSingleton(mapper);

            return services;
        }
    }
}

