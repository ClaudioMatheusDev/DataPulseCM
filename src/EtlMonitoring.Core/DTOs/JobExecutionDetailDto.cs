namespace EtlMonitoring.Core.DTOs
{
    public class JobExecutionDetailDto
    {
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string StepStatus { get; set; } = string.Empty;
        public string? StepMessage { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }

    public class CreateJobExecutionDetailRequest
    {
        public long ExecutionId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string? StepMessage { get; set; }
    }

    public class UpdateJobExecutionDetailRequest
    {
        public string StepStatus { get; set; } = string.Empty;
        public string? StepMessage { get; set; }
    }
}
