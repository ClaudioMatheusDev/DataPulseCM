using EtlMonitoring.Core.Core.Interfaces;
using EtlMonitoring.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

builder.Services.AddScoped<IJobExecutionRepository>(
    provider => new JobExecutionRepository(connectionString)
);

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Mapear Health Checks
app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();