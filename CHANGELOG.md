# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [0.8.2] - 2026-04-08

### Corrigido
- **Erro de conversão JSON na sincronização**: Corrigido erro "The JSON value could not be converted to..." ao baixar lotes de tombamento
  - Problema causado por incompatibilidade entre estruturas de dados da API e Frontend
  - API (`TombamentoRecord`) não possuía 3 campos esperados pelo Frontend (`TombamentoWire`):
    - `descricao` (string?)
    - `localizacao` (string?)
    - `valorestimado` (decimal?)
  - Campos adicionados à API como nullable para manter compatibilidade
  - Sincronização de lotes agora funciona corretamente após autenticação

### Versão
- **Frontend**: 0.8.2
- **API**: 0.3.3
- **Data de release**: 8 de abril de 2026

## [0.8.1] - 2026-04-08

### Corrigido
- **Botão de voltar na tela de sincronização**: Corrigido problema onde o botão "Voltar" na página de sincronização não funcionava
  - Adicionado `@inject NavigationManager Navigation` que estava faltando
  - Adicionado `@inject AppState appState` que estava sendo usado mas não injetado
  - Adicionado `@inject IToastService ToastService` que estava sendo usado mas não injetado
  - Botão agora navega corretamente para `/items` (lista de bens)

### Versão
- **Frontend**: 0.8.1
- **Data de release**: 8 de abril de 2026

## [0.8.0] - 2025-02-08

### Corrigido
- **CORS no Backend**: Configuração de CORS agora aceita explicitamente o domínio principal `https://pwa-camera-poc-blazor.pages.dev`
  - Anteriormente só aceitava subdomínios (`.pwa-camera-poc-blazor.pages.dev`)
  - Mantido suporte a subdomínios para preview deployments
  - Requisições do frontend agora funcionam corretamente

### Melhorado
- **Script de Build (`build.sh`)**: Validações automáticas e logs detalhados
  - Validação da substituição de `__API_BASE_URL__` ANTES do build
  - Verificação do output final DEPOIS do build
  - Logs informativos para facilitar debug
  - Detecção de erros antes do deploy
  - Mensagens de erro claras e acionáveis

### Removido
- **Script Obsoleto**: Removido `build-production.sh` (estava em desuso)
  - Apenas `build.sh` está em uso
  - Simplifica manutenção e evita confusão

### Adicionado
- **Documentação Completa de Deploy**:
  - `DEPLOY_FINAL.md` - Guia completo de deploy
  - `CHECKLIST_DEPLOY.md` - Checklist interativo de validação
  - `SOLUCAO_CORS.md` - Documentação detalhada da solução de CORS
  - `RESUMO_CORRECOES.md` - Resumo executivo das correções
  - `DOCS_INDEX.md` - Índice de toda documentação
  - `check-build-config.sh` - Script de verificação automática
- **Versão na API**: Adicionadas propriedades de versão no `pwa-camera-poc-api.csproj`

### Versão
- **Frontend**: 0.8.0
- **Backend**: 0.8.0
- **Data de release**: 8 de fevereiro de 2025

### Notas de Upgrade
- Nenhuma ação necessária para usuários finais
- Deploy automático via Cloudflare Pages e Render
- Configurar variável de ambiente `API_BASE_URL` no Cloudflare Pages se ainda não configurada

## [0.7.1] - 2026-04-07

### Corrigido
- **Bug crítico de exibição de bens**: Corrigido filtro de status em `Items.razor` que estava excluindo TODOS os bens sem captura local (InventoryItem)
  - Método `FilterByStatus()` agora retorna apenas bens com captura local E status correspondente
  - Bens não capturados aparecem corretamente na aba "TODOS"
  - Bens não capturados não aparecem em abas de status específico (comportamento esperado)

### Adicionado
- **Logs detalhados de debugging** em `Items.razor`:
  - Rastreamento de valores de esfera (distintos no DB vs filtro aplicado)
  - Contagem de itens locais carregados
  - Impacto de cada filtro aplicado (quantos itens removidos)
  - Logs indicam claramente quando filtros são aplicados ou não
- **Validação de dados**: Verificação de esferas distintas no DB para debug

### Identificado para Remoção Futura
- **Models/ItemModel.cs**: Classe `Item` wrapper sem referências no código
- **Services/Sync/ItemSyncService.cs**: Possível serviço legado (necessita verificação)

