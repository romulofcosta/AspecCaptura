---
description: Como configurar e rodar o ambiente de desenvolvimento local (API + PWA)
---

Este fluxo descreve os passos para rodar o ecossistema ASPEC Capture localmente.

### Pré-requisitos
- .NET 8.0 SDK instalado
- Visual Studio ou VS Code

### Passo 1: Executar a API BFF
A API deve ser iniciada primeiro para que o PWA possa se comunicar com ela.

// turbo
1. Em um terminal, navegue até a pasta da API e execute:
```powershell
cd pwa-camera-poc-api
dotnet run
```
A API estará disponível por padrão em `http://localhost:5069`.

### Passo 2: Executar o PWA Blazor
// turbo
1. Em outro terminal, navegue até a pasta do PWA e execute:
```powershell
cd pwa-camera-poc-blazor
dotnet run
```
O PWA estará disponível em `http://localhost:5230`.

### Passo 3: Configuração de Teste
1. Acesse `http://localhost:5230` no navegador.
2. Faça login com o usuário corporativo padrão:
   - **Usuário**: `admin`
   - **Senha**: `admin`
3. Certifique-se de selecionar uma **Unidade Organizadora** no menu lateral antes de iniciar a captura OCR.
