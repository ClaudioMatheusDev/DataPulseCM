---
sidebar_position: 2
title: Referência Completa
---

# 📖 Referência Completa da API

Documentação detalhada de todos os endpoints da API DataPulseCM.

## 📊 Jobs - Consultas

### GET /api/jobs

Lista execuções recentes de jobs.

**Parâmetros de Query:**

| Parâmetro | Tipo | Obrigatório | Padrão | Descrição |
|-----------|------|-------------|--------|-----------|
| `limit` | int | Não | 50 | Quantidade de registros |

**Exemplo de Requisição:**

```http
GET /api/jobs?limit=100
```

**Resposta de Sucesso (200):**

```json
{
  "data": [
    {
      "executionID": 37,
      "jobName": "ETL_ImportarVendas",
      "status": "Sucesso",
      "startDate": "2026-02-09T10:30:00",
      "endDate": "2026-02-09T10:35:00",
      "duration": 300,
      "recordsProcessed": 15000,
      "errorMessage": null
    }
  ],
  "count": 1
}
```

---

### GET /api/jobs/{id}

Busca uma execução específica por ID.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | long | ID da execução |

**Exemplo de Requisição:**

```http
GET /api/jobs/37
```

**Resposta de Sucesso (200):**

```json
{
  "executionID": 37,
  "jobName": "ETL_ImportarVendas",
  "status": "Sucesso",
  "startDate": "2026-02-09T10:30:00",
  "endDate": "2026-02-09T10:35:00",
  "duration": 300,
  "recordsProcessed": 15000,
  "errorMessage": null
}
```

**Resposta de Erro (404):**

```json
{
  "message": "Execução com ID 999 não encontrada"
}
```

---

### GET /api/jobs/filter

Filtra execuções com múltiplos critérios.

**Parâmetros de Query:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `jobName` | string | Não | Nome do job |
| `status` | string | Não | Status (Sucesso/Falha) |
| `startDate` | datetime | Não | Data inicial |
| `endDate` | datetime | Não | Data final |

**Exemplo de Requisição:**

```http
GET /api/jobs/filter?jobName=ETL_Vendas&status=Sucesso&startDate=2026-02-01&endDate=2026-02-09
```

**Resposta de Sucesso (200):**

```json
{
  "data": [...],
  "count": 25
}
```

---

## 🎯 Jobs - Ações

### POST /api/jobs/start

Inicia uma nova execução de job.

**Body (JSON):**

```json
{
  "jobName": "ETL_ImportarVendas"
}
```

**Validações:**
- `jobName` é obrigatório
- `jobName` não pode ser vazio

**Exemplo de Requisição:**

```http
POST /api/jobs/start
Content-Type: application/json

{
  "jobName": "ETL_ImportarVendas"
}
```

**Resposta de Sucesso (200):**

```json
{
  "executionId": 123,
  "jobName": "ETL_ImportarVendas",
  "message": "Job iniciado com sucesso"
}
```

**Resposta de Erro (400):**

```json
{
  "message": "Nome do job é obrigatório"
}
```

---

### POST /api/jobs/{id}/finish

Finaliza uma execução de job.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | long | ID da execução |

**Body (JSON):**

```json
{
  "status": "Sucesso",
  "errorMessage": null
}
```

**Valores válidos para status:**
- `"Sucesso"`
- `"Falha"`
- `"Em Execução"`

**Exemplo de Requisição (Sucesso):**

```http
POST /api/jobs/123/finish
Content-Type: application/json

{
  "status": "Sucesso",
  "errorMessage": null
}
```

**Exemplo de Requisição (Falha):**

```http
POST /api/jobs/123/finish
Content-Type: application/json

{
  "status": "Falha",
  "errorMessage": "Timeout na conexão com API externa"
}
```

**Resposta de Sucesso (200):**

```json
{
  "executionId": 123,
  "message": "Job finalizado com sucesso"
}
```

---

## 📈 Estatísticas

### GET /api/jobs/statistics

Retorna estatísticas gerais de execuções.

**Parâmetros de Query (Opcionais):**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `startDate` | datetime | Data inicial do período |
| `endDate` | datetime | Data final do período |

**Exemplo de Requisição:**

```http
GET /api/jobs/statistics
```

**Resposta de Sucesso (200):**

```json
{
  "total": 1500,
  "successful": 1380,
  "failed": 120,
  "successRate": 92.0,
  "byStatus": {
    "Sucesso": 1380,
    "Falha": 120
  },
  "period": {
    "startDate": null,
    "endDate": null
  }
}
```

---

### GET /api/jobs/failed

Lista execuções com falha.

**Parâmetros de Query:**

| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|---------|-----------|
| `limit` | int | 20 | Quantidade de registros |

**Exemplo de Requisição:**

```http
GET /api/jobs/failed?limit=50
```

**Resposta de Sucesso (200):**

```json
{
  "data": [
    {
      "executionID": 35,
      "jobName": "ETL_ImportarFornecedores",
      "status": "Falha",
      "startDate": "2026-02-09T09:00:00",
      "endDate": "2026-02-09T09:05:00",
      "duration": 300,
      "recordsProcessed": 0,
      "errorMessage": "Conexão recusada pelo servidor"
    }
  ],
  "count": 1
}
```

---

## 🔍 Jobs Específicos

