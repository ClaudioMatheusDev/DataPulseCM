# 📘 Como Usar o Exemplo EtlJobWithSteps

## 🎯 Objetivo

Este exemplo mostra como implementar um processo ETL completo que registra todas as etapas no sistema de monitoramento DataPulseCM.

---

## 📋 Pré-requisitos

### 1. **Banco de Dados Atualizado**

Execute o script SQL para adicionar os novos campos:

```sql
USE [DataPulseCM]
GO

-- Adicionar campos de métricas na tabela de detalhes
ALTER TABLE [dbo].[ETL_JobExecutionDetails]
ADD 
    [RowsProcessed] INT NULL,
    [RowsInserted] INT NULL,
    [RowsUpdated] INT NULL,
    [RowsDeleted] INT NULL,
    [RowsFailed] INT NULL,
    [ProgressPercentage] DECIMAL(5,2) NULL,
    [ExecutionDurationMs] AS (DATEDIFF(MILLISECOND, [StartDateTime], [EndDateTime]));
GO
```

### 2. **API Rodando**

Inicie a API do projeto:

```bash
cd src/EtlMonitoring.Api
dotnet run
```

A API deve estar disponível em: `http://localhost:5000` (ou conforme sua configuração)

---

## 🚀 Como Executar o Exemplo

### **Opção 1: Executar como Script**

1. Copie o arquivo `EtlJobWithSteps.cs` para um novo projeto console:

```bash
# Criar projeto de teste
dotnet new console -n TesteETL
cd TesteETL

# Adicionar pacote HttpClient
dotnet add package System.Net.Http.Json

# Copiar o arquivo EtlJobWithSteps.cs para o projeto
# Renomear para Program.cs ou incluir no projeto
```

2. Execute:

```bash
dotnet run
```

---

### **Opção 2: Integrar no seu Código ETL Existente**

Copie apenas as partes relevantes para o seu código:

```csharp
using System.Net.Http.Json;

public class MeuETL
{
    private readonly HttpClient _httpClient;

    public MeuETL()
    {
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:5000") 
        };
    }

    public async Task ExecutarMeuProcesso()
    {
        long executionId = 0;
        long stepId = 0;

        try
        {
            // 1. Iniciar o Job
            var startResult = await _httpClient.PostAsJsonAsync(
                "/api/jobs/start", 
                new { jobName = "MeuJob" }
            );
            var job = await startResult.Content.ReadFromJsonAsync<dynamic>();
            executionId = (long)job.executionId;

            // 2. Iniciar Step Extract
            var stepResult = await _httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/details/start",
                new 
                { 
                    stepName = "Extract",
                    stepOrder = 1,
                    stepMessage = "Iniciando extração..."
                }
            );
            var step = await stepResult.Content.ReadFromJsonAsync<dynamic>();
            stepId = (long)step.detailId;

            // 3. Executar sua lógica aqui
            var dados = MinhaFuncaoDeExtracao();

            // 4. Atualizar progresso (opcional)
            await _httpClient.PutAsJsonAsync(
                $"/api/jobs/details/{stepId}/progress",
                new 
                {
                    rowsProcessed = 5000,
                    progressPercentage = 50.0m,
                    stepMessage = "Processando..."
                }
            );

            // 5. Finalizar Step
            await _httpClient.PostAsJsonAsync(
                $"/api/jobs/details/{stepId}/finish",
                new 
                {
                    stepStatus = "Sucesso",
                    stepMessage = "Concluído!",
                    rowsProcessed = dados.Count
                }
            );

            // 6. Finalizar Job
            await _httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/finish",
                new 
                {
                    status = "Sucesso"
                }
            );
        }
        catch (Exception ex)
        {
            // Registrar falha
            if (stepId > 0)
            {
                await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/details/{stepId}/finish",
                    new { stepStatus = "Falha", stepMessage = ex.Message }
                );
            }
            if (executionId > 0)
            {
                await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/{executionId}/finish",
                    new { status = "Falha", errorMessage = ex.Message }
                );
            }
            throw;
        }
    }

    private List<object> MinhaFuncaoDeExtracao()
    {
        // Sua lógica aqui
        return new List<object>();
    }
}
```

---

## 📊 Fluxo Completo do Exemplo

### **Passo a Passo:**

```
┌─────────────────────────────────────────────────────────────┐
│ 1. INICIAR JOB                                              │
│    POST /api/jobs/start                                     │
│    → Retorna: executionId                                   │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. EXTRACT (Extração)                                       │
│    POST /api/jobs/{id}/details/start                        │
│    → Retorna: detailId                                      │
│                                                             │
│    [Processar dados...]                                     │
│                                                             │
│    PUT /api/jobs/details/{detailId}/progress (tempo real)   │
│    POST /api/jobs/details/{detailId}/finish                 │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. TRANSFORM (Transformação)                                │
│    POST /api/jobs/{id}/details/start                        │
│    → Retorna: detailId                                      │
│                                                             │
│    [Transformar dados...]                                   │
│                                                             │
│    PUT /api/jobs/details/{detailId}/progress                │
│    POST /api/jobs/details/{detailId}/finish                 │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. LOAD (Carga)                                             │
│    POST /api/jobs/{id}/details/start                        │
│    → Retorna: detailId                                      │
│                                                             │
│    [Carregar no destino...]                                 │
│                                                             │
│    PUT /api/jobs/details/{detailId}/progress                │
│    POST /api/jobs/details/{detailId}/finish                 │
│    (com métricas: inserted, updated, failed)                │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. FINALIZAR JOB                                            │
│    POST /api/jobs/{id}/finish                               │
│    → Status: Sucesso ou Falha                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 Endpoints Utilizados

### **1. Iniciar Job**
```http
POST /api/jobs/start
Content-Type: application/json

