# Configuração de Variáveis de Ambiente para Deploy

Este documento descreve como configurar as variáveis de ambiente necessárias para o deploy da aplicação no Cloudflare Pages.

## Variáveis AWS Necessárias

As seguintes variáveis devem ser configuradas no painel do Cloudflare Pages:

### Configuração no Cloudflare Pages

1. Acesse o painel do Cloudflare Pages
2. Selecione seu projeto
3. Vá para **Settings** → **Environment Variables**
4. Adicione as seguintes variáveis:

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `AWS_REGION` | Região do bucket S3 | `us-east-1` |
| `AWS_BUCKET_NAME` | Nome do bucket S3 | `aspec-inventory-uploads` |
| `AWS_ACCESS_KEY` | Access Key do usuário IAM | `AKIAIOSFODNN7EXAMPLE` |
| `AWS_SECRET_KEY` | Secret Key do usuário IAM | `wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY` |

### Permissões IAM Necessárias

O usuário IAM deve ter as seguintes permissões no bucket S3:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::aspec-inventory-uploads/*",
        "arn:aws:s3:::aspec-inventory-uploads"
      ]
    }
  ]
}
```

### Política CORS do Bucket S3

Configure a política CORS do bucket para permitir requisições do domínio da aplicação:

```json
[
  {
    "AllowedHeaders": ["*"],
    "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
    "AllowedOrigins": [
      "http://localhost:5230",
      "https://seu-dominio.pages.dev"
    ],
    "ExposeHeaders": ["ETag"]
  }
]
```

## Isolamento de Dados

O isolamento de dados no bucket S3 é feito através do campo `usuarioEnvio` no arquivo JSON de metadados. Cada item sincronizado contém:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nome": "Notebook Dell",
  "codigo": "NB001",
  "usuarioEnvio": "joao.silva",
  "dataEnvio": "2026-01-08T14:30:00Z",
  ...
}
```

O módulo Desktop deve filtrar os itens por `usuarioEnvio` para garantir que cada fiscal visualize apenas seus próprios registros.

## Estrutura de Arquivos no S3

```
capturas/
├── {itemId1}.jpg          # Foto do item 1
├── {itemId1}.json         # Metadados do item 1
├── {itemId2}.jpg          # Foto do item 2
├── {itemId2}.json         # Metadados do item 2
└── ...
```

## Segurança

⚠️ **IMPORTANTE**: 
- Nunca commite credenciais AWS no repositório
- Use variáveis de ambiente para todas as credenciais sensíveis
- Rotacione as chaves periodicamente
- Monitore o uso do bucket S3 para detectar acessos não autorizados

## Migração Futura para Cognito

Para produção, recomenda-se migrar para AWS Cognito Identity Pools, que permite:
- Credenciais temporárias (STS)
- Isolamento por usuário via IAM Policy Variables
- Eliminação de chaves fixas no client-side
- Melhor auditoria e controle de acesso
