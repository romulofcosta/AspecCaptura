#!/usr/bin/env bash
# Script para instalar o .NET 8 e publicar o projeto Blazor PWA no Netlify

# Interromper em caso de erro
set -e

echo "Iniciando instalação do .NET 8..."
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Configurar variáveis de ambiente para o dotnet
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

echo "Verificando versão do dotnet:"
dotnet --version

echo "Executando dotnet publish..."
dotnet publish pwa-camera-poc-blazor.csproj -c Release
