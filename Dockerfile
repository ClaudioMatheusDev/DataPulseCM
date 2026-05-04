# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto primeiro para aproveitar cache de camadas
COPY EtlMonitoring.sln .
COPY src/EtlMonitoring.Api/EtlMonitoring.Api.csproj                         src/EtlMonitoring.Api/
COPY src/EtlMonitoring.Core/EtlMonitoring.Core.csproj                       src/EtlMonitoring.Core/
COPY src/EtlMonitoring.Infrastructure/EtlMonitoring.Infrastructure.csproj   src/EtlMonitoring.Infrastructure/

RUN dotnet restore EtlMonitoring.sln

# Copiar restante do código-fonte
COPY src/ src/

RUN dotnet publish src/EtlMonitoring.Api/EtlMonitoring.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Criar usuário não-root para segurança
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Instalar curl para healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Criar diretório de logs com permissões corretas
RUN mkdir -p logs && chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EtlMonitoring.Api.dll"]
