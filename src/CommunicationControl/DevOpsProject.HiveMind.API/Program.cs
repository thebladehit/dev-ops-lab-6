using Asp.Versioning;
using Asp.Versioning.Builder;
using DevOpsProject.HiveMind.API.DI;
using DevOpsProject.HiveMind.API.Middleware;
using DevOpsProject.HiveMind.Logic.Services.Interfaces;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Configuration;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Serilog;

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HiveMind - V1", Version = "v1.0" });
});
builder.Services.AddHiveMindLogic();

builder.Services.Configure<HiveCommunicationConfig>(builder.Configuration.GetSection("CommunicationConfiguration"));

var communicationControlTelemetryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
builder.Services.AddHttpClient<HiveMindHttpClient>()
    .AddPolicyHandler(communicationControlTelemetryPolicy);

// register NAMED client for connect request
builder.Services.AddHttpClient("HiveConnectClient");

string corsPolicyName = "HiveMindCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
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

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var hiveMindService = scope.ServiceProvider.GetRequiredService<IHiveMindService>();
        await hiveMindService.ConnectHive();
    }
    catch (Exception ex)
    {
        logger.LogError($"Error occured while connecting Hive to Communication Control. \nException text: {ex.Message}");
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(corsPolicyName);

//app.UseHttpsRedirection();

app.UseAuthorization();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder groupBuilder = app.MapGroup("api/v{apiVersion:apiVersion}").WithApiVersionSet(apiVersionSet);

groupBuilder.MapGet("ping", (IOptionsSnapshot<HiveCommunicationConfig> config) =>
{
    return Results.Ok(new
    {
        Timestamp = DateTime.Now,
        ID = config.Value.HiveID
    });
});

groupBuilder.MapPost("command", (MoveHiveMindCommand command, IHiveMindMovingService hiveMindMovingService) =>
{
    hiveMindMovingService.MoveToLocation(command.Location);
    return Results.Ok();
});

app.Run();
