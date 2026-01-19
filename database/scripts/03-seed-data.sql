

DECLARE @ExecId BIGINT;

-- Job 1: Sucesso
EXEC usp_ETL_StartJobExecution 'ETL_ImportarVendas', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Sucesso', NULL, 1500;

-- Job 2: Falha
EXEC usp_ETL_StartJobExecution 'ETL_ImportarClientes', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Falha', 'Timeout na conexão', 0;

-- Job 3: Sucesso com grande volume
EXEC usp_ETL_StartJobExecution 'ETL_ProcessarPedidos', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Sucesso', NULL, 25000;

-- Job 4: Falha por dados inválidos
EXEC usp_ETL_StartJobExecution 'ETL_AtualizarEstoque', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Falha', 'Formato de arquivo inválido', 0;

-- Job 5: Sucesso com poucos registros
EXEC usp_ETL_StartJobExecution 'ETL_ExportarRelatorios', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Sucesso', NULL, 45;

-- Job 6: Falha por permissão
EXEC usp_ETL_StartJobExecution 'ETL_ImportarFornecedores', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Falha', 'Acesso negado ao diretório de origem', 0;

-- Job 7: Sucesso médio
EXEC usp_ETL_StartJobExecution 'ETL_ConsolidarDados', @ExecId OUTPUT;
EXEC usp_ETL_EndJobExecution @ExecId, 'Sucesso', NULL, 8750;