### GET /api/jobs/by-name/{jobName}

Retorna a última execução de um job específico.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `jobName` | string | Nome do job |

**Exemplo de Requisição:**

```http
GET /api/jobs/by-name/ETL_ImportarVendas
```

**Resposta de Sucesso (200):**

```json
{
  "executionID": 123,
  "jobName": "ETL_ImportarVendas",
  "status": "Sucesso",
  ...
}
```

**Resposta de Erro (404):**

```json
{
  "message": "Nenhuma execução encontrada para o job 'ETL_NaoExiste'"
}
```

---

### GET /api/jobs/by-name/{jobName}/history

Retorna histórico de execuções de um job.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `jobName` | string | Nome do job |

**Parâmetros de Query:**

| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|---------|-----------|
| `limit` | int | 50 | Quantidade de registros |

**Exemplo de Requisição:**

```http
GET /api/jobs/by-name/ETL_ImportarVendas/history?limit=100
```

**Resposta de Sucesso (200):**

```json
{
  "jobName": "ETL_ImportarVendas",
  "data": [...],
  "count": 85
}
```

---

### GET /api/jobs/by-name/{jobName}/success-rate

Calcula taxa de sucesso de um job.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `jobName` | string | Nome do job |

**Parâmetros de Query (Opcionais):**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `startDate` | datetime | Data inicial |
| `endDate` | datetime | Data final |

**Exemplo de Requisição:**

```http
GET /api/jobs/by-name/ETL_ImportarVendas/success-rate
```

**Resposta de Sucesso (200):**

```json
{
  "jobName": "ETL_ImportarVendas",
  "successRate": 95.5,
  "period": {
    "startDate": null,
    "endDate": null
  }
}
```

---

## 📝 Detalhes de Execução (Steps)

### GET /api/jobs/{id}/details

Lista todos os steps de uma execução.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | long | ID da execução |

**Exemplo de Requisição:**

```http
GET /api/jobs/123/details
```

**Resposta de Sucesso (200):**

```json
{
  "executionId": 123,
  "steps": [
    {
      "detailId": 1,
      "executionId": 123,
      "stepName": "Extrair dados da API",
      "stepOrder": 1,
      "status": "Sucesso",
      "startTime": "2026-02-09T10:30:00",
      "endTime": "2026-02-09T10:31:00",
      "recordsProcessed": 5000,
      "errorMessage": null
    },
    {
      "detailId": 2,
      "executionId": 123,
      "stepName": "Transformar dados",
      "stepOrder": 2,
      "status": "Sucesso",
      "startTime": "2026-02-09T10:31:00",
      "endTime": "2026-02-09T10:33:00",
      "recordsProcessed": 5000,
      "errorMessage": null
    }
  ],
  "count": 2
}
```

---

### POST /api/jobs/{id}/details/start

Inicia um step de execução.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | long | ID da execução |

**Body (JSON):**

```json
{
  "stepName": "Extrair dados da API",
  "stepOrder": 1,
  "stepMessage": "Iniciando extração"
}
```

**Exemplo de Requisição:**

```http
POST /api/jobs/123/details/start
Content-Type: application/json

{
  "stepName": "Extrair dados da API",
  "stepOrder": 1,
  "stepMessage": "Conectando na API..."
}
```

**Resposta de Sucesso (200):**

```json
{
  "detailId": 456,
  "executionId": 123,
  "stepName": "Extrair dados da API",
  "message": "Step iniciado com sucesso"
}
```

---

### POST /api/jobs/details/{detailId}/finish

Finaliza um step de execução.

**Parâmetros de URL:**

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `detailId` | long | ID do step |

**Body (JSON):**

```json
{
  "stepStatus": "Sucesso",
  "stepMessage": "5000 registros extraídos"
}
```

**Exemplo de Requisição:**

```http
POST /api/jobs/details/456/finish
Content-Type: application/json

{
  "stepStatus": "Sucesso",
  "stepMessage": "5000 registros extraídos com sucesso"
}
```

**Resposta de Sucesso (200):**

```json
{
  "detailId": 456,
  "message": "Step finalizado com sucesso"
}
```

---

## ❤️ Health Check

### GET /health

Verifica saúde da API e banco de dados.

**Exemplo de Requisição:**

```http
GET /health
```

**Resposta de Sucesso (200):**

```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy"
  }
}
```

**Resposta de Erro (503):**

```json
{
  "status": "Unhealthy",
  "checks": {
    "database": "Unhealthy"
  }
}
```

---

## 💡 Dicas de Uso

### Ordem Correta dos Chamados

```
1. POST /api/jobs/start → Recebe executionId
2. POST /api/jobs/{executionId}/details/start → Recebe detailId (para cada step)
3. POST /api/jobs/details/{detailId}/finish → Finaliza cada step
4. POST /api/jobs/{executionId}/finish → Finaliza execução
```

### Tratamento de Erros

Sempre verifique o `status` HTTP e o campo `message` nas respostas.

### Performance

- Use o parâmetro `limit` para controlar quantidade de dados
- Implemente cache no cliente para consultas frequentes
- Considere paginação para datasets grandes

---

## 📚 Próximos Passos

- [Exemplos Práticos](./examples)
- [Integração com Jobs](./integration)
- [Best Practices](./best-practices)
