#!/usr/bin/env bash
# Script para instalar o .NET 8 e publicar o projeto Blazor PWA no Netlify

# Interromper em caso de erro
set -e

echo "=== Iniciando instalação do .NET SDK 8.0.416 ==="
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 8.0.416 --install-dir "$PWD/.dotnet"

# Configurar variáveis de ambiente para o dotnet
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
hash -r

echo "=== Verificando binário dotnet em uso ==="
which dotnet
DOTNET_VERSION=$(dotnet --version || echo "não detectada")
echo "Versão detectada: $DOTNET_VERSION (esperado: 8.0.416 ou compatível)"

echo "=== SDKs instalados localmente ==="
ls $DOTNET_ROOT/sdk || echo "Nenhum SDK encontrado em $DOTNET_ROOT/sdk"

# Se não encontrar o SDK 8.0.416, aborta explicitamente
if [ ! -d "$DOTNET_ROOT/sdk/8.0.416" ]; then
    echo "Erro: SDK 8.0.416 não foi instalado corretamente."
    exit 1
fi

echo "=== Executando dotnet restore ==="
dotnet restore

echo "=== Limpando diretórios de publicação antigos ==="
rm -rf bin/Release/net8.0/publish

echo "=== Executando dotnet publish ==="
dotnet publish pwa-camera-poc-blazor.csproj -c Release -o bin/Release/net8.0/publish

echo "=== Ajustando estrutura de arquivos para o Netlify ==="
if [ -d "bin/Release/net8.0/publish/wwwroot" ]; then
    echo "Movendo conteúdo de wwwroot para a raiz..."
    cp -rv bin/Release/net8.0/publish/wwwroot/* bin/Release/net8.0/publish/
    # rm -rf bin/Release/net8.0/publish/wwwroot
fi

echo "=== Conteúdo final do diretório de publicação ==="
ls -F bin/Release/net8.0/publish/

echo "=== Build concluído com sucesso ==="
