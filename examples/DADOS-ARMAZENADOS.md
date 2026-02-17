# 📊 DADOS ARMAZENADOS NO BANCO

## 📋 Tabela: ETL_JobExecutionLog

Quando você executa o exemplo, esta tabela registra o **JOB PRINCIPAL**:

```
ExecutionId | JobName                | StartDateTime        | EndDateTime          | Status   | RowsProcessed | ExecutionDurationMs
------------|------------------------|----------------------|----------------------|----------|---------------|--------------------
123         | ImportacaoClientes     | 2026-02-17 10:00:00  | 2026-02-17 10:05:30  | Sucesso  | 10000         | 330000
124         | ImportacaoProdutos     | 2026-02-17 10:10:00  | 2026-02-17 10:12:00  | Falha    | 5000          | 120000
125         | TesteETL_PowerShell    | 2026-02-17 10:15:00  | 2026-02-17 10:15:15  | Sucesso  | 9500          | 15000
```

---

## 📋 Tabela: ETL_JobExecutionDetails

E esta tabela registra **CADA ETAPA** do job:

### Exemplo: ExecutionId = 123 (ImportacaoClientes)

```
DetailId | ExecutionId | StepName  | StepOrder | StepStatus | StartDateTime        | EndDateTime          | RowsProcessed | RowsInserted | RowsUpdated | ProgressPercentage | ExecutionDurationMs
---------|-------------|-----------|-----------|------------|----------------------|----------------------|---------------|--------------|-------------|--------------------|--------------------
456      | 123         | Extract   | 1         | Sucesso    | 2026-02-17 10:00:00  | 2026-02-17 10:02:00  | 10000         | NULL         | NULL        | 100.00             | 120000
457      | 123         | Transform | 2         | Sucesso    | 2026-02-17 10:02:00  | 2026-02-17 10:04:00  | 10000         | NULL         | NULL        | 100.00             | 120000
458      | 123         | Load      | 3         | Sucesso    | 2026-02-17 10:04:00  | 2026-02-17 10:05:30  | 10000         | 7000         | 2500        | 100.00             | 90000
```

### Exemplo: ExecutionId = 124 (ImportacaoProdutos - COM FALHA)

```
DetailId | ExecutionId | StepName  | StepOrder | StepStatus | StartDateTime        | EndDateTime          | RowsProcessed | StepMessage
---------|-------------|-----------|-----------|------------|----------------------|----------------------|---------------|----------------------------------
459      | 124         | Extract   | 1         | Sucesso    | 2026-02-17 10:10:00  | 2026-02-17 10:11:00  | 5000          | Extração concluída
460      | 124         | Transform | 2         | Falha      | 2026-02-17 10:11:00  | 2026-02-17 10:12:00  | 2500          | Erro: Formato de data inválido
```

**Nota:** Quando Transform falha, o Load nunca é executado!

---

## 📊 Consultas Úteis

### Ver todos os steps de um job específico:
```sql
SELECT 
    StepName,
    StepOrder,
    StepStatus,
    RowsProcessed,
    RowsInserted,
    RowsUpdated,
    DATEDIFF(SECOND, StartDateTime, EndDateTime) AS DurationSeconds,
    StepMessage
FROM ETL_JobExecutionDetails
WHERE ExecutionId = 123
ORDER BY StepOrder
```

**Resultado:**
```
StepName  | StepOrder | StepStatus | RowsProcessed | RowsInserted | RowsUpdated | DurationSeconds | StepMessage
----------|-----------|------------|---------------|--------------|-------------|-----------------|------------------------
Extract   | 1         | Sucesso    | 10000         | NULL         | NULL        | 120             | Extração concluída
Transform | 2         | Sucesso    | 10000         | NULL         | NULL        | 120             | Transformação concluída
Load      | 3         | Sucesso    | 10000         | 7000         | 2500        | 90              | Carga concluída com sucesso
```

### Ver jobs com seus steps:
```sql
SELECT 
    j.ExecutionId,
    j.JobName,
    j.Status AS JobStatus,
    j.StartDateTime AS JobStart,
    j.EndDateTime AS JobEnd,
    d.StepName,
    d.StepStatus,
    d.RowsProcessed AS StepRows,
    d.RowsInserted,
    d.RowsUpdated
FROM ETL_JobExecutionLog j
LEFT JOIN ETL_JobExecutionDetails d ON j.ExecutionId = d.ExecutionId
WHERE j.JobName = 'ImportacaoClientes'
ORDER BY j.StartDateTime DESC, d.StepOrder
```

