namespace EtlMonitoring.Core.DTOs
{
    public class JobExecutionDto
    {
        public string JobName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class JobExecutionFiltrosDto
    {
        public string? JobName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Limite { get; set; } = 100;
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
