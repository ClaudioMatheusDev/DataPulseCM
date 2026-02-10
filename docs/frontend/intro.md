---
sidebar_position: 1
title: Introdução
---

# 📊 DataPulseCM - Sistema de Monitoramento ETL

Bem-vindo ao **DataPulseCM**, um sistema completo de monitoramento centralizado para jobs de ETL (Extract, Transform, Load).

## 🎯 O que é o DataPulseCM?

DataPulseCM é uma solução moderna que permite:

- **Rastreamento em tempo real** de execuções de jobs ETL
- **Visualização clara** de métricas e estatísticas
- **Análise detalhada** de falhas e erros
- **Histórico completo** de execuções
- **Dashboard intuitivo** para acompanhamento operacional

## ✨ Principais Funcionalidades

### 📈 Dashboard Interativo
- Cards com estatísticas em tempo real (total, sucessos, falhas, taxa de sucesso)
- Tabela de execuções recentes com filtros
- Atualização automática a cada 30 segundos

### 🔍 Detalhamento de Execuções
- Visualização completa de cada execução
- Timeline de steps individuais
- Mensagens de erro detalhadas
- Métricas de duração e registros processados

### 🔌 API REST Completa
- Endpoints para consulta e gerenciamento
- Documentação interativa com Swagger
- Suporte a filtros avançados
- Integração fácil com jobs existentes

### 📊 Monitoramento Avançado
- Logs estruturados com Serilog
- Integração opcional com Seq
- Health checks para monitoramento de infraestrutura

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** e está dividido em:

```
DataPulseCM/
├── API (.NET 9.0)           # Backend REST API
├── Frontend (React + TS)     # Dashboard Web
├── Database (SQL Server)     # Armazenamento de dados
└── Logging (Serilog + Seq)   # Sistema de logs
```

### Tecnologias Utilizadas

**Backend:**
- .NET 9.0
- ASP.NET Core Web API
- Dapper (Micro-ORM)
- Serilog (Logging estruturado)
- FluentValidation
- Swagger/OpenAPI

**Frontend:**
- React 19
- TypeScript
- Material-UI (MUI)
- React Router
- Axios
- Recharts
- date-fns

**Database:**
- SQL Server 2019+
- Stored Procedures otimizadas

## 🚀 Casos de Uso

### Cenário 1: Monitoramento de Jobs de Integração
Uma empresa executa múltiplos jobs ETL diariamente (importação de vendas, atualização de estoque, consolidação de dados). Com DataPulseCM, a equipe pode:
- Ver em tempo real quais jobs estão executando
- Receber alertas imediatos em caso de falha
- Analisar histórico para identificar padrões

### Cenário 2: Análise de Desempenho
Desenvolvedores precisam otimizar jobs lentos. DataPulseCM oferece:
- Métricas de duração por execução
- Comparação entre execuções
- Identificação de steps problemáticos

### Cenário 3: Auditoria e Compliance
Para atender requisitos de auditoria, o sistema fornece:
- Histórico completo de todas as execuções
- Rastreabilidade de dados processados
- Logs estruturados para análise forense

## 📋 Pré-requisitos

Antes de começar, você precisará de:

- **.NET 9.0 SDK** ou superior
- **SQL Server 2019+** (ou SQL Server Express)
- **Node.js 18+** e npm
- **Visual Studio 2022** ou VS Code

## ⚡ Quick Start

```bash
# 1. Clone o repositório
git clone https://github.com/ClaudioMatheusDev/DataPulseCM.git
cd DataPulseCM

# 2. Configure o banco de dados
# Execute os scripts em database/scripts/

# 3. Configure a connection string
# Edite src/EtlMonitoring.Api/appsettings.json

# 4. Inicie a API
cd src/EtlMonitoring.Api
dotnet run

# 5. Inicie o Frontend (em outro terminal)
cd frontend
npm install
npm run dev
```

Acesse:
- **Dashboard:** http://localhost:5173
- **API Swagger:** http://localhost:5105/swagger

## 📚 Próximos Passos

Explore a documentação completa:

1. [**Instalação**](./installation) - Guia passo a passo de instalação
2. [**Configuração**](./configuration) - Configurações detalhadas
3. [**API Reference**](./api/overview) - Documentação completa da API
4. [**Frontend Guide**](./frontend/overview) - Guia do dashboard
5. [**Exemplos**](./examples/basic-usage) - Exemplos práticos

## 🤝 Contribuindo

Contribuições são bem-vindas! Veja nosso [guia de contribuição](./contributing) para começar.

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](../LICENSE) para detalhes.

## 👨‍💻 Autor

**Claudio Matheus**
- GitHub: [@ClaudioMatheusDev](https://github.com/ClaudioMatheusDev)
