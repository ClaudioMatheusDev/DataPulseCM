using System.Diagnostics;
using System.Text.Json;

namespace EtlMonitoring.Examples
{
    /// <summary>
    /// Exemplo de cliente ETL que utiliza o sistema de logging profissional
    /// </summary>
    public class EtlJobClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public EtlJobClient(string apiBaseUrl = "http://localhost:5000")
        {
            _apiBaseUrl = apiBaseUrl;
            _httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
        }

        /// <summary>
        /// Exemplo 1: Job ETL simples sem steps
        /// </summary>
        public async Task ExecutarJobSimplesAsync()
        {
            long? executionId = null;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 1. Iniciar job
                executionId = await IniciarJobAsync("ETL_Produtos_Simples");
                Console.WriteLine($"✓ Job iniciado - ExecutionId: {executionId}");

                // 2. Executar processamento ETL
                await Task.Delay(2000); // Simula processamento
                var registrosProcessados = 1500;

                // 3. Finalizar job com sucesso
                await FinalizarJobAsync(executionId.Value, "Sucesso");
                Console.WriteLine($"✓ Job finalizado - {registrosProcessados} registros processados em {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                // Finalizar job com falha
                if (executionId.HasValue)
                {
                    await FinalizarJobAsync(executionId.Value, "Falha", ex.Message);
                }
                Console.WriteLine($"✗ Job falhou: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exemplo 2: Job ETL completo com steps detalhados (RECOMENDADO)
        /// </summary>
        public async Task ExecutarJobCompletoComStepsAsync()
        {
            long? executionId = null;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ========== INICIAR JOB ==========
                executionId = await IniciarJobAsync("ETL_Vendas_Completo");
                Console.WriteLine($"\n🚀 Job iniciado - ExecutionId: {executionId}\n");

                // ========== STEP 1: EXTRACT ==========
                var extractStepId = await IniciarStepAsync(
                    executionId.Value, 
                    "Extract - SQL Server Origem", 
                    1, 
                    "Conectando ao servidor de origem...");

                var registrosExtraidos = await SimularExtracaoAsync();

                await FinalizarStepAsync(
                    extractStepId, 
                    "Sucesso", 
                    $"Extraídos {registrosExtraidos:N0} registros");

                Console.WriteLine($"  ✓ Step 1 - Extract: {registrosExtraidos:N0} registros extraídos");

                // ========== STEP 2: TRANSFORM ==========
                var transformStepId = await IniciarStepAsync(
                    executionId.Value, 
                    "Transform - Limpeza e Validação", 
                    2, 
                    "Aplicando regras de negócio e validações...");

                var (registrosValidos, registrosRejeitados) = await SimularTransformacaoAsync(registrosExtraidos);

                await FinalizarStepAsync(
                    transformStepId, 
                    "Sucesso", 
                    $"{registrosValidos:N0} válidos, {registrosRejeitados:N0} rejeitados");

                Console.WriteLine($"  ✓ Step 2 - Transform: {registrosValidos:N0} válidos, {registrosRejeitados:N0} rejeitados");

                // ========== STEP 3: LOAD ==========
                var loadStepId = await IniciarStepAsync(
                    executionId.Value, 
                    "Load - Inserção no Data Warehouse", 
                    3, 
                    "Carregando dados no DW...");

                var registrosInseridos = await SimularCargaAsync(registrosValidos);

                await FinalizarStepAsync(
                    loadStepId, 
                    "Sucesso", 
                    $"{registrosInseridos:N0} registros inseridos");

                Console.WriteLine($"  ✓ Step 3 - Load: {registrosInseridos:N0} registros inseridos");

                // ========== STEP 4: VALIDATE ==========
                var validateStepId = await IniciarStepAsync(
                    executionId.Value, 
                    "Validate - Verificação de Integridade", 
                    4, 
                    "Validando integridade dos dados...");

                var validacaoOk = await SimularValidacaoAsync();

                await FinalizarStepAsync(
                    validateStepId, 
                    validacaoOk ? "Sucesso" : "Falha", 
                    validacaoOk ? "Validação concluída - Dados íntegros" : "Falha na validação");

                Console.WriteLine($"  ✓ Step 4 - Validate: {(validacaoOk ? "OK" : "FALHA")}");

                // ========== FINALIZAR JOB ==========
                await FinalizarJobAsync(executionId.Value, "Sucesso");
                
                stopwatch.Stop();
                Console.WriteLine($"\n✅ Job concluído com sucesso em {stopwatch.Elapsed.TotalSeconds:F2}s\n");
            }
            catch (Exception ex)
            {
                if (executionId.HasValue)
                {
                    await FinalizarJobAsync(executionId.Value, "Falha", ex.Message);
                }
                Console.WriteLine($"\n❌ Job falhou: {ex.Message}\n");
                throw;
            }
        }

        /// <summary>
        /// Exemplo 3: Job com retry automático
        /// </summary>
        public async Task ExecutarJobComRetryAsync(int maxRetries = 3)
        {
            var tentativa = 0;
            Exception? ultimoErro = null;

            while (tentativa < maxRetries)
            {
                tentativa++;
                long? executionId = null;

                try
                {
                    Console.WriteLine($"\n🔄 Tentativa {tentativa}/{maxRetries}");
                    
                    executionId = await IniciarJobAsync($"ETL_Com_Retry");
                    
                    // Simular falha aleatória
                    if (new Random().Next(0, 2) == 0 && tentativa < 3)
                    {
                        throw new Exception("Timeout ao conectar no servidor de origem");
                    }

                    // Processamento bem-sucedido
                    await Task.Delay(1000);
                    await FinalizarJobAsync(executionId.Value, "Sucesso");
                    
                    Console.WriteLine($"✅ Job concluído na tentativa {tentativa}");
                    return; // Sucesso!
                }
                catch (Exception ex)
                {
                    ultimoErro = ex;
                    
                    if (executionId.HasValue)
                    {
                        await FinalizarJobAsync(
                            executionId.Value, 
                            "Falha", 
                            $"Tentativa {tentativa}: {ex.Message}");
                    }

                    if (tentativa < maxRetries)
                    {
                        var delayMs = tentativa * 2000; // Backoff exponencial
                        Console.WriteLine($"⚠️ Falhou. Aguardando {delayMs}ms antes de retry...");
                        await Task.Delay(delayMs);
                    }
                }
            }

            throw new Exception($"Job falhou após {maxRetries} tentativas", ultimoErro);
        }

        // ========================================
        // MÉTODOS AUXILIARES DA API
        // ========================================

        private async Task<long> IniciarJobAsync(string jobName)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/jobs/start", new { jobName });
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("executionId").GetInt64();
        }

        private async Task FinalizarJobAsync(long executionId, string status, string? errorMessage = null)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/finish", 
                new { status, errorMessage });
            
