# 📊 DataPulseCM - ETL Monitoring Dashboard

Sistema de monitoramento centralizado para jobs de ETL, oferecendo visibilidade operacional e facilitando análise de falhas em tempo real.

![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red)
![License](https://img.shields.io/badge/license-MIT-blue)

## 🎯 Objetivo

Centralizar informações sobre a execução de jobs de ETL, aumentando a visibilidade e facilitando a análise de falhas através de:
- 📝 Logging padronizado de execuções
- 🔌 API REST completa para consultas e gerenciamento
- 📊 Visualização clara de status, métricas e erros
- 📈 Estatísticas e taxa de sucesso
- 🔍 Rastreamento detalhado de execuções

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e está organizado em camadas:

```
DataPulseCM/
├── src/
│   ├── EtlMonitoring.Api/          # API REST (Controllers, Program.cs)
│   ├── EtlMonitoring.Core/         # Entidades, DTOs, Interfaces
│   └── EtlMonitoring.Infrastructure/ # Repositórios, Data Access
└── database/
    └── scripts/                     # Scripts SQL (tabelas, procedures)
```

### Camadas:
- **API Layer**: Controllers e configuração da API
- **Core Layer**: Entidades de domínio, interfaces e DTOs
- **Infrastructure Layer**: Implementação de repositórios e acesso a dados

## 🚀 Tecnologias

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **Dapper** - Micro ORM para acesso a dados
- **SQL Server** - Banco de dados
- **Swagger/OpenAPI** - Documentação da API

## 📋 Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019+ (ou SQL Server Express)
- Visual Studio 2022+ ou VS Code

## ⚙️ Configuração

### 1. Clonar o repositório

```bash
git clone https://github.com/ClaudioMatheusDev/DataPulseCM.git
cd DataPulseCM
```

### 2. Configurar o banco de dados

**a) Executar os scripts SQL na ordem:**

```sql
-- 1. Criar banco e tabelas
database/scripts/01-create-tables.sql

-- 2. Criar stored procedures
database/scripts/02-create-stored-procedures.sql

-- 3. (Opcional) Inserir dados de exemplo
database/scripts/03-seed-data.sql
```

**b) Configurar a connection string:**

Edite `src/EtlMonitoring.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DataPulseCM;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

Ou para autenticação SQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DataPulseCM;User Id=seu_usuario;Password=sua_senha;TrustServerCertificate=true;"
  }
}
```

### 3. Restaurar dependências e executar

```bash
cd src/EtlMonitoring.Api
dotnet restore
dotnet build
dotnet run
```

### 4. Acessar a API

- **Swagger UI**: https://localhost:7268/swagger
- **API Base URL**: https://localhost:7268/api
- **HTTP**: http://localhost:5105

## 📚 API Endpoints

### Jobs

| Método | Endpoint | Descrição |
|--------|----------|------------|
| `GET` | `/api/jobs` | Lista execuções recentes (limit opcional) |
| `GET` | `/api/jobs/{id}` | Busca execução por ID |
| `GET` | `/api/jobs/filter` | Filtra execuções (jobName, status, datas) |
| `POST` | `/api/jobs/start` | Inicia nova execução de job |
| `POST` | `/api/jobs/{id}/finish` | Finaliza execução |
| `GET` | `/api/jobs/statistics` | Retorna estatísticas gerais |
| `GET` | `/api/jobs/failed` | Lista execuções com falha |
| `GET` | `/api/jobs/by-name/{jobName}` | Última execução do job |
| `GET` | `/api/jobs/by-name/{jobName}/history` | Histórico de execuções do job |
| `GET` | `/api/jobs/by-name/{jobName}/success-rate` | Taxa de sucesso do job |

### Exemplos de Uso

#### Iniciar Job

```bash
curl -X POST "https://localhost:7268/api/jobs/start" \
  -H "Content-Type: application/json" \
  -d '{"jobName": "ImportacaoClientes"}'
```

**Resposta:**
```json
{
  "executionId": 123,
  "jobName": "ImportacaoClientes",
  "message": "Job iniciado com sucesso"
}
```

#### Finalizar Job

```bash
curl -X POST "https://localhost:7268/api/jobs/123/finish" \
  -H "Content-Type: application/json" \
  -d '{"status": "Sucesso", "errorMessage": null}'
```

**Status válidos:**
- `Sucesso`
- `Falha`
- `Parcial`
- `EmExecucao`
- `Cancelado`

#### Obter Estatísticas

```bash
curl -X GET "https://localhost:7268/api/jobs/statistics?startDate=2026-01-01&endDate=2026-01-31"
```

**Resposta:**
```json
{
  "total": 150,
  "successful": 142,
  "failed": 8,
  "successRate": 94.67,
  "byStatus": {
    "Sucesso": 142,
    "Falha": 8
  },
  "period": {
    "startDate": "2026-01-01",
    "endDate": "2026-01-31"
  }
}
```

## 🗄️ Estrutura do Banco de Dados

### Tabela: ETL_JobExecutionLog

| Coluna | Tipo | Descrição |
|--------|------|------------|
| ExecutionId | BIGINT | ID único da execução |
| JobName | NVARCHAR(200) | Nome do job |
| StartDateTime | DATETIME2(3) | Data/hora de início |
| EndDateTime | DATETIME2(3) | Data/hora de término |
| Status | VARCHAR(20) | Status da execução |
| ErrorMessage | NVARCHAR(MAX) | Mensagem de erro |
| RowsProcessed | INT | Linhas processadas |
| RowsInserted | INT | Linhas inseridas |
| RowsUpdated | INT | Linhas atualizadas |
| RowsDeleted | INT | Linhas deletadas |
| ExecutionDurationMs | INT | Duração em milissegundos |
| ServerName | NVARCHAR(100) | Nome do servidor |
| DatabaseName | NVARCHAR(100) | Nome do banco |

### Stored Procedures

- `usp_ETL_StartJobExecution` - Inicia execução e retorna ID
- `usp_ETL_EndJobExecution` - Finaliza execução com status
- `usp_ETL_GetJobExecutions` - Lista execuções com filtros
- `usp_ETL_GetRecentExecutions` - Lista execuções recentes

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT.

## 👨‍💻 Autor

**Claudio Matheus**
- GitHub: [@ClaudioMatheusDev](https://github.com/ClaudioMatheusDev)

## 📞 Suporte

Para reportar bugs ou sugerir melhorias, abra uma [issue](https://github.com/ClaudioMatheusDev/DataPulseCM/issues).

---

⭐ Se este projeto foi útil, considere dar uma estrela!
