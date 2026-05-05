using EtlMonitoring.Core.Core.Interfaces;
using EtlMonitoring.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using EtlMonitoring.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

// Configurar Serilog ANTES de criar o builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "DataPulseCM")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/datapulsecm-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760) // 10MB
    .WriteTo.Seq(
        serverUrl: Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341",
        apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY")) // Configure SEQ_URL e SEQ_API_KEY como variáveis de ambiente
    .CreateLogger();

try
{
    Log.Information("Iniciando aplicação DataPulseCM...");

    var builder = WebApplication.CreateBuilder(args);

    // Adicionar Serilog
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // Exception Handler (registro opcional + middleware explicito no pipeline)
    builder.Services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
    builder.Services.AddProblemDetails();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "DataPulseCM - ETL Monitoring API",
            Version = "v1",
            Description = "API REST para monitoramento centralizado de jobs ETL. Permite rastreamento de execuções, consulta de histórico e análise de falhas.",
            Contact = new OpenApiContact
            {
                Name = "Claudio Matheus",
                Url = new Uri("https://github.com/ClaudioMatheusDev")
            }
        });
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "sql", "sqlserver" }
    );


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
}

// Registrar fábrica de conexão e repositório via DI
builder.Services.AddTransient<System.Func<Microsoft.Data.SqlClient.SqlConnection>>(provider =>
    () => new Microsoft.Data.SqlClient.SqlConnection(connectionString));

builder.Services.AddScoped<IJobExecutionRepository, EtlMonitoring.Infrastructure.Repositories.JobExecutionRepository>();

var app = builder.Build();

// Adicionar Exception Handler no pipeline (usa implementação registrada via AddExceptionHandler)
app.UseExceptionHandler();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Adicionar Request Logging do Serilog
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Mapear Health Checks
// /health/live → apenas verifica se o processo está vivo (sem checar DB) — usado pelo docker healthcheck
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // não executa nenhum check, retorna sempre 200
});
// /health/ready → checa todas as dependências (DB, etc.)
app.MapHealthChecks("/health/ready");
// /health → compatibilidade retroativa
app.MapHealthChecks("/health");

app.UseAuthorization();
app.MapControllers();

    Log.Information("Aplicação DataPulseCM iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}