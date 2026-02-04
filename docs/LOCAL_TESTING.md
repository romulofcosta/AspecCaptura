# Guia de Teste Local

Com a migração para a arquitetura BFF, o PWA não utiliza mais chaves AWS diretamente. Siga estes passos para configurar seu ambiente de teste local:

## 🔌 Configuração da API BFF
1. Navegue até o projeto `pwa-camera-poc-api`.
2. Configure suas credenciais AWS no `appsettings.json` da API (consulte o README da API).
3. Inicie a API com `dotnet run`. Por padrão, ela rodará em `http://localhost:5069`.

## 📱 Configuração do PWA
1. No projeto `pwa-camera-poc-blazor`, verifique o arquivo `wwwroot/appsettings.json`.
2. O campo `ApiBaseUrl` deve apontar para o endereço da sua API local:
   ```json
   {
     "ApiBaseUrl": "http://localhost:5069"
   }
   ```
3. Inicie o PWA com `dotnet run`.

## 🧪 Roteiro de Testes

### 1. Login e Unidade
- Faça login com `admin`/`admin`.
- **Importante**: Selecione uma Unidade Organizadora no Drawer (Menu Lateral). Sem isso, o OCR não validará os itens.

### 2. Scanner OCR
- Vá para a tela de Câmera.
- Use a função "Escanear Patrimônio" (Ícone de Mira).
- Aponte para um código de patrimônio que conste no arquivo `/sample-data/unit-{id}-inventory.json` da unidade selecionada.
- Verifique se os dados (Nome, Código) são preenchidos automaticamente após a validação bem-sucedida.

### 3. Sincronização
- Capture um item e salve-o.
- Note o ícone de nuvem cinza (pendente).
- Vá em `/sync` e clique em "Sincronizar Tudo".
- Verifique no console do navegador (F12) se as chamadas para a API BFF estão retornando `200 OK`.
- Verifique se o ícone na Home mudou para verde.

## 📁 Verificação no S3
Os arquivos devem aparecer no bucket configurado na API seguindo a estrutura:
- `capturas/{itemId}/foto-1.jpg`
- `capturas/{itemId}/metadata.json`
