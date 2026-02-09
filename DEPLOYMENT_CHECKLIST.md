# ✅ CHECKLIST DE DEPLOYMENT - DataPulseCM

## 📋 Checklist de Implantação

### 🗄️ **1. BANCO DE DADOS**

- [ ] SQL Server instalado e acessível
- [ ] Connection string configurada em `appsettings.json`
- [ ] Script 01 executado: `01-create-tables.sql` ✅
- [ ] Script 02 executado: `02-create-stored-procedures.sql` ✅
- [ ] Script 03 executado: `03-seed-data.sql` (opcional)
- [ ] **Script 04 executado: `04-enrich-log-fields.sql` ⭐ NOVO**
- [ ] Tabelas criadas com sucesso:
  - [ ] `ETL_JobExecutionLog`
  - [ ] `ETL_JobExecutionDetails`
- [ ] Views criadas:
  - [ ] `vw_ETL_LatestExecutions`
  - [ ] `vw_ETL_ExecutionMetrics`
- [ ] Índices criados e funcionando
- [ ] Permissions configuradas para usuário da aplicação

---

### 🔧 **2. CONFIGURAÇÃO DA API**

- [ ] .NET 9.0 SDK instalado
- [ ] Pacotes NuGet restaurados (`dotnet restore`)
- [ ] Build bem-sucedido (`dotnet build`)
- [ ] `appsettings.json` configurado:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=...;Database=DataPulseCM;..."
    }
  }
  ```
- [ ] `appsettings.Development.json` para ambiente de dev
- [ ] `appsettings.Production.json` para produção

---

### 📝 **3. SERILOG (LOGGING)**

- [ ] Pacotes Serilog instalados ✅
- [ ] Pasta `logs/` criada (ou permissão para criar)
- [ ] Seq configurado (opcional, mas recomendado):
  - [ ] Docker Seq rodando: `docker ps | grep seq`
  - [ ] Acessível em `http://localhost:5341`
  - [ ] API Key gerada em Seq
  - [ ] API Key configurada em `Program.cs`
- [ ] Teste de log funcionando:
  ```bash
  dotnet run
  # Verificar logs aparecendo em Console, File e Seq
  ```

---

### 🛡️ **4. VALIDAÇÃO E SEGURANÇA**

- [ ] FluentValidation configurado ✅
- [ ] Global Exception Handler ativo ✅
- [ ] Validadores testados:
  - [ ] StartJobRequest
  - [ ] FinishJobRequest
  - [ ] JobExecutionFiltrosDto
- [ ] CORS configurado adequadamente
- [ ] HTTPS habilitado (produção)
- [ ] Rate limiting configurado (futuro)
- [ ] Autenticação JWT (futuro)

---

### 🧪 **5. TESTES**

- [ ] Health Check funcionando: `GET /health`
- [ ] Swagger acessível: `http://localhost:5000/swagger`
- [ ] Testar endpoint: `POST /api/jobs/start`
- [ ] Testar endpoint: `POST /api/jobs/{id}/finish`
- [ ] Testar Steps: `POST /api/jobs/{id}/details/start`
- [ ] Testar validações (requests inválidos devem retornar 400)
- [ ] Testar exception handling (forçar erro, verificar log)
- [ ] Verificar logs no Seq
- [ ] Executar exemplos do arquivo `examples/api-requests.http`
- [ ] Executar job completo com steps
- [ ] Verificar dados no banco

---

### 🚀 **6. DEPLOYMENT PRODUÇÃO**

#### 6.1 - Checklist Pré-Deploy
- [ ] Testes em ambiente de staging concluídos
- [ ] Backup do banco de dados atual
- [ ] Scripts de rollback preparados
- [ ] Documentação atualizada
- [ ] Variáveis de ambiente configuradas

#### 6.2 - Publicar Aplicação
```bash
# Build para produção
dotnet publish -c Release -o ./publish

# Verificar arquivos
ls ./publish
```

#### 6.3 - IIS (Windows Server)
- [ ] IIS instalado
- [ ] .NET 9.0 Hosting Bundle instalado
- [ ] Pool de aplicações criado (.NET CLR Version: No Managed Code)
- [ ] Site criado apontando para pasta publish
- [ ] Binding configurado (porta 80/443)
- [ ] Certificado SSL instalado
- [ ] Permissions de pasta configuradas
- [ ] Connection string em `appsettings.Production.json`
- [ ] Testar acesso: `https://seu-dominio.com/health`

#### 6.4 - Linux (Systemd)
```bash
# Copiar arquivos
scp -r ./publish user@server:/var/www/datapulsecm

# Criar service
sudo nano /etc/systemd/system/datapulsecm.service

# Conteúdo:
[Unit]
Description=DataPulseCM ETL Monitoring API

[Service]
WorkingDirectory=/var/www/datapulsecm
ExecStart=/usr/bin/dotnet /var/www/datapulsecm/EtlMonitoring.Api.dll
Restart=always
RestartSec=10
SyslogIdentifier=datapulsecm
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target

# Ativar e iniciar
sudo systemctl enable datapulsecm
sudo systemctl start datapulsecm
sudo systemctl status datapulsecm
```