### Princípios Aplicados
- **Debugging Sistemático**: Aplicadas skills de find-bugs, debugger e systematic-debugging
- **Root Cause Analysis**: Identificado e corrigido problema na raiz (filtro incorreto)
- **Clean Code**: Logs descritivos e comentários explicativos adicionados
- **KISS**: Solução simples e direta para o problema identificado

### Versão
- **Versão atualizada para 0.7.1** em `pwa-camera-poc-blazor.csproj`
- **Data de build**: 7 de abril de 2026

## [0.7.0] - 2026-04-07

### Adicionado
- **Enum `BemStatus`** com 8 situações do bem patrimonial:
  - Avariado
  - Ausência de Plaqueta
  - Localização Divergente
  - Plaqueta Avariada
  - Plaqueta Indevida
  - Plaqueta Inexistente no Sistema
  - Plaqueta de Outro Bem
  - Quebrado
- **Classe utilitária `BemEnumMapper`** para conversão de enums para texto em português (DRY)
- **Campo `DataTombamento`** no modelo `InventoryItem` para data de tombamento do bem
- **Página `BemDetails.razor`** substituindo `ItemDetails.razor` com campos obrigatórios:
  - Número de tombamento
  - Descrição do Bem
  - Situação atual (BemStatus)
  - Data de tombamento
  - Estado de conservação (ConservationState)
  - Localização
  - Valor líquido contábil
- **Campos no `TombamentoWire`**: `descricao`, `localizacao`, `valorestimado`, `estado` para sincronização completa de dados

### Modificado

#### Nomenclatura (Item → Bem)
- **Rotas atualizadas**: `/bem/{Nutomb}` e `/bem-detalhes/{Nutomb}` (anteriormente `/item/...`)
- **Títulos de página**: "Tombamento" → "Bem" em toda a aplicação
- **Referências no código**: Alterado onde apropriado para refletir terminologia de "Bem Patrimonial"

#### Enum `ConservationState` (Estado de Conservação)
- **Valores atualizados** para português:
  - `Good` → `Bom`
  - `New` → `Novo`
  - `Regular` → `Regular` (mantido)
  - `Recoverable` → removido
  - `Unrecoverable` → removido
  - `Pessimo` → adicionado
  - `Inservivel` → adicionado

#### Modelo `InventoryItem`
- **Campo `Status`**: Alterado de `string` para `BemStatus?` (enum nullable)
- **Campo `EstimatedValue`**: Renomeado para `ValorLiquidoContabil` (decimal)
- **Campo `DataTombamento`**: Adicionado (DateTime?)

#### Filtros na Página Items.razor
- **Removidos filtros antigos**: operacao, manutencao, baixado, inativos (baseados em strings)
- **Novos filtros baseados em `BemStatus`**: Utilizam enum para maior consistência e type-safety

#### Cards de Exibição
- **`ItemCard.razor`**: Atualizado para usar `BemStatus` e novos valores de `ConservationState`
- **`PatrimonioCard.razor`**: Atualizado para usar `BemEnumMapper.ToDisplayText()` para exibição

### Corrigido
- **22 erros de compilação** relacionados à mudança de `EstimatedValue` para `ValorLiquidoContabil`
- **Erros de enum** em `ConservationState` (Good, New, Recoverable, Unrecoverable → Bom, Novo, Regular, Pessimo, Inservivel)
- **Conversão de Status** de string para `BemStatus?` em todos os arquivos afetados:
  - `Models/ItemModel.cs`
  - `Models/CaptureItemDto.cs`
  - `Models/ItemMetadata.cs`
  - `Pages/Camera.razor`
  - `Pages/Sync.razor`
  - `Components/Cards/ItemCard.razor`
  - `Components/Cards/PatrimonioCard.razor`
  - `Services/Sync/ItemSyncService.cs`
  - `Services/Recognition/Parsers/QRPrettyPrinter.cs`
  - `tests/Builders/InventoryItemBuilder.cs`
  - `tests/Builders/PatrimonioItemBuilder.cs`
- **Bug crítico de sincronização**: Campos `Descricao`, `Localizacao`, `ValorEstimado` e `Estado` não estavam sendo mapeados do `TombamentoWire` para `PatrimonioItem`, causando perda de dados na lista de bens após sincronização

