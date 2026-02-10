using Dapper;
using Microsoft.Data.SqlClient;
using EtlMonitoring.Core.Core.Interfaces;
using EtlMonitoring.Core.Entities;
using EtlMonitoring.Core.DTOs;

namespace EtlMonitoring.Infrastructure.Repositories
{
    public class JobExecutionRepository : IJobExecutionRepository
    {
        private readonly string _connectionString;

        public JobExecutionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<long> CreateJobExecutionAsync(string jobName)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@JobName", jobName);
            parameters.Add("@ExecutionId", dbType: System.Data.DbType.Int64, direction: System.Data.ParameterDirection.Output);

            await connection.ExecuteAsync(
                "usp_ETL_StartJobExecution",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );

            return parameters.Get<long>("@ExecutionId");
        }

        public async Task UpdateJobExecutionAsync(long executionId, string status, string? errorMessage = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@ExecutionId", executionId);
            parameters.Add("@Status", status);
            parameters.Add("@ErrorMessage", errorMessage);

            await connection.ExecuteAsync(
                "usp_ETL_EndJobExecution",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );
        }

        public async Task<IList<JobExecutionDto>> GetJobExecutionsAsync(JobExecutionFiltrosDto filtros)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@JobName", filtros.JobName);
            parameters.Add("@Status", filtros.Status);
            parameters.Add("@StartDate", filtros.StartDate);
            parameters.Add("@EndDate", filtros.EndDate);
            parameters.Add("@Limit", filtros.Limite);

            var result = await connection.QueryAsync<JobExecutionDto>(
                "usp_ETL_GetJobExecutions",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<JobExecution?> GetJobExecutionByIdAsync(long executionId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
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
                WHERE ExecutionId = @ExecutionId";

            var result = await connection.QueryFirstOrDefaultAsync<JobExecution>(sql, new { ExecutionId = executionId });
            return result;
        }

        public async Task<IEnumerable<JobExecution>> GetRecentExecutionsAsync(int limit = 50)
        {
            using var connection = new SqlConnection(_connectionString);

            var result = await connection.QueryAsync<JobExecution>(
                "usp_ETL_GetRecentExecutions",
                new { Limit = limit },
                commandType: System.Data.CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<int> GetTotalExecutionsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(*)
                FROM [dbo].[ETL_JobExecutionLog]
                WHERE (@StartDate IS NULL OR StartDateTime >= @StartDate)
                  AND (@EndDate IS NULL OR StartDateTime <= @EndDate)";

            var result = await connection.ExecuteScalarAsync<int>(sql, new { StartDate = startDate, EndDate = endDate });
            return result;
        }

        public async Task<int> GetSuccessfulExecutionsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(*)
                FROM [dbo].[ETL_JobExecutionLog]
                WHERE Status = 'Sucesso'
                  AND (@StartDate IS NULL OR StartDateTime >= @StartDate)
                  AND (@EndDate IS NULL OR StartDateTime <= @EndDate)";

            var result = await connection.ExecuteScalarAsync<int>(sql, new { StartDate = startDate, EndDate = endDate });
            return result;
        }

        public async Task<int> GetFailedExecutionsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT COUNT(*)
                FROM [dbo].[ETL_JobExecutionLog]
                WHERE Status = 'Falha'
                  AND (@StartDate IS NULL OR StartDateTime >= @StartDate)
                  AND (@EndDate IS NULL OR StartDateTime <= @EndDate)";

            var result = await connection.ExecuteScalarAsync<int>(sql, new { StartDate = startDate, EndDate = endDate });
            return result;
        }

        public async Task<decimal> GetSuccessRateAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    CASE 
                        WHEN COUNT(*) = 0 THEN 0
                        ELSE CAST(SUM(CASE WHEN Status = 'Sucesso' THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*) * 100
                    END
                FROM [dbo].[ETL_JobExecutionLog]
                WHERE (@StartDate IS NULL OR StartDateTime >= @StartDate)
                  AND (@EndDate IS NULL OR StartDateTime <= @EndDate)";

            var result = await connection.ExecuteScalarAsync<decimal>(sql, new { StartDate = startDate, EndDate = endDate });
            return result;
        }

