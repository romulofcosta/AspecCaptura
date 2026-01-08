# Guia de Teste Local

Como as credenciais AWS foram removidas do arquivo principal `appsettings.json` por segurança, siga estes passos para testar a aplicação localmente:

## Opção 1: Usar appsettings.Development.json (Recomendado)

Crie um arquivo chamado `appsettings.Development.json` na pasta `wwwroot` com suas credenciais reais. Este arquivo substituirá as configurações do `appsettings.json` quando rodar em ambiente de desenvolvimento.

```json
{
    "Aws": {
        "Region": "us-east-1",
        "BucketName": "SEU_BUCKET_NAME",
        "AccessKey": "SUA_ACCESS_KEY_REAL",
        "SecretKey": "SUA_SECRET_KEY_REAL"
    }
}
```

> **Nota:** Certifique-se de que este arquivo esteja listado no `.gitignore` para não ser enviado para o repositório.

## Opção 2: Testar Sincronização

1. **Login:** Acesse a aplicação com qualquer usuário (ex: `admin`/`admin`).
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
