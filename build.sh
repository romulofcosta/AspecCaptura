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

echo "=== Verificando versão do dotnet instalada ==="
DOTNET_VERSION=$(dotnet --version || echo "não detectada")
echo "Versão detectada: $DOTNET_VERSION (esperado: 8.0.416 ou compatível)"

echo "=== Executando dotnet restore ==="
dotnet restore

echo "=== Limpando diretórios de publicação antigos ==="
rm -rf bin/Release/net8.0/publish

echo "=== Executando dotnet publish ==="
dotnet publish pwa-camera-poc-blazor.csproj -c Release -o bin/Release/net8.0/publish

echo "=== Ajustando estrutura de arquivos para o Netlify ==="
# Garante que os arquivos do framework e assets estejam na raiz do diretório de publicação
if [ -d "bin/Release/net8.0/publish/wwwroot" ]; then
    echo "Movendo conteúdo de wwwroot para a raiz..."
    cp -rv bin/Release/net8.0/publish/wwwroot/* bin/Release/net8.0/publish/
    # Opcional: remover a pasta wwwroot vazia para evitar confusão
    # rm -rf bin/Release/net8.0/publish/wwwroot
fi

echo "=== Conteúdo final do diretório de publicação ==="
ls -F bin/Release/net8.0/publish/

echo "=== Build concluído com sucesso ==="
