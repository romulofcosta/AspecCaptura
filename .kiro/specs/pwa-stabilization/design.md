# Design Document — PWA Stabilization

## Visão Geral

Este documento descreve as mudanças técnicas para estabilizar o protótipo AspecCaptura em campo. O sistema é composto por uma Blazor WebAssembly PWA (`AspecCaptura`) e uma ASP.NET Core Minimal API (`AspecCapturaApi`), usando AWS S3 como única fonte de dados (arquivo JSON por município, ~42 MB).

Os quatro problemas a corrigir são:

1. **Cache duplo do S3** — `BuildOrGetChunkIndexAsync` e `SerializeChunkPayloadAsync` fazem chamadas independentes ao S3 para o mesmo arquivo, causando latência e consumo de dados desnecessários.
2. **JWT de 1 hora** — Agentes de campo ficam desconectados no meio da jornada (8 h).
3. **Sem interceptor 401** — Quando o token expira, o app trava silenciosamente em vez de redirecionar para o login.
4. **Invalidação de cache incompleta** — Após uma captura, apenas `TOMB-INDEX` é removido do cache; `TOMB-DATA` e `TOMB-CHUNK` permanecem obsoletos.

---

## Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│  Blazor WASM (AspecCaptura)                                     │
│                                                                 │
│  Program.cs                                                     │
│    └─ HttpClient "BackendApi"                                   │
│         └─ AuthorizationMessageHandler  ← [REQ 3 - NOVO]       │
│              └─ DelegatingHandler chain                         │
│                                                                 │
│  Services/Auth/AuthService.cs                                   │
│    └─ ExpiresAt = UtcNow.AddMinutes(480)  ← [REQ 2]            │
└─────────────────────────────────────────────────────────────────┘
                          │ HTTP
┌─────────────────────────────────────────────────────────────────┐
│  ASP.NET Core Minimal API (AspecCapturaApi)                     │
│                                                                 │
│  Program.cs                                                     │
│    ├─ GetOrLoadUnifiedDataAsync()  ← [REQ 1 - NOVO]             │
│    │    └─ IMemoryCache key: TOMB-DATA:{bucket}:{key}:{ver}     │
│    ├─ BuildOrGetChunkIndexAsync()  ← [REQ 1 - REFATORADO]       │
│    │    └─ chama GetOrLoadUnifiedDataAsync                      │
│    ├─ SerializeChunkPayloadAsync() ← [REQ 1 - REFATORADO]       │
│    │    └─ recebe UnifiedDataRecord como parâmetro              │
│    └─ CaptureEndpoints             ← [REQ 4 - REFATORADO]       │
│         └─ InvalidateCacheForPrefix()  ← [REQ 4 - NOVO]        │
│              └─ Remove TOMB-INDEX, TOMB-DATA, TOMB-CHUNK        │
│                                                                 │
│  IMemoryCache                                                   │
│    ├─ TOMB-INDEX:{bucket}:{key}:{ver}  → ChunkIndex (1h)        │
│    ├─ TOMB-DATA:{bucket}:{key}:{ver}   → UnifiedDataRecord (1h) │
│    └─ TOMB-CHUNK:{bucket}:{key}:{ver}:{id} → byte[] (1h)        │
└─────────────────────────────────────────────────────────────────┘
                          │ S3 API
┌─────────────────────────────────────────────────────────────────┐
│  AWS S3                                                         │
│    └─ usuarios/{PREFIX}.json  (~42 MB por município)            │
└─────────────────────────────────────────────────────────────────┘
```

### Fluxo de chamadas ao S3 após a correção

Para N chunks solicitados em sequência (primeira sincronização):

```
1. GET /api/tombamentos/sync-info?prefix=CE999
   → GetObjectMetadataAsync (1 chamada)
   → GetOrLoadUnifiedDataAsync → GetObjectAsync (1 chamada, armazena TOMB-DATA)
   → BuildOrGetChunkIndexAsync → usa TOMB-DATA do cache (0 chamadas extras)
   → armazena TOMB-INDEX

