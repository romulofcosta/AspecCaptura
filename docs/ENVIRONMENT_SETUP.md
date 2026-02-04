# Configuração de Ambiente e Deploy

Este documento descreve como configurar as variáveis de ambiente necessárias para o ecossistema ASPEC Capture (PWA + API BFF).

## 🖥️ Backend for Frontend (API BFF)

A API é responsável por gerenciar credenciais AWS e gerar Pre-signed URLs.

### Variáveis Necessárias (API)

| Variável | Descrição | Localização |
|----------|-----------|-------------|
| `AWS:Region` | Região do bucket S3 | `appsettings.json` ou Env Var |
| `AWS:BucketName` | Nome do bucket S3 | `appsettings.json` ou Env Var |
| `AWS:AccessKey` | Access Key do usuário IAM | **Secret** (Secret Manager/Env Var) |
| `AWS:SecretKey` | Secret Key do usuário IAM | **Secret** (Secret Manager/Env Var) |
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` ou `Production` |

### Permissões IAM (API)

O usuário IAM configurado na API deve ter permissões de `PutObject` e `GetObjectMetadata` no bucket alvo.

---

## 📱 PWA (Blazor WebAssembly)

O PWA agora é agnóstico em relação às chaves da AWS, comunicando-se apenas com o BFF.

### Variáveis no Cloudflare Pages

Ao realizar o deploy no Cloudflare Pages, configure:

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `API_BASE_URL` | URL base da API BFF | `https://api-aspec-capture.azurewebsites.net` |

### Configuração de CORS

Para que o PWA consiga realizar o upload direto para o S3 após receber a Pre-signed URL, o bucket S3 **deve** ter a seguinte política CORS:

```json
[
  {
    "AllowedHeaders": ["*"],
    "AllowedMethods": ["GET", "PUT", "POST", "DELETE", "HEAD"],
    "AllowedOrigins": ["https://sua-pwa.pages.dev", "http://localhost:5230"],
    "ExposeHeaders": ["ETag", "x-amz-meta-asset-code"],
    "MaxAgeSeconds": 3000
  }
]
```

## 📂 Estrutura de Armazenamento (S3)

Os arquivos são organizados no bucket seguindo o padrão compatível com o módulo Desktop:

```
capturas/
├── {itemId}/
│   ├── foto-1.jpg
│   ├── foto-2.jpg
│   └── metadata.json
```

## 🔐 Segurança e Boas Práticas

1. **Segregação**: Nunca exponha `AWS_SECRET_KEY` no projeto Blazor. O Blazor é código cliente e pode ser inspecionado.
2. **Pre-signed URLs**: As URLs geradas pelo BFF têm validade curta (default: 10 minutos).
3. **HTTPS**: Todo o tráfego entre PWA, BFF e S3 deve obrigatoriamente utilizar HTTPS.
4. **Validadores**: O BFF sanitiza nomes de arquivos e chaves para evitar ataques de Path Traversal.