### Ver performance por step:
```sql
SELECT 
    StepName,
    AVG(DATEDIFF(SECOND, StartDateTime, EndDateTime)) AS AvgDurationSeconds,
    AVG(RowsProcessed) AS AvgRowsProcessed,
    COUNT(*) AS TotalExecutions,
    SUM(CASE WHEN StepStatus = 'Falha' THEN 1 ELSE 0 END) AS FailedCount
FROM ETL_JobExecutionDetails
WHERE ExecutionId IN (SELECT ExecutionId FROM ETL_JobExecutionLog WHERE JobName = 'ImportacaoClientes')
GROUP BY StepName
ORDER BY StepName
```

**Resultado:**
```
StepName  | AvgDurationSeconds | AvgRowsProcessed | TotalExecutions | FailedCount
----------|--------------------|--------------------|-----------------|------------
Extract   | 125                | 9800               | 50              | 2
Load      | 95                 | 9500               | 48              | 0
Transform | 118                | 9700               | 48              | 1
```

---

## 🔍 Como os Dados Aparecem na API

### GET /api/jobs/123
```json
{
  "executionId": 123,
  "jobName": "ImportacaoClientes",
  "startDateTime": "2026-02-17T10:00:00",
  "endDateTime": "2026-02-17T10:05:30",
  "status": "Sucesso",
  "executionDurationMs": 330000,
  "rowsProcessed": 10000
}
```

### GET /api/jobs/123/details
```json
{
  "executionId": 123,
  "steps": [
    {
      "detailId": 456,
      "stepName": "Extract",
      "stepOrder": 1,
      "stepStatus": "Sucesso",
      "startDateTime": "2026-02-17T10:00:00",
      "endDateTime": "2026-02-17T10:02:00",
      "rowsProcessed": 10000,
      "durationInSeconds": 120.0,
      "progressPercentage": 100.00
    },
    {
      "detailId": 457,
      "stepName": "Transform",
      "stepOrder": 2,
      "stepStatus": "Sucesso",
      "startDateTime": "2026-02-17T10:02:00",
      "endDateTime": "2026-02-17T10:04:00",
      "rowsProcessed": 10000,
      "durationInSeconds": 120.0,
      "progressPercentage": 100.00
    },
    {
      "detailId": 458,
      "stepName": "Load",
      "stepOrder": 3,
      "stepStatus": "Sucesso",
      "startDateTime": "2026-02-17T10:04:00",
      "endDateTime": "2026-02-17T10:05:30",
      "rowsProcessed": 10000,
      "rowsInserted": 7000,
      "rowsUpdated": 2500,
      "rowsFailed": 500,
      "durationInSeconds": 90.0,
      "progressPercentage": 100.00
    }
  ],
  "count": 3
}
```

---

## 📈 Métricas Importantes

### Por Job:
- **ExecutionDurationMs**: Tempo total do job em milissegundos
- **Status**: Sucesso, Falha, EmExecucao, Cancelado
- **RowsProcessed**: Total de registros (do último step)

### Por Step:
- **ExecutionDurationMs**: Tempo do step (calculado automaticamente)
- **ProgressPercentage**: 0-100% (atualizado em tempo real)
- **RowsProcessed**: Total processado neste step
- **RowsInserted**: Novos registros (Load)
- **RowsUpdated**: Registros atualizados (Load)
- **RowsDeleted**: Registros deletados (Load)
- **RowsFailed**: Registros com erro (Load)

---

## 🎯 Casos de Uso

### 1. Monitorar progresso em tempo real
```
Extract:   [████████████████████] 100% (10000/10000)
Transform: [██████████░░░░░░░░░░]  50% (5000/10000)  ← EM EXECUÇÃO
Load:      [░░░░░░░░░░░░░░░░░░░░]   0% (0/0)
```

### 2. Identificar gargalos
```
Extract:   120s → OK
Transform: 240s → ⚠️ LENTO (esperado: 120s)
Load:      90s  → OK
```

### 3. Rastrear falhas
```
Job #124 FALHOU
├─ Extract:   ✅ Sucesso (5000 registros)
├─ Transform: ❌ FALHA (Erro: Formato de data inválido)
└─ Load:      ⏭️ Não executado
```

---

**Agora você tem visibilidade completa do seu ETL! 🚀**
