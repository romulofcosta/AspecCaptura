#!/bin/bash

# Script para executar testes com cobertura de código
# Uso: ./run-tests-with-coverage.sh [--watch]

set -e

echo "🧪 Executando testes com cobertura de código..."

# Limpar resultados anteriores
rm -rf TestResults/
rm -rf CoverageReports/

# Executar testes com cobertura
if [ "$1" = "--watch" ]; then
    echo "📊 Modo watch ativado - executando testes continuamente..."
    dotnet watch test --settings coverlet.runsettings --collect:"XPlat Code Coverage" --logger trx --results-directory TestResults/
else
    echo "📊 Executando testes uma vez..."
    dotnet test --settings coverlet.runsettings --collect:"XPlat Code Coverage" --logger trx --results-directory TestResults/
fi

# Gerar relatório HTML se não estiver em modo watch
if [ "$1" != "--watch" ]; then
    echo "📈 Gerando relatório de cobertura..."
    
    # Encontrar o arquivo de cobertura mais recente
    COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" | head -1)
    
    if [ -n "$COVERAGE_FILE" ]; then
        # Gerar relatório HTML
        dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.0 || true
        reportgenerator -reports:"$COVERAGE_FILE" -targetdir:"CoverageReports" -reporttypes:"Html;TextSummary"
        
        echo "✅ Relatório de cobertura gerado em: CoverageReports/index.html"
        echo "📊 Resumo da cobertura:"
        cat CoverageReports/Summary.txt
    else
        echo "⚠️  Arquivo de cobertura não encontrado"
    fi
fi

echo "🎉 Execução concluída!"