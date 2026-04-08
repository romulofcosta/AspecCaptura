#!/bin/bash

echo "🔍 Verificando configuração de build do Cloudflare Pages"
echo ""

# Verificar se há arquivo de configuração local
if [ -f "wrangler.toml" ]; then
    echo "✅ Arquivo wrangler.toml encontrado"
    cat wrangler.toml
else
    echo "⚠️ Arquivo wrangler.toml não encontrado"
fi

echo ""
echo "📋 Para verificar qual build está sendo usado:"
echo ""
echo "1. Acesse: https://dash.cloudflare.com"
echo "2. Vá em: Workers & Pages > pwa-camera-poc-blazor"
echo "3. Clique em: Settings > Builds & deployments"
echo "4. Verifique:"
echo "   - Build command: qual script está configurado?"
echo "   - Build output directory: qual diretório?"
echo ""
echo "🔎 Verificando site em produção..."
echo ""

# Tentar baixar appsettings.json do site em produção
PROD_URL="https://pwa-camera-poc-blazor.pages.dev/appsettings.json"
echo "Baixando: $PROD_URL"
echo ""

if command -v curl &> /dev/null; then
    RESPONSE=$(curl -s "$PROD_URL")
    echo "Conteúdo do appsettings.json em produção:"
    echo "$RESPONSE"
    echo ""
    
    if echo "$RESPONSE" | grep -q "__API_BASE_URL__"; then
        echo "❌ PROBLEMA: A variável __API_BASE_URL__ NÃO foi substituída!"
        echo "   O build não está configurando a URL da API corretamente."
        echo ""
        echo "   Possíveis causas:"
        echo "   1. Script de build incorreto"
        echo "   2. Variável de ambiente API_BASE_URL não configurada"
        echo "   3. Ordem de execução incorreta no build"
    elif echo "$RESPONSE" | grep -q "http"; then
        API_URL=$(echo "$RESPONSE" | grep -oP '"ApiBaseUrl":\s*"\K[^"]+')
        echo "✅ SUCESSO: URL da API configurada corretamente!"
        echo "   API URL: $API_URL"
        echo ""
        
        # Testar se a API responde
        echo "🔗 Testando conexão com a API..."
        if curl -s -o /dev/null -w "%{http_code}" "$API_URL/health" | grep -q "200"; then
            echo "✅ API está respondendo corretamente!"
        else
            echo "⚠️ API não está respondendo ou está offline"
        fi
    else
        echo "⚠️ Formato inesperado no appsettings.json"
    fi
else
    echo "⚠️ curl não está instalado. Instale para verificar o site em produção."
fi

echo ""
echo "📊 Resumo dos scripts disponíveis:"
echo ""
echo "1. build.sh"
echo "   - Output: bin/Release/net8.0/publish/wwwroot"
echo "   - Substitui variáveis ANTES do build"
echo "   - Cria _headers e _redirects"
echo "   - ✅ RECOMENDADO"
echo ""
echo "2. build-production.sh"
echo "   - Output: dist/wwwroot"
echo "   - Substitui variáveis DEPOIS do build"
echo "   - Agora cria _headers e _redirects"
echo "   - ✅ Atualizado"
echo ""
echo "💡 Dica: Use 'build.sh' para maior confiabilidade"
