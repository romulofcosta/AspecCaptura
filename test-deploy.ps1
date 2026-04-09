# Script de teste para validar as correções de deploy
# Uso: .\test-deploy.ps1 -ApiUrl "https://pwa-camera-poc-api.onrender.com" -FrontendUrl "https://pwa-camera-poc-blazor.pages.dev"

param(
    [string]$ApiUrl = "https://pwa-camera-poc-api.onrender.com",
    [string]$FrontendUrl = "https://pwa-camera-poc-blazor.pages.dev",
    [string]$PreviewUrl = "https://e82ab59d.pwa-camera-poc-blazor.pages.dev"
)

# Cores para output
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$White = [System.ConsoleColor]::White

# Contador de testes
$TotalTests = 0
$PassedTests = 0
$FailedTests = 0

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Testes de Deploy - PWA Camera PoC" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "API URL: $ApiUrl"
Write-Host "Frontend URL: $FrontendUrl"
Write-Host "Preview URL: $PreviewUrl"
Write-Host ""

# Função para executar teste
function Run-Test {
    param(
        [string]$TestName,
        [scriptblock]$TestCommand
    )
    
    $script:TotalTests++
    Write-Host "[$script:TotalTests] $TestName... " -NoNewline
    
    try {
        $result = & $TestCommand
        if ($result) {
            Write-Host "✓ PASSOU" -ForegroundColor $Green
            $script:PassedTests++
            return $true
        } else {
            Write-Host "✗ FALHOU" -ForegroundColor $Red
            $script:FailedTests++
            return $false
        }
    } catch {
        Write-Host "✗ FALHOU" -ForegroundColor $Red
        Write-Host "  Erro: $($_.Exception.Message)" -ForegroundColor $Red
        $script:FailedTests++
        return $false
    }
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Iniciando Testes" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Teste 1: Health Check
Run-Test "Health Check da API" {
    try {
        $response = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -ErrorAction Stop
        return $response.status -eq "healthy"
    } catch {
        return $false
    }
}

# Teste 2: CORS - Domínio Principal
Write-Host ""
Write-Host "[$($TotalTests + 1)] Testando CORS - Domínio Principal..."
$TotalTests++
try {
    $headers = @{
        "Origin" = $FrontendUrl
        "Access-Control-Request-Method" = "POST"
        "Access-Control-Request-Headers" = "Content-Type"
    }
    
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/auth/login" -Method Options -Headers $headers -ErrorAction Stop
    
    if ($response.Headers["Access-Control-Allow-Origin"]) {
        Write-Host "✓ PASSOU" -ForegroundColor $Green
        Write-Host "  Headers CORS encontrados:"
        $response.Headers.GetEnumerator() | Where-Object { $_.Key -like "Access-Control*" } | ForEach-Object {
            Write-Host "    $($_.Key): $($_.Value)"
        }
        $PassedTests++
    } else {
        Write-Host "✗ FALHOU" -ForegroundColor $Red
        Write-Host "  Headers CORS não encontrados na resposta"
        $FailedTests++
    }
} catch {
    Write-Host "✗ FALHOU" -ForegroundColor $Red
    Write-Host "  Erro: $($_.Exception.Message)" -ForegroundColor $Red
    $FailedTests++
}

# Teste 3: CORS - Subdomínio de Preview
Write-Host ""
Write-Host "[$($TotalTests + 1)] Testando CORS - Subdomínio de Preview..."
$TotalTests++
try {
    $headers = @{
        "Origin" = $PreviewUrl
        "Access-Control-Request-Method" = "POST"
        "Access-Control-Request-Headers" = "Content-Type"
    }
    
    $response = Invoke-WebRequest -Uri "$ApiUrl/api/auth/login" -Method Options -Headers $headers -ErrorAction Stop
    
    if ($response.Headers["Access-Control-Allow-Origin"]) {
        Write-Host "✓ PASSOU" -ForegroundColor $Green
        Write-Host "  Headers CORS encontrados para preview:"
        $response.Headers.GetEnumerator() | Where-Object { $_.Key -like "Access-Control*" } | ForEach-Object {
            Write-Host "    $($_.Key): $($_.Value)"
        }
        $PassedTests++
    } else {
        Write-Host "✗ FALHOU" -ForegroundColor $Red
        Write-Host "  Headers CORS não encontrados para preview"
        $FailedTests++
    }
} catch {
    Write-Host "✗ FALHOU" -ForegroundColor $Red
    Write-Host "  Erro: $($_.Exception.Message)" -ForegroundColor $Red
    $FailedTests++
}

# Teste 4: Swagger UI (se disponível)
Run-Test "Swagger UI acessível" {
    try {
        $response = Invoke-WebRequest -Uri "$ApiUrl/swagger/index.html" -Method Get -ErrorAction Stop
        return $response.Content -match "Swagger UI"
    } catch {
        return $false
    }
}

# Teste 5: Endpoint de login existe
Run-Test "Endpoint de login existe" {
    try {
        $body = @{} | ConvertTo-Json
        $response = Invoke-WebRequest -Uri "$ApiUrl/api/auth/login" -Method Post -Body $body -ContentType "application/json" -ErrorAction Stop
        return $false # Não deveria chegar aqui com body vazio
    } catch {
        # Esperamos 400 ou 401 com body vazio
        return $_.Exception.Response.StatusCode -in @(400, 401)
    }
}

# Teste 6: Frontend está acessível
Run-Test "Frontend está acessível" {
    try {
        $response = Invoke-WebRequest -Uri $FrontendUrl -Method Get -ErrorAction Stop
        return $response.Content -match "Aspec Captura"
    } catch {
        return $false
    }
}

# Teste 7: Service Worker está presente
Run-Test "Service Worker está presente" {
    try {
        $response = Invoke-WebRequest -Uri "$FrontendUrl/service-worker.js" -Method Get -ErrorAction Stop
        return $response.Content -match "self.addEventListener"
    } catch {
        return $false
    }
}

# Teste 8: Manifest.json está presente
Run-Test "Manifest.json está presente" {
    try {
        $response = Invoke-RestMethod -Uri "$FrontendUrl/manifest.json" -Method Get -ErrorAction Stop
        return $response.name -match "Aspec Captura"
    } catch {
        return $false
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Resumo dos Testes" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total de testes: $TotalTests"
Write-Host "Testes passados: $PassedTests" -ForegroundColor $Green
Write-Host "Testes falhados: $FailedTests" -ForegroundColor $Red
Write-Host ""

if ($FailedTests -eq 0) {
    Write-Host "✓ Todos os testes passaram!" -ForegroundColor $Green
    Write-Host ""
    Write-Host "Próximos passos:"
    Write-Host "1. Teste o login manualmente no navegador"
    Write-Host "2. Teste a sincronização de dados"
    Write-Host "3. Verifique os logs da API no Render"
    exit 0
} else {
    Write-Host "✗ Alguns testes falharam" -ForegroundColor $Red
    Write-Host ""
    Write-Host "Ações recomendadas:"
    Write-Host "1. Verifique os logs da API no Render"
    Write-Host "2. Confirme que as variáveis de ambiente estão configuradas"
    Write-Host "3. Verifique se o deploy foi concluído com sucesso"
    Write-Host "4. Consulte o arquivo RENDER_DEPLOY_FIX.md"
    exit 1
}
