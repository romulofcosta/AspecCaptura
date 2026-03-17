# Plano de Tasks — Integração Backend: Fluxo de Captura com Reconhecimento

## Contexto

O frontend já possui o ciclo completo de captura implementado:
- Reconhecimento OCR > Barcode > QR Code via Web Workers
- `RecognitionService` com priorização, circuit breaker e callbacks JS
- `PatrimonioSearchService` buscando no IndexedDB local
- `Camera.razor` com auto-fill de formulário e overlays visuais
- Upload de fotos e metadados via presigned URL (S3)
- Login via API retornando tombamentos + hierarquia de órgãos

O arquivo de dados do município (`usuarios/{PREFIX}.json`) no S3 contém a estrutura:
```json
{
  "cliente": "ce999",
  "usuarios": [...],
  "tabelas": {
    "xxorga": [...],
    "xxunid": [...],
    "paarea": [...],
    "pasarea": [...],
    "localizacao": [...],
    "tombamentos": [
      {
        "idpatomb": 619859188188,
        "nutomb": "00000005",
        "databomb": 20200615,
        "cdprod": 65380,
        "deprod": "ARMARIO 2 PORTAS",
        "estado": "BOM",
        "dataestado": 20200615,
        "situacao": "Alocado",
        "datasituacao": 20200615,
        "idlocalizacao": 123456789123,
        "esfera": "E"
      }
    ]
  }
}
```

A captura **edita diretamente** o array `tabelas.tombamentos` nesse arquivo — atualizando um tombamento existente ou adicionando um novo.

---

## Débitos Técnicos (fora do escopo desta implementação)

> Implementar como cenário feliz agora. Estes pontos ficam para análise posterior.

- **Concorrência**: dois usuários capturando simultaneamente podem sobrescrever um ao outro (last-write-wins). Solução futura: lock otimista via ETag do S3 com retry.
- **Performance de escrita**: regravar o arquivo completo (pode chegar a 42MB) a cada captura é custoso. Solução futura: arquivo delta separado com merge periódico, ou migração para banco de dados.
- **Invalidação de cache**: o `IMemoryCache` do chunk index (`TOMB-INDEX:...`) precisa ser invalidado após cada escrita. Implementado de forma simples nesta versão (remove a key do cache após PUT).

---

## Tasks

### Fase 1 — API Backend (pwa-camera-poc-api)

- [x] 1. Criar endpoint `POST /api/capture/item` — atualiza tombamento no arquivo S3 do município
  - [x] 1.1 Adicionar ao `ApiModels.cs` o record `CaptureItemRequest` com campos: `prefixo`, `idpatomb`, `nutomb`, `estado`, `situacao`, `idlocalizacao`, `fotoKey`, `capturedBy`, `capturedAt`, `source`
  - [x] 1.2 Adicionar ao `ApiModels.cs` o record `CaptureItemResponse` com: `idpatomb`, `nutomb`, `status` ("updated" | "created"), `updatedAt`
  - [x] 1.3 Adicionar ao `TombamentoRecord` os campos de captura: `estado`, `dataestado`, `situacao`, `datasituacao`, `idlocalizacao` (já existem no JSON real — verificar se o record atual os mapeia)
  - [x] 1.4 Implementar handler do endpoint em `Program.cs`:
    - Carregar `usuarios/{PREFIX}.json` do S3
    - Localizar tombamento por `idpatomb` (fallback: por `nutomb`)
    - Se encontrado: atualizar `estado`, `dataestado` (hoje em YYYYMMDD), `situacao`, `datasituacao`, `idlocalizacao`
    - Se não encontrado: adicionar novo objeto no array `tabelas.tombamentos`
    - Serializar e fazer `PutObject` no S3 sobrescrevendo o arquivo
    - Invalidar entrada do `IMemoryCache` para a key do chunk index desse prefixo
    - Retornar `CaptureItemResponse`