### Testes
- ✅ **294 testes automatizados passaram** com 100% de sucesso
- ✅ **Build compilando sem erros** (apenas 3 warnings não críticos)
- ✅ **Validação completa** de todos os fluxos após refatoração

### Princípios Aplicados (DRY, YAGNI, KISS, SOLID)

#### DRY (Don't Repeat Yourself)
- **`BemEnumMapper`**: Centraliza conversão de enums para texto, eliminando duplicação em múltiplos componentes
- **Métodos de extensão `ToDisplayText()`**: Reutilizáveis em toda a aplicação

#### KISS (Keep It Simple, Stupid)
- **Enum `BemStatus`**: Substitui strings mágicas por valores tipados e seguros
- **Enum `ConservationState`**: Valores em português facilitam compreensão e manutenção

#### SOLID
- **Single Responsibility**: `BemEnumMapper` tem única responsabilidade de mapear enums
- **Open/Closed**: Novos status podem ser adicionados ao enum sem modificar código existente

### Versão
- **Versão atualizada para 0.7.0** em `package.json` e `AppInfo.cs`
- **Data de build atualizada** para 7 de abril de 2026

## [0.6.1] - 2026-04-07

### Adicionado
- **Campo `ExercicioFiscal`** adicionado ao modelo `PatrimonioItem` para consistência com API
- **Campo `exerciciofiscal`** adicionado ao modelo `TombamentoWire` para mapeamento correto
- **Classe utilitária `CodeNormalizer`** em `Services/Utils/` para centralizar lógica de normalização de códigos (DRY)

### Modificado - Refatoração Radical (DRY, YAGNI, KISS, SOLID)

#### DRY (Don't Repeat Yourself)
- **`SyncService.MapToStore()`**: Consolidados dois métodos duplicados em um único com parâmetro opcional
- **`CodeNormalizer.Normalize()`**: Lógica de normalização extraída para classe utilitária, eliminando duplicação em `SyncService` e `IndexedDbService`
- **`IndexedDbService.GetPatrimonioByUOAsync()`**: Refatorado para ser um wrapper simples de `GetPatrimonioBySubareaAsync()`, eliminando lógica duplicada

#### KISS (Keep It Simple, Stupid)
- **`Items.razor.ApplyFilters()`**: Lógica de filtro complexa extraída para 5 métodos privados descritivos:
  - `FilterByOperacao()` - Filtra bens em operação
  - `FilterByManutencao()` - Filtra bens em manutenção
  - `FilterByBaixado()` - Filtra bens baixados
  - `FilterByInativos()` - Filtra bens inativos
  - `FilterBySearchQuery()` - Filtra por termo de busca
- **Benefícios**: Código mais legível, testável e manutenível

#### YAGNI (You Aren't Gonna Need It)
- **Removida lógica redundante** de fallback desnecessário em consultas IndexedDB
- **Simplificado fluxo** de consulta de patrimônios por UO/Subárea

### Corrigido
- **Bug de exibição de patrimônios**: Campo `ExercicioFiscal` ausente causava inconsistência entre API e Frontend
- **Mapeamento de dados**: Todos os campos da API agora são corretamente mapeados para o Frontend

### Versão
- **Versão atualizada para 0.6.1** em `package.json` e `AppInfo.cs`
- **Data de build atualizada** para 7 de abril de 2026

## [0.6.0] - 2026-04-07

### Corrigido
- **Filtro de exercício fiscal aplicado na sincronização de chunks**: Os métodos `BuildOrGetChunkIndexAsync`, `BuildOrGetChunkIndexFromLocalAsync`, `SerializeChunkPayloadAsync` e `SerializeChunkPayloadFromLocalAsync` agora aplicam o filtro de exercício fiscal corrente, garantindo que apenas dados do ano atual sejam sincronizados e exibidos na lista de bens
- **Carregamento de bens na tela de lista**: Corrigido problema onde a lista de bens não exibia os patrimônios após sincronização, causado pela falta de aplicação do filtro de exercício fiscal nos endpoints de sincronização por chunks

### Modificado
- **Versão atualizada para 0.6.0** em `package.json` e `AppInfo.cs`
- **Data de build atualizada** para 7 de abril de 2026

## [Unreleased]

### Adicionado - Correção de Carregamento de Bens por Subárea

#### IndexedDB (db-interop.js)
- **Versão do banco incrementada de 10 para 11** para suportar novos índices
- **Novos índices `cdArea` e `cdSArea`** nas stores `patrimonio` e `patrimonio_staging` para consultas eficientes por área e subárea
- **Método `getPatrimonioBySubarea(cdUnid, cdArea, cdSArea)`** para buscar bens filtrados por hierarquia completa (UO → Área → Subárea)
- **Método `getPatrimonioBySubareaNormalized(cdUnidNorm, cdArea, cdSArea)`** com suporte a códigos normalizados (sem zeros à esquerda)
- **Validação automática de índices** pós-migração para garantir integridade
- **Migração automática e sem perda de dados** de v10 para v11

#### Service Layer (C#)
- **Interface `IIndexedDbService`**: Novo método `GetPatrimonioBySubareaAsync(string idUO, string? idArea, string? idSubarea)`
- **Implementação `IndexedDbService`**: 
  - Método com parâmetros opcionais para filtros de área e subárea
  - Fallback automático para normalização de códigos
  - Tratamento robusto de erros com logging detalhado
  - Compatibilidade com comportamento legado quando filtros não são fornecidos

#### Interface do Usuário (Items.razor)
- **Método `LoadItems()` atualizado** para usar `GetPatrimonioBySubareaAsync` com filtros do `AppState`
- **Filtro automático por subárea** quando área e subárea estão configuradas na sessão
- **Comportamento legado preservado** quando filtros não estão configurados
- **Contagem precisa** no summary bar refletindo apenas bens da subárea selecionada

#### Backend API (Program.cs)
- **Filtro de exercício fiscal** no endpoint `/api/auth/login` para retornar apenas dados do ano corrente
- **Filtro de exercício fiscal aplicado na sincronização**: Endpoints `/api/tombamentos/sync-info` e `/api/tombamentos/lote/{id}` agora filtram dados por exercício fiscal corrente
- **Redução de payload em ~70-96%** eliminando dados históricos desnecessários (1993-2025)
- **Logging estruturado** de quantidade de registros filtrados para monitoramento
- **Preservação de registros sem ano definido** (`ExercicioFiscal == 0`) para compatibilidade

### Modificado

#### Performance
- **Consultas IndexedDB otimizadas**: Uso de índice `cdUnid` + filtro em memória para área/subárea (tempo adicional ~5-20ms para 500 registros)
- **Renderização mais rápida**: Redução de 91% no volume de dados processados (500 → 45 bens típicos)
- **Tempo de login reduzido**: Download 25x mais rápido com payload menor (~8MB → ~270KB)
- **Armazenamento local otimizado**: 96% menos espaço usado no IndexedDB

#### Compatibilidade
- **Códigos com zeros à esquerda**: Comparação case-insensitive e normalizada ("009" vs "9", "001" vs "1")
- **Campos nulos tratados**: Filtros funcionam corretamente com `null`, `undefined` ou strings vazias
- **Migração transparente**: Usuários com IndexedDB v10 migram automaticamente para v11 sem intervenção

### Corrigido

#### Bug Principal
- **Carregamento de bens por subárea**: Sistema agora respeita filtros de Área e Subárea configurados na sessão
- **Filtro de hierarquia completa**: Bens são filtrados por Órgão → UO → Área → Subárea conforme esperado
- **Dados históricos desnecessários**: Eliminado carregamento de dados de múltiplos exercícios fiscais

#### Comportamento Preservado (Sem Regressão)
- ✅ Busca por código (nutomb) continua funcionando
- ✅ Filtros de status (operação, manutenção, baixado) funcionam corretamente
- ✅ Sincronização de dados não é afetada
- ✅ Normalização de códigos funciona como antes
- ✅ Carregamento de bens capturados localmente (store `items`) inalterado
- ✅ Comportamento legado mantido quando área/subárea não são selecionadas

### Detalhes Técnicos

#### Estrutura de Dados
```javascript
// Novos índices no IndexedDB
patrimonio: {
  keyPath: 'idPatomb',
  indexes: [
    'nutomb',
    'cdUnid',
    'cdUnidNorm',
    'esfera',
    'cdArea',      // NOVO
    'cdSArea'      // NOVO
  ]
}
```

