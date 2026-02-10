# 🚀 Guia de Uso - Sistema de Logging Profissional

## 📦 O que foi implementado

### ✅ 1. Serilog com Logging Estruturado
- Logs estruturados com contexto rico
- Múltiplos sinks: Console, File, Seq (opcional)
- Enrichers: MachineName, ThreadId, ProcessId, Environment
- Request logging automático com métricas de performance
- Logs rotativos diários com retenção de 30 dias

### ✅ 2. Global Exception Handler
- Middleware para captura global de exceções
- RFC 7807 Problem Details
- TraceId para rastreamento distribuído
- Logging automático de erros

### ✅ 3. FluentValidation
- Validação automática de requests
- Mensagens de erro customizadas
- Validators para StartJobRequest, FinishJobRequest e filtros

### ✅ 4. Tabela ETL_JobExecutionDetails
- Rastreamento de steps individuais do ETL
- Status por step (Extract, Transform, Load, etc.)
- Tempo de execução por step
- Mensagens de log por step

### ✅ 5. Campos Enriquecidos de Log
- **Source/Destination**: Origem e destino dos dados
- **RecordsExpected/RecordsActual**: Validação de completude
- **DataQualityScore**: Score de qualidade dos dados
- **RetryCount**: Contagem de retentativas
- **ParentExecutionId**: Jobs dependentes/hierárquicos
- **ExecutionContext**: JSON com contexto adicional
- **MachineName/ProcessId/ThreadId**: Identificação de execução
- **MemoryUsageMB**: Uso de memória
- **Tags**: Categorização flexível
- **CorrelationId**: Rastreamento distribuído
- **UserName**: Identificação do usuário

---

## 🔧 Configuração Inicial

### 1. Executar Scripts SQL

```bash
# Na ordem:
1. database/scripts/01-create-tables.sql
2. database/scripts/02-create-stored-procedures.sql
3. database/scripts/03-seed-data.sql
4. database/scripts/04-enrich-log-fields.sql  # NOVO!
```

### 2. Configurar Seq (Opcional - Recomendado)

```bash
# Docker
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

# Acessar: http://localhost:5341
```

### 3. Configurar API Key do Seq

Edite `Program.cs` e substitua:
```csharp
.WriteTo.Seq("http://localhost:5341", apiKey: "your-seq-api-key-here")
```

---

## 📝 Como Usar - Exemplos Práticos

### Exemplo 1: Job Simples com Logging

```http
### 1. Iniciar Job
POST http://localhost:5000/api/jobs/start
Content-Type: application/json

{
  "jobName": "ImportarClientes"
}

### Response
{
  "executionId": 123,
  "jobName": "ImportarClientes",
  "message": "Job iniciado com sucesso"
}

### 2. Finalizar Job
POST http://localhost:5000/api/jobs/123/finish
Content-Type: application/json

{
  "status": "Sucesso",
  "errorMessage": null
}
```

### Exemplo 2: Job com Steps Detalhados (RECOMENDADO!)

```http
### 1. Iniciar Job Principal
POST http://localhost:5000/api/jobs/start
Content-Type: application/json

{
  "jobName": "ETL_Vendas_Completo"
}

# Response: executionId = 456

### 2. Step 1 - Extract
POST http://localhost:5000/api/jobs/456/details/start
Content-Type: application/json

{
  "executionId": 456,
  "stepName": "Extract - SQL Server Origem",
  "stepOrder": 1,
  "stepMessage": "Iniciando extração de 100k registros"
}

# Response: detailId = 1

### 3. Finalizar Step 1
POST http://localhost:5000/api/jobs/details/1/finish
Content-Type: application/json

{
  "stepStatus": "Sucesso",
  "stepMessage": "Extraídos 100.523 registros em 45s"
}

### 4. Step 2 - Transform
POST http://localhost:5000/api/jobs/456/details/start
Content-Type: application/json

{
  "executionId": 456,
  "stepName": "Transform - Limpeza e Validação",
  "stepOrder": 2,
  "stepMessage": "Aplicando regras de negócio"
}

# Response: detailId = 2

### 5. Finalizar Step 2
POST http://localhost:5000/api/jobs/details/2/finish
Content-Type: application/json

{
  "stepStatus": "Sucesso",
  "stepMessage": "98.520 registros válidos, 2.003 rejeitados"
}

### 6. Step 3 - Load
POST http://localhost:5000/api/jobs/456/details/start
Content-Type: application/json

{
  "executionId": 456,
  "stepName": "Load - Inserção no DW",
  "stepOrder": 3,
  "stepMessage": "Carregando dados no Data Warehouse"
}

# Response: detailId = 3

### 7. Finalizar Step 3
POST http://localhost:5000/api/jobs/details/3/finish
Content-Type: application/json

{
  "stepStatus": "Sucesso",
  "stepMessage": "98.520 registros inseridos com sucesso"
}

### 8. Finalizar Job Principal
POST http://localhost:5000/api/jobs/456/finish
Content-Type: application/json

{
  "status": "Sucesso"
}

### 9. Consultar Todos os Steps
GET http://localhost:5000/api/jobs/456/details
```

