# ====================================================================
# SCRIPT POWERSHELL - TESTAR ETL COM STEPS
# Execute: .\TestarETL.ps1
# ====================================================================

$apiUrl = "http://localhost:5105"

Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?   TESTE DE ETL COM MONITORAMENTO DE ETAPAS                 ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# ====================================================================
# 1. INICIAR O JOB
# ====================================================================
Write-Host "?? Iniciando Job..." -ForegroundColor Yellow

$startJobBody = @{
    jobName = "TesteETL_PowerShell"
} | ConvertTo-Json

$jobResponse = Invoke-RestMethod -Uri "$apiUrl/api/jobs/start" `
    -Method POST `
    -Body $startJobBody `
    -ContentType "application/json"

$executionId = $jobResponse.executionId
Write-Host "? Job iniciado - ExecutionId: $executionId" -ForegroundColor Green
Write-Host ""

try {
    # ====================================================================
    # 2. STEP 1: EXTRACT
    # ====================================================================
    Write-Host "?? STEP 1: EXTRACT" -ForegroundColor Yellow
    Write-Host "------------------------------------------------------------"

    $extractBody = @{
        stepName = "Extract"
        stepOrder = 1
        stepMessage = "Iniciando extra??o de dados..."
    } | ConvertTo-Json

    $extractResponse = Invoke-RestMethod -Uri "$apiUrl/api/jobs/$executionId/details/start" `
        -Method POST `
        -Body $extractBody `
        -ContentType "application/json"

    $extractId = $extractResponse.detailId
    Write-Host "   DetailId: $extractId" -ForegroundColor Gray

    # Simular processamento
    Write-Host "   Processando..." -ForegroundColor Gray
    Start-Sleep -Seconds 2

    # Atualizar progresso
    $progressBody = @{
        rowsProcessed = 5000
        progressPercentage = 50.0
        stepMessage = "50% conclu?do..."
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$apiUrl/api/jobs/details/$extractId/progress" `
        -Method PUT `
        -Body $progressBody `
        -ContentType "application/json" | Out-Null

    Write-Host "   Progresso: 50%" -ForegroundColor Gray
    Start-Sleep -Seconds 2

    # Finalizar Extract
    $finishExtractBody = @{
        stepStatus = "Sucesso"
        stepMessage = "Extra??o conclu?da"
        rowsProcessed = 10000
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$apiUrl/api/jobs/details/$extractId/finish" `
        -Method POST `
        -Body $finishExtractBody `
        -ContentType "application/json" | Out-Null

    Write-Host "? Extract conclu?do - 10.000 registros" -ForegroundColor Green
    Write-Host ""

    # ====================================================================
    # 3. STEP 2: TRANSFORM
    # ====================================================================
    Write-Host "?? STEP 2: TRANSFORM" -ForegroundColor Yellow
    Write-Host "------------------------------------------------------------"

    $transformBody = @{
        stepName = "Transform"
        stepOrder = 2
        stepMessage = "Iniciando transforma??o..."
    } | ConvertTo-Json

    $transformResponse = Invoke-RestMethod -Uri "$apiUrl/api/jobs/$executionId/details/start" `
        -Method POST `
        -Body $transformBody `
        -ContentType "application/json"

    $transformId = $transformResponse.detailId
    Write-Host "   DetailId: $transformId" -ForegroundColor Gray
    Write-Host "   Processando..." -ForegroundColor Gray
    Start-Sleep -Seconds 2

    $finishTransformBody = @{
        stepStatus = "Sucesso"
        stepMessage = "Transforma??o conclu?da"
        rowsProcessed = 9500
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$apiUrl/api/jobs/details/$transformId/finish" `
        -Method POST `
        -Body $finishTransformBody `
        -ContentType "application/json" | Out-Null

    Write-Host "? Transform conclu?do - 9.500 registros" -ForegroundColor Green
    Write-Host ""

    # ====================================================================
    # 4. STEP 3: LOAD
    # ====================================================================
    Write-Host "?? STEP 3: LOAD" -ForegroundColor Yellow
    Write-Host "------------------------------------------------------------"

    $loadBody = @{
        stepName = "Load"
        stepOrder = 3
        stepMessage = "Iniciando carga..."
    } | ConvertTo-Json

    $loadResponse = Invoke-RestMethod -Uri "$apiUrl/api/jobs/$executionId/details/start" `
        -Method POST `
        -Body $loadBody `
        -ContentType "application/json"

    $loadId = $loadResponse.detailId
    Write-Host "   DetailId: $loadId" -ForegroundColor Gray
    Write-Host "   Processando..." -ForegroundColor Gray
    Start-Sleep -Seconds 2

    $finishLoadBody = @{
        stepStatus = "Sucesso"
        stepMessage = "Carga conclu?da"
        rowsProcessed = 9500
        rowsInserted = 8000
        rowsUpdated = 1500
        rowsFailed = 0
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$apiUrl/api/jobs/details/$loadId/finish" `
        -Method POST `
        -Body $finishLoadBody `
        -ContentType "application/json" | Out-Null

    Write-Host "? Load conclu?do:" -ForegroundColor Green
    Write-Host "   - Inseridos: 8.000" -ForegroundColor Green
    Write-Host "   - Atualizados: 1.500" -ForegroundColor Green
    Write-Host ""

    # ====================================================================
    # 5. FINALIZAR JOB
    # ====================================================================
    $finishJobBody = @{
        status = "Sucesso"
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$apiUrl/api/jobs/$executionId/finish" `
        -Method POST `
        -Body $finishJobBody `
        -ContentType "application/json" | Out-Null

    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host "? JOB FINALIZADO COM SUCESSO!" -ForegroundColor Green
    Write-Host "   ExecutionId: $executionId" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Cyan

} catch {
    Write-Host ""
    Write-Host "? ERRO: $($_.Exception.Message)" -ForegroundColor Red
    
    # Marcar job como falha
    $failBody = @{
        status = "Falha"
        errorMessage = $_.Exception.Message
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$apiUrl/api/jobs/$executionId/finish" `
        -Method POST `
        -Body $failBody `
        -ContentType "application/json" | Out-Null

    Write-Host "? Falha registrada no sistema" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Para ver os resultados, acesse:" -ForegroundColor Cyan
Write-Host "  GET $apiUrl/api/jobs/$executionId" -ForegroundColor Gray
Write-Host "  GET $apiUrl/api/jobs/$executionId/details" -ForegroundColor Gray
Write-Host ""