#### 6.5 - Docker
```dockerfile
# Criar Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY ./publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "EtlMonitoring.Api.dll"]
```

```bash
# Build image
docker build -t datapulsecm:latest .

# Run container
docker run -d \
  --name datapulsecm \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="Server=..." \
  datapulsecm:latest

# Verificar
docker logs datapulsecm
```

---

### 📊 **7. MONITORAMENTO PÓS-DEPLOY**

- [ ] Logs sendo gerados corretamente
- [ ] Seq recebendo logs (se configurado)
- [ ] Health check retornando 200 OK
- [ ] Dashboard Seq configurado
- [ ] Alertas configurados:
  - [ ] Jobs falhados
  - [ ] Jobs lentos (> 5 min)
  - [ ] Erros críticos
- [ ] Métricas sendo coletadas
- [ ] Backup de logs configurado
- [ ] Rotação de logs funcionando (30 dias)

---

### 📚 **8. DOCUMENTAÇÃO**

- [ ] README.md atualizado ✅
- [ ] LOGGING_GUIDE.md criado ✅
- [ ] SEQ_GUIDE.md criado ✅
- [ ] IMPLEMENTATION_SUMMARY.md criado ✅
- [ ] Exemplos em `examples/` criados ✅
- [ ] Swagger documentado
- [ ] Runbook para operações criado
- [ ] Diagrama de arquitetura disponível ✅

---

### 🔄 **9. INTEGRAÇÃO COM JOBS ETL**

#### 9.1 - Criar Cliente SDK
- [ ] Copiar `examples/EtlJobClient.cs` para projeto ETL
- [ ] Configurar URL da API
- [ ] Implementar tratamento de erro
- [ ] Implementar retry logic

#### 9.2 - Atualizar Jobs Existentes
```csharp
// Antes
public void ExecutarETL()
{
    // processar dados...
}

// Depois
public async Task ExecutarETL()
{
    var client = new EtlJobClient("http://api-url");
    var executionId = await client.IniciarJobAsync("MeuJob");
    
    try
    {
        // Extract
        var extractId = await client.IniciarStepAsync(executionId, "Extract", 1);
        // ... processar ...
        await client.FinalizarStepAsync(extractId, "Sucesso");
        
        // Transform
        var transformId = await client.IniciarStepAsync(executionId, "Transform", 2);
        // ... processar ...
        await client.FinalizarStepAsync(transformId, "Sucesso");
        
        // Load
        var loadId = await client.IniciarStepAsync(executionId, "Load", 3);
        // ... processar ...
        await client.FinalizarStepAsync(loadId, "Sucesso");
        
        await client.FinalizarJobAsync(executionId, "Sucesso");
    }
    catch (Exception ex)
    {
        await client.FinalizarJobAsync(executionId, "Falha", ex.Message);
        throw;
    }
}
```

---

### 🎯 **10. PRÓXIMOS PASSOS (ROADMAP)**

#### Fase 2 - Observabilidade Avançada
- [ ] OpenTelemetry para tracing distribuído
- [ ] Application Insights (Azure)
- [ ] Prometheus + Grafana
- [ ] Métricas customizadas

#### Fase 3 - Resiliência
- [ ] Polly (Retry + Circuit Breaker)
- [ ] Rate Limiting
- [ ] Timeout policies
- [ ] Dead letter queue

#### Fase 4 - Segurança
- [ ] JWT Authentication
- [ ] API Keys por job
- [ ] Audit trail completo
- [ ] Encryption at rest

#### Fase 5 - UI/UX
- [ ] Dashboard Web (React/Blazor)
- [ ] Visualizações em tempo real
- [ ] Notificações (Email, Slack, Teams)
- [ ] Mobile app

---

## ✅ VALIDAÇÃO FINAL

### Teste Completo End-to-End

```bash
# 1. Iniciar API
cd src/EtlMonitoring.Api
dotnet run

# 2. Abrir nova janela de terminal
# 3. Executar requests de teste
# Use VSCode REST Client ou Postman com examples/api-requests.http

# 4. Verificar Seq
# Abrir http://localhost:5341
# Buscar logs com: Application = "DataPulseCM"

# 5. Consultar banco de dados
# Verificar registros em ETL_JobExecutionLog e ETL_JobExecutionDetails
```

### Critérios de Sucesso
- ✅ Build sem erros
- ✅ Health check retorna 200
- ✅ Logs aparecem no Console, File e Seq
- ✅ Job criado com sucesso via API
- ✅ Steps registrados no banco
- ✅ Validações funcionando (400 para requests inválidos)
- ✅ Exceptions tratadas globalmente
- ✅ Dados persistidos no SQL Server

---

## 🎉 DEPLOYMENT CONCLUÍDO!

Se todos os itens acima estão marcados ✅, seu sistema está pronto para produção!

**Sistema**: DataPulseCM - ETL Monitoring Dashboard  
**Versão**: 1.0.0 (Fase 1 - Logging Profissional)  
**Status**: ✅ PRODUÇÃO READY  
**Data**: 09/02/2026  

---

**Documentação completa em:**
- [README.md](README.md)
- [LOGGING_GUIDE.md](LOGGING_GUIDE.md)
- [SEQ_GUIDE.md](SEQ_GUIDE.md)
- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)
