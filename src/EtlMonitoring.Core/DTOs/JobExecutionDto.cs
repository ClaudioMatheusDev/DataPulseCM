namespace EtlMonitoring.Core.DTOs
{
    public class JobExecutionDto
    {
        public long ExecutionId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int? RowsProcessed { get; set; }
        public int? RowsInserted { get; set; }
        public int? RowsUpdated { get; set; }
        public int? RowsDeleted { get; set; }
        public int? ExecutionDurationMs { get; set; }
        public string? ServerName { get; set; }
        public string? DatabaseName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class JobExecutionFiltrosDto
    {
        public string? JobName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Limite { get; set; } = 50;
    }

    public class StartJobRequest
    {
        public string JobName { get; set; } = string.Empty;
    }

    public class FinishJobRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}