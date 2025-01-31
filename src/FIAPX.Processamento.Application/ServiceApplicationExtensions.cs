using AutoMapper;
using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.Services;
using FIAPX.Processamento.Application.UseCase;
using FIAPX.Processamento.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FIAPX.Processamento.Application
{
    [ExcludeFromCodeCoverage]
    public static class ServiceApplicationExtensions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddScoped<IArquivoUseCase, ArquivoUseCase>();
            services.AddScoped<IEmailService, EmailService>();

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

