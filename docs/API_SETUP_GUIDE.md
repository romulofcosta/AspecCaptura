# Guia de Configuração - API Backend

## Status Atual

A API está implementada em `pwa-camera-poc-api/Program.cs` com os seguintes endpoints:

### Endpoints Disponíveis

1. **POST /api/auth/login** - Autenticação
   - Entrada: `{ "usuario": "string", "senha": "string" }`
   - Saída: Token JWT + dados do usuário
   - Requer: Arquivo `usuarios/{PREFIX}.json` no S3

2. **GET /api/storage/presigned-url** - URL pré-assinada para upload
3. **GET /api/tombamentos/sync-info** - Informações de sincronização
4. **GET /api/tombamentos/lote/{id}** - Lote de tombamentos

## Pré-requisitos

### 1. Variáveis de Ambiente

Configure as seguintes variáveis:

```bash
# AWS
AWS__AccessKey=sua_access_key
AWS__SecretKey=sua_secret_key
AWS__Region=us-east-1
AWS__BucketName=seu_bucket

# ASP.NET
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5069
```

### 2. Estrutura de Dados no S3

A API espera a seguinte estrutura no bucket S3:

```
bucket/
├── usuarios/
│   ├── CE999.json          # Dados do município CE999
│   ├── SP001.json          # Dados do município SP001
│   └── ...
└── patrimonio/
    ├── CE999_01.json       # Patrimônio do município
    └── ...
```

### 3. Formato do Arquivo `usuarios/{PREFIX}.json`

```json
{
  "usuarios": [
    {
      "nmUsuario": "ce999.usuario1",
      "nomeCompleto": "Usuário Um",
      "pwdUsuario": "senha123",
      "esfera": "M",
      "prefixo": "CE999"
    }
  ],
  "tabelas": {
    "tombamentos": [...],
    "xxOrga": [...],
    "xxUnid": [...],
    "paArea": [...],
    "pasArea": [...],
    "localizacao": [...]
  }
}
```

## Executar a API

### Opção 1: Linha de Comando

```bash
cd pwa-camera-poc-api
dotnet run
```

A API estará disponível em:
- **API:** http://localhost:5069
- **Swagger:** http://localhost:5069/swagger

### Opção 2: Visual Studio

1. Abra `pwa-camera-poc-api.sln`
2. Defina como projeto de inicialização
3. Pressione F5

### Opção 3: Docker

```bash
docker build -t pwa-camera-poc-api .
docker run -p 5069:5069 \
  -e AWS__AccessKey=sua_key \
  -e AWS__SecretKey=sua_secret \
  -e AWS__BucketName=seu_bucket \
  pwa-camera-poc-api
```

## Testar o Login

### Usando Postman/Insomnia

```
POST http://localhost:5069/api/auth/login
Content-Type: application/json

{
  "usuario": "ce999.usuario1",
  "senha": "senha123"
}
```

### Resposta Esperada (200 OK)

```json
{
  "nomeCompleto": "Usuário Um",
  "prefixo": "CE999",
  "esfera": "M",
  "orgaos": [...],
  "tombamentos": [...],
  "token": "uuid-token"
}
```

### Erros Comuns

| Erro | Causa | Solução |
|------|-------|--------|
| 404 Not Found | Arquivo não existe no S3 | Verificar se `usuarios/CE999.json` existe |
| 401 Unauthorized | Usuário ou senha inválidos | Verificar credenciais no arquivo JSON |
| 500 Internal Server | Erro de parsing JSON | Validar formato do arquivo no S3 |
| CORS Error | Frontend não autorizado | Verificar CORS em `Program.cs` |

## Configuração CORS

A API permite requisições de:
- **Development:** Qualquer origem (`*`)
- **Production:** Apenas `*.pwa-camera-poc-blazor.pages.dev`

Para adicionar mais origens, edite `Program.cs`:

```csharp
corsBuilder.SetIsOriginAllowed(origin =>
{
    if (builder.Environment.IsDevelopment()) return true;
    return origin.EndsWith(".pwa-camera-poc-blazor.pages.dev") ||
           origin == "https://seu-dominio.com";
});
```

## Troubleshooting

### API não inicia

```bash
# Verificar porta em uso
netstat -ano | findstr :5069

# Matar processo na porta
taskkill /PID <PID> /F
```

### Erro de conexão no Frontend

1. Verificar se API está rodando: `curl http://localhost:5069/health`
2. Verificar CORS: Abrir DevTools → Network → Procurar por erro CORS
3. Verificar firewall: Permitir porta 5069

### Erro de autenticação

1. Verificar credenciais no arquivo S3
2. Verificar formato do usuário: `prefixo.usuario`
3. Verificar se arquivo existe no S3

## Próximos Passos

1. Configurar AWS S3 com dados de teste
2. Iniciar a API
3. Testar login no frontend
4. Verificar fluxo de sincronização

## Referências

- **API Program:** `pwa-camera-poc-api/Program.cs`
- **Frontend Auth:** `pwa-camera-poc-blazor/Services/Auth/AuthService.cs`
- **Login Page:** `pwa-camera-poc-blazor/Pages/Login.razor`
- **Swagger:** http://localhost:5069/swagger (quando API está rodando)
