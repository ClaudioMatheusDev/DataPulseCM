# 🔍 Guia de Configuração - Seq (Visualização de Logs)

## O que é Seq?

**Seq** é uma plataforma moderna de agregação e pesquisa de logs estruturados, perfeita para trabalhar com Serilog. Oferece:

- 🔎 Busca avançada em logs estruturados
- 📊 Dashboards e visualizações em tempo real
- 🚨 Alertas customizáveis
- 📈 Análise de performance
- 🎯 Queries SQL-like para logs

---

## 🚀 Instalação

### Opção 1: Docker (Recomendado)

```bash
# Baixar e executar Seq
docker run --name seq -d --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  -v C:\seq-data:/data \
  datalust/seq:latest

# Verificar se está rodando
docker ps | grep seq
```

### Opção 2: Windows Installer

1. Baixar: https://datalust.co/download
2. Executar instalador
3. Acessar: http://localhost:5341

---

## 🔧 Configuração da API

### 1. Obter API Key (Opcional, mas recomendado)

1. Acesse `http://localhost:5341`
2. Vá em `Settings` → `API Keys`
3. Clique em `Add API Key`
4. Nome: `DataPulseCM`
5. Copie a chave gerada

### 2. Atualizar Program.cs

Substitua no arquivo `src/EtlMonitoring.Api/Program.cs`:

```csharp
.WriteTo.Seq("http://localhost:5341", apiKey: "SUA_API_KEY_AQUI")
```

Por:

```csharp
.WriteTo.Seq("http://localhost:5341", apiKey: "pT2KxxxxxxxxxxxxxxxxxxxxxxxxxQ")  // Cole sua chave
```

### 3. Reiniciar a API

```bash
cd src/EtlMonitoring.Api
dotnet run
```

---

## 📊 Usando Seq para Monitoramento

### Painel Inicial

Acesse: http://localhost:5341

Você verá todos os logs em tempo real.

### 🔍 Queries Úteis

#### 1. Ver apenas logs do DataPulseCM
```
Application = "DataPulseCM"
```

#### 2. Jobs iniciados hoje
```
@Message like '%Job%iniciado%' and @Timestamp > Now()-1d
```

#### 3. Jobs que falharam
```
@Message like '%falhou%' or @Level = 'Error'
```

#### 4. Jobs por ExecutionId específico
```
ExecutionId = 123
```

#### 5. Logs de um step específico
```
@Message like '%Step%Extract%'
```

#### 6. Jobs lentos (mais de 5 minutos)
```
DurationInSeconds > 300
```

#### 7. Erros por JobName (agregado)
```sql
select JobName, count(*) as Total
from stream
where @Level = 'Error'
group by JobName
order by Total desc
```

#### 8. Taxa de sucesso por job
```sql
select 
  JobName,
  count(*) as Total,
  sum(case when Status = 'Sucesso' then 1 else 0 end) as Sucessos,
  sum(case when Status = 'Falha' then 1 else 0 end) as Falhas
from stream
where Status is not null
group by JobName
```

#### 9. Jobs executados por hora (hoje)
```sql
select 
  datepart(hour, @Timestamp) as Hora,
  count(*) as Execucoes
from stream
where @Timestamp > Now()-1d and @Message like '%Job%iniciado%'
group by datepart(hour, @Timestamp)
order by Hora
```

#### 10. Tempo médio de execução por Job
```sql
select 
  JobName,
  avg(DurationInSeconds) as TempoMedio,
  min(DurationInSeconds) as TempoMinimo,
  max(DurationInSeconds) as TempoMaximo
from stream
where DurationInSeconds is not null
group by JobName
```

---

## 🎨 Criando Dashboards

### Dashboard: Visão Geral ETL

1. Vá em `Dashboards` → `Add Dashboard`
2. Nome: `DataPulseCM - Visão Geral`
3. Adicione os seguintes charts:

#### Chart 1: Total de Execuções (Hoje)
- Tipo: `Number`
- Query:
```sql
select count(*) from stream 
where @Timestamp > Now()-1d and @Message like '%Job%iniciado%'
```

#### Chart 2: Taxa de Sucesso (Hoje)
- Tipo: `Number` (Percentage)
- Query:
```sql
select 
  cast(sum(case when Status = 'Sucesso' then 1.0 else 0.0 end) / count(*) * 100 as decimal(5,2)) as TaxaSucesso
from stream
where @Timestamp > Now()-1d and Status is not null
```

