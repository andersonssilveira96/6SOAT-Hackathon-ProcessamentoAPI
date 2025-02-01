using FIAPX.Processamento.Api.Middleware;
using FIAPX.Processamento.Application;
using FIAPX.Processamento.Infra.Data;
using FIAPX.Processamento.Infra.MessageBroker;
using FIAPX.Processamento.Infra.Data.Context;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using FIAPX.Processamento.Domain.Consumer;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Amazon.Extensions.NETCore.Setup;
using Amazon;
using SendGrid;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 524288000; });

builder.WebHost.ConfigureKestrel(options => {  options.Limits.MaxRequestBodySize = 524288000; });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationService();
builder.Services.AddInfraDataServices();
builder.Services.AddInfraMessageBrokerServices();

builder.Services.AddTransient<UnitOfWorkMiddleware>();
builder.Services.AddDbContext<FIAPXContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    Region = RegionEndpoint.USEast1
});
// Adiciona a configuração do SendGridClient
builder.Services.AddSingleton<ISendGridClient>(provider =>
    new SendGridClient(builder.Configuration["SendGridApiKey"]));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Invocar o serviço
using var scope = app.Services.CreateScope();
var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageBrokerConsumer>();
_ = Task.Run(() => messageConsumer.ReceiveMessageAsync());

app.UseMiddleware<UnitOfWorkMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