- [x] 2. Criar endpoint `GET /api/capture/validate/{nutomb}` — valida tombamento antes da captura
  - [x] 2.1 Parâmetros: `nutomb` (path), `prefix` (query)
  - [x] 2.2 Carregar `usuarios/{PREFIX}.json` do S3 via `IMemoryCache` (reutilizar `BuildOrGetChunkIndexAsync` ou criar helper similar)
  - [x] 2.3 Buscar tombamento em `tabelas.tombamentos` por `nutomb`
  - [x] 2.4 Adicionar ao `ApiModels.cs` o record `ValidateTombamentoResponse` com: `exists`, `esfera`, `deprod`, `cdprod`, `estado`, `situacao`, `idlocalizacao`, `nutomb`, `idpatomb`
  - [x] 2.5 Retornar 200 com `ValidateTombamentoResponse` (exists=false se não encontrado)

- [x] 3. Criar endpoint `GET /api/capture/list` — lista tombamentos capturados por prefixo/órgão
  - [x] 3.1 Parâmetros: `prefix` (query), `cdorgao` (query, opcional), `cdunid` (query, opcional)
  - [x] 3.2 Carregar `usuarios/{PREFIX}.json` do S3 via cache
  - [x] 3.3 Filtrar `tabelas.tombamentos` onde `situacao != null` (indica que foi capturado/atualizado)
  - [x] 3.4 Aplicar filtros opcionais de `cdorgao` e `cdunid` se fornecidos
  - [x] 3.5 Adicionar ao `ApiModels.cs` o record `CapturedItemSummary` com: `idpatomb`, `nutomb`, `deprod`, `estado`, `situacao`, `esfera`
  - [x] 3.6 Retornar lista de `CapturedItemSummary`

- [x] 4. Criar endpoint `POST /api/capture/sync` — sincronização em lote de capturas pendentes
  - [x] 4.1 Receber `SyncBatchRequest` com array de `CaptureItemRequest` (máx 50 itens)
  - [x] 4.2 Carregar `usuarios/{PREFIX}.json` uma única vez para o batch inteiro
  - [x] 4.3 Para cada item: localizar e atualizar (ou adicionar) no array `tabelas.tombamentos` em memória
  - [x] 4.4 Após processar todos os itens do batch: fazer um único `PutObject` no S3
  - [x] 4.5 Invalidar cache após a escrita
  - [x] 4.6 Adicionar ao `ApiModels.cs` os records `SyncBatchRequest`, `SyncBatchResponse` e `SyncItemResult`
  - [x] 4.7 Retornar `SyncBatchResponse` com: `total`, `updated`, `created`, `failed`, `results[]`

- [x] 5. Adicionar testes de integração na API (pwa-camera-poc-api/tests)
  - [x] 5.1 Criar `CaptureEndpointTests.cs` com testes para `POST /api/capture/item`
    - Happy path: atualiza tombamento existente
    - Happy path: adiciona tombamento novo
    - Erro: prefixo não encontrado no S3
    - Erro: campos obrigatórios ausentes
  - [x] 5.2 Criar `ValidateEndpointTests.cs` com testes para `GET /api/capture/validate/{nutomb}`
    - Tombamento encontrado
    - Tombamento não encontrado (exists=false)
    - Prefixo inválido
  - [x] 5.3 Criar `SyncEndpointTests.cs` com testes para `POST /api/capture/sync`
    - Batch com múltiplos itens (mix de update e create)
    - Batch vazio
    - Item com dados inválidos
  - [x] 5.4 Usar `WebApplicationFactory` com `IAmazonS3` mockado
  - [x] 5.5 Executar `dotnet test` na API e garantir todos passando

---

### Fase 2 — Frontend Blazor (pwa-camera-poc-blazor)

- [x] 6. Criar `ICaptureApiService` e `CaptureApiService`
  - [x] 6.1 Criar `Services/Capture/ICaptureApiService.cs` com métodos:
    - `ValidateTombamentoAsync(string nutomb, string prefix)`
    - `SaveCaptureAsync(CaptureItemDto item)`
    - `SyncPendingItemsAsync(List<CaptureItemDto> items)`
  - [x] 6.2 Criar `Services/Capture/CaptureApiService.cs` implementando a interface usando `HttpClient` ("BackendApi")
  - [x] 6.3 Criar `Models/CaptureItemDto.cs` mapeando os campos do `InventoryItem` para o `CaptureItemRequest` da API
  - [x] 6.4 Registrar `ICaptureApiService` / `CaptureApiService` no DI em `Program.cs`

