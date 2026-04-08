#!/usr/bin/env bash
# Build script universal - Netlify e Cloudflare Pages
set -e

# Detectar versão do último commit com tag
VERSION=$(git log --oneline | grep -oE 'v[0-9]+\.[0-9]+\.[0-9]+' | head -1)
if [ -z "$VERSION" ]; then
    VERSION="v0.8.0"
fi

echo "=== Versão detectada: $VERSION ==="

# Detectar plataforma
if [ -n "$NETLIFY" ]; then
    PLATFORM="netlify"
    OUTPUT_DIR="bin/Release/net8.0/publish"
elif [ -n "$CF_PAGES" ]; then
    PLATFORM="cloudflare"
    OUTPUT_DIR="bin/Release/net8.0/publish/wwwroot"
else
    PLATFORM="local"
    OUTPUT_DIR="bin/Release/net8.0/publish/wwwroot"
fi

echo "=== Plataforma detectada: $PLATFORM ==="

# Instalar .NET se necessário
if ! command -v dotnet &> /dev/null; then
    echo "Instalando .NET SDK 8.0.416..."
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.416 --install-dir "$PWD/.dotnet"
    export DOTNET_ROOT="$PWD/.dotnet"
    export PATH="$DOTNET_ROOT:$PATH"
    DOTNET_EXEC="$DOTNET_ROOT/dotnet"
    chmod +x "$DOTNET_EXEC"
else
    DOTNET_EXEC="dotnet"
fi

echo "=== Substituindo variáveis ANTES do build ==="
if [ -z "$API_BASE_URL" ]; then
    echo "⚠️  Aviso: API_BASE_URL não definida. Usando padrão local: http://localhost:5069"
    API_BASE_URL="http://localhost:5069"
else
    echo "✅ API_BASE_URL configurada: $API_BASE_URL"
fi

# Substituir nos arquivos FONTE (antes do publish)
echo "📝 Substituindo __API_BASE_URL__ por $API_BASE_URL em wwwroot/appsettings.json"
sed -i "s|__API_BASE_URL__|$API_BASE_URL|g" wwwroot/appsettings.json

# Verificar se a substituição funcionou
if grep -q "__API_BASE_URL__" wwwroot/appsettings.json; then
    echo "❌ ERRO: Falha ao substituir __API_BASE_URL__"
    exit 1
else
    echo "✅ Substituição bem-sucedida"
fi

echo "=== Restaurando pacotes e workloads ==="
"$DOTNET_EXEC" restore
"$DOTNET_EXEC" workload install wasm-tools --skip-manifest-update

echo "=== Instalando dependências Node/Tailwind CSS ==="
npm install

rm -rf bin/Release/net8.0/publish
"$DOTNET_EXEC" publish pwa-camera-poc-blazor.csproj -c Release -o bin/Release/net8.0/publish

# Ajustes específicos por plataforma
if [ "$PLATFORM" = "netlify" ]; then
    echo "=== Ajustando para Netlify ==="
    cp -rv bin/Release/net8.0/publish/wwwroot/* bin/Release/net8.0/publish/
    cat > bin/Release/net8.0/publish/_redirects << 'EOF'
/*    /index.html   200
EOF

elif [ "$PLATFORM" = "cloudflare" ] || [ "$PLATFORM" = "local" ]; then
    echo "=== Ajustando para Cloudflare Pages ==="
    
    cat > bin/Release/net8.0/publish/wwwroot/_headers << 'EOF'
/*
  X-Frame-Options: DENY
  X-Content-Type-Options: nosniff
  Referrer-Policy: strict-origin-when-cross-origin

/_framework/*
  Cache-Control: public, max-age=31536000, immutable

/service-worker.js
  Cache-Control: no-cache
  Content-Type: application/javascript

/service-worker-assets.js
  Cache-Control: no-cache
  Content-Type: application/javascript

/manifest.json
  Content-Type: application/json
EOF
    
    cat > bin/Release/net8.0/publish/wwwroot/_redirects << 'EOF'
/*   /index.html   200
EOF
fi

echo "=== Build concluído! Saída: $OUTPUT_DIR ==="
ls -lh "$OUTPUT_DIR" | head -15

echo ""
echo "=== Validações finais ==="

# Verificar se appsettings.json foi copiado corretamente
if [ -f "$OUTPUT_DIR/appsettings.json" ]; then
    echo "✅ appsettings.json encontrado no output"
    if grep -q "__API_BASE_URL__" "$OUTPUT_DIR/appsettings.json"; then
        echo "❌ ERRO: __API_BASE_URL__ ainda presente no output!"
        cat "$OUTPUT_DIR/appsettings.json"
        exit 1
    else
        echo "✅ API_BASE_URL configurada corretamente no output"
        echo "   Conteúdo: $(cat "$OUTPUT_DIR/appsettings.json")"
    fi
else
    echo "❌ ERRO: appsettings.json não encontrado no output!"
    exit 1
fi

# Verificar arquivos de configuração do Cloudflare
if [ "$PLATFORM" = "cloudflare" ] || [ "$PLATFORM" = "local" ]; then
    if [ -f "$OUTPUT_DIR/_headers" ]; then
        echo "✅ _headers criado"
    else
        echo "⚠️  _headers não encontrado"
    fi
    
    if [ -f "$OUTPUT_DIR/_redirects" ]; then
        echo "✅ _redirects criado"
    else
        echo "⚠️  _redirects não encontrado"
    fi
fi

echo ""
echo "🎉 Build validado com sucesso!"
echo "📦 Pronto para deploy no Cloudflare Pages"