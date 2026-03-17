@echo off
REM Script para executar testes com cobertura de código no Windows
REM Uso: run-tests-with-coverage.bat [watch]

echo 🧪 Executando testes com cobertura de código...

REM Limpar resultados anteriores
if exist TestResults rmdir /s /q TestResults
if exist CoverageReports rmdir /s /q CoverageReports

REM Executar testes com cobertura
if "%1"=="watch" (
    echo 📊 Modo watch ativado - executando testes continuamente...
    dotnet watch test --settings coverlet.runsettings --collect:"XPlat Code Coverage" --logger trx --results-directory TestResults/
) else (
    echo 📊 Executando testes uma vez...
    dotnet test --settings coverlet.runsettings --collect:"XPlat Code Coverage" --logger trx --results-directory TestResults/
    
    echo 📈 Gerando relatório de cobertura...
    
    REM Instalar ReportGenerator se necessário
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.0 2>nul
    
    REM Encontrar arquivo de cobertura
    for /r TestResults %%i in (coverage.cobertura.xml) do set COVERAGE_FILE=%%i
    
    if defined COVERAGE_FILE (
        REM Gerar relatório HTML
        reportgenerator -reports:"%COVERAGE_FILE%" -targetdir:"CoverageReports" -reporttypes:"Html;TextSummary"
        
        echo ✅ Relatório de cobertura gerado em: CoverageReports\index.html
        echo 📊 Resumo da cobertura:
        type CoverageReports\Summary.txt
    ) else (
        echo ⚠️  Arquivo de cobertura não encontrado
    )
)

echo 🎉 Execução concluída!