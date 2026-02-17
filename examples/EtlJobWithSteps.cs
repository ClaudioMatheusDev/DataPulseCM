using System.Net.Http.Json;

namespace EtlMonitoring.Examples
{
    /// <summary>
    /// Exemplo de como implementar um processo ETL completo
    /// registrando cada etapa (Extract, Transform, Load) no sistema de monitoramento
    /// </summary>
    public class EtlJobWithSteps
    {
        private readonly HttpClient _httpClient;

        public EtlJobWithSteps(string apiBaseUrl = "http://localhost:5000")
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
        }

        /// <summary>
        /// Executa um processo ETL completo com registro de todas as etapas
        /// </summary>
        public async Task ExecutarProcessoETL(string jobName)
        {
            long executionId = 0;
            long extractDetailId = 0;
            long transformDetailId = 0;
            long loadDetailId = 0;

            try
            {
                Console.WriteLine($"🚀 Iniciando processo ETL: {jobName}");
                Console.WriteLine(new string('=', 60));

                // ========================================
                // 1️⃣ INICIAR O JOB PRINCIPAL
                // ========================================
                var startResponse = await _httpClient.PostAsJsonAsync("/api/jobs/start", new
                {
                    jobName = jobName
                });
                
                startResponse.EnsureSuccessStatusCode();
                var startResult = await startResponse.Content.ReadFromJsonAsync<dynamic>();
                executionId = (long)startResult.executionId;

                Console.WriteLine($"✅ Job iniciado - ExecutionId: {executionId}");
                Console.WriteLine();

                // ========================================
                // 2️⃣ STEP 1: EXTRACT (Extração de Dados)
                // ========================================
                Console.WriteLine("📥 STEP 1: EXTRACT");
                Console.WriteLine(new string('-', 60));

                var extractResponse = await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/{executionId}/details/start", 
                    new
                    {
                        stepName = "Extract",
                        stepOrder = 1,
                        stepMessage = "Iniciando extração de dados da fonte..."
                    });

                extractResponse.EnsureSuccessStatusCode();
                var extractResult = await extractResponse.Content.ReadFromJsonAsync<dynamic>();
                extractDetailId = (long)extractResult.detailId;

                // Executar a extração com progresso em tempo real
                var dadosExtraidos = await ExecutarExtracao(extractDetailId);

                // Finalizar step Extract com métricas
                await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/details/{extractDetailId}/finish", 
                    new
                    {
                        stepStatus = "Sucesso",
                        stepMessage = "Extração concluída com sucesso",
                        rowsProcessed = dadosExtraidos.Count
                    });

                Console.WriteLine($"✅ Extract concluído - {dadosExtraidos.Count} registros extraídos");
                Console.WriteLine();

                // ========================================
                // 3️⃣ STEP 2: TRANSFORM (Transformação)
                // ========================================
                Console.WriteLine("🔄 STEP 2: TRANSFORM");
                Console.WriteLine(new string('-', 60));

