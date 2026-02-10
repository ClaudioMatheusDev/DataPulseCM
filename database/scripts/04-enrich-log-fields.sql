-- =============================================
-- Script de Melhoria - Enriquecimento de Logs
-- Adiciona campos profissionais para rastreamento ETL
-- =============================================

USE [DataPulseCM]
GO

PRINT '========================================='
PRINT 'Iniciando enriquecimento da tabela de logs...'
PRINT '========================================='

-- Verificar e adicionar campos adicionais para enriquecimento de logs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'Source')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [Source] NVARCHAR(100) NULL;
    PRINT '✓ Campo Source adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'Destination')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [Destination] NVARCHAR(100) NULL;
    PRINT '✓ Campo Destination adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'RecordsExpected')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [RecordsExpected] INT NULL;
    PRINT '✓ Campo RecordsExpected adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'RecordsActual')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [RecordsActual] INT NULL;
    PRINT '✓ Campo RecordsActual adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'DataQualityScore')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [DataQualityScore] DECIMAL(5,2) NULL;
    PRINT '✓ Campo DataQualityScore adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'RetryCount')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [RetryCount] INT DEFAULT 0;
    PRINT '✓ Campo RetryCount adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'ParentExecutionId')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [ParentExecutionId] BIGINT NULL;
    PRINT '✓ Campo ParentExecutionId adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'ExecutionContext')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [ExecutionContext] NVARCHAR(MAX) NULL;
    PRINT '✓ Campo ExecutionContext adicionado (JSON)';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'MachineName')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [MachineName] NVARCHAR(100) NULL;
    PRINT '✓ Campo MachineName adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'ProcessId')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [ProcessId] INT NULL;
    PRINT '✓ Campo ProcessId adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'ThreadId')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [ThreadId] INT NULL;
    PRINT '✓ Campo ThreadId adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'MemoryUsageMB')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [MemoryUsageMB] DECIMAL(10,2) NULL;
    PRINT '✓ Campo MemoryUsageMB adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'Tags')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [Tags] NVARCHAR(500) NULL;
    PRINT '✓ Campo Tags adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'CorrelationId')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [CorrelationId] NVARCHAR(50) NULL;
    PRINT '✓ Campo CorrelationId adicionado';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND name = 'UserName')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD [UserName] NVARCHAR(100) NULL;
    PRINT '✓ Campo UserName adicionado';
END

GO

-- Criar índice para CorrelationId (para rastreamento distribuído)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ETL_JobExecutionLog_CorrelationId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ETL_JobExecutionLog_CorrelationId]
    ON [dbo].[ETL_JobExecutionLog] ([CorrelationId])
    INCLUDE ([JobName], [Status], [StartDateTime]);
    
    PRINT '✓ Índice IX_ETL_JobExecutionLog_CorrelationId criado';
END
GO

-- Criar índice para ParentExecutionId (para jobs dependentes)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ETL_JobExecutionLog_ParentExecutionId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ETL_JobExecutionLog_ParentExecutionId]
    ON [dbo].[ETL_JobExecutionLog] ([ParentExecutionId])
    INCLUDE ([ExecutionId], [JobName], [Status]);
    
    PRINT '✓ Índice IX_ETL_JobExecutionLog_ParentExecutionId criado';
END
GO

-- Criar Foreign Key para ParentExecutionId (self-reference)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ETL_JobExecutionLog_Parent')
BEGIN
    ALTER TABLE [dbo].[ETL_JobExecutionLog]
    ADD CONSTRAINT [FK_ETL_JobExecutionLog_Parent] FOREIGN KEY ([ParentExecutionId])
        REFERENCES [dbo].[ETL_JobExecutionLog] ([ExecutionId]);
    
    PRINT '✓ Foreign Key FK_ETL_JobExecutionLog_Parent criada';
END
GO

-- View melhorada com novos campos
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ETL_ExecutionMetrics')
    DROP VIEW [dbo].[vw_ETL_ExecutionMetrics];
GO

CREATE VIEW [dbo].[vw_ETL_ExecutionMetrics]
AS
SELECT 
    ExecutionId,
    JobName,
    Status,
    StartDateTime,
    EndDateTime,
    DATEDIFF(SECOND, StartDateTime, EndDateTime) AS DurationSeconds,
    RowsProcessed,
    RowsInserted,
    RowsUpdated,
    RowsDeleted,
    Source,
    Destination,
    RecordsExpected,
    RecordsActual,
    CASE 
        WHEN RecordsExpected > 0 THEN 
            CAST(RecordsActual AS DECIMAL(10,2)) / RecordsExpected * 100
        ELSE NULL 
    END AS DataCompletenessPercent,
    DataQualityScore,
    RetryCount,
    CorrelationId,
    MachineName,
    ErrorMessage,
    Tags,
    CreatedAt
FROM [dbo].[ETL_JobExecutionLog]
WHERE EndDateTime IS NOT NULL;
GO

PRINT '✓ View vw_ETL_ExecutionMetrics criada';
GO

PRINT '========================================='
PRINT 'Enriquecimento concluído com sucesso!'
PRINT '========================================='