        public async Task<IEnumerable<JobExecution>> GetFailedExecutionsAsync(int limit = 20)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
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
                WHERE Status = 'Falha'
                ORDER BY StartDateTime DESC";

            var result = await connection.QueryAsync<JobExecution>(sql, new { Limit = limit });
            return result;
        }

        public async Task<IDictionary<string, int>> GetExecutionsByStatusAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT Status, COUNT(*) AS Count
                FROM [dbo].[ETL_JobExecutionLog]
                WHERE (@StartDate IS NULL OR StartDateTime >= @StartDate)
                  AND (@EndDate IS NULL OR StartDateTime <= @EndDate)
                GROUP BY Status";

            var result = await connection.QueryAsync<(string Status, int Count)>(sql, new { StartDate = startDate, EndDate = endDate });
            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<JobExecution?> GetLastExecutionByJobNameAsync(string jobName)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT TOP 1
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
                WHERE JobName = @JobName
                ORDER BY StartDateTime DESC";

            var result = await connection.QueryFirstOrDefaultAsync<JobExecution>(sql, new { JobName = jobName });
            return result;
        }

        public async Task<IEnumerable<JobExecution>> GetExecutionHistoryByJobNameAsync(string jobName, int limit = 50)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
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
                WHERE JobName = @JobName
                ORDER BY StartDateTime DESC";

            var result = await connection.QueryAsync<JobExecution>(sql, new { JobName = jobName, Limit = limit });
            return result;
        }

        public async Task<decimal> GetJobSuccessRateAsync(string jobName, DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    CASE 
                        WHEN COUNT(*) = 0 THEN 0
                        ELSE CAST(SUM(CASE WHEN Status = 'Sucesso' THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*) * 100
                    END
                FROM [dbo].[ETL_JobExecutionLog]
                WHERE JobName = @JobName
                  AND (@StartDate IS NULL OR StartDateTime >= @StartDate)
                  AND (@EndDate IS NULL OR StartDateTime <= @EndDate)";

            var result = await connection.ExecuteScalarAsync<decimal>(sql, new { JobName = jobName, StartDate = startDate, EndDate = endDate });
            return result;
        }

        // Job Execution Details (Steps)
        public async Task<long> CreateJobExecutionDetailAsync(long executionId, string stepName, int stepOrder, string? stepMessage = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO [dbo].[ETL_JobExecutionDetails] 
                (ExecutionId, StepName, StepOrder, StepStatus, StepMessage, StartDateTime)
                VALUES (@ExecutionId, @StepName, @StepOrder, 'EmExecucao', @StepMessage, GETUTCDATE());
                
                SELECT CAST(SCOPE_IDENTITY() as bigint);";

            var detailId = await connection.ExecuteScalarAsync<long>(sql, new 
            { 
                ExecutionId = executionId,
                StepName = stepName,
                StepOrder = stepOrder,
                StepMessage = stepMessage
            });

            return detailId;
        }

        public async Task UpdateJobExecutionDetailAsync(long detailId, string stepStatus, string? stepMessage = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE [dbo].[ETL_JobExecutionDetails]
                SET 
                    EndDateTime = GETUTCDATE(),
                    StepStatus = @StepStatus,
                    StepMessage = COALESCE(@StepMessage, StepMessage)
                WHERE DetailId = @DetailId";

            await connection.ExecuteAsync(sql, new 
            { 
                DetailId = detailId,
                StepStatus = stepStatus,
                StepMessage = stepMessage
            });
        }

        public async Task<IEnumerable<JobExecutionDetail>> GetExecutionDetailsByExecutionIdAsync(long executionId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    DetailId,
                    ExecutionId,
                    StepName,
                    StepOrder,
                    StepStatus,
                    StepMessage,
                    StartDateTime,
                    EndDateTime,
                    CreatedAt
                FROM [dbo].[ETL_JobExecutionDetails]
                WHERE ExecutionId = @ExecutionId
                ORDER BY StepOrder ASC";

            var result = await connection.QueryAsync<JobExecutionDetail>(sql, new { ExecutionId = executionId });
            return result;
        }
    }
}