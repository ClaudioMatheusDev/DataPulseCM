namespace EtlMonitoring.Core.Entities
{
    public class JobExecution
    {
        public long ExecutionID { get; set; }
        public string JobName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int? RowsProcessed { get; set; }
        public int? RowsInserted { get; set; }
        public int? RowsUpdated { get; set; }
        public int? RowsDeleted { get; set; }
        public int? ExecutionDurationMs { get; set; }
        public string? ServerName { get; set; }
        public string? DatabaseName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Campos enriquecidos
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public int? RecordsExpected { get; set; }
        public int? RecordsActual { get; set; }
        public decimal? DataQualityScore { get; set; }
        public int RetryCount { get; set; }
        public long? ParentExecutionId { get; set; }
        public string? ExecutionContext { get; set; }
        public string? MachineName { get; set; }
        public int? ProcessId { get; set; }
        public int? ThreadId { get; set; }
        public decimal? MemoryUsageMB { get; set; }
        public string? Tags { get; set; }
        public string? CorrelationId { get; set; }
        public string? UserName { get; set; }

        public double? DurationInSeconds
        {
            get
            {
                if (EndDate.HasValue)
                    return (EndDate.Value - StartDate).TotalSeconds;
                return null;
            }
        }
    }
}
