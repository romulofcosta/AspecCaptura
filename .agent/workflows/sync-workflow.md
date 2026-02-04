---
description: Guia do Fluxo de Sincronização (PWA -> S3)
---

Este documento detalha o processo técnico de sincronização de itens do IndexedDB para o AWS S3.

### Fluxo Técnico

1. **Geração de URL Assinada**:
   - O componente `Sync.razor` chama `AwsStorageService.GetPresignedUrlAsync`.
   - O serviço faz um POST para a API BFF (`/api/storage/presigned-url`) com os metadados do arquivo.
   - A API retorna a URL assinada e a chave final do objeto no S3.

2. **Upload Binário**:
   - O PWA utiliza `HttpClient` para fazer um `PUT` direto na URL recebida.
   - O binário (JPEG convertido de Base64) é enviado via stream.

3. **Confirmação e Limpeza**:
   - Após o upload bem-sucedido de todas as fotos e do JSON de metadados, o item é atualizado no IndexedDB:
     - `Synced = true`
     - `Photos = []` (removidas para economizar espaço)
     - `RemoteUrls = [...]` (URLs finais no bucket)

### Troubleshooting
- **Erro 403 (S3)**: Geralmente indica falha na política CORS do bucket ou permissões IAM insuficientes na API.
- **Erro de Rede (CORS)**: Verifique se o domínio (localhost ou pages.dev) está na lista de `AllowedOrigins` do S3.
- **WASM Memory**: Uploads de arquivos muito grandes (>10MB) podem causar instabilidade no runtime Mono; recomenda-se fotos < 1MB.
