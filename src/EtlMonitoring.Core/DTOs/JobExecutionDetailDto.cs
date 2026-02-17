namespace EtlMonitoring.Core.DTOs
{
    public class JobExecutionDetailDto
    {
        public long DetailId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string StepStatus { get; set; } = string.Empty;
        public string? StepMessage { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        
        // Campos de métricas
        public int? RowsProcessed { get; set; }
        public int? RowsInserted { get; set; }
        public int? RowsUpdated { get; set; }
        public int? RowsDeleted { get; set; }
        public int? RowsFailed { get; set; }
        public decimal? ProgressPercentage { get; set; }
        public int? DurationInMilliseconds { get; set; }
        public double? DurationInSeconds { get; set; }
    }

    public class CreateJobExecutionDetailRequest
    {
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string? StepMessage { get; set; }
    }

    public class UpdateJobExecutionDetailRequest
    {
        public string StepStatus { get; set; } = string.Empty;
        public string? StepMessage { get; set; }
        
        // Campos opcionais para métricas
        public int? RowsProcessed { get; set; }
        public int? RowsInserted { get; set; }
        public int? RowsUpdated { get; set; }
        public int? RowsDeleted { get; set; }
        public int? RowsFailed { get; set; }
    }

    public class UpdateStepProgressRequest
    {
        public int? RowsProcessed { get; set; }
        public decimal? ProgressPercentage { get; set; }
        public string? StepMessage { get; set; }
    }
}