{
  "jobName": "ImportacaoClientes"
}

Response:
{
  "executionId": 123,
  "jobName": "ImportacaoClientes",
  "message": "Job iniciado com sucesso"
}
```

### **2. Iniciar Step**
```http
POST /api/jobs/123/details/start
Content-Type: application/json

{
  "stepName": "Extract",
  "stepOrder": 1,
  "stepMessage": "Iniciando extração..."
}

Response:
{
  "detailId": 456,
  "executionId": 123,
  "stepName": "Extract"
}
```

### **3. Atualizar Progresso (Tempo Real)**
```http
PUT /api/jobs/details/456/progress
Content-Type: application/json

{
  "rowsProcessed": 5000,
  "progressPercentage": 50.0,
  "stepMessage": "Processando 5000/10000..."
}
```

### **4. Finalizar Step**
```http
POST /api/jobs/details/456/finish
Content-Type: application/json

{
  "stepStatus": "Sucesso",
  "stepMessage": "Extração concluída",
  "rowsProcessed": 10000
}
```

### **5. Finalizar Job**
```http
POST /api/jobs/123/finish
Content-Type: application/json

{
  "status": "Sucesso",
  "errorMessage": null
}
```

---

## 📈 O Que é Registrado

### **Por Job:**
- ✅ Nome do job
- ✅ Horário de início e fim
- ✅ Status final (Sucesso/Falha)
- ✅ Duração total
- ✅ Mensagem de erro (se houver)

### **Por Step (Etapa):**
- ✅ Nome do step (Extract, Transform, Load)
- ✅ Ordem de execução
- ✅ Status (EmExecucao, Sucesso, Falha)
- ✅ Horário de início e fim
- ✅ Duração da etapa
- ✅ Registros processados
- ✅ Registros inseridos/atualizados/deletados
- ✅ Progresso em tempo real (%)
- ✅ Mensagens de log

---

## 🎨 Saída do Exemplo

Quando você executar o exemplo, verá algo assim:

```
╔════════════════════════════════════════════════════════════╗
║   EXEMPLO DE ETL COM MONITORAMENTO DE ETAPAS               ║
╚════════════════════════════════════════════════════════════╝

🌐 API URL: http://localhost:5000

🚀 Iniciando processo ETL: ImportacaoClientes
============================================================
✅ Job iniciado - ExecutionId: 123

📥 STEP 1: EXTRACT
------------------------------------------------------------
Extraindo 10000 registros...
   Progresso: 10.0% (1,000/10,000)
   Progresso: 20.0% (2,000/10,000)
   ...
   Progresso: 100.0% (10,000/10,000)
✅ Extract concluído - 10000 registros extraídos

🔄 STEP 2: TRANSFORM
------------------------------------------------------------
Transformando 10000 registros...
   Progresso: 10.0% (1,000/10,000)
   ...
   Progresso: 100.0% (10,000/10,000)
✅ Transform concluído - 10000 registros transformados

💾 STEP 3: LOAD
------------------------------------------------------------
Carregando 10000 registros...
   Progresso: 10.0% (1,000/10,000)
   ...
   Progresso: 100.0% (10,000/10,000)
✅ Load concluído:
   - Inseridos: 7000
   - Atualizados: 2500
   - Falhas: 500

============================================================
✅ JOB FINALIZADO COM SUCESSO!
   ExecutionId: 123
============================================================
```

---

## 🔧 Personalizações

### **Ajustar URL da API**
```csharp
var etl = new EtlJobWithSteps("http://seu-servidor:porta");
```

### **Adicionar mais steps**
```csharp
// Step 4: Validação
var validationResponse = await _httpClient.PostAsJsonAsync(
    $"/api/jobs/{executionId}/details/start",
    new
    {
        stepName = "Validation",
        stepOrder = 4,
        stepMessage = "Validando dados carregados..."
    });
```

### **Mudar frequência de atualização de progresso**
```csharp
// Atualizar a cada 500 registros em vez de 1000
if (i % 500 == 0 && i > 0)
{
    // Atualizar progresso...
}
```

---

## 🐛 Tratamento de Erros

O exemplo já inclui tratamento completo de erros:

- ✅ Marca o step atual como "Falha"
- ✅ Marca o job principal como "Falha"
- ✅ Registra a mensagem de erro
- ✅ Re-lança a exceção para tratamento adicional

---

## 📚 Próximos Passos

1. ✅ Execute o exemplo para entender o fluxo
2. ✅ Adapte para o seu processo ETL real
3. ✅ Visualize os logs no dashboard
4. ✅ Monitore performance e identifique gargalos

---

## ❓ Dúvidas Comuns

### **A API não está respondendo**
Verifique se está rodando:
```bash
cd src/EtlMonitoring.Api
dotnet run
```

### **Erro de conexão com o banco**
Verifique a connection string em `appsettings.json`

### **Como ver os dados registrados?**
Use os endpoints:
- `GET /api/jobs/{id}` - Ver job específico
- `GET /api/jobs/{id}/details` - Ver todas as etapas
- `GET /api/jobs` - Listar jobs recentes

---

**Pronto para usar! 🚀**
