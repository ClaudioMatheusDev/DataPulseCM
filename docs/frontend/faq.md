---
sidebar_position: 99
title: FAQ e Troubleshooting
---

# ❓ FAQ e Troubleshooting

Respostas para perguntas frequentes e soluções para problemas comuns.

## 🔧 Problemas Comuns - API

### Erro: "Connection string não encontrada"

**Sintoma:**
```
InvalidOperationException: Connection string 'DefaultConnection' não encontrada.
```

**Causa:** Arquivo `appsettings.json` não configurado ou sem a connection string.

**Solução:**

1. Abra `src/EtlMonitoring.Api/appsettings.json`
2. Adicione/edite a connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DataPulseCM;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

---

### Erro: "Cannot connect to SQL Server"

**Sintoma:**
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```

**Causa:** SQL Server não está rodando ou connection string incorreta.

**Solução:**

1. Verifique se SQL Server está rodando:
   - Windows: Services → SQL Server (MSSQLSERVER)
   - SQL Server Configuration Manager

2. Teste a conexão em SSMS com as mesmas credenciais

3. Verifique o nome do servidor:
   ```sql
   SELECT @@SERVERNAME
   ```

4. Ajuste a connection string conforme necessário

---

### Erro de CORS no navegador

**Sintoma:**
```
Access to XMLHttpRequest at 'http://localhost:5105/api/jobs' from origin 'http://localhost:5173' 
has been blocked by CORS policy
```

**Causa:** CORS não configurado corretamente ou na ordem errada.

**Solução:**

Verifique em `Program.cs` se CORS está ANTES de `UseHttpsRedirection()`:

```csharp
// CORRETO
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// ERRADO
app.UseHttpsRedirection();
app.UseCors("AllowAll");
```

Reinicie a API após corrigir.

---

### API não retorna dados

**Sintoma:** Endpoints retornam arrays vazios `{ data: [], count: 0 }`

**Causa:** Banco de dados vazio (sem dados de seed).

**Solução:**

Execute o script de seed:
```sql
-- Execute: database/scripts/03-seed-data.sql
```

Ou crie uma execução manualmente:
```http
POST /api/jobs/start
Content-Type: application/json

{
  "jobName": "TesteJob"
}
```

---

## 🎨 Problemas Comuns - Frontend

### Erro: "Module not found"

**Sintoma:**
```
Error: Cannot find module '@mui/material'
```

**Causa:** Dependências não instaladas.

**Solução:**

```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

---

### Dashboard não carrega dados

**Sintoma:** Erro "Falha ao carregar dados do dashboard"

**Causas Possíveis:**

1. **API não está rodando**
   ```bash
   # Verifique se API está rodando
   curl http://localhost:5105/health
   ```

2. **URL incorreta**
   
   Verifique `frontend/src/services/api.ts`:
   ```typescript
   const API_BASE = 'http://localhost:5105/api'; // Porta correta?
   ```

3 **Firewall bloqueando**
   
   Permita conexões na porta 5105

---

### Tabela mostra dados vazios (- - -)

**Sintoma:** Colunas de Data, Duração, Registros mostram apenas `-`

**Causa:** Dados no banco com valores padrão (`0001-01-01`, `NULL`, `0`)

**Solução:**

Isso é normal para dados de seed. Para dados reais:

1. Use datas válidas ao criar jobs:
   ```sql
   StartDate = GETDATE()
   ```

2. Calcule e armazene a duração:
   ```sql
   Duration = DATEDIFF(SECOND, StartDate, EndDate)
   ```

3. Registre `RecordsProcessed` ao finalizar o job

---

### Erro: "Grid2 is not exported"

**Sintoma:**
```
The requested module does not provide an export named 'Grid2'
```

**Causa:** Versão antiga do Material-UI.

**Solução:**

Já corrigido no código atual usando `Box` + flexbox ao invés de `Grid2`.

Se ainda ocorrer, atualize MUI:
```bash
npm install @mui/material@latest
```

---

## 🗄️ Problemas Comuns - Banco de Dados

### Tabelas não foram criadas

**Sintoma:** Erro ao executar API ou queries

**Solução:**

1. Conecte-se ao SQL Server
2. Execute os scripts na ordem:
   ```sql
   -- 1. database/scripts/01-create-tables.sql
   -- 2. database/scripts/02-create-stored-procedures.sql
   ```