            response.EnsureSuccessStatusCode();
        }

        private async Task<long> IniciarStepAsync(long executionId, string stepName, int stepOrder, string? stepMessage = null)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/details/start", 
                new { executionId, stepName, stepOrder, stepMessage });
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("detailId").GetInt64();
        }

        private async Task FinalizarStepAsync(long detailId, string stepStatus, string? stepMessage = null)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/jobs/details/{detailId}/finish", 
                new { stepStatus, stepMessage });
            
            response.EnsureSuccessStatusCode();
        }

        // ========================================
        // SIMULAÇÕES DE PROCESSAMENTO ETL
        // ========================================

        private async Task<int> SimularExtracaoAsync()
        {
            await Task.Delay(1500); // Simula consulta SQL
            return new Random().Next(50000, 150000);
        }

        private async Task<(int validos, int rejeitados)> SimularTransformacaoAsync(int totalRegistros)
        {
            await Task.Delay(2000); // Simula transformação
            var rejeitados = (int)(totalRegistros * 0.02); // 2% de rejeição
            return (totalRegistros - rejeitados, rejeitados);
        }

        private async Task<int> SimularCargaAsync(int registros)
        {
            await Task.Delay(1800); // Simula inserção em lote
            return registros;
        }

        private async Task<bool> SimularValidacaoAsync()
        {
            await Task.Delay(800); // Simula queries de validação
            return true;
        }
    }

    // ========================================
    // PROGRAMA DE TESTE
    // ========================================

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new EtlJobClient("http://localhost:5000");

            Console.WriteLine("===========================================");
            Console.WriteLine("    EXEMPLOS DE USO - ETL LOGGING");
            Console.WriteLine("===========================================\n");

            // Exemplo 1
            Console.WriteLine("--- EXEMPLO 1: Job Simples ---");
            await client.ExecutarJobSimplesAsync();
            await Task.Delay(2000);

            // Exemplo 2
            Console.WriteLine("\n--- EXEMPLO 2: Job Completo com Steps ---");
            await client.ExecutarJobCompletoComStepsAsync();
            await Task.Delay(2000);

            // Exemplo 3
            Console.WriteLine("\n--- EXEMPLO 3: Job com Retry ---");
            await client.ExecutarJobComRetryAsync(maxRetries: 3);

            Console.WriteLine("\n===========================================");
            Console.WriteLine("  ✅ Todos os exemplos executados!");
            Console.WriteLine("===========================================\n");
        }
    }
}
