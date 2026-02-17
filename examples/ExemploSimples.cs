using System.Net.Http.Json;

// ====================================================================
// EXEMPLO RÁPIDO - COPIE E COLE NO SEU CÓDIGO
// ====================================================================

public class ExemploSimples
{
    public static async Task Main()
    {
        var httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("http://localhost:5000") 
        };

        // ==========================================
        // PASSO 1: Iniciar o Job
        // ==========================================
        var startResponse = await httpClient.PostAsJsonAsync("/api/jobs/start", new
        {
            jobName = "MeuETL"
        });

        var jobData = await startResponse.Content.ReadFromJsonAsync<JobResponse>();
        long executionId = jobData!.executionId;
        
        Console.WriteLine($"✅ Job iniciado - ID: {executionId}");

        try
        {
            // ==========================================
            // PASSO 2: Extract
            // ==========================================
            var extractResponse = await httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/details/start",
                new { stepName = "Extract", stepOrder = 1 });

            var extractData = await extractResponse.Content.ReadFromJsonAsync<StepResponse>();
            long extractId = extractData!.detailId;

            // *** AQUI VAI SUA LÓGICA DE EXTRAÇÃO ***
            Console.WriteLine("Extraindo dados...");
            await Task.Delay(2000); // Simular processamento
            int registrosExtraidos = 1000;

            // Finalizar Extract
            await httpClient.PostAsJsonAsync($"/api/jobs/details/{extractId}/finish", new
            {
                stepStatus = "Sucesso",
                rowsProcessed = registrosExtraidos
            });
            Console.WriteLine($"✅ Extract: {registrosExtraidos} registros");

            // ==========================================
            // PASSO 3: Transform
            // ==========================================
            var transformResponse = await httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/details/start",
                new { stepName = "Transform", stepOrder = 2 });

            var transformData = await transformResponse.Content.ReadFromJsonAsync<StepResponse>();
            long transformId = transformData!.detailId;

            // *** AQUI VAI SUA LÓGICA DE TRANSFORMAÇÃO ***
            Console.WriteLine("Transformando dados...");
            await Task.Delay(2000);
            int registrosTransformados = 950;

            await httpClient.PostAsJsonAsync($"/api/jobs/details/{transformId}/finish", new
            {
                stepStatus = "Sucesso",
                rowsProcessed = registrosTransformados
            });
            Console.WriteLine($"✅ Transform: {registrosTransformados} registros");

            // ==========================================
            // PASSO 4: Load
            // ==========================================
            var loadResponse = await httpClient.PostAsJsonAsync(
                $"/api/jobs/{executionId}/details/start",
                new { stepName = "Load", stepOrder = 3 });

            var loadData = await loadResponse.Content.ReadFromJsonAsync<StepResponse>();
            long loadId = loadData!.detailId;

            // *** AQUI VAI SUA LÓGICA DE CARGA ***
            Console.WriteLine("Carregando dados...");
            await Task.Delay(2000);

            await httpClient.PostAsJsonAsync($"/api/jobs/details/{loadId}/finish", new
            {
                stepStatus = "Sucesso",
                rowsProcessed = 950,
                rowsInserted = 800,
                rowsUpdated = 150
            });
            Console.WriteLine($"✅ Load: 800 inseridos, 150 atualizados");

            // ==========================================
            // PASSO 5: Finalizar Job com Sucesso
            // ==========================================
            await httpClient.PostAsJsonAsync($"/api/jobs/{executionId}/finish", new
            {
                status = "Sucesso"
            });
            Console.WriteLine($"✅ Job finalizado com sucesso!");
        }
        catch (Exception ex)
        {
            // ==========================================
            // TRATAMENTO DE ERRO
            // ==========================================
            Console.WriteLine($"❌ Erro: {ex.Message}");
            
            await httpClient.PostAsJsonAsync($"/api/jobs/{executionId}/finish", new
            {
                status = "Falha",
                errorMessage = ex.Message
            });
        }
    }

    // Classes auxiliares
    public record JobResponse(long executionId, string jobName);
    public record StepResponse(long detailId, long executionId, string stepName);
}
