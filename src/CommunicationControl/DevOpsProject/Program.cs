using Asp.Versioning;
using DevOpsProject.CommunicationControl.API.DI;
using DevOpsProject.CommunicationControl.API.Middleware;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Configuration;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // TODO: consider this approach
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommunicationControl - V1", Version = "v1.0" });
        });

        // TODO: LATER - ADD OpenTelemtry

        builder.Services.AddRedis(builder.Configuration);
        builder.Services.AddCommunicationControlLogic();

        builder.Services.Configure<OperationalAreaConfig>(builder.Configuration.GetSection("OperationalArea"));
        builder.Services.Configure<ComControlCommunicationConfiguration>(builder.Configuration.GetSection("CommunicationConfiguration"));
        
        var hiveRetryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        builder.Services.AddHttpClient<CommunicationControlHttpClient>()
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