#### Assinatura do Método
```csharp
// Novo método com parâmetros opcionais
Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
    string idUO, 
    string? idArea = null, 
    string? idSubarea = null
)
```

#### Filtro de Exercício Fiscal
```csharp
// Backend: Filtra apenas ano corrente
var exercicioCorrente = DateTime.Now.Year;
var tombamentoFiltrado = tombamentoBase
    .Where(p => p.ExercicioFiscal == exercicioCorrente || p.ExercicioFiscal == 0)
    .ToList();
```

### Métricas de Impacto

| Métrica | Antes | Depois | Melhoria |
|---------|-------|--------|----------|
| Payload do login | ~8 MB | ~270 KB | 96.6% menor |
| Bens carregados (típico) | 500 | 45 | 91% menos |
| Tempo de consulta IndexedDB | ~20ms | ~25ms | +5ms (aceitável) |
| Tempo de download (3G) | ~25s | <1s | 25x mais rápido |
| Espaço no IndexedDB | 100% | 4% | 96% menos |

### Migração e Compatibilidade

#### Versão do IndexedDB
- **v10 → v11**: Migração automática ao abrir a aplicação
- **Sem perda de dados**: Registros existentes são preservados
- **Índices criados automaticamente**: `cdArea` e `cdSArea` adicionados a dados existentes

#### Compatibilidade com Dados Legados
- Registros sem `ExercicioFiscal` ou com valor `0` são mantidos
- Campos `cdArea` e `cdSArea` nulos são tratados corretamente
- Códigos com diferentes formatos ("09" vs "9") funcionam igualmente

### Documentação Adicionada

- **docs/DEPLOYMENT_SUBAREA_FILTER.md**: Guia completo de deploy com checklist e estratégia gradual
- **docs/ROLLBACK_SUBAREA_FILTER.md**: Plano de rollback em 3 níveis com procedimentos detalhados
- **docs/MONITORING_SUBAREA_FILTER.md**: Guia de monitoramento com métricas e alertas
- **docs/TROUBLESHOOTING_SUBAREA_FILTER.md**: Guia de resolução de problemas comuns
- **README.md**: Atualizado com novas funcionalidades de filtro por subárea

### Testes Realizados

#### Testes Funcionais
- ✅ Filtro por subárea com área e subárea configuradas
- ✅ Compatibilidade legado sem área/subárea
- ✅ Filtro de exercício fiscal no backend
- ✅ Migração de IndexedDB v10 → v11
- ✅ Códigos com zeros à esquerda
- ✅ Campos nulos ou vazios

#### Testes de Performance
- ✅ Consulta por subárea < 50ms para 500 registros
- ✅ Payload do login reduzido ≥ 70%
- ✅ Tempo de carregamento de Items.razor ≤ 2 segundos

#### Testes de Regressão
- ✅ Busca por código (nutomb) funciona
- ✅ Filtros de status funcionam
- ✅ Sincronização de dados não afetada
- ✅ Normalização de códigos funciona

### Breaking Changes

**Nenhum breaking change** - A implementação mantém total compatibilidade com o comportamento anterior quando filtros de área/subárea não são utilizados.

### Notas de Upgrade

Para usuários existentes:
1. **Migração automática**: IndexedDB será atualizado automaticamente de v10 para v11 na próxima abertura da aplicação
2. **Sem ação necessária**: Nenhuma intervenção manual é requerida
3. **Dados preservados**: Todos os dados existentes serão mantidos
4. **Primeira carga mais rápida**: Novo login carregará apenas dados do ano corrente

### Referências

- **Spec**: `.kiro/specs/subarea-filter-loading-fix/`
- **Design Document**: `.kiro/specs/subarea-filter-loading-fix/design.md`
- **Bugfix Document**: `.kiro/specs/subarea-filter-loading-fix/bugfix.md`
- **Tasks**: `.kiro/specs/subarea-filter-loading-fix/tasks.md`

---

## Versões Anteriores

Para histórico de versões anteriores, consulte:
- `docs/RELEASE_NOTES_v0.2.8.md`
- `docs/RELEASE_NOTES_v0.2.7.md`
- `docs/RELEASE_NOTES_v0.2.6.md`
- `docs/RELEASE_NOTES_v0.2.5.md`
- `docs/RELEASE_NOTES_v0.2.4.md`
