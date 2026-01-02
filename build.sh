#!/usr/bin/env bash
# Build script universal - Netlify e Cloudflare Pages
set -e

# Detectar plataforma
if [ -n "$NETLIFY" ]; then
    PLATFORM="netlify"
    OUTPUT_DIR="bin/Release/net8.0/publish"
elif [ -n "$CF_PAGES" ]; then
    PLATFORM="cloudflare"
    OUTPUT_DIR="bin/Release/net8.0/publish/wwwroot"
else
    # Local/padrão
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

echo "=== Restaurando e publicando ==="
"$DOTNET_EXEC" restore
rm -rf bin/Release/net8.0/publish
"$DOTNET_EXEC" publish pwa-camera-poc-blazor.csproj -c Release -o bin/Release/net8.0/publish

# Ajustes específicos por plataforma
if [ "$PLATFORM" = "netlify" ]; then
    echo "=== Ajustando para Netlify ==="
    cp -rv bin/Release/net8.0/publish/wwwroot/* bin/Release/net8.0/publish/
    
    # _redirects na raiz para Netlify
    cat > bin/Release/net8.0/publish/_redirects << 'EOF'
/*    /index.html   200
EOF

elif [ "$PLATFORM" = "cloudflare" ] || [ "$PLATFORM" = "local" ]; then
    echo "=== Ajustando para Cloudflare Pages ==="
    
    # _redirects dentro de wwwroot
    cat > bin/Release/net8.0/publish/wwwroot/_redirects << 'EOF'
/*    /index.html   200
EOF

    # _headers dentro de wwwroot
    cat > bin/Release/net8.0/publish/wwwroot/_headers << 'EOF'
/*
  X-Frame-Options: DENY
  X-Content-Type-Options: nosniff
/_framework/*
  Cache-Control: public, max-age=31536000, immutable
/service-worker.js
  Cache-Control: no-cache
EOF
fi

echo "=== Build concluído! Saída: $OUTPUT_DIR ==="
ls -lh "$OUTPUT_DIR" | head -15