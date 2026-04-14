# Documento de Requisitos

## Introdução

Este documento descreve as correções de estabilidade para o sistema AspecCaptura — um PWA de inventário patrimonial em campo (Blazor WebAssembly + ASP.NET Core Minimal API + AWS S3). O escopo cobre quatro problemas críticos que causam degradação de desempenho e falhas silenciosas durante o uso em campo com smartphones.

## Glossário

- **API**: ASP.NET Core Minimal API (AspecCapturaApi)
- **PWA**: Aplicação Blazor WebAssembly (AspecCaptura)
- **S3**: Serviço de armazenamento AWS S3
- **UnifiedDataRecord**: Objeto C# que representa o arquivo JSON de município deserializado do S3
- **IMemoryCache**: Cache em memória do ASP.NET Core (`Microsoft.Extensions.Caching.Memory`)
- **ChunkIndex**: Índice de chunks construído a partir do `UnifiedDataRecord`, armazenado em cache
- **JWT**: Token de autenticação JSON Web Token emitido pela API no login
- **AuthorizationMessageHandler**: `DelegatingHandler` do Blazor que intercepta respostas HTTP
- **SyncService**: Serviço do PWA responsável pelo download de chunks de tombamentos
- **CaptureEndpoint**: Endpoints da API que recebem capturas de campo (`/api/capture/item`, `/api/capture/sync`)
- **Prefixo**: Código do município (ex: `CE999`), usado como chave de partição no S3
- **CacheKey**: Chave composta usada para identificar entradas no `IMemoryCache`

---

## Requisitos

### Requisito 1: Cache do UnifiedDataRecord na API

**User Story:** Como operador de campo, quero que a sincronização de tombamentos seja rápida, para que o aplicativo carregue os dados sem consumir dados móveis excessivos nem travar o dispositivo.

#### Critérios de Aceitação

1. WHEN `BuildOrGetChunkIndexAsync` é invocado para um `Prefixo` e versão já presentes no `IMemoryCache`, THE `API` SHALL retornar o `ChunkIndex` em cache sem realizar nenhuma chamada ao `S3`.

2. WHEN `SerializeChunkPayloadAsync` é invocado para um chunk cujo `UnifiedDataRecord` já está em cache, THE `API` SHALL serializar o payload a partir do objeto em memória sem realizar nenhuma chamada ao `S3`.

3. THE `API` SHALL armazenar o `UnifiedDataRecord` deserializado no `IMemoryCache` com chave no formato `TOMB-DATA:{bucket}:{s3Key}:{version}`, onde `version` é derivado do campo `LastModified` do S3 no formato `yyyyMMddHHmmss`.

4. THE `API` SHALL definir o tempo de expiração do cache do `UnifiedDataRecord` em 1 hora, alinhado com o tempo de expiração do `ChunkIndex`.

5. WHEN um arquivo de município com 10 chunks é solicitado em sequência, THE `API` SHALL realizar no máximo 2 chamadas ao `S3` (1 para `GetObjectMetadata` e 1 para `GetObject`) para servir todos os 10 chunks.

6. IF o `IMemoryCache` não contiver o `UnifiedDataRecord` para a versão atual, THEN THE `API` SHALL buscar o arquivo no `S3`, deserializar para `UnifiedDataRecord` e armazenar no cache antes de construir o `ChunkIndex` ou serializar o payload.

---

### Requisito 2: Expiração do JWT alinhada com jornada de campo

**User Story:** Como agente de campo, quero permanecer autenticado durante toda a jornada de trabalho, para que o aplicativo não me desconecte inesperadamente durante o inventário.

#### Critérios de Aceitação

1. THE `API` SHALL emitir tokens JWT com tempo de expiração de 480 minutos (8 horas) quando a variável de ambiente `JWT_EXPIRATION_MINUTES` não estiver definida ou estiver com valor padrão.

2. THE `API` SHALL ler o valor de expiração do JWT exclusivamente da variável de ambiente `JWT_EXPIRATION_MINUTES`, mantendo o comportamento atual de fallback para a configuração `Security:JwtExpirationMinutes`.

3. THE `.env.example` SHALL conter `JWT_EXPIRATION_MINUTES=480` como valor padrão documentado.

4. WHEN o `AuthService` do PWA armazena o token após login bem-sucedido, THE `AuthService` SHALL definir `ExpiresAt = DateTime.UtcNow.AddMinutes(480)`, alinhado com o tempo de expiração real do JWT emitido pela API.

---

### Requisito 3: Interceptor HTTP para respostas 401

**User Story:** Como agente de campo, quero ser redirecionado automaticamente para a tela de login quando minha sessão expirar, para que eu entenda o que aconteceu e possa autenticar novamente sem precisar fechar e reabrir o aplicativo.

#### Critérios de Aceitação

1. THE `PWA` SHALL registrar um `AuthorizationMessageHandler` como `DelegatingHandler` no `HttpClient` nomeado `BackendApi`.

2. WHEN o `AuthorizationMessageHandler` recebe uma resposta HTTP com status `401 Unauthorized`, THE `AuthorizationMessageHandler` SHALL invocar `NavigationManager.NavigateTo("/login", forceLoad: false)`.

3. WHEN o `AuthorizationMessageHandler` recebe uma resposta HTTP com status diferente de `401`, THE `AuthorizationMessageHandler` SHALL retornar a resposta sem modificação para o chamador.

4. THE `AuthorizationMessageHandler` SHALL propagar a resposta `401` original após o redirecionamento, para que o chamador possa tratar o erro se necessário.

5. IF o usuário já estiver na rota `/login` quando uma resposta `401` for recebida, THEN THE `AuthorizationMessageHandler` SHALL não realizar novo redirecionamento.

---

### Requisito 4: Invalidação correta de cache após captura

**User Story:** Como agente de campo, quero que os dados capturados apareçam corretamente na próxima sincronização, para que o inventário reflita o trabalho realizado.

#### Critérios de Aceitação

1. WHEN `PutObjectAsync` é concluído com sucesso em qualquer `CaptureEndpoint`, THE `API` SHALL invalidar todas as entradas do `IMemoryCache` cujas chaves contenham o prefixo `TOMB-` seguido do `bucket` e `s3Key` do arquivo modificado.

2. THE `API` SHALL utilizar um `CancellationTokenSource` por arquivo de município como token de expiração de cache, de modo que `cache.Remove` ou o cancelamento do token invalide simultaneamente as entradas `TOMB-INDEX:*`, `TOMB-DATA:*` e `TOMB-CHUNK:*` relacionadas ao mesmo arquivo.

3. WHEN o cache é invalidado após uma captura bem-sucedida, THE `API` SHALL registrar em log a invalidação com nível `Information`, incluindo o `s3Key` e o número de entradas invalidadas.

4. WHEN `BuildOrGetChunkIndexAsync` é chamado após uma invalidação de cache, THE `API` SHALL reconstruir o `ChunkIndex` a partir do arquivo atualizado no `S3`, refletindo os dados da captura mais recente.

5. IF `PutObjectAsync` falhar, THEN THE `API` SHALL não invalidar o cache, preservando os dados consistentes da versão anterior.
