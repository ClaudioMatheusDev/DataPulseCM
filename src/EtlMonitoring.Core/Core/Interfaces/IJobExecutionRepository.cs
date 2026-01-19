using EtlMonitoring.Core.DTOs;
using EtlMonitoring.Core.Entities;

namespace EtlMonitoring.Core.Core.Interfaces
{
    public interface IJobExecutionRepository
    {
        // Operações básicas
        Task<long> CreateJobExecutionAsync(string jobName);
        Task UpdateJobExecutionAsync(long executionId, string status, string? errorMessage = null);
        
        // Consultas
        Task<IList<JobExecutionDto>> GetJobExecutionsAsync(JobExecutionFiltrosDto filtros);
        Task<JobExecution?> GetJobExecutionByIdAsync(long executionId);
        Task<IEnumerable<JobExecution>> GetRecentExecutionsAsync(int limit = 50);
        
        // Estatísticas e Dashboard
        Task<int> GetTotalExecutionsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetSuccessfulExecutionsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetFailedExecutionsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetSuccessRateAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<JobExecution>> GetFailedExecutionsAsync(int limit = 20);
        Task<IDictionary<string, int>> GetExecutionsByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        // Por Job específico
        Task<JobExecution?> GetLastExecutionByJobNameAsync(string jobName);
        Task<IEnumerable<JobExecution>> GetExecutionHistoryByJobNameAsync(string jobName, int limit = 50);
        Task<decimal> GetJobSuccessRateAsync(string jobName, DateTime? startDate = null, DateTime? endDate = null);
    }
}
