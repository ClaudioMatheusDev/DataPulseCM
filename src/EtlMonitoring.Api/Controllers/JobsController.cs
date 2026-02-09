using Microsoft.AspNetCore.Mvc;
using EtlMonitoring.Core.DTOs;
using EtlMonitoring.Core.Core.Interfaces;

namespace EtlMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobExecutionRepository _repository;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IJobExecutionRepository repository, ILogger<JobsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/jobs
        [HttpGet]
        public async Task<IActionResult> GetRecentJobs([FromQuery] int limit = 50)
        {
            // TODO: Implementar
            // 1. Chamar repository
            // 2. Converter para DTO (se necessário)
            // 3. Retornar Ok(result)

            var jobs = await _repository.GetRecentExecutionsAsync(limit);
            return Ok(new { data = jobs, count = jobs.Count() });
        }

        // GET: api/jobs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(long id)
        {
            var job = await _repository.GetJobExecutionByIdAsync(id);
            
            if (job == null)
                return NotFound(new { message = $"Execução com ID {id} não encontrada" });
            
            return Ok(job);
        }

        // GET: api/jobs/filter
        [HttpGet("filter")]
        public async Task<IActionResult> FilterJobs([FromQuery] JobExecutionFiltrosDto filter)
        {
            var jobs = await _repository.GetJobExecutionsAsync(filter);
            return Ok(new { data = jobs, count = jobs.Count });
        }

        // POST: api/jobs/start
        [HttpPost("start")]
        public async Task<IActionResult> StartJob([FromBody] StartJobRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.JobName))
                return BadRequest(new { message = "Nome do job é obrigatório" });

            var executionId = await _repository.CreateJobExecutionAsync(request.JobName);
            
            _logger.LogInformation("Job {JobName} iniciado | ExecutionId: {ExecutionId}", 
                request.JobName, executionId);
            
            return Ok(new { executionId, jobName = request.JobName, message = "Job iniciado com sucesso" });
        }

        // POST: api/jobs/{id}/finish
        [HttpPost("{id}/finish")]
        public async Task<IActionResult> FinishJob(long id, [FromBody] FinishJobRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { message = "Status é obrigatório" });

            await _repository.UpdateJobExecutionAsync(id, request.Status, request.ErrorMessage);
            
            _logger.LogInformation("Job finalizado | ExecutionId: {ExecutionId} | Status: {Status}", 
                id, request.Status);
            
            if (request.Status == "Falha")
            {
                _logger.LogWarning("Job falhou | ExecutionId: {ExecutionId} | Erro: {ErrorMessage}", 
                    id, request.ErrorMessage);
            }
            
            return Ok(new { executionId = id, message = "Job finalizado com sucesso" });
        }

        // GET: api/jobs/{id}/details
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetJobDetails(long id)
        {
            var details = await _repository.GetExecutionDetailsByExecutionIdAsync(id);
            return Ok(new { executionId = id, steps = details, count = details.Count() });
        }

        // POST: api/jobs/{id}/details/start
        [HttpPost("{id}/details/start")]
        public async Task<IActionResult> StartJobStep(long id, [FromBody] CreateJobExecutionDetailRequest request)
        {
            var detailId = await _repository.CreateJobExecutionDetailAsync(
                id, 
                request.StepName, 
                request.StepOrder, 
                request.StepMessage);

            _logger.LogInformation("Step iniciado | ExecutionId: {ExecutionId} | Step: {StepName} | Order: {StepOrder}", 
                id, request.StepName, request.StepOrder);

            return Ok(new { detailId, executionId = id, stepName = request.StepName, message = "Step iniciado com sucesso" });
        }

        // POST: api/jobs/details/{detailId}/finish
        [HttpPost("details/{detailId}/finish")]
        public async Task<IActionResult> FinishJobStep(long detailId, [FromBody] UpdateJobExecutionDetailRequest request)
        {
            await _repository.UpdateJobExecutionDetailAsync(detailId, request.StepStatus, request.StepMessage);

            _logger.LogInformation("Step finalizado | DetailId: {DetailId} | Status: {StepStatus}", 
                detailId, request.StepStatus);

            return Ok(new { detailId, message = "Step finalizado com sucesso" });
        }

        // GET: api/jobs/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var total = await _repository.GetTotalExecutionsAsync(startDate, endDate);
            var successful = await _repository.GetSuccessfulExecutionsAsync(startDate, endDate);
            var failed = await _repository.GetFailedExecutionsAsync(startDate, endDate);
            var successRate = await _repository.GetSuccessRateAsync(startDate, endDate);
            var byStatus = await _repository.GetExecutionsByStatusAsync(startDate, endDate);

            return Ok(new
            {
                total,
                successful,
                failed,
                successRate,
                byStatus,
                period = new { startDate, endDate }
            });
        }

        // GET: api/jobs/failed
        [HttpGet("failed")]
        public async Task<IActionResult> GetFailedJobs([FromQuery] int limit = 20)
        {
            var failedJobs = await _repository.GetFailedExecutionsAsync(limit);
            return Ok(new { data = failedJobs, count = failedJobs.Count() });
        }

        // GET: api/jobs/by-name/{jobName}
        [HttpGet("by-name/{jobName}")]
        public async Task<IActionResult> GetJobByName(string jobName)
        {
            var lastExecution = await _repository.GetLastExecutionByJobNameAsync(jobName);
            
            if (lastExecution == null)
                return NotFound(new { message = $"Nenhuma execução encontrada para o job '{jobName}'" });
            
            return Ok(lastExecution);
        }

        // GET: api/jobs/by-name/{jobName}/history
        [HttpGet("by-name/{jobName}/history")]
        public async Task<IActionResult> GetJobHistory(string jobName, [FromQuery] int limit = 50)
        {
            var history = await _repository.GetExecutionHistoryByJobNameAsync(jobName, limit);
            return Ok(new { jobName, data = history, count = history.Count() });
        }

        // GET: api/jobs/by-name/{jobName}/success-rate
        [HttpGet("by-name/{jobName}/success-rate")]
        public async Task<IActionResult> GetJobSuccessRate(string jobName, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var successRate = await _repository.GetJobSuccessRateAsync(jobName, startDate, endDate);
            return Ok(new { jobName, successRate, period = new { startDate, endDate } });
        }
    }
}