3. Verifique:
   ```sql
   USE DataPulseCM;
   SELECT * FROM INFORMATION_SCHEMA.TABLES;
   ```

---

### Stored Procedures retornam erro

**Sintoma:** Erro ao chamar SPs da API

**Solução:**

1. Verifique se as SPs existem:
   ```sql
   SELECT * FROM sys.procedures;
   ```

2. Teste manualmente:
   ```sql
   EXEC sp_CreateJobExecution @JobName = 'Teste';
   ```

3. Se necessário, reexecute `02-create-stored-procedures.sql`

---

## 🐛 Problemas de Logging

### Logs não aparecem no console

**Causa:** Nível de log configurado incorretamente.

**Solução:**

Em `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

### Seq não recebe logs

**Causa:** Seq não está rodando ou URL incorreta.

**Solução:**

1. Verifique se Seq está rodando:
   ```bash
   docker ps | grep seq
   ```

2. Acesse http://localhost:5341

3. Verifique a URL em `Program.cs`:
   ```csharp
   .WriteTo.Seq("http://localhost:5341")
   ```

---

## ❓ Perguntas Frequentes

### Como mudar a porta da API?

Edite `launchSettings.json`:

```json
{
  "applicationUrl": "http://localhost:SUAPORTA"
}
```

E atualize `frontend/src/services/api.ts`:

```typescript
const API_BASE = 'http://localhost:SUAPORTA/api';
```

---

### Como adicionar autenticação JWT?

Planejado para versões futuras. Para implementar agora:

1. Adicione pacote:
   ```bash
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   ```

2. Configure em `Program.cs`
3. Adicione `[Authorize]` nos controllers

---

### Posso usar PostgreSQL ao invés de SQL Server?

Sim, mas requer adaptações:

1. Trocar Dapper queries
2. Adaptar scripts SQL
3. Atualizar connection string
4. Trocar Health Check

---

### Como fazer deploy em produção?

**API:**
```bash
dotnet publish -c Release -o ./publish
```

Deploy em:
- IIS (Windows Server)
- Azure App Service
- Docker container
- Linux com systemd

**Frontend:**
```bash
npm run build
```

Deploy `dist/` em:
- Azure Static Web Apps
- Netlify
- Vercel
- Nginx/Apache

---

### Como adicionar novos campos à JobExecution?

1. Altere a tabela no banco:
   ```sql
   ALTER TABLE JobExecutions ADD NovoCampo VARCHAR(100);
   ```

2. Atualize a entidade em `Entities/JobExecution.cs`

3. Atualize o DTO em `DTOs/JobExecutionDto.cs`

4. Atualize tipo TypeScript em `frontend/src/types/job.types.ts`

5. Atualize componentes para exibir novo campo

---

### A API suporta múltiplos jobs simultâneos?

Sim! A API é stateless e pode processar múltiplas requisições simultâneas sem interferência.

---

### Como limpar execuções antigas?

Execute manualmente:

```sql
DELETE FROM JobExecutionDetails 
WHERE ExecutionID IN (
  SELECT ExecutionID FROM JobExecutions 
  WHERE StartDate < DATEADD(DAY, -90, GETDATE())
);

DELETE FROM JobExecutions 
WHERE StartDate < DATEADD(DAY, -90, GETDATE());
```

Ou crie um job scheduled (futuro: Hangfire).

---

### Posso usar DataPulseCM com jobs em Python/Java/Node?

**Sim!** A API é independente de linguagem. Veja [Exemplos](./api/examples) em várias linguagens.

---

### Como contribuir com o projeto?

1. Fork o repositório
2. Crie uma branch: `git checkout -b feature/nova-funcionalidade`
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

---

## 📞 Ainda com problemas?

- [Abra uma issue no GitHub](https://github.com/ClaudioMatheusDev/DataPulseCM/issues)
- Consulte a [documentação completa](./intro)
- Veja os [exemplos práticos](./api/examples)

## 💡 Dicas Gerais

### Performance

- Use `limit` apropriado nas consultas
- Implemente paginação para grandes datasets
- Configure índices no banco de dados

### Segurança

- Use HTTPS em produção
- Implemente autenticação/autorização
- Valide todos os inputs
- Use SQL parametrizado (já implementado)

### Monitoramento

- Configure alertas para falhas
- Monitore uso de recursos
- Use Application Insights ou similar

### Backup

- Faça backup regular do banco
- Documente configurações
- Versione scripts SQL no Git