                var transformResponse = await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/{executionId}/details/start", 
                    new
                    {
                        stepName = "Transform",
                        stepOrder = 2,
                        stepMessage = "Iniciando transformação e limpeza dos dados..."
                    });

                transformResponse.EnsureSuccessStatusCode();
                var transformResult = await transformResponse.Content.ReadFromJsonAsync<dynamic>();
                transformDetailId = (long)transformResult.detailId;

                // Executar a transformação com progresso
                var dadosTransformados = await ExecutarTransformacao(dadosExtraidos, transformDetailId);

                // Finalizar step Transform
                await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/details/{transformDetailId}/finish", 
                    new
                    {
                        stepStatus = "Sucesso",
                        stepMessage = "Transformação concluída",
                        rowsProcessed = dadosTransformados.Count
                    });

                Console.WriteLine($"✅ Transform concluído - {dadosTransformados.Count} registros transformados");
                Console.WriteLine();

                // ========================================
                // 4️⃣ STEP 3: LOAD (Carga para destino)
                // ========================================
                Console.WriteLine("💾 STEP 3: LOAD");
                Console.WriteLine(new string('-', 60));

                var loadResponse = await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/{executionId}/details/start", 
                    new
                    {
                        stepName = "Load",
                        stepOrder = 3,
                        stepMessage = "Iniciando carga de dados no destino..."
                    });

                loadResponse.EnsureSuccessStatusCode();
                var loadResult = await loadResponse.Content.ReadFromJsonAsync<dynamic>();
                loadDetailId = (long)loadResult.detailId;

                // Executar a carga com métricas detalhadas
                var resultadoCarga = await ExecutarCarga(dadosTransformados, loadDetailId);

                // Finalizar step Load com todas as métricas
                await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/details/{loadDetailId}/finish", 
                    new
                    {
                        stepStatus = "Sucesso",
                        stepMessage = "Carga concluída com sucesso",
                        rowsProcessed = resultadoCarga.TotalProcessado,
                        rowsInserted = resultadoCarga.Inseridos,
                        rowsUpdated = resultadoCarga.Atualizados,
                        rowsFailed = resultadoCarga.Falhas
                    });

                Console.WriteLine($"✅ Load concluído:");
                Console.WriteLine($"   - Inseridos: {resultadoCarga.Inseridos}");
                Console.WriteLine($"   - Atualizados: {resultadoCarga.Atualizados}");
                Console.WriteLine($"   - Falhas: {resultadoCarga.Falhas}");
                Console.WriteLine();

                // ========================================
                // 5️⃣ FINALIZAR O JOB PRINCIPAL
                // ========================================
                await _httpClient.PostAsJsonAsync(
                    $"/api/jobs/{executionId}/finish", 
                    new
                    {
                        status = "Sucesso",
                        errorMessage = (string?)null
                    });

                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"✅ JOB FINALIZADO COM SUCESSO!");
                Console.WriteLine($"   ExecutionId: {executionId}");
                Console.WriteLine(new string('=', 60));
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"❌ ERRO NO ETL: {ex.Message}");
                Console.WriteLine(new string('=', 60));

                // Marcar o step atual como falha (se houver)
                try
                {
                    if (loadDetailId > 0)
                    {
                        await _httpClient.PostAsJsonAsync(
                            $"/api/jobs/details/{loadDetailId}/finish", 
                            new
                            {
                                stepStatus = "Falha",
                                stepMessage = $"Erro no Load: {ex.Message}"
                            });
                    }
                    else if (transformDetailId > 0)
                    {
                        await _httpClient.PostAsJsonAsync(
                            $"/api/jobs/details/{transformDetailId}/finish", 
                            new
                            {
                                stepStatus = "Falha",
                                stepMessage = $"Erro no Transform: {ex.Message}"
                            });
                    }
                    else if (extractDetailId > 0)
                    {
                        await _httpClient.PostAsJsonAsync(
                            $"/api/jobs/details/{extractDetailId}/finish", 
                            new
                            {
                                stepStatus = "Falha",
                                stepMessage = $"Erro no Extract: {ex.Message}"
                            });
                    }

                    // Marcar o job principal como falha
                    if (executionId > 0)
                    {
                        await _httpClient.PostAsJsonAsync(
                            $"/api/jobs/{executionId}/finish", 
                            new
                            {
                                status = "Falha",
                                errorMessage = ex.Message
                            });
                    }

                    Console.WriteLine("✅ Falha registrada no sistema de monitoramento");
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"⚠️ Erro ao registrar falha: {innerEx.Message}");
                }

                throw; // Re-lançar a exceção
            }
        }

        /// <summary>
        /// Simula extração de dados com atualização de progresso em tempo real
        /// </summary>
        private async Task<List<DadoExtraido>> ExecutarExtracao(long detailId)
        {
            var dados = new List<DadoExtraido>();
            int totalRegistros = 10000;

            Console.WriteLine($"Extraindo {totalRegistros} registros...");

            for (int i = 0; i < totalRegistros; i += 100)
            {
                // ====================================
                // Simular processamento de 100 registros
                // ====================================
                await Task.Delay(50); // Simular tempo de processamento
                
                for (int j = 0; j < 100 && (i + j) < totalRegistros; j++)
                {
                    dados.Add(new DadoExtraido 
                    { 
                        Id = i + j, 
                        Nome = $"Registro {i + j}",
                        DataCriacao = DateTime.Now 
                    });
                }

                // ====================================
                // Atualizar progresso a cada 1000 registros
                // ====================================
                if (i % 1000 == 0 && i > 0)
                {
                    decimal progresso = ((decimal)i / totalRegistros) * 100;
                    
                    await _httpClient.PutAsJsonAsync(
                        $"/api/jobs/details/{detailId}/progress", 
                        new
                        {
                            rowsProcessed = i,
                            progressPercentage = progresso,
                            stepMessage = $"Extraindo... {i:N0}/{totalRegistros:N0} registros"
                        });

                    Console.WriteLine($"   Progresso: {progresso:F1}% ({i:N0}/{totalRegistros:N0})");
                }
            }

            Console.WriteLine($"   Progresso: 100.0% ({totalRegistros:N0}/{totalRegistros:N0})");
            return dados;
        }

        /// <summary>
        /// Simula transformação de dados com validação e limpeza
        /// </summary>
        private async Task<List<DadoTransformado>> ExecutarTransformacao(
            List<DadoExtraido> dados, 
            long detailId)
        {
            var resultado = new List<DadoTransformado>();
            int total = dados.Count;

            Console.WriteLine($"Transformando {total} registros...");

            for (int i = 0; i < dados.Count; i++)
            {
                // Simular transformação
                await Task.Delay(5);
                
                resultado.Add(new DadoTransformado 
                { 
                    Id = dados[i].Id,
                    NomeProcessado = dados[i].Nome?.ToUpper() ?? "",
                    DataProcessamento = DateTime.Now,
                    Validado = true
                });

                // Atualizar progresso a cada 1000 registros
                if (i % 1000 == 0 && i > 0)
                {
                    decimal progresso = ((decimal)i / total) * 100;
                    
                    await _httpClient.PutAsJsonAsync(
                        $"/api/jobs/details/{detailId}/progress", 
                        new
                        {
                            rowsProcessed = i,
                            progressPercentage = progresso,
                            stepMessage = $"Transformando... {i:N0}/{total:N0} registros"
                        });

                    Console.WriteLine($"   Progresso: {progresso:F1}% ({i:N0}/{total:N0})");
                }
            }

            Console.WriteLine($"   Progresso: 100.0% ({total:N0}/{total:N0})");
            return resultado;
        }

        /// <summary>
        /// Simula carga de dados com inserções e atualizações
        /// </summary>
        private async Task<ResultadoCarga> ExecutarCarga(
            List<DadoTransformado> dados, 
            long detailId)
        {
            var resultado = new ResultadoCarga();
            int total = dados.Count;

            Console.WriteLine($"Carregando {total} registros...");

            for (int i = 0; i < dados.Count; i++)
            {
                // Simular insert/update
                await Task.Delay(5);
                
                // 70% insert, 25% update, 5% falha
                var random = new Random().Next(100);
                if (random < 70)
                    resultado.Inseridos++;
                else if (random < 95)
                    resultado.Atualizados++;
                else
                    resultado.Falhas++;

                resultado.TotalProcessado++;

                // Atualizar progresso a cada 1000 registros
                if (i % 1000 == 0 && i > 0)
                {
                    decimal progresso = ((decimal)i / total) * 100;
                    
                    await _httpClient.PutAsJsonAsync(
                        $"/api/jobs/details/{detailId}/progress", 
                        new
                        {
                            rowsProcessed = i,
                            progressPercentage = progresso,
                            stepMessage = $"Carregando... {i:N0}/{total:N0} registros (I:{resultado.Inseridos} U:{resultado.Atualizados} F:{resultado.Falhas})"
                        });

                    Console.WriteLine($"   Progresso: {progresso:F1}% ({i:N0}/{total:N0})");
                }
            }

            Console.WriteLine($"   Progresso: 100.0% ({total:N0}/{total:N0})");
            return resultado;
        }

        // ========================================
        // Classes auxiliares
        // ========================================

        public class DadoExtraido
        {
            public int Id { get; set; }
            public string? Nome { get; set; }
            public DateTime DataCriacao { get; set; }
        }

        public class DadoTransformado
        {
            public int Id { get; set; }
            public string NomeProcessado { get; set; } = string.Empty;
            public DateTime DataProcessamento { get; set; }
            public bool Validado { get; set; }
        }

        public class ResultadoCarga
        {
            public int TotalProcessado { get; set; }
            public int Inseridos { get; set; }
            public int Atualizados { get; set; }
            public int Falhas { get; set; }
        }
    }

    // ========================================
    // PROGRAMA DE TESTE
    // ========================================
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   EXEMPLO DE ETL COM MONITORAMENTO DE ETAPAS               ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // IMPORTANTE: Certifique-se de que a API está rodando!
            var apiUrl = "http://localhost:5000"; // Ajuste conforme necessário
            
            Console.WriteLine($"🌐 API URL: {apiUrl}");
            Console.WriteLine();

            var etl = new EtlJobWithSteps(apiUrl);

            try
            {
                // Executar o ETL
                await etl.ExecutarProcessoETL("ImportacaoClientes");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"💥 Falha na execução: {ex.Message}");
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}
