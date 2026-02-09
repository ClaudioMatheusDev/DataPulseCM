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

        public double? DurationInSeconds
        {
            get
            {
                if (EndDateTime.HasValue)
                    return (EndDateTime.Value - StartDateTime).TotalSeconds;
                return null;
            }
        }
    }
}
