using DevOpsProject.CommunicationControl.API.DI;
using DevOpsProject.CommunicationControl.API.Middleware;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
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

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // TODO: LATER - ADD OpenTelemtry

        builder.Services.AddRedis(builder.Configuration);
        builder.Services.AddCommunicationControlLogic();

        builder.Services.Configure<OperationalAreaConfig>(builder.Configuration.GetSection("OperationalArea"));
        builder.Services.AddSingleton<IOptionsMonitor<OperationalAreaConfig>, OptionsMonitor<OperationalAreaConfig>>();

        
        var hiveRetryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        builder.Services.AddHttpClient<HiveHttpClient>()
            .AddPolicyHandler(hiveRetryPolicy);


        var corsPolicyName = "AllowReactApp";
        var localCorsPolicyName = "AllowLocalHtml";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicyName,
                policy =>
                {
                    policy.AllowAnyOrigin() //SECURITY WARNING ! Never allow all origins
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });

            options.AddPolicy(name: localCorsPolicyName,
                policy =>
                {
                    policy.AllowAnyOrigin() //SECURITY WARNING ! Never allow all origins
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

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
        //app.UseCors(localCorsPolicyName);

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}