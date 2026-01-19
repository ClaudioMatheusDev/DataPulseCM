CREATE DATABASE DataPulseCM;
GO

USE [DataPulseCM]
GO

IF NOT EXISTS (SELECT * FROM sys. objects WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ETL_JobExecutionLog] (
        [ExecutionId]       BIGINT IDENTITY(1,1) NOT NULL,
        [JobName]           NVARCHAR(200) NOT NULL,
        [StartDateTime]     DATETIME2(3) NOT NULL,
        [EndDateTime]       DATETIME2(3) NULL,
        [Status]            VARCHAR(20) NOT NULL,
        [ErrorMessage]      NVARCHAR(MAX) NULL,
        [RowsProcessed]     INT NULL,
        [RowsInserted]      INT NULL,
        [RowsUpdated]       INT NULL,
        [RowsDeleted]       INT NULL,
        [ExecutionDurationMs] AS (DATEDIFF(MILLISECOND, [StartDateTime], [EndDateTime])),
        [ServerName]        NVARCHAR(100) NULL,
        [DatabaseName]      NVARCHAR(100) NULL,
        [CreatedBy]         NVARCHAR(100) DEFAULT SYSTEM_USER,
        [CreatedAt]         DATETIME2(3) DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_ETL_JobExecutionLog] PRIMARY KEY CLUSTERED ([ExecutionId] ASC),
        CONSTRAINT [CK_ETL_JobExecutionLog_Status] CHECK ([Status] IN ('Sucesso', 'Falha', 'Parcial', 'EmExecucao', 'Cancelado'))
    );
    
    PRINT 'Tabela ETL_JobExecutionLog criada com sucesso! ';
END
ELSE
BEGIN
    PRINT 'Tabela ETL_JobExecutionLog já existe. ';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ETL_JobExecutionLog_JobName_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ETL_JobExecutionLog_JobName_Status]
    ON [dbo].[ETL_JobExecutionLog] ([JobName], [Status])
    INCLUDE ([StartDateTime], [EndDateTime]);
    
    PRINT 'Índice IX_ETL_JobExecutionLog_JobName_Status criado! ';
END
GO

IF NOT EXISTS (SELECT * FROM sys. indexes WHERE name = 'IX_ETL_JobExecutionLog_StartDateTime')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ETL_JobExecutionLog_StartDateTime]
    ON [dbo].[ETL_JobExecutionLog] ([StartDateTime] DESC)
    INCLUDE ([JobName], [Status], [EndDateTime]);
    
    PRINT 'Índice IX_ETL_JobExecutionLog_StartDateTime criado!';
END
GO


IF NOT EXISTS (SELECT * FROM sys. objects WHERE object_id = OBJECT_ID(N'[dbo].[ETL_JobExecutionDetails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ETL_JobExecutionDetails] (
        [DetailId]          BIGINT IDENTITY(1,1) NOT NULL,
        [ExecutionId]       BIGINT NOT NULL,
        [StepName]          NVARCHAR(200) NOT NULL,
        [StepOrder]         INT NOT NULL,
        [StepStatus]        VARCHAR(20) NOT NULL,
        [StepMessage]       NVARCHAR(MAX) NULL,
        [StartDateTime]     DATETIME2(3) NOT NULL,
        [EndDateTime]       DATETIME2(3) NULL,
        [CreatedAt]         DATETIME2(3) DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_ETL_JobExecutionDetails] PRIMARY KEY CLUSTERED ([DetailId] ASC),
        CONSTRAINT [FK_ETL_JobExecutionDetails_Log] FOREIGN KEY ([ExecutionId])
            REFERENCES [dbo].[ETL_JobExecutionLog] ([ExecutionId])
            ON DELETE CASCADE
    );

END
GO

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ETL_LatestExecutions')
    DROP VIEW [dbo].[vw_ETL_LatestExecutions];
GO

CREATE VIEW [dbo].[vw_ETL_LatestExecutions]
AS
SELECT TOP 100
    ExecutionId,
    JobName,
    StartDateTime,
    EndDateTime,
    Status,
    ErrorMessage,
    RowsProcessed,
    ExecutionDurationMs,
    CASE 
        WHEN ExecutionDurationMs IS NULL THEN NULL
        WHEN ExecutionDurationMs < 60000 THEN CAST(ExecutionDurationMs / 1000.0 AS DECIMAL(10,2))
        ELSE NULL
    END AS DurationSeconds,
    ServerName,
    DatabaseName,
    CreatedAt
FROM [dbo].[ETL_JobExecutionLog]
ORDER BY StartDateTime DESC;
GO
