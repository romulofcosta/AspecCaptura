# Relatório de Análise Técnica: PWA Camera POC
**Data:** 09/01/2026
**Status:** Validação de Sincronização e Login
**Autor:** Antigravity Agent

## 1. Resumo Executivo
Foi realizada uma bateria de testes "end-to-end" automatizados focando no fluxo principal do usuário: **Login -> Captura Offline -> Sincronização Online**. 

Os testes identificaram dois pontos de obstrução:
1.  **Bloqueio de Validação no Login (UX/Fluxo):** Impede o uso das credenciais padrão de teste via UI.
2.  **Falha Crítica no Upload S3 (Infraestrutura/Runtime):** O AWS SDK para .NET lançou exceção de plataforma não suportada ao tentar enviar arquivos do navegador (WASM).

---

## 2. Detalhamento dos Problemas

### 2.1. Bloqueio de Validação de Login
*   **Ocorrência:** Ao tentar logar com o usuário `admin` (definido no `AuthService.cs`), a interface `Login.razor` bloqueia o envio do formulário.
*   **Causa:** O campo está anotado com `[EmailAddress]`, mas o usuário de teste `admin` não é um e-mail válido.
*   **Solução Tática Aplicada (Teste):** Remoção temporária da anotação e alteração do `InputType` para bypass.
*   **Recomendação:**
    *   Alterar usuário padrão para `admin@aspec.com`; OU
    *   Remover validação de formato de e-mail do `LoginModel`, permitindo usernames simples.

### 2.2. Incompatibilidade do AWS SDK em WebAssembly
*   **Ocorrência:** Falha ao executar `Sincronizar Tudo Agora` na página `/sync`.
*   **Erro Capturado:** `PlatformNotSupportedException: Operation is not supported on this platform.`
*   **Contexto:** O erro ocorreu dentro do método `PutObjectAsync` do `AmazonS3Client`.
*   **Análise Técnica:** 
    *   O Blazor WebAssembly roda em um ambiente "sandbox" restrito (browser).
    *   O AWS SDK for .NET padrão tenta realizar operações criptográficas (assinatura de payload SHA256) ou de rede (sockets diretos) que não são totalmente mapeadas para as APIs do navegador pelo runtime Mono/WASM atual.
    *   O erro "Operation is not supported" é sintomático de chamadas do sistema (syscalls) ausentes no navegador.

---

## 3. Soluções Alternativas Propostas (Para Validação)

Solicito validação das seguintes arquiteturas para contornar a limitação do AWS SDK no Browser:

### Opção A: Upload via Pre-Signed URLs (Recomendada para Serverless/Client-Side)
Desacopla o SDK pesado do cliente leve.
1.  **Fluxo:**
    *   O App Blazor solicita uma URL de upload para uma API (Azure Function / Lambda / Backend API).
    *   A API (que possui as credenciais AWS) gera uma **Pre-Signed URL** do S3 com validade curta (ex: 5 min).
    *   O App Blazor usa `System.Net.Http.HttpClient` padrão para fazer um `PUT` direto nessa URL com o binário da imagem.
2.  **Vantagens:** Remove dependência total do AWS SDK no WASM; nativo do navegador; seguro (credenciais ficam no server).

### Opção B: Proxy de Upload (Backend)
1.  **Fluxo:** 
    *   O App envia a imagem para o Backend (ASP.NET Core API).
    *   O Backend recebe e retransmite para o S3.
2.  **Vantagens:** Controle total.
3.  **Desvantagens:** Dobro de tráfego (Cliente -> Server -> S3); latência maior.

### Opção C: Configuração Específica do SDK (Tentativa de Fix)
Tentar configurar o `AmazonS3Client` para ser "WASM-friendly", embora instável em algumas versões.
*   Configurar `AmazonS3Config`:
    *   `UseHttp = true` (se for problema de SSL handshake, improvável hoje em dia).
    *   `DisablePayloadSigning = true` (anularia a necessidade de hash SHA256 pesado, mas reduz segurança e requer bucket configuration adequada).

---

---

## 5. Resolução das Obstruções (Atualizado em 16/01/2026)

### 5.1. Solução para Upload S3
Foi implementada a **Opção A (Pre-Signed URLs)** através da criação da API `pwa-camera-poc-api`. O fluxo agora é:
1. PWA solicita URL ao BFF.
2. BFF (com credenciais seguras) gera URL assinada.
3. PWA faz PUT direto no S3.
**Resultado:** Sucesso. O AWS SDK foi removido do projeto Blazor, reduzindo o tamanho do bundle e eliminando problemas de compatibilidade WASM.

### 5.2. Solução para o Login
A tela de login foi ajustada para suportar tanto e-mails quanto usernames simples, facilitando os testes com a conta `admin`.

---

## 6. Conclusão Final
O sistema agora está estável para captura e sincronização online/offline utilizando a arquitetura BFF. Próximas baterias de teste devem focar na performance do OCR em dispositivos de baixa gama.