#### Chart 3: Jobs por Status (Pie Chart)
- Tipo: `Pie Chart`
- Query:
```sql
select Status, count(*) as Total
from stream
where @Timestamp > Now()-1d and Status is not null
group by Status
```

#### Chart 4: Execuções por Hora (Line Chart)
- Tipo: `Line Chart`
- Query:
```sql
select 
  datepart(hour, @Timestamp) as Hora,
  count(*) as Execucoes
from stream
where @Timestamp > Now()-1d and @Message like '%Job%iniciado%'
group by datepart(hour, @Timestamp)
order by Hora
```

#### Chart 5: Top 5 Jobs Mais Executados
- Tipo: `Bar Chart`
- Query:
```sql
select top 5 JobName, count(*) as Total
from stream
where JobName is not null
group by JobName
order by Total desc
```

---

## 🚨 Alertas

### Criar Alerta de Falha

1. Vá em `Settings` → `Alerts & Watches`
2. Clique em `Add Alert`
3. Configure:

**Nome**: `Job Falhou - Crítico`

**Query**:
```
@Level = 'Error' and @Message like '%Job%falhou%'
```

**Condição**: `At least 1 event in the last 5 minutes`

**Ação**: `Send to Email` ou `Webhook` (Slack/Teams)

### Criar Alerta de Job Lento

**Nome**: `Job Lento - Warning`

**Query**:
```
DurationInSeconds > 300
```

**Condição**: `At least 1 event`

---

## 📧 Integração com Slack/Teams

### Slack Webhook

1. Em Seq: `Settings` → `Apps`
2. Instale `Slack App`
3. Configure webhook URL do Slack
4. Nos alertas, selecione `Send to Slack`

### Microsoft Teams Webhook

1. No Teams, configure Incoming Webhook
2. Em Seq, use `HTTP POST` como ação
3. URL: URL do webhook do Teams
4. Body:
```json
{
  "@type": "MessageCard",
  "title": "🚨 DataPulseCM Alert",
  "text": "{$Message}"
}
```

---

## 🎯 Boas Práticas

### 1. Retenção de Logs
Configure para reter logs por 30 dias:
- `Settings` → `Retention`
- `Delete events after`: `30 days`

### 2. Backup de Dados
```bash
# Copiar dados do Seq (Docker)
docker cp seq:/data C:\backup\seq-data
```

### 3. Monitorar Performance do Seq
- Acesse `Settings` → `Diagnostics`
- Verifique uso de disco e memória

### 4. Criar Signals (Campos Calculados)
Exemplo: Identificar jobs lentos automaticamente

- `Settings` → `Signals`
- Nome: `Job Lento`
- Filter:
```
DurationInSeconds > 300
```
- Adicionar tag: `slow-job`

---

## 🔐 Segurança

### Habilitar Autenticação

1. `Settings` → `Users`
2. `Enable Authentication`
3. Criar usuários e senhas
4. Configurar permissões

### HTTPS

```bash
# Docker com HTTPS
docker run --name seq -d \
  -e ACCEPT_EULA=Y \
  -p 443:443 \
  -v C:\seq-certs:/certs \
  -e HTTPS_PORT=443 \
  -e HTTPS_CERTIFICATE=/certs/seq.pfx \
  -e HTTPS_CERTIFICATE_PASSWORD=senha \
  datalust/seq:latest
```

---

## 📚 Recursos Adicionais

- **Documentação Oficial**: https://docs.datalust.co/docs
- **Queries SQL**: https://docs.datalust.co/docs/the-seq-query-language
- **Integrações**: https://docs.datalust.co/docs/applications

---

## 🆘 Troubleshooting

### Seq não está recebendo logs

1. Verificar se container está rodando:
```bash
docker ps | grep seq
```

2. Verificar logs do container:
```bash
docker logs seq
```

3. Testar conexão:
```bash
curl http://localhost:5341/api
```

4. Verificar firewall (porta 5341)

### Logs duplicados

- Verifique se não há múltiplos sinks configurados
- Certifique-se que `UseSerilog()` está chamado apenas uma vez

### Seq muito lento

- Reduzir período de retenção
- Criar índices em campos frequentemente consultados
- Aumentar recursos do container Docker:
```bash
docker update seq --memory=4g --cpus=2
```

---

**Seq configurado! Agora você tem visibilidade completa dos seus logs ETL.** 🎉
