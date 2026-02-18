# Guia de Teste Local

Este guia descreve como testar a aplicação localmente. Devido à mudança para o modelo de **Acesso Provisionado**, o fluxo de teste requer a API Backend em execução.

## Pré-requisitos
- Executar o projeto `pwa-camera-poc-api` localmente.
- Configurar o `appsettings.json` ou `wwwroot/appsettings.json` para apontar para a URL da API (padrão: `http://localhost:5069`).

## Opção 2: Testar Sincronização

1. **Login:** Utilize as credenciais provisionadas na sua API local.
2. **Captura:**
   - Vá para a tela de Câmera (`/camera`).
   - Capture uma foto e salve um item.
   - Verifique se ele aparece na Home com ícone de nuvem **Cinza** (Pendente).
3. **Mudar de Usuário (Teste de Isolamento):**
   - Faça logout e login com outro usuário (ex: `user2`/`password`).
   - Verifique que o item criado pelo `admin` **NÃO** aparece na lista.
   - Crie um novo item para `user2`.
4. **Sincronização:**
   - Volte para o usuário que tem itens pendentes.
   - Vá para a tela de Sincronização (`/sync`).
   - Clique em "Sincronizar Tudo".
   - Após sucesso, verifique na Home se o ícone mudou para **Verde** (Sincronizado) ou se ele sumiu (dependendo da lógica de limpeza, mas neste caso mantemos o item localmente apenas marcando como synced).

## Verificação no S3

Acesse o console da AWS S3 e verifique se os arquivos foram criados na pasta `capturas/`:
- `{GUID}.jpg`
- `{GUID}.json` (Abra e verifique o campo `usuarioEnvio`).
