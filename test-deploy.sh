#!/bin/bash

# Script de teste para validar as correções de deploy
# Uso: ./test-deploy.sh <API_URL> <FRONTEND_URL>

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Variáveis
API_URL="${1:-https://pwa-camera-poc-api.onrender.com}"
FRONTEND_URL="${2:-https://pwa-camera-poc-blazor.pages.dev}"
PREVIEW_URL="${3:-https://e82ab59d.pwa-camera-poc-blazor.pages.dev}"

echo "=========================================="
echo "  Testes de Deploy - PWA Camera PoC"
echo "=========================================="
echo ""
echo "API URL: $API_URL"
echo "Frontend URL: $FRONTEND_URL"
echo "Preview URL: $PREVIEW_URL"
echo ""

# Contador de testes
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Função para executar teste
run_test() {
    local test_name="$1"
    local test_command="$2"
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    echo -n "[$TOTAL_TESTS] $test_name... "
    
    if eval "$test_command" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ PASSOU${NC}"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        return 0
    else
        echo -e "${RED}✗ FALHOU${NC}"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        return 1
    fi
}

# Função para executar teste com output
run_test_with_output() {
    local test_name="$1"
    local test_command="$2"
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    echo "[$TOTAL_TESTS] $test_name..."
    
    if eval "$test_command"; then
        echo -e "${GREEN}✓ PASSOU${NC}"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        return 0
    else
        echo -e "${RED}✗ FALHOU${NC}"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        return 1
    fi
}

echo "=========================================="
echo "  Iniciando Testes"
echo "=========================================="
echo ""

# Teste 1: Health Check
run_test "Health Check da API" \
    "curl -s -f $API_URL/health | grep -q 'healthy'"

# Teste 2: CORS - Domínio Principal
echo ""
echo "[$((TOTAL_TESTS + 1))] Testando CORS - Domínio Principal..."
CORS_RESPONSE=$(curl -s -I \
    -H "Origin: $FRONTEND_URL" \
    -H "Access-Control-Request-Method: POST" \
    -H "Access-Control-Request-Headers: Content-Type" \
    -X OPTIONS \
    "$API_URL/api/auth/login")

TOTAL_TESTS=$((TOTAL_TESTS + 1))
if echo "$CORS_RESPONSE" | grep -q "Access-Control-Allow-Origin"; then
    echo -e "${GREEN}✓ PASSOU${NC}"
    echo "  Headers CORS encontrados:"
    echo "$CORS_RESPONSE" | grep "Access-Control" | sed 's/^/    /'
    PASSED_TESTS=$((PASSED_TESTS + 1))
else
    echo -e "${RED}✗ FALHOU${NC}"
    echo "  Headers CORS não encontrados na resposta"
    FAILED_TESTS=$((FAILED_TESTS + 1))
fi

# Teste 3: CORS - Subdomínio de Preview
echo ""
echo "[$((TOTAL_TESTS + 1))] Testando CORS - Subdomínio de Preview..."
CORS_PREVIEW_RESPONSE=$(curl -s -I \
    -H "Origin: $PREVIEW_URL" \
    -H "Access-Control-Request-Method: POST" \
    -H "Access-Control-Request-Headers: Content-Type" \
    -X OPTIONS \
    "$API_URL/api/auth/login")

TOTAL_TESTS=$((TOTAL_TESTS + 1))
if echo "$CORS_PREVIEW_RESPONSE" | grep -q "Access-Control-Allow-Origin"; then
    echo -e "${GREEN}✓ PASSOU${NC}"
    echo "  Headers CORS encontrados para preview:"
    echo "$CORS_PREVIEW_RESPONSE" | grep "Access-Control" | sed 's/^/    /'
    PASSED_TESTS=$((PASSED_TESTS + 1))
else
    echo -e "${RED}✗ FALHOU${NC}"
    echo "  Headers CORS não encontrados para preview"
    FAILED_TESTS=$((FAILED_TESTS + 1))
fi

# Teste 4: Swagger UI (se disponível)
run_test "Swagger UI acessível" \
    "curl -s -f $API_URL/swagger/index.html | grep -q 'Swagger UI'"

# Teste 5: Endpoint de login existe
run_test "Endpoint de login existe" \
    "curl -s -o /dev/null -w '%{http_code}' -X POST $API_URL/api/auth/login -H 'Content-Type: application/json' -d '{}' | grep -q '400\|401'"

# Teste 6: Frontend está acessível
run_test "Frontend está acessível" \
    "curl -s -f $FRONTEND_URL | grep -q 'Aspec Captura'"

# Teste 7: Service Worker está presente
run_test "Service Worker está presente" \
    "curl -s -f $FRONTEND_URL/service-worker.js | grep -q 'self.addEventListener'"

# Teste 8: Manifest.json está presente
run_test "Manifest.json está presente" \
    "curl -s -f $FRONTEND_URL/manifest.json | grep -q 'Aspec Captura'"

echo ""
echo "=========================================="
echo "  Resumo dos Testes"
echo "=========================================="
echo ""
echo "Total de testes: $TOTAL_TESTS"
echo -e "${GREEN}Testes passados: $PASSED_TESTS${NC}"
echo -e "${RED}Testes falhados: $FAILED_TESTS${NC}"
echo ""

if [ $FAILED_TESTS -eq 0 ]; then
    echo -e "${GREEN}✓ Todos os testes passaram!${NC}"
    echo ""
    echo "Próximos passos:"
    echo "1. Teste o login manualmente no navegador"
    echo "2. Teste a sincronização de dados"
    echo "3. Verifique os logs da API no Render"
    exit 0
else
    echo -e "${RED}✗ Alguns testes falharam${NC}"
    echo ""
    echo "Ações recomendadas:"
    echo "1. Verifique os logs da API no Render"
    echo "2. Confirme que as variáveis de ambiente estão configuradas"
    echo "3. Verifique se o deploy foi concluído com sucesso"
    echo "4. Consulte o arquivo RENDER_DEPLOY_FIX.md"
    exit 1
fi
