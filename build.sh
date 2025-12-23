#!/usr/bin/env bash
# Script para instalar o .NET 8 e publicar o projeto Blazor PWA no Netlify

# Interromper em caso de erro
set -e

echo "=== Iniciando instalação do .NET SDK 8.0.416 ==="
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.416 --install-dir "$PWD/.dotnet"

# Configurar variáveis de ambiente para o dotnet
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

# Criar alias para garantir o uso do dotnet local
DOTNET_EXEC="$DOTNET_ROOT/dotnet"

echo "=== Verificando binário dotnet em uso ==="
echo "Caminho do dotnet local: $DOTNET_EXEC"
if [ -f "$DOTNET_EXEC" ]; then
    echo "Binário encontrado!"
    DOTNET_VERSION=$($DOTNET_EXEC --version 2>&1)
    echo "Versão detectada: $DOTNET_VERSION"
else
    echo "ERRO: Binário dotnet não encontrado em $DOTNET_EXEC"
    exit 1
fi

echo "=== SDKs instalados localmente ==="
ls -la "$DOTNET_ROOT/sdk" 2>/dev/null || echo "Nenhum SDK encontrado"

# Verificar se o SDK 8.0.416 existe
if [ ! -d "$DOTNET_ROOT/sdk/8.0.416" ]; then
    echo "Erro: SDK 8.0.416 não foi instalado corretamente."
    exit 1
fi

echo "=== Executando dotnet restore ==="
$DOTNET_EXEC restore

echo "=== Limpando diretórios de publicação antigos ==="
rm -rf bin/Release/net8.0/publish

echo "=== Executando dotnet publish ==="
$DOTNET_EXEC publish pwa-camera-poc-blazor.csproj -c Release -o bin/Release/net8.0/publish

echo "=== Ajustando estrutura de arquivos para o Netlify ==="
if [ -d "bin/Release/net8.0/publish/wwwroot" ]; then
    echo "Movendo conteúdo de wwwroot para a raiz..."
    cp -rv bin/Release/net8.0/publish/wwwroot/* bin/Release/net8.0/publish/
    # rm -rf bin/Release/net8.0/publish/wwwroot
fi

echo "=== Conteúdo final do diretório de publicação ==="
ls -F bin/Release/net8.0/publish/

echo "=== Build concluído com sucesso ==="