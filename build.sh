#!/usr/bin/env bash
# Build script universal - Netlify e Cloudflare Pages
set -e

# Detectar versão do último commit com tag
VERSION=$(git log --oneline | grep -oE 'v[0-9]+\.[0-9]+\.[0-9]+' | head -1)
if [ -z "$VERSION" ]; then
    VERSION="v0.2.2"
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
    echo "Aviso: API_BASE_URL não definida. Usando padrão local: http://localhost:5069"
    API_BASE_URL="http://localhost:5069"
fi

# Substituir nos arquivos FONTE (antes do publish)
sed -i "s|__API_BASE_URL__|$API_BASE_URL|g" wwwroot/appsettings.json
sed -i "s|__APP_VERSION__|$VERSION|g" wwwroot/index.html

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