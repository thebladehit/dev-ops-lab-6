using DevOpsProject.CommunicationControl.API.DI;
using DevOpsProject.CommunicationControl.API.Middleware;
using Microsoft.OpenApi.Models;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext());

        builder.Services.AddApiVersioningConfiguration();

        // TODO: consider this approach
        builder.Services.AddJsonControllerOptionsConfiguration();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommunicationControl - V1", Version = "v1.0" });
        });

        // TODO: LATER - ADD OpenTelemtry

        builder.Services.AddRedis(builder.Configuration);
        builder.Services.AddCommunicationControlLogic();

        builder.Services.AddOptionsConfiguration(builder.Configuration);

        builder.Services.AddHttpClientsConfiguration();

        var corsPolicyName = "AllowReactApp";
        builder.Services.AddCorsConfiguration(corsPolicyName);

        builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(corsPolicyName);

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}