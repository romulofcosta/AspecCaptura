# Plano de Implementação — PWA Stabilization

## Tasks

- [x] 1. REQ 1 — Extrair `GetOrLoadUnifiedDataAsync` e refatorar `BuildOrGetChunkIndexAsync`
  - [x] 1.1 Adicionar função estática `GetOrLoadUnifiedDataAsync` em `AspecCapturaApi/Program.cs` com chave de cache `TOMB-DATA:{bucket}:{key}:{version}` e expiração de 1 hora
  - [x] 1.2 Refatorar `BuildOrGetChunkIndexAsync` para chamar `GetOrLoadUnifiedDataAsync` em vez de `s3.GetObjectAsync` diretamente (remover o bloco `using var obj = await s3.GetObjectAsync(...)` interno)
  - [x] 1.3 Alterar a assinatura de `SerializeChunkPayloadAsync` para receber `UnifiedDataRecord data` como parâmetro em vez de `IAmazonS3 s3, string bucket, string key`
  - [x] 1.4 Atualizar o endpoint `/api/tombamentos/lote/{id}` para obter o `UnifiedDataRecord` via `GetOrLoadUnifiedDataAsync` e passá-lo para `SerializeChunkPayloadAsync`

- [x] 2. REQ 2 — Atualizar expiração padrão do JWT para 480 minutos
  - [x] 2.1 Alterar `JWT_EXPIRATION_MINUTES=60` para `JWT_EXPIRATION_MINUTES=480` em `AspecCapturaApi/.env.example`
  - [x] 2.2 Alterar `"JwtExpirationMinutes": 60` para `"JwtExpirationMinutes": 480` em `AspecCapturaApi/appsettings.json`
  - [x] 2.3 Alterar `ExpiresAt = DateTime.UtcNow.AddHours(8)` para `ExpiresAt = DateTime.UtcNow.AddMinutes(480)` em `AspecCaptura/Services/Auth/AuthService.cs`

- [x] 3. REQ 3 — Criar e registrar `AuthorizationMessageHandler`
  - [x] 3.1 Criar arquivo `AspecCaptura/Services/Auth/AuthorizationMessageHandler.cs` com a classe `AuthorizationMessageHandler : DelegatingHandler` que intercepta respostas 401 e chama `NavigationManager.NavigateTo("/login")` quando a rota atual não for `/login`
  - [x] 3.2 Registrar `AuthorizationMessageHandler` como serviço scoped e adicioná-lo ao `HttpClient` nomeado `BackendApi` em `AspecCaptura/Program.cs`

- [x] 4. REQ 4 — Corrigir invalidação de cache após captura
  - [x] 4.1 Adicionar função estática `InvalidateCacheForPrefixAsync` em `AspecCapturaApi/Program.cs` que remove `TOMB-INDEX` e `TOMB-DATA` do cache e registra log com nível `Information` incluindo `s3Key` e número de entradas removidas
  - [x] 4.2 Substituir `cache.Remove(cacheKey)` no endpoint `/api/capture/item` por chamada a `InvalidateCacheForPrefixAsync` (somente após `PutObjectAsync` bem-sucedido)
  - [x] 4.3 Substituir `cache.Remove(...)` no endpoint `/api/capture/sync` por chamada a `InvalidateCacheForPrefixAsync` (somente após `PutObjectAsync` bem-sucedido)
