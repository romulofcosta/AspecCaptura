#!/usr/bin/env bash
# Script para instalar o .NET 8 e publicar o projeto Blazor PWA no Netlify

# Interromper em caso de erro
set -e

echo "Iniciando instalação do .NET 8..."
# Instala o .NET no diretório .dotnet na raiz do projeto para persistência relativa
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --install-dir "$PWD/.dotnet"

# Configurar variáveis de ambiente para o dotnet
export DOTNET_ROOT="$PWD/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"

echo "Verificando versão do dotnet:"
dotnet --version

echo "Executando dotnet publish..."
# Publica o projeto no diretório específico solicitado
dotnet publish pwa-camera-poc-blazor.csproj -c Release -o bin/Release/net8.0/publish

echo "Ajustando estrutura de arquivos para o Netlify..."
# Move o conteúdo de wwwroot para a raiz do diretório de publicação
# Isso garante que index.html e _framework fiquem no local esperado pelo Netlify
cp -r bin/Release/net8.0/publish/wwwroot/* bin/Release/net8.0/publish/

echo "Build concluído com sucesso."