2. GET /api/tombamentos/lote/1..N?prefix=CE999
   → BuildOrGetChunkIndexAsync → usa TOMB-INDEX do cache (0 chamadas S3)
   → SerializeChunkPayloadAsync → usa TOMB-DATA do cache (0 chamadas S3)
   → armazena TOMB-CHUNK:{id}

Total S3: 1 GetObjectMetadata + 1 GetObject = 2 chamadas para N chunks
```

---

## Componentes e Interfaces

### REQ 1 — `GetOrLoadUnifiedDataAsync` (nova função auxiliar)

**Localização:** `AspecCapturaApi/Program.cs` (função estática local)

```csharp
static async Task<UnifiedDataRecord> GetOrLoadUnifiedDataAsync(
    IAmazonS3 s3, string bucket, string key, string version,
    IMemoryCache cache, JsonSerializerOptions options, ILogger? log = null)
```

**Responsabilidade:** Única fonte de verdade para carregar e cachear o `UnifiedDataRecord`. Elimina a duplicação entre `BuildOrGetChunkIndexAsync` e `SerializeChunkPayloadAsync`.

**Chave de cache:** `TOMB-DATA:{bucket}:{key}:{version}`

**Expiração:** 1 hora (alinhada com `TOMB-INDEX` e `TOMB-CHUNK`)

---

### REQ 1 — `BuildOrGetChunkIndexAsync` (refatorado)

Remove o bloco `using var obj = await s3.GetObjectAsync(...)` interno e substitui por:

```csharp
var data = await GetOrLoadUnifiedDataAsync(s3, bucket, key, version, cache, options, log);
var itens = data.Tabelas?.Tombamentos?.ToList();
```

---

### REQ 1 — `SerializeChunkPayloadAsync` (refatorado)

Assinatura alterada para receber `UnifiedDataRecord` diretamente:

```csharp
static Task<byte[]> SerializeChunkPayloadAsync(
    UnifiedDataRecord data, ChunkMeta meta, JsonSerializerOptions options)