### Exemplo 3: Job com Falha e Retry

```http
### Tentativa 1 - Falhou
POST http://localhost:5000/api/jobs/start
{
  "jobName": "ImportarProdutos"
}

POST http://localhost:5000/api/jobs/789/finish
{
  "status": "Falha",
  "errorMessage": "Timeout ao conectar no servidor de origem"
}

### Tentativa 2 - Sucesso (Retry)
POST http://localhost:5000/api/jobs/start
{
  "jobName": "ImportarProdutos"
}
# Este job terá RetryCount = 1 se implementado no código
```

---

## 📊 Endpoints Disponíveis

### Jobs Principais
```
GET    /api/jobs                    # Lista jobs recentes
GET    /api/jobs/{id}               # Detalhes de um job
GET    /api/jobs/filter             # Filtrar jobs
POST   /api/jobs/start              # Iniciar job
POST   /api/jobs/{id}/finish        # Finalizar job
GET    /api/jobs/{id}/details       # Ver steps do job
```

### Steps (Detalhes)
```
POST   /api/jobs/{id}/details/start      # Iniciar step
POST   /api/jobs/details/{detailId}/finish  # Finalizar step
```

### Estatísticas
```
GET    /api/jobs/statistics         # Estatísticas gerais
GET    /api/jobs/failed             # Jobs falhados
GET    /api/jobs/by-name/{name}     # Último job por nome
GET    /api/jobs/by-name/{name}/history    # Histórico
GET    /api/jobs/by-name/{name}/success-rate # Taxa de sucesso
```

### Health Check
```
GET    /health                      # Status da API e banco
```

---

## 📁 Estrutura de Logs

### Logs em Arquivo
```
logs/
  datapulsecm-20260209.log
  datapulsecm-20260208.log
  ...
```

### Formato do Log
```
2026-02-09 14:23:45.123 -03:00 [INF] [EtlMonitor.Api.Controllers.JobsController] Job ImportarClientes iniciado | ExecutionId: 123 {"Application":"DataPulseCM","Environment":"Development","MachineName":"DEV-SERVER","ThreadId":12,"ProcessId":4512}
```

---

## 🔍 Monitoramento com Seq

Acesse http://localhost:5341 para:
- Pesquisar logs em tempo real
- Filtrar por propriedades estruturadas
- Criar dashboards customizados
- Configurar alertas
- Análise de performance

### Queries Úteis no Seq
```
# Jobs falhados hoje
Status = "Falha" AND @Timestamp > Now()-1d

# Jobs lentos (>5min)
DurationInSeconds > 300

# Erros por job
SELECT JobName, COUNT(*) FROM stream WHERE Status = "Falha" GROUP BY JobName
```

---

## 🎯 Boas práticas

### 1. Sempre use Steps para ETL complexos
```
Extract → Transform → Load → Validate → Cleanup
```

### 2. Capture métricas importantes
```csharp
- RowsProcessed
- RowsInserted
- RowsUpdated
- RowsDeleted
- RecordsExpected vs RecordsActual
```

### 3. Use Tags para categorização
```
Tags: "daily,production,critical"
Tags: "hourly,staging,experimental"
```

### 4. Implemente CorrelationId
Para rastrear jobs relacionados em um pipeline

### 5. Defina DataQualityScore
Score de 0-100 baseado em regras de qualidade

---

## 🚨 Troubleshooting

### Logs não aparecem no arquivo
- Verifique permissões da pasta `logs/`
- Crie a pasta manualmente se necessário

### Erro ao conectar no Seq
- Verifique se container Docker está rodando
- Comente a linha `.WriteTo.Seq()` se não quiser usar

### Validação falhando
- Verifique se o JobName contém apenas: `a-z A-Z 0-9 _ - .`
- Status deve ser: `Sucesso`, `Falha`, `Parcial`, `Cancelado`

---

## 📚 Próximos Passos Recomendados

1. **Implementar Polly para Retry**
2. **Adicionar Application Insights** (Azure)
3. **Criar Dashboard Web** (React/Blazor)
4. **Implementar Notificações** (Email/Slack)
5. **Adicionar Rate Limiting**
6. **Implementar Autenticação JWT**

---

**Desenvolvido com ❤️ por Claudio Matheus**
