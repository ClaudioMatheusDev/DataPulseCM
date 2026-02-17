namespace EtlMonitoring.Core.Entities
{
    public class JobExecutionDetail
    {
        public long DetailId { get; set; }
        public long ExecutionId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string StepStatus { get; set; } = string.Empty;
        public string? StepMessage { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RowsProcessed { get; set; }
        public int? RowsInserted { get; set; }
        public int? RowsUpdated { get; set; }
        public int? RowsDeleted { get; set; }
        public int? RowsFailed { get; set; }
        public decimal? ProgressPercentage { get; set; }

        public double? DurationInSeconds
        {
            get
            {
                if (EndDateTime.HasValue)
                    return (EndDateTime.Value - StartDateTime).TotalSeconds;
                return null;
            }
        }

        public int? DurationInMilliseconds
        {
            get
            {
                if (EndDateTime.HasValue)
                    return (int)(EndDateTime.Value - StartDateTime).TotalMilliseconds;
                return null;
            }
        }
    }
}