```

O endpoint `/api/tombamentos/lote/{id}` passa o `UnifiedDataRecord` obtido via `GetOrLoadUnifiedDataAsync`.

---

### REQ 2 — Expiração JWT

**`AspecCapturaApi/.env.example`:** `JWT_EXPIRATION_MINUTES=480`

**`AspecCapturaApi/appsettings.json`:** `"JwtExpirationMinutes": 480`

**`AspecCaptura/Services/Auth/AuthService.cs`:** A linha que define `ExpiresAt` passa de `AddHours(8)` para `AddMinutes(480)` (semanticamente equivalente, mas explicitamente alinhada com a variável de ambiente).

> Nota: O `AuthService` da API já lê `JWT_EXPIRATION_MINUTES` da variável de ambiente com fallback para `Security:JwtExpirationMinutes`. Apenas o valor padrão precisa ser atualizado de 60 para 480.

---

### REQ 3 — `AuthorizationMessageHandler`

**Localização:** `AspecCaptura/Services/Auth/AuthorizationMessageHandler.cs` (arquivo novo)

```csharp
public class AuthorizationMessageHandler(NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var response = await base.SendAsync(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var uri = new Uri(navigation.Uri);
            if (!uri.AbsolutePath.Equals("/login", StringComparison.OrdinalIgnoreCase))
                navigation.NavigateTo("/login");
        }
        return response;
    }
}
```

**Registro em `AspecCaptura/Program.cs`:**

```csharp
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("BackendApi", client => { ... })
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
```

---

### REQ 4 — Invalidação de cache após captura

**Estratégia:** Para o protótipo, usar `cache.Remove` explícito com as 3 chaves conhecidas após obter a versão atual do arquivo via `GetObjectMetadataAsync`.

**Nova função auxiliar:**

```csharp
static async Task InvalidateCacheForPrefixAsync(
    IAmazonS3 s3, string bucket, string s3Key,
    IMemoryCache cache, ILogger log)
{
    // Obtém a versão atual para construir as chaves exatas
    var head = await s3.GetObjectMetadataAsync(bucket, s3Key);
    var version = (head.LastModified ?? DateTime.UtcNow)
                    .ToUniversalTime().ToString("yyyyMMddHHmmss");

    var keysToRemove = new[]
    {
        $"TOMB-INDEX:{bucket}:{s3Key}:{version}",
        $"TOMB-DATA:{bucket}:{s3Key}:{version}",
    };

    int removed = 0;
    foreach (var k in keysToRemove)
    {
        cache.Remove(k);
        removed++;
    }

    // TOMB-CHUNK tem sufixo de chunkId — remove por prefixo via ChangeToken
    // Para o protótipo: os chunks serão reconstruídos na próxima requisição
    // (o TOMB-INDEX invalidado força reconstrução, que por sua vez não usa TOMB-CHUNK antigo)

    log.LogInformation(
        "Cache invalidado para {S3Key}: {Count} entradas removidas (versão {Version})",
        s3Key, removed, version);
}
```

**Chamada nos endpoints de captura** (`/api/capture/item` e `/api/capture/sync`): após `PutObjectAsync` bem-sucedido, substituir `cache.Remove(cacheKey)` por `await InvalidateCacheForPrefixAsync(...)`.

> Nota sobre `TOMB-CHUNK`: Os chunks individuais têm chaves com sufixo `:{chunkId}`. Como o `IMemoryCache` padrão não suporta remoção por prefixo, a estratégia é: ao invalidar `TOMB-INDEX` e `TOMB-DATA`, a próxima chamada a `BuildOrGetChunkIndexAsync` reconstruirá o índice com nova versão, tornando as chaves `TOMB-CHUNK` da versão anterior inacessíveis (elas expirarão naturalmente em 1 hora). Isso é aceitável para o protótipo.

---

## Modelos de Dados

Nenhum modelo novo é introduzido. As alterações são:

| Componente | Mudança |
|---|---|
| `ChunkIndex` record | Sem alteração |
| `UnifiedDataRecord` record | Sem alteração |
| `SessionToken` (PWA) | `ExpiresAt` calculado com `AddMinutes(480)` |
| `IMemoryCache` keys | Adição de `TOMB-DATA:*` como nova família de chaves |

---

## Propriedades de Correção

*Uma propriedade é uma característica ou comportamento que deve ser verdadeiro em todas as execuções válidas do sistema — essencialmente, uma declaração formal sobre o que o sistema deve fazer. Propriedades servem como ponte entre especificações legíveis por humanos e garantias de correção verificáveis por máquina.*

### Propriedade 1: Cache hit elimina chamadas ao S3

*Para qualquer* combinação de bucket, s3Key e version, se `GetOrLoadUnifiedDataAsync` for chamado duas vezes com os mesmos parâmetros, a segunda chamada não deve invocar `s3.GetObjectAsync`.

**Valida: Requisitos 1.1, 1.2, 1.6**

---

### Propriedade 2: Chave de cache segue o formato especificado

*Para qualquer* combinação válida de bucket, s3Key e version, após chamar `GetOrLoadUnifiedDataAsync`, o `IMemoryCache` deve conter uma entrada acessível pela chave `TOMB-DATA:{bucket}:{s3Key}:{version}` que retorna o mesmo `UnifiedDataRecord` deserializado do S3.

**Valida: Requisito 1.3**

---

### Propriedade 3: N chunks consomem no máximo 2 chamadas ao S3

*Para qualquer* N ≥ 1 chunks solicitados em sequência para o mesmo prefixo, o número total de chamadas a `s3.GetObjectAsync` deve ser ≤ 1 e a `s3.GetObjectMetadataAsync` deve ser ≤ 1 (totalizando ≤ 2 chamadas ao S3).

**Valida: Requisito 1.5**

---

### Propriedade 4: Expiração do JWT reflete o valor configurado

*Para qualquer* valor de `JWT_EXPIRATION_MINUTES` entre 1 e 1440, o token JWT gerado por `GenerateJwtToken` deve ter `exp - iat` igual a esse valor em segundos (± 5 s de tolerância de clock).

**Valida: Requisito 2.2**

---

### Propriedade 5: Handler 401 redireciona para /login

*Para qualquer* requisição HTTP cujo handler interno retorne status 401, e desde que a URL atual não seja `/login`, o `AuthorizationMessageHandler` deve chamar `NavigationManager.NavigateTo("/login")` e retornar a resposta 401 original ao chamador.

**Valida: Requisitos 3.2, 3.4**

---

### Propriedade 6: Handler não-401 não modifica a resposta

*Para qualquer* status code HTTP diferente de 401 (ex: 200, 400, 403, 404, 500), o `AuthorizationMessageHandler` deve retornar a resposta sem modificação e sem chamar `NavigateTo`.

**Valida: Requisito 3.3**

---

### Propriedade 7: Cache é invalidado após captura bem-sucedida

*Para qualquer* prefixo de município, se o cache contiver entradas `TOMB-INDEX` e `TOMB-DATA` para esse prefixo e uma captura for concluída com sucesso (`PutObjectAsync` sem exceção), então essas entradas devem ser removidas do cache.

**Valida: Requisitos 4.1, 4.4**

---

### Propriedade 8: Falha no PutObjectAsync preserva o cache

*Para qualquer* estado de cache e qualquer exceção lançada por `PutObjectAsync`, as entradas de cache existentes devem permanecer inalteradas após a falha.

**Valida: Requisito 4.5**

---

## Tratamento de Erros

| Cenário | Comportamento |
|---|---|
| `GetObjectAsync` falha durante `GetOrLoadUnifiedDataAsync` | Exceção propagada; cache não é populado; endpoint retorna 500 |
| `GetObjectMetadataAsync` falha durante `InvalidateCacheForPrefixAsync` | Log de warning; invalidação não ocorre; cache permanece consistente |
| `NavigateTo("/login")` chamado quando já em `/login` | Guard `!uri.AbsolutePath.Equals("/login")` previne loop |
| Token JWT com `JWT_EXPIRATION_MINUTES` inválido (não numérico) | `int.TryParse` retorna false; fallback para `Security:JwtExpirationMinutes` (480) |
| `UnifiedDataRecord` deserializado como null | `?? throw new InvalidOperationException("Arquivo inválido.")` — comportamento atual mantido |

---

## Estratégia de Testes

### Testes unitários (xUnit + Moq)

Cobrir os cenários de exemplo e edge cases:

- `GetOrLoadUnifiedDataAsync`: cache hit não chama S3; cache miss chama S3 e popula cache
- `AuthorizationMessageHandler`: 401 redireciona; não-401 passa sem modificação; `/login` não redireciona novamente
- `GenerateJwtToken`: expiração padrão = 480 min quando variável não definida
- `InvalidateCacheForPrefixAsync`: entradas removidas após sucesso; entradas preservadas após falha

### Testes de propriedade (xUnit + FsCheck)

Usar [FsCheck.Xunit](https://fscheck.github.io/FsCheck/) — já disponível no ecossistema .NET, sem novos pacotes NuGet se já referenciado, ou usar geradores manuais com `Arbitrary<T>`.

> Se FsCheck não estiver no projeto, os testes de propriedade podem ser implementados como loops parametrizados com `Bogus` ou dados gerados manualmente, mantendo a semântica de "para qualquer entrada válida".

Cada teste de propriedade deve rodar mínimo 100 iterações e referenciar a propriedade do design:

```
// Feature: pwa-stabilization, Propriedade 1: Cache hit elimina chamadas ao S3
// Feature: pwa-stabilization, Propriedade 3: N chunks consomem no máximo 2 chamadas ao S3
// Feature: pwa-stabilization, Propriedade 5: Handler 401 redireciona para /login
// Feature: pwa-stabilization, Propriedade 6: Handler não-401 não modifica a resposta
// Feature: pwa-stabilization, Propriedade 7: Cache é invalidado após captura bem-sucedida
// Feature: pwa-stabilization, Propriedade 8: Falha no PutObjectAsync preserva o cache
```

### Testes de fumaça (manual / CI)

- Verificar que `.env.example` contém `JWT_EXPIRATION_MINUTES=480`
- Verificar que `appsettings.json` contém `"JwtExpirationMinutes": 480`
- Verificar que `AuthorizationMessageHandler` está registrado no DI container do PWA