- [x] 7. Integrar validação de tombamento no fluxo de reconhecimento (`Camera.razor`)
  - [x] 7.1 Em `OnPatrimonioFound`, antes de chamar `AutoFillForm`, chamar `ValidateTombamentoAsync`
  - [x] 7.2 Exibir indicador de loading durante a validação (desabilitar botões)
  - [x] 7.3 Se tombamento não existe no backend (`exists=false`): exibir aviso mas permitir continuar
  - [x] 7.4 Se tombamento existe: usar dados retornados para enriquecer o auto-fill (`deprod`, `esfera`, `estado` atual)

- [x] 8. Integrar salvamento no backend ao confirmar captura (`HandleSave` em `Camera.razor`)
  - [x] 8.1 Após `DbService.AddAsync("items", itemModel)`, chamar `SaveCaptureAsync` com os dados mapeados
  - [x] 8.2 Incluir `source` (OCR/QR/Barcode/Manual) no payload — já disponível no `RecognitionResult`
  - [x] 8.3 Se backend retornar sucesso: marcar `itemModel.Synced = true` antes de salvar no IndexedDB
  - [x] 8.4 Se backend falhar (offline/erro): manter `Synced = false`, logar o erro, continuar normalmente (não bloquear o usuário)

- [x] 9. Atualizar `SyncService` para usar o endpoint de sincronização em lote
  - [x] 9.1 Buscar todos os `InventoryItem` com `Synced = false` no IndexedDB
  - [x] 9.2 Mapear para `List<CaptureItemDto>` e chamar `SyncPendingItemsAsync` em batches de 50
  - [x] 9.3 Para cada item com `status == "updated"` ou `"created"` na resposta: atualizar `Synced = true` no IndexedDB
  - [x] 9.4 Manter itens com `status == "failed"` como `Synced = false` para retry na próxima sincronização

- [x] 10. Criar testes para os novos serviços e fluxos no frontend
  - [x] 10.1 Criar `tests/Services/Capture/CaptureApiServiceTests.cs` com mocks de `HttpClient`
    - `ValidateTombamentoAsync`: tombamento encontrado, não encontrado, erro de rede
    - `SaveCaptureAsync`: sucesso, falha de rede (não deve lançar exceção)
    - `SyncPendingItemsAsync`: batch com sucesso parcial
  - [x] 10.2 Executar suite completa `dotnet test` no projeto blazor e garantir 0 regressões

---

### Fase 3 — Validação Final

- [x] 11. Executar suite completa de testes (frontend + backend)
  - [x] 11.1 `dotnet test` no projeto `pwa-camera-poc-blazor/tests`
  - [x] 11.2 `dotnet test` no projeto `pwa-camera-poc-api/tests`
  - [x] 11.3 Corrigir qualquer regressão encontrada

- [ ] 12.* Adicionar endpoint `GET /api/capture/stats` com estatísticas de captura por prefixo
  - [ ] 12.1 Retornar: `totalTombamentos`, `totalCapturados` (situacao != null), `porEsfera`, `porEstado`

---

## Resumo do Escopo

| Área | O que muda |
|------|-----------|
| API Backend | 4 novos endpoints + modelos + testes |
| Frontend Service | `CaptureApiService` novo |
| Camera.razor | Validação pré-formulário + save com fallback offline |
| SyncService | Migração para sync em lote |
| Testes | Cobertura dos novos fluxos + regressão |

**Pré-condição**: API em .NET 8, CORS configurado, S3 operacional, login funcionando, `TombamentoRecord` em `ApiModels.cs` precisa ter os campos `estado`, `dataestado`, `situacao`, `datasituacao`, `idlocalizacao` mapeados.
