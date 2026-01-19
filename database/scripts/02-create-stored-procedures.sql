-- =============================================
-- Stored Procedures para ETL Logging
-- =============================================

USE [DataPulseCM]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_ETL_StartJobExecution')
    DROP PROCEDURE [dbo].[usp_ETL_StartJobExecution];
GO

CREATE PROCEDURE [dbo].[usp_ETL_StartJobExecution]
    @JobName NVARCHAR(200),
    @ExecutionId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[ETL_JobExecutionLog] (
        JobName,
        StartDateTime,
        Status,
        ServerName,
        DatabaseName
    )
    VALUES (
        @JobName,
        GETUTCDATE(),
        'EmExecucao',
        @@SERVERNAME,
        DB_NAME()
    );
    
    SET @ExecutionId = SCOPE_IDENTITY();
    
    SELECT @ExecutionId AS ExecutionId;
END
GO


IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_ETL_EndJobExecution')
    DROP PROCEDURE [dbo].[usp_ETL_EndJobExecution];
GO

CREATE PROCEDURE [dbo].[usp_ETL_EndJobExecution]
    @ExecutionId BIGINT,
    @Status VARCHAR(20),
    @ErrorMessage NVARCHAR(MAX) = NULL,
    @RowsProcessed INT = NULL,
    @RowsInserted INT = NULL,
    @RowsUpdated INT = NULL,
    @RowsDeleted INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[ETL_JobExecutionLog]
    SET 
        EndDateTime = GETUTCDATE(),
        Status = @Status,
        ErrorMessage = @ErrorMessage,
        RowsProcessed = @RowsProcessed,
        RowsInserted = @RowsInserted,
        RowsUpdated = @RowsUpdated,
        RowsDeleted = @RowsDeleted
    WHERE ExecutionId = @ExecutionId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO


IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'usp_ETL_GetRecentExecutions')
    DROP PROCEDURE [dbo].[usp_ETL_GetRecentExecutions];
GO

CREATE PROCEDURE [dbo].[usp_ETL_GetRecentExecutions]
    @Limit INT = 50,
    @JobName NVARCHAR(200) = NULL,
    @Status VARCHAR(20) = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Limit)
        ExecutionId,
        JobName,
        StartDateTime,
        EndDateTime,
        Status,
        ErrorMessage,
        RowsProcessed,
        RowsInserted,
        RowsUpdated,
        RowsDeleted,
        ExecutionDurationMs,
        ServerName,
        DatabaseName,
        CreatedAt
    FROM [dbo].[ETL_JobExecutionLog]
    WHERE 
        (@JobName IS NULL OR JobName LIKE '%' + @JobName + '%')
        AND (@Status IS NULL OR Status = @Status)
        AND (@StartDate IS NULL OR StartDateTime >= @StartDate)
        AND (@EndDate IS NULL OR StartDateTime <= @EndDate)
    ORDER BY StartDateTime DESC;
END
GO

PRINT 'Stored Procedures criadas com sucesso!';
GO