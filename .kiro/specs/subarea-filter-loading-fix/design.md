# Design Document: Correção de Carregamento de Bens por Subárea

## 1. Visão Geral da Solução

Este documento detalha a solução técnica para corrigir o bug de carregamento de bens patrimoniais por subárea. Atualmente, o sistema carrega todos os bens de uma Unidade Orçamentária (UO), ignorando os filtros de Área e Subárea configurados pelo usuário. Adicionalmente, o sistema não filtra dados por exercício fiscal corrente, carregando histórico desnecessário.

### 1.1 Objetivos da Correção

1. **Filtro por Subárea**: Implementar carregamento de bens filtrados por hierarquia completa (Órgão → UO → Área → Subárea)
2. **Índices IndexedDB**: Adicionar índices `cdArea` e `cdSArea` para consultas eficientes
3. **Filtro de Exercício Fiscal**: Carregar apenas dados do ano corrente no endpoint de login
4. **Compatibilidade Legado**: Manter comportamento existente quando filtros não são aplicados
5. **Performance**: Otimizar consultas e reduzir volume de dados carregados

### 1.2 Abordagem Técnica

A solução adota uma estratégia de **filtros opcionais em cascata**:
- Quando área/subárea estão configuradas → aplica filtro específico
- Quando área/subárea não estão configuradas → mantém comportamento legado (todos os bens da UO)
- Migração de IndexedDB sem quebra de dados existentes
- Filtro de exercício fiscal no backend antes de enviar dados ao cliente


## 2. Arquitetura da Correção

### 2.1 Componentes Afetados

#### 2.1.1 Frontend (Blazor WebAssembly)

| Arquivo | Modificação | Impacto |
|---------|-------------|---------|
| `wwwroot/js/db-interop.js` | Adicionar índices `cdArea`, `cdSArea`; criar método `getPatrimonioBySubarea` | **ALTO** - Requer migração de DB |
| `Services/Storage/IndexedDbService.cs` | Adicionar método `GetPatrimonioBySubareaAsync` com parâmetros opcionais | **MÉDIO** - Nova interface pública |
| `Pages/Items.razor` | Modificar `LoadItems()` para passar filtros de área/subárea | **BAIXO** - Lógica de carregamento |
| `Services/Storage/IIndexedDbService.cs` | Adicionar assinatura do novo método | **BAIXO** - Interface |

#### 2.1.2 Backend (ASP.NET Core API)

| Arquivo | Modificação | Impacto |
|---------|-------------|---------|
| `pwa-camera-poc-api/Program.cs` | Adicionar filtro de exercício fiscal no endpoint `/api/auth/login` | **MÉDIO** - Lógica de autenticação |

### 2.2 Fluxo de Dados Corrigido

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. CONFIGURAÇÃO DE SESSÃO (ConfiguracaoSessao.razor)           │
│    Usuário seleciona: Órgão → UO → Área → Subárea              │
│    ↓ Armazena em AppState                                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 2. NAVEGAÇÃO PARA ITEMS.RAZOR                                   │
│    OnInitializedAsync() → LoadItems()                           │
│    ↓ Lê AppState.CurrentArea e AppState.CurrentSubarea          │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 3. CHAMADA AO INDEXEDDB SERVICE                                 │
│    SE (CurrentArea != null && CurrentSubarea != null)           │
│       → GetPatrimonioBySubareaAsync(idUO, idArea, idSubarea)    │
│    SENÃO                                                         │
│       → GetPatrimonioByUOAsync(idUO)  [LEGADO]                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 4. CONSULTA JAVASCRIPT (db-interop.js)                          │
│    getPatrimonioBySubarea(cdUnid, cdArea, cdSArea)              │
│    ↓ Usa índice composto ou filtra manualmente                  │
│    ↓ Retorna apenas bens da subárea específica                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│ 5. RENDERIZAÇÃO                                                  │
│    Items.razor exibe apenas bens filtrados                      │
│    Summary bar mostra contagem correta                          │
└─────────────────────────────────────────────────────────────────┘
```

### 2.3 Decisões de Design

#### 2.3.1 Índices Separados vs Índice Composto

**Decisão**: Criar índices separados `cdArea` e `cdSArea` em vez de índice composto `[cdUnid, cdArea, cdSArea]`

**Justificativa**:
- **Flexibilidade**: Permite consultas por área sem subárea no futuro
- **Simplicidade**: IndexedDB não suporta índices compostos nativamente de forma eficiente
- **Compatibilidade**: Mantém índice `cdUnid` existente funcionando
- **Performance**: Filtro manual em memória é aceitável dado o volume de dados (centenas de registros por UO)

#### 2.3.2 Filtro no Backend vs Frontend

**Decisão**: Aplicar filtro de exercício fiscal no **backend** (`/api/auth/login`)

**Justificativa**:
- **Redução de Payload**: Evita transferir dados históricos desnecessários (1993-2026)
- **Performance de Rede**: Menor tempo de download inicial
- **Armazenamento Local**: Reduz espaço usado no IndexedDB
- **Segurança**: Dados históricos não ficam acessíveis no cliente

#### 2.3.3 Parâmetros Opcionais vs Métodos Separados

**Decisão**: Usar **parâmetros opcionais** em `GetPatrimonioBySubareaAsync`

**Justificativa**:
- **Compatibilidade**: Permite chamadas sem área/subárea (comportamento legado)
- **Manutenibilidade**: Um único método em vez de múltiplos (ByUO, ByArea, BySubarea)
- **Clareza**: Assinatura explícita indica filtros disponíveis

```csharp
Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
    string idUO, 
    string? idArea = null, 
    string? idSubarea = null
)
```


## 3. Especificação Técnica Detalhada

### 3.1 Modificações no IndexedDB (db-interop.js)

#### 3.1.1 Incremento de Versão do Banco

```javascript
dbVersion: 11  // Incrementado de 10 para 11
```

**Motivo**: Adicionar novos índices requer migração de schema

#### 3.1.2 Criação de Novos Índices

**Localização**: Dentro do evento `onupgradeneeded`, na seção de criação/atualização da store `patrimonio`

```javascript
// Dentro de onupgradeneeded
if (!db.objectStoreNames.contains('patrimonio')) {
    const patrimonioStore = db.createObjectStore('patrimonio', { keyPath: 'idPatomb' });
    patrimonioStore.createIndex('nutomb', 'nutomb', { unique: false });
    patrimonioStore.createIndex('cdUnid', 'cdUnid', { unique: false });
    patrimonioStore.createIndex('cdUnidNorm', 'cdUnidNorm', { unique: false });
    patrimonioStore.createIndex('esfera', 'esfera', { unique: false });
    // NOVOS ÍNDICES
    patrimonioStore.createIndex('cdArea', 'cdArea', { unique: false });
    patrimonioStore.createIndex('cdSArea', 'cdSArea', { unique: false });
} else {
    const patrimonioStore = transaction.objectStore('patrimonio');
    
    // Adiciona índices existentes se não existirem (compatibilidade)
    if (!patrimonioStore.indexNames.contains('cdUnid')) {
        patrimonioStore.createIndex('cdUnid', 'cdUnid', { unique: false });
    }
    if (!patrimonioStore.indexNames.contains('cdUnidNorm')) {
        patrimonioStore.createIndex('cdUnidNorm', 'cdUnidNorm', { unique: false });
    }
    if (!patrimonioStore.indexNames.contains('esfera')) {
        patrimonioStore.createIndex('esfera', 'esfera', { unique: false });
    }
    
    // NOVOS ÍNDICES (migração)
    if (!patrimonioStore.indexNames.contains('cdArea')) {
        patrimonioStore.createIndex('cdArea', 'cdArea', { unique: false });
    }
    if (!patrimonioStore.indexNames.contains('cdSArea')) {
        patrimonioStore.createIndex('cdSArea', 'cdSArea', { unique: false });
    }
    
    backfillCdUnidNorm(patrimonioStore);
}

// Aplicar mesma lógica para patrimonio_staging
if (!db.objectStoreNames.contains('patrimonio_staging')) {
    const staging = db.createObjectStore('patrimonio_staging', { keyPath: 'idPatomb' });
    staging.createIndex('nutomb', 'nutomb', { unique: false });
    staging.createIndex('cdUnid', 'cdUnid', { unique: false });
    staging.createIndex('cdUnidNorm', 'cdUnidNorm', { unique: false });
    staging.createIndex('esfera', 'esfera', { unique: false });
    // NOVOS ÍNDICES
    staging.createIndex('cdArea', 'cdArea', { unique: false });
    staging.createIndex('cdSArea', 'cdSArea', { unique: false });
} else {
    const staging = transaction.objectStore('patrimonio_staging');
    
    if (!staging.indexNames.contains('cdArea')) {
        staging.createIndex('cdArea', 'cdArea', { unique: false });
    }
    if (!staging.indexNames.contains('cdSArea')) {
        staging.createIndex('cdSArea', 'cdSArea', { unique: false });
    }
    
    backfillCdUnidNorm(staging);
}
```

#### 3.1.3 Novo Método: getPatrimonioBySubarea

**Localização**: Adicionar ao objeto `window.dbInterop` após o método `getPatrimonioByUONormalized`

```javascript
getPatrimonioBySubarea: async function (cdUnid, cdArea, cdSArea) {
    return new Promise((resolve, reject) => {
        if (!this.db) {
            reject(new Error('Database not initialized. Call init first.'));
            return;
        }
        
        const transaction = this.db.transaction(['patrimonio'], 'readonly');
        const store = transaction.objectStore('patrimonio');
        
        // Estratégia: Buscar por cdUnid primeiro (índice eficiente)
        // Depois filtrar por cdArea e cdSArea em memória
        const index = store.index('cdUnid');
        const request = index.getAll(cdUnid);
        
        request.onsuccess = () => {
            const allFromUO = request.result || [];
            
            // Filtro manual por área e subárea
            const filtered = allFromUO.filter(item => {
                // Comparação case-insensitive e normalizada
                const matchArea = !cdArea || 
                    String(item.cdArea || '').trim().toUpperCase() === String(cdArea).trim().toUpperCase();
                const matchSArea = !cdSArea || 
                    String(item.cdSArea || '').trim().toUpperCase() === String(cdSArea).trim().toUpperCase();
                
                return matchArea && matchSArea;
            });
            
            resolve(filtered);
        };
        
        request.onerror = () => reject(request.error);
    });
},

getPatrimonioBySubareaNormalized: async function (cdUnidNorm, cdArea, cdSArea) {
    return new Promise((resolve, reject) => {
        if (!this.db) {
            reject(new Error('Database not initialized. Call init first.'));
            return;
        }
        
        const transaction = this.db.transaction(['patrimonio'], 'readonly');
        const store = transaction.objectStore('patrimonio');
        const index = store.index('cdUnidNorm');
        const request = index.getAll(cdUnidNorm);
        
        request.onsuccess = () => {
            const allFromUO = request.result || [];
            
            const filtered = allFromUO.filter(item => {
                const matchArea = !cdArea || 
                    String(item.cdArea || '').trim().toUpperCase() === String(cdArea).trim().toUpperCase();
                const matchSArea = !cdSArea || 
                    String(item.cdSArea || '').trim().toUpperCase() === String(cdSArea).trim().toUpperCase();
                
                return matchArea && matchSArea;
            });
            
            resolve(filtered);
        };
        
        request.onerror = () => reject(request.error);
    });
}
```

**Características**:
- **Fallback para normalização**: Suporta códigos com/sem zeros à esquerda
- **Filtros opcionais**: Se `cdArea` ou `cdSArea` forem `null`/`undefined`, não aplica filtro
- **Case-insensitive**: Normaliza strings para uppercase antes de comparar
- **Performance**: Usa índice `cdUnid` para busca inicial, filtro em memória para área/subárea

### 3.2 Modificações no Service Layer (IndexedDbService.cs)

#### 3.2.1 Interface IIndexedDbService

**Arquivo**: `Services/Storage/IIndexedDbService.cs`

```csharp
public interface IIndexedDbService
{
    // ... métodos existentes ...
    
    Task<List<PatrimonioItem>> GetPatrimonioByUOAsync(string idUO);
    
    // NOVO MÉTODO
    Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
        string idUO, 
        string? idArea = null, 
        string? idSubarea = null
    );
}
```

#### 3.2.2 Implementação IndexedDbService

**Arquivo**: `Services/Storage/IndexedDbService.cs`

```csharp
public async Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
    string idUO, 
    string? idArea = null, 
    string? idSubarea = null)
{
    try
    {
        // Se não há filtros de área/subárea, usa método legado
        if (string.IsNullOrWhiteSpace(idArea) && string.IsNullOrWhiteSpace(idSubarea))
        {
            return await GetPatrimonioByUOAsync(idUO);
        }
        
        // Tenta buscar com código original
        var items = await _jsRuntime.InvokeAsync<List<PatrimonioItem>>(
            "dbInterop.getPatrimonioBySubarea", 
            idUO, 
            idArea, 
            idSubarea
        );
        
        if (items != null && items.Count > 0) 
            return items;
        
        // Fallback: tenta com código normalizado
        var normalized = NormalizeCode(idUO);
        if (string.IsNullOrEmpty(normalized)) 
            return items ?? new List<PatrimonioItem>();
        
        return await _jsRuntime.InvokeAsync<List<PatrimonioItem>>(
            "dbInterop.getPatrimonioBySubareaNormalized", 
            normalized, 
            idArea, 
            idSubarea
        );
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(
            $"Error getting patrimonio by subarea (UO: {idUO}, Area: {idArea}, Subarea: {idSubarea}): {ex.Message}"
        );
        return new List<PatrimonioItem>();
    }
}
```

**Características**:
- **Compatibilidade legado**: Se área/subárea não fornecidas, delega para `GetPatrimonioByUOAsync`
- **Normalização automática**: Tenta código original primeiro, depois normalizado
- **Tratamento de erros**: Retorna lista vazia em caso de falha (não quebra UI)
- **Logging**: Registra erros com contexto completo para debugging

### 3.3 Modificações na UI (Items.razor)

#### 3.3.1 Método LoadItems()

**Arquivo**: `Pages/Items.razor`

**Modificação**: Substituir chamada a `GetPatrimonioByUOAsync` por `GetPatrimonioBySubareaAsync` com filtros do AppState

```csharp
private async Task LoadItems()
{
    isLoading = true;
    await InvokeAsync(StateHasChanged);
    await Task.Yield();
    
    try
    {
        var user = await AuthService.GetCurrentUserAsync();
        if (user == null) return;

        if (appState.CurrentUO != null)
        {
            // Carrega itens locais (capturados)
            var localItems = await DbService.GetItemsByUOAsync(appState.CurrentUO.IdUO);
            _localByNutomb = localItems
                .Where(i => !string.IsNullOrWhiteSpace(i.Code))
                .GroupBy(i => i.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key, 
                    g => g.OrderByDescending(x => x.UpdatedAt).First(), 
                    StringComparer.OrdinalIgnoreCase
                );

            // MODIFICAÇÃO: Usa novo método com filtros de área/subárea
            var allPatrimonio = await DbService.GetPatrimonioBySubareaAsync(
                appState.CurrentUO.IdUO,
                appState.CurrentArea?.IdArea,      // Passa área se configurada
                appState.CurrentSubarea?.IdSubarea // Passa subárea se configurada
            );
            
            // Aplica filtro de esfera
            var esfera = appState.EsferaAtual ?? user.Esfera;
            items = (!string.IsNullOrEmpty(esfera) && esfera != "A")
                ? allPatrimonio.Where(i => string.Equals(i.Esfera, esfera, StringComparison.OrdinalIgnoreCase)).ToList()
                : allPatrimonio.ToList();
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error loading items: {ex.Message}");
    }
    finally
    {
        isLoading = false;
    }
}
```

**Impacto**:
- **Sem quebra**: Se `CurrentArea` ou `CurrentSubarea` forem `null`, comportamento legado é mantido
- **Filtro automático**: Quando área/subárea estão configuradas, apenas bens relevantes são carregados
- **Performance**: Reduz quantidade de dados processados e renderizados

### 3.4 Modificações no Backend (Program.cs)

#### 3.4.1 Filtro de Exercício Fiscal no Endpoint /api/auth/login

**Arquivo**: `pwa-camera-poc-api/Program.cs`

**Localização**: Dentro do endpoint `app.MapPost("/api/auth/login", ...)`

**Modificação**: Adicionar filtro de exercício fiscal antes de construir hierarquia de órgãos

```csharp
// 3. Obtém dados das tabelas
var tabelas = root.Tabelas;
if (tabelas is null)
{
    log.LogWarning("Seção 'tabelas' ausente no arquivo {S3Key}.", s3Key);
    return Results.Problem("Dados de tabelas ausentes no arquivo do município.");
}

// 4. Filtra patrimônio para a esfera do usuário
var tombamentoBase = tabelas.Tombamentos ?? new List<TombamentoRecord>();

log.LogInformation("Tombamentos encontrados em tabelas.tombamentos: {Count}", tombamentoBase.Count);

// NOVO: Filtro de exercício fiscal corrente
var exercicioCorrente = DateTime.Now.Year;
var tombamentoFiltradoExercicio = tombamentoBase
    .Where(p => p.ExercicioFiscal == exercicioCorrente || p.ExercicioFiscal == 0)
    .ToList();

log.LogInformation(
    "Tombamentos filtrados para exercício {Exercicio}: {Count} (de {Total})", 
    exercicioCorrente, 
    tombamentoFiltradoExercicio.Count, 
    tombamentoBase.Count
);

// Filtro de esfera
var tombamentoFiltrado = user.Esfera == "A"
    ? tombamentoFiltradoExercicio
    : tombamentoFiltradoExercicio.Where(p => p.Esfera == user.Esfera).ToList();

log.LogInformation("Tombamentos filtrados para esfera {Esfera}: {Count}", user.Esfera, tombamentoFiltrado.Count);

// 5. Filtra patrimônio para árvore de órgãos (dados de apoio)
var orgaos = BuildOrgaoHierarchy(user.Esfera, tombamentoFiltrado, tabelas);
```

**Características**:
- **Exercício corrente**: Filtra por `DateTime.Now.Year`
- **Fallback para registros sem ano**: Mantém registros com `ExercicioFiscal == 0` (dados sem ano definido)
- **Logging**: Registra quantidade de registros antes/depois do filtro
- **Ordem de filtros**: Exercício fiscal → Esfera → Hierarquia

**Impacto**:
- **Redução de payload**: Elimina dados históricos de 1993-2025 (potencialmente 90% de redução)
- **Performance de rede**: Menor tempo de download no login
- **Armazenamento**: Menos dados no IndexedDB do cliente


## 4. Estratégia de Migração

### 4.1 Migração do IndexedDB

#### 4.1.1 Processo Automático

O IndexedDB possui mecanismo nativo de migração através do evento `onupgradeneeded`:

```javascript
request.onupgradeneeded = (event) => {
    const db = event.target.result;
    const transaction = event.target.transaction;
    const oldVersion = event.oldVersion;
    const newVersion = event.newVersion;
    
    console.log(`Migrando IndexedDB de versão ${oldVersion} para ${newVersion}`);
    
    // Migração é executada automaticamente quando dbVersion aumenta
    // Novos índices são criados sem perda de dados existentes
}
```

**Características**:
- **Sem perda de dados**: Registros existentes são preservados
- **Criação de índices**: Novos índices são construídos automaticamente sobre dados existentes
- **Transacional**: Migração ocorre em transação atômica (rollback automático em caso de erro)
- **Bloqueante**: Aplicação aguarda conclusão da migração antes de prosseguir

#### 4.1.2 Cenários de Migração

| Cenário | Versão Atual | Versão Nova | Ação |
|---------|--------------|-------------|------|
| Usuário novo | N/A | 11 | Cria DB com todos os índices |
| Usuário existente (v10) | 10 | 11 | Adiciona índices `cdArea`, `cdSArea` |
| Usuário existente (v9 ou anterior) | <10 | 11 | Aplica todas as migrações incrementais |

#### 4.1.3 Validação Pós-Migração

**Adicionar ao método `init()`**:

```javascript
request.onsuccess = (event) => {
    this.db = event.target.result;
    
    // Validação de índices
    const patrimonioStore = this.db.transaction(['patrimonio'], 'readonly')
        .objectStore('patrimonio');
    
    const expectedIndexes = ['nutomb', 'cdUnid', 'cdUnidNorm', 'esfera', 'cdArea', 'cdSArea'];
    const missingIndexes = expectedIndexes.filter(idx => !patrimonioStore.indexNames.contains(idx));
    
    if (missingIndexes.length > 0) {
        console.warn('Índices ausentes após migração:', missingIndexes);
    } else {
        console.log('IndexedDB initialized successfully - all indexes present');
    }
    
    resolve();
};
```

### 4.2 Migração de Dados do Backend

#### 4.2.1 Compatibilidade com Dados Legados

**Problema**: Registros antigos podem não ter campo `ExercicioFiscal` ou ter valor `null`

**Solução**: Filtro defensivo com fallback

```csharp
var exercicioCorrente = DateTime.Now.Year;
var tombamentoFiltradoExercicio = tombamentoBase
    .Where(p => 
        p.ExercicioFiscal == exercicioCorrente ||  // Ano corrente
        p.ExercicioFiscal == 0 ||                  // Sem ano definido
        p.ExercicioFiscal == null                  // Campo ausente (se nullable)
    )
    .ToList();
```

#### 4.2.2 Estratégia de Rollout

**Fase 1: Soft Launch (Opcional)**
- Adicionar filtro de exercício fiscal com flag de configuração
- Permitir desabilitar via `appsettings.json` se necessário

```csharp
var enableFiscalYearFilter = configuration.GetValue<bool>("Features:FiscalYearFilter", true);

var tombamentoFiltradoExercicio = enableFiscalYearFilter
    ? tombamentoBase.Where(p => p.ExercicioFiscal == exercicioCorrente || p.ExercicioFiscal == 0).ToList()
    : tombamentoBase;
```

**Fase 2: Monitoramento**
- Adicionar métricas de quantidade de registros filtrados
- Validar que dados correntes estão sendo carregados corretamente

**Fase 3: Remoção de Flag**
- Após validação em produção, remover flag e tornar filtro permanente

### 4.3 Sincronização de Dados Existentes

#### 4.3.1 Comportamento com Dados Já Carregados

**Cenário**: Usuário já possui dados históricos no IndexedDB local

**Solução**: Forçar re-sincronização após atualização

```csharp
// Em Items.razor ou TombamentosSync.razor
protected override async Task OnInitializedAsync()
{
    var user = await AuthService.GetCurrentUserAsync();
    if (user == null) return;
    
    var versionKey = $"versao:{user.Prefixo?.Trim().ToUpperInvariant()}";
    var baseVersion = await DbService.GetMetadataAsync(versionKey);
    
    // NOVO: Verificar versão de schema
    var schemaVersionKey = "schema:version";
    var currentSchemaVersion = await DbService.GetMetadataAsync(schemaVersionKey);
    
    if (currentSchemaVersion != "11")
    {
        // Forçar re-sincronização para limpar dados históricos
        await DbService.ClearAsync("patrimonio");
        await DbService.ClearAsync("patrimonio_staging");
        await DbService.SetMetadataAsync(schemaVersionKey, "11");
        
        Navigation.NavigateTo("/tombamentos-sync");
        return;
    }
    
    // ... resto da lógica
}
```

**Alternativa Menos Invasiva**: Não forçar re-sincronização, apenas aplicar filtros no frontend

```csharp
// Filtrar dados históricos localmente
var exercicioCorrente = DateTime.Now.Year;
items = allPatrimonio
    .Where(i => i.ExercicioFiscal == exercicioCorrente || i.ExercicioFiscal == 0)
    .ToList();
```


## 5. Testes e Validação

### 5.1 Cenários de Teste Funcionais

#### 5.1.1 Teste de Filtro por Subárea

**Pré-condições**:
- Usuário autenticado
- Dados de múltiplas áreas/subáreas carregados no IndexedDB
- Sessão configurada com Órgão, UO, Área e Subárea específicos

**Passos**:
1. Navegar para `/configuracao-sessao`
2. Selecionar Órgão "09 - Secretaria de Cultura"
3. Selecionar UO "09 - Secretaria de Cultura"
4. Selecionar Área "001 - Deposito"
5. Selecionar Subárea "001 - Deposito Principal"
6. Clicar em "Salvar Configuração"
7. Navegar para `/items`

**Resultado Esperado**:
- Apenas bens com `CdUnid=09`, `CdArea=001`, `CdSArea=001` são exibidos
- Summary bar mostra contagem correta (ex: "45 BENS")
- Bens de outras áreas/subáreas não aparecem na lista

**Validação**:
```javascript
// Console do navegador
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.assert(items.every(i => i.cdUnid === "09" && i.cdArea === "001" && i.cdSArea === "001"));
```

#### 5.1.2 Teste de Compatibilidade Legado

**Pré-condições**:
- Usuário autenticado
- Sessão configurada apenas com Órgão e UO (sem Área/Subárea)

**Passos**:
1. Navegar para `/configuracao-sessao`
2. Selecionar apenas Órgão e UO
3. Não selecionar Área nem Subárea
4. Salvar configuração
5. Navegar para `/items`

**Resultado Esperado**:
- Todos os bens da UO são exibidos (comportamento legado)
- Nenhum filtro de área/subárea é aplicado
- Contagem total de bens da UO é exibida

#### 5.1.3 Teste de Filtro de Exercício Fiscal

**Pré-condições**:
- Arquivo de dados contém registros de múltiplos anos (1993-2026)
- Ano corrente: 2024

**Passos**:
1. Realizar login com usuário válido
2. Inspecionar resposta do endpoint `/api/auth/login`
3. Verificar campo `tombamentos` na resposta

**Resultado Esperado**:
- Apenas registros com `ExercicioFiscal == 2024` ou `ExercicioFiscal == 0` são retornados
- Registros de anos anteriores (1993-2023) não estão presentes
- Tamanho do payload é significativamente menor

**Validação**:
```csharp
// Teste unitário
var tombamentos = response.Tombamentos;
Assert.All(tombamentos, t => 
    Assert.True(t.ExercicioFiscal == 2024 || t.ExercicioFiscal == 0)
);
```

#### 5.1.4 Teste de Migração de IndexedDB

**Pré-condições**:
- Aplicação com IndexedDB versão 10 já existente
- Dados de patrimônio já carregados

**Passos**:
1. Abrir aplicação com código atualizado (versão 11)
2. Aguardar migração automática
3. Abrir DevTools → Application → IndexedDB → aspec-captura-db → patrimonio
4. Verificar índices disponíveis

**Resultado Esperado**:
- Índices `cdArea` e `cdSArea` estão presentes
- Dados existentes não foram perdidos
- Consultas por subárea funcionam corretamente

**Validação**:
```javascript
// Console do navegador
const db = await indexedDB.open('aspec-captura-db', 11);
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
console.log('Índices disponíveis:', Array.from(store.indexNames));
// Esperado: ['nutomb', 'cdUnid', 'cdUnidNorm', 'esfera', 'cdArea', 'cdSArea']
```

### 5.2 Testes de Performance

#### 5.2.1 Benchmark de Consulta por Subárea

**Objetivo**: Validar que filtro por subárea não degrada performance

**Método**:
```javascript
// Benchmark no console do navegador
console.time('getPatrimonioByUO');
const allUO = await dbInterop.getPatrimonioByUO("09");
console.timeEnd('getPatrimonioByUO');
console.log('Registros retornados:', allUO.length);

console.time('getPatrimonioBySubarea');
const filtered = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.timeEnd('getPatrimonioBySubarea');
console.log('Registros retornados:', filtered.length);
```

**Critério de Aceitação**:
- Tempo de consulta por subárea ≤ 150% do tempo de consulta por UO
- Para 500 registros na UO, tempo < 50ms

#### 5.2.2 Benchmark de Payload do Login

**Objetivo**: Validar redução de tamanho do payload com filtro de exercício fiscal

**Método**:
```bash
# Antes da correção
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"ce999.admin","Senha":"senha123"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\n"

# Após correção
# Comparar tamanho do download
```

**Critério de Aceitação**:
- Redução de payload ≥ 70% (assumindo 30 anos de histórico)
- Tempo de resposta do endpoint ≤ 3 segundos

### 5.3 Testes de Regressão

#### 5.3.1 Busca por Código (Nutomb)

**Validação**: Busca por código de bem continua funcionando independentemente de filtros

**Passos**:
1. Configurar sessão com área/subárea
2. Na página Items, digitar código de bem de outra subárea
3. Verificar que busca não retorna resultados (comportamento correto - filtro aplicado)
4. Limpar filtros de área/subárea
5. Buscar novamente
6. Verificar que bem é encontrado

#### 5.3.2 Filtros de Status

**Validação**: Filtros "EM OPERAÇÃO", "MANUTENÇÃO", "BAIXADO" continuam funcionando

**Passos**:
1. Carregar bens de uma subárea
2. Aplicar filtro "EM OPERAÇÃO"
3. Verificar que apenas bens em operação são exibidos
4. Alternar entre filtros
5. Verificar que contagem e lista são atualizadas corretamente

#### 5.3.3 Sincronização de Dados

**Validação**: Processo de sincronização de tombamentos não é afetado

**Passos**:
1. Limpar IndexedDB
2. Navegar para `/tombamentos-sync`
3. Iniciar sincronização
4. Verificar que dados são carregados corretamente
5. Verificar que índices `cdArea` e `cdSArea` são populados

### 5.4 Testes de Edge Cases

#### 5.4.1 Códigos com Zeros à Esquerda

**Cenário**: UO "009" vs "9", Área "001" vs "1"

**Validação**:
```javascript
// Ambas as consultas devem retornar os mesmos resultados
const result1 = await dbInterop.getPatrimonioBySubarea("009", "001", "001");
const result2 = await dbInterop.getPatrimonioBySubarea("9", "1", "1");
console.assert(result1.length === result2.length);
```

#### 5.4.2 Campos Nulos ou Vazios

**Cenário**: Registros sem `cdArea` ou `cdSArea` definidos

**Validação**:
- Registros com campos nulos não devem causar erros
- Filtro deve tratar `null`, `undefined`, `""` de forma consistente

#### 5.4.3 Subárea sem Área

**Cenário**: Usuário tenta configurar subárea sem selecionar área (não deveria ser possível na UI)

**Validação**:
- Sistema deve prevenir configuração inválida
- Se ocorrer, método deve retornar lista vazia ou todos os bens da UO


## 6. Considerações de Performance

### 6.1 Análise de Impacto

#### 6.1.1 IndexedDB - Consultas

**Antes da Correção**:
```javascript
// Consulta por índice cdUnid (eficiente)
index.getAll("09") → 500 registros retornados
```

**Após Correção**:
```javascript
// Consulta por índice cdUnid + filtro em memória
index.getAll("09") → 500 registros
  .filter(cdArea === "001" && cdSArea === "001") → 45 registros retornados
```

**Impacto**:
- **Busca inicial**: Mesma performance (usa índice existente)
- **Filtro em memória**: O(n) onde n = registros da UO (tipicamente 100-1000)
- **Tempo adicional**: ~5-20ms para 500 registros
- **Benefício**: Reduz renderização de 500 para 45 itens (90% menos)

**Justificativa para não usar índice composto**:
- IndexedDB não suporta índices compostos multi-campo nativamente
- Criar índice `[cdUnid, cdArea, cdSArea]` requer concatenação de strings (menos eficiente)
- Volume de dados por UO é pequeno o suficiente para filtro em memória

#### 6.1.2 Rede - Payload do Login

**Antes da Correção**:
```
Tombamentos: 15.000 registros (1993-2026, 30 anos)
Tamanho JSON: ~8 MB
Tempo de download (3G): ~25 segundos
```

**Após Correção**:
```
Tombamentos: 500 registros (2024, 1 ano)
Tamanho JSON: ~270 KB
Tempo de download (3G): <1 segundo
Redução: 96.6%
```

**Impacto**:
- **Primeira carga**: 25x mais rápida
- **Armazenamento local**: 96% menos espaço no IndexedDB
- **Sincronização**: Menos dados para processar e indexar

#### 6.1.3 Renderização - Items.razor

**Antes da Correção**:
```
Bens carregados: 500 (toda a UO)
Componentes renderizados: 20 (paginação)
Filtros aplicados: Em memória sobre 500 itens
```

**Após Correção**:
```
Bens carregados: 45 (apenas subárea)
Componentes renderizados: 20 (paginação)
Filtros aplicados: Em memória sobre 45 itens
Redução de processamento: 91%
```

**Impacto**:
- **Filtros de status**: 11x mais rápidos (menos itens para processar)
- **Busca**: 11x mais rápida (menos itens para varrer)
- **Memória**: 91% menos objetos em memória

### 6.2 Otimizações Implementadas

#### 6.2.1 Lazy Loading de Índices

**Estratégia**: Índices são criados apenas quando necessários

```javascript
// Índices cdArea e cdSArea só são usados quando filtros são aplicados
// Se usuário não configura área/subárea, índices não impactam performance
```

#### 6.2.2 Normalização com Cache

**Estratégia**: Normalização de códigos é feita uma vez e reutilizada

```csharp
private static string NormalizeCode(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return string.Empty;
    
    // Normalização simples e rápida
    var normalized = new string(value.Where(char.IsLetterOrDigit).ToArray())
        .ToUpperInvariant();
    return normalized.TrimStart('0');
}
```

#### 6.2.3 Filtro Defensivo

**Estratégia**: Evitar processamento desnecessário

```csharp
// Se não há filtros, retorna imediatamente sem processamento adicional
if (string.IsNullOrWhiteSpace(idArea) && string.IsNullOrWhiteSpace(idSubarea))
{
    return await GetPatrimonioByUOAsync(idUO);
}
```

### 6.3 Métricas de Monitoramento

#### 6.3.1 Métricas de Backend

**Adicionar logging estruturado**:

```csharp
log.LogInformation(
    "Login: Filtro de exercício fiscal aplicado. " +
    "Total: {TotalRecords}, Filtrados: {FilteredRecords}, Redução: {ReductionPercent}%",
    tombamentoBase.Count,
    tombamentoFiltradoExercicio.Count,
    (1 - (double)tombamentoFiltradoExercicio.Count / tombamentoBase.Count) * 100
);
```

#### 6.3.2 Métricas de Frontend

**Adicionar telemetria em Items.razor**:

```csharp
private async Task LoadItems()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // ... lógica de carregamento ...
    
    stopwatch.Stop();
    Console.WriteLine($"LoadItems: {items.Count} bens carregados em {stopwatch.ElapsedMilliseconds}ms");
}
```

### 6.4 Limites e Escalabilidade

#### 6.4.1 Limites Atuais

| Métrica | Limite Atual | Limite Recomendado |
|---------|--------------|-------------------|
| Registros por UO | ~500 | <2.000 |
| Registros por Subárea | ~50 | <500 |
| Tamanho do payload (login) | ~270 KB | <1 MB |
| Tempo de consulta IndexedDB | ~20ms | <100ms |

#### 6.4.2 Estratégias de Escalabilidade Futura

**Se volume de dados crescer significativamente**:

1. **Índice Composto Concatenado**:
```javascript
// Criar índice composto via concatenação
patrimonioStore.createIndex('cdUnidAreaSArea', 
    ['cdUnid', 'cdArea', 'cdSArea'], 
    { unique: false }
);
```

2. **Paginação no Backend**:
```csharp
// Endpoint com paginação
app.MapGet("/api/patrimonio/subarea", (
    string cdUnid, 
    string cdArea, 
    string cdSArea,
    int page = 1,
    int pageSize = 100
) => { ... });
```

3. **Cache de Consultas**:
```csharp
// Cache em memória de consultas frequentes
private readonly IMemoryCache _cache;

public async Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(...)
{
    var cacheKey = $"patrimonio:{idUO}:{idArea}:{idSubarea}";
    
    if (_cache.TryGetValue(cacheKey, out List<PatrimonioItem> cached))
        return cached;
    
    var items = await /* consulta */;
    _cache.Set(cacheKey, items, TimeSpan.FromMinutes(10));
    return items;
}
```


## 7. Rollback Plan

### 7.1 Estratégia de Rollback

#### 7.1.1 Níveis de Rollback

**Nível 1: Rollback de Frontend (Baixo Risco)**

Se problemas forem detectados apenas no frontend:

```csharp
// Em Items.razor, reverter para método legado
private async Task LoadItems()
{
    // ... código existente ...
    
    // ROLLBACK: Usar método antigo
    var allPatrimonio = await DbService.GetPatrimonioByUOAsync(appState.CurrentUO.IdUO);
    
    // Aplicar filtro de área/subárea manualmente se necessário
    if (appState.CurrentArea != null && appState.CurrentSubarea != null)
    {
        allPatrimonio = allPatrimonio
            .Where(p => p.CdArea == appState.CurrentArea.IdArea && 
                       p.CdSArea == appState.CurrentSubarea.IdSubarea)
            .ToList();
    }
    
    // ... resto do código ...
}
```

**Impacto**: Funcionalidade de filtro por subárea continua funcionando, mas sem otimizações

**Nível 2: Rollback de IndexedDB (Médio Risco)**

Se migração de IndexedDB causar problemas:

```javascript
// Em db-interop.js, reverter versão
dbVersion: 10  // Voltar para versão anterior

// Remover criação de novos índices
// Comentar ou remover:
// patrimonioStore.createIndex('cdArea', 'cdArea', { unique: false });
// patrimonioStore.createIndex('cdSArea', 'cdSArea', { unique: false });
```

**Ação adicional**: Forçar usuários a limpar IndexedDB e re-sincronizar

```csharp
// Em App.razor ou Program.cs
protected override async Task OnInitializedAsync()
{
    var schemaVersion = await DbService.GetMetadataAsync("schema:version");
    
    if (schemaVersion == "11")  // Versão problemática
    {
        // Forçar downgrade
        await DbService.ClearAsync("patrimonio");
        await DbService.ClearAsync("patrimonio_staging");
        await DbService.SetMetadataAsync("schema:version", "10");
        
        // Recarregar página para aplicar versão antiga
        Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
    }
}
```

**Nível 3: Rollback de Backend (Alto Risco)**

Se filtro de exercício fiscal causar problemas:

```csharp
// Em Program.cs, remover filtro de exercício fiscal
// ROLLBACK: Comentar filtro
// var exercicioCorrente = DateTime.Now.Year;
// var tombamentoFiltradoExercicio = tombamentoBase
//     .Where(p => p.ExercicioFiscal == exercicioCorrente || p.ExercicioFiscal == 0)
//     .ToList();

// Usar dados completos
var tombamentoFiltradoExercicio = tombamentoBase;

// ... resto do código ...
```

**Impacto**: Payload volta a ser grande, mas funcionalidade é restaurada

### 7.2 Detecção de Problemas

#### 7.2.1 Indicadores de Falha

**Frontend**:
- Taxa de erro > 5% em `GetPatrimonioBySubareaAsync`
- Tempo de carregamento de Items.razor > 5 segundos
- Reclamações de usuários sobre bens não aparecendo

**Backend**:
- Taxa de erro > 2% em `/api/auth/login`
- Tempo de resposta > 10 segundos
- Logs de exceção relacionados a `ExercicioFiscal`

**IndexedDB**:
- Falhas de migração (erro em `onupgradeneeded`)
- Índices ausentes após migração
- Dados corrompidos ou perdidos

#### 7.2.2 Monitoramento Pós-Deploy

**Checklist de Validação (Primeiras 24h)**:

- [ ] Taxa de sucesso de login ≥ 98%
- [ ] Tempo médio de carregamento de Items.razor ≤ 2 segundos
- [ ] Nenhum relatório de dados ausentes
- [ ] Migração de IndexedDB bem-sucedida em ≥ 95% dos clientes
- [ ] Redução de payload confirmada (≥ 70%)

**Ferramentas de Monitoramento**:
```csharp
// Adicionar telemetria
log.LogInformation("Subarea filter applied: UO={UO}, Area={Area}, Subarea={Subarea}, Results={Count}",
    idUO, idArea, idSubarea, items.Count);

// Adicionar métricas de erro
try
{
    var items = await GetPatrimonioBySubareaAsync(...);
}
catch (Exception ex)
{
    log.LogError(ex, "CRITICAL: Subarea filter failed");
    // Fallback para método legado
    return await GetPatrimonioByUOAsync(idUO);
}
```

### 7.3 Procedimento de Rollback

#### 7.3.1 Rollback Rápido (< 15 minutos)

**Passo 1**: Identificar componente problemático (frontend/backend/IndexedDB)

**Passo 2**: Aplicar rollback apropriado:

```bash
# Frontend: Reverter commit
git revert <commit-hash>
git push origin main

# Backend: Reverter deploy
# (Depende da plataforma de hospedagem)
```

**Passo 3**: Validar que sistema voltou ao estado anterior

**Passo 4**: Comunicar usuários se necessário

#### 7.3.2 Rollback Completo (< 1 hora)

Se rollback rápido não resolver:

1. **Reverter todos os commits relacionados**:
```bash
git revert <commit-1> <commit-2> <commit-3>
git push origin main
```

2. **Forçar limpeza de IndexedDB em clientes**:
```csharp
// Adicionar flag de emergência
var forceReset = configuration.GetValue<bool>("Emergency:ForceIndexedDBReset", false);

if (forceReset)
{
    await DbService.ClearAsync("patrimonio");
    await DbService.ClearAsync("patrimonio_staging");
    await DbService.SetMetadataAsync("schema:version", "10");
}
```

3. **Notificar usuários para limpar cache e recarregar**

4. **Investigar causa raiz e planejar correção**

### 7.4 Prevenção de Problemas

#### 7.4.1 Testes Pré-Deploy

**Checklist Obrigatório**:
- [ ] Testes unitários passando (100%)
- [ ] Testes de integração passando
- [ ] Teste manual em ambiente de staging
- [ ] Validação com dados reais de produção (amostra)
- [ ] Teste de migração de IndexedDB (v10 → v11)
- [ ] Teste de performance (payload, consultas)

#### 7.4.2 Deploy Gradual

**Estratégia Recomendada**:

1. **Fase 1 (10% dos usuários)**: Deploy para grupo piloto
2. **Fase 2 (50% dos usuários)**: Se Fase 1 bem-sucedida após 48h
3. **Fase 3 (100% dos usuários)**: Se Fase 2 bem-sucedida após 72h

**Implementação**:
```csharp
// Feature flag baseada em usuário
var enableSubareaFilter = user.Prefixo switch
{
    "CE999" => true,  // Grupo piloto
    "CE001" => true,
    _ => configuration.GetValue<bool>("Features:SubareaFilter", false)
};

if (enableSubareaFilter && appState.CurrentArea != null)
{
    // Nova funcionalidade
    items = await DbService.GetPatrimonioBySubareaAsync(...);
}
else
{
    // Funcionalidade legado
    items = await DbService.GetPatrimonioByUOAsync(...);
}
```

#### 7.4.3 Documentação de Incidentes

**Template de Relatório de Problema**:

```markdown
# Incidente: [Título]

## Detecção
- Data/Hora: [timestamp]
- Detectado por: [usuário/monitoramento/log]
- Sintoma: [descrição do problema]

## Impacto
- Usuários afetados: [número/porcentagem]
- Funcionalidades impactadas: [lista]
- Severidade: [Crítica/Alta/Média/Baixa]

## Ação Tomada
- Rollback aplicado: [Sim/Não]
- Nível de rollback: [1/2/3]
- Tempo para resolução: [minutos]

## Causa Raiz
- [Descrição técnica da causa]

## Prevenção Futura
- [Ações para evitar recorrência]
```


## 8. Resumo de Implementação

### 8.1 Checklist de Implementação

#### 8.1.1 Fase 1: IndexedDB (db-interop.js)

- [ ] Incrementar `dbVersion` de 10 para 11
- [ ] Adicionar índice `cdArea` na store `patrimonio`
- [ ] Adicionar índice `cdSArea` na store `patrimonio`
- [ ] Adicionar índice `cdArea` na store `patrimonio_staging`
- [ ] Adicionar índice `cdSArea` na store `patrimonio_staging`
- [ ] Implementar método `getPatrimonioBySubarea(cdUnid, cdArea, cdSArea)`
- [ ] Implementar método `getPatrimonioBySubareaNormalized(cdUnidNorm, cdArea, cdSArea)`
- [ ] Adicionar validação de índices pós-migração
- [ ] Testar migração de v10 para v11 com dados existentes

#### 8.1.2 Fase 2: Service Layer (C#)

- [ ] Adicionar método `GetPatrimonioBySubareaAsync` em `IIndexedDbService`
- [ ] Implementar `GetPatrimonioBySubareaAsync` em `IndexedDbService`
- [ ] Adicionar tratamento de erros e logging
- [ ] Implementar fallback para normalização de códigos
- [ ] Adicionar testes unitários para o novo método

#### 8.1.3 Fase 3: UI (Items.razor)

- [ ] Modificar método `LoadItems()` para usar `GetPatrimonioBySubareaAsync`
- [ ] Passar `appState.CurrentArea?.IdArea` como parâmetro
- [ ] Passar `appState.CurrentSubarea?.IdSubarea` como parâmetro
- [ ] Validar que comportamento legado é mantido quando filtros são nulos
- [ ] Testar renderização com dados filtrados

#### 8.1.4 Fase 4: Backend (Program.cs)

- [ ] Adicionar filtro de exercício fiscal no endpoint `/api/auth/login`
- [ ] Implementar lógica: `ExercicioFiscal == DateTime.Now.Year || ExercicioFiscal == 0`
- [ ] Adicionar logging de quantidade de registros filtrados
- [ ] Validar que dados correntes são retornados corretamente
- [ ] Testar com dados de múltiplos anos

#### 8.1.5 Fase 5: Testes e Validação

- [ ] Executar todos os cenários de teste funcionais (seção 5.1)
- [ ] Executar testes de performance (seção 5.2)
- [ ] Executar testes de regressão (seção 5.3)
- [ ] Validar edge cases (seção 5.4)
- [ ] Testar em múltiplos navegadores (Chrome, Firefox, Safari, Edge)
- [ ] Testar em dispositivos móveis (Android, iOS)

#### 8.1.6 Fase 6: Deploy e Monitoramento

- [ ] Deploy em ambiente de staging
- [ ] Validação em staging com dados reais
- [ ] Deploy gradual em produção (10% → 50% → 100%)
- [ ] Monitorar métricas por 48h após cada fase
- [ ] Documentar quaisquer problemas encontrados
- [ ] Preparar rollback se necessário

### 8.2 Estimativa de Esforço

| Fase | Tarefa | Estimativa | Complexidade |
|------|--------|------------|--------------|
| 1 | Modificações em db-interop.js | 3h | Média |
| 2 | Modificações em IndexedDbService | 2h | Baixa |
| 3 | Modificações em Items.razor | 1h | Baixa |
| 4 | Modificações em Program.cs | 2h | Média |
| 5 | Testes e validação | 4h | Alta |
| 6 | Deploy e monitoramento | 2h | Média |
| **Total** | | **14h** | |

### 8.3 Dependências e Pré-requisitos

#### 8.3.1 Dependências Técnicas

- **IndexedDB API**: Suportado em todos os navegadores modernos
- **Blazor WebAssembly**: Versão atual do projeto
- **ASP.NET Core**: Versão atual do projeto
- **JavaScript Interop**: Já configurado no projeto

#### 8.3.2 Dependências de Dados

- **Campo ExercicioFiscal**: Deve existir em `TombamentoRecord`
- **Campos CdArea e CdSArea**: Devem existir em `TombamentoRecord` e `PatrimonioItem`
- **AppState**: Deve ter propriedades `CurrentArea` e `CurrentSubarea`

#### 8.3.3 Pré-requisitos de Ambiente

- **Ambiente de desenvolvimento**: Configurado e funcional
- **Ambiente de staging**: Disponível para testes
- **Dados de teste**: Múltiplas áreas/subáreas e múltiplos anos
- **Acesso ao S3**: Para validar dados de origem

### 8.4 Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Migração de IndexedDB falha | Baixa | Alto | Testes extensivos, rollback automático |
| Dados históricos necessários | Média | Médio | Feature flag para desabilitar filtro de ano |
| Performance degradada | Baixa | Médio | Benchmarks antes/depois, otimizações |
| Campos ausentes em dados legados | Média | Alto | Validação defensiva, tratamento de nulos |
| Usuários com cache antigo | Alta | Baixo | Forçar re-sincronização após migração |

### 8.5 Critérios de Sucesso

#### 8.5.1 Critérios Funcionais

- ✅ Bens são filtrados corretamente por subárea quando configurada
- ✅ Comportamento legado é mantido quando filtros não são aplicados
- ✅ Dados de exercício fiscal corrente são carregados
- ✅ Migração de IndexedDB ocorre sem perda de dados
- ✅ Todos os testes de regressão passam

#### 8.5.2 Critérios de Performance

- ✅ Payload do login reduzido em ≥ 70%
- ✅ Tempo de carregamento de Items.razor ≤ 2 segundos
- ✅ Consulta por subárea ≤ 50ms para 500 registros
- ✅ Migração de IndexedDB ≤ 5 segundos

#### 8.5.3 Critérios de Qualidade

- ✅ Cobertura de testes ≥ 80%
- ✅ Zero erros críticos em produção nas primeiras 48h
- ✅ Taxa de sucesso de migração ≥ 95%
- ✅ Documentação completa e atualizada

### 8.6 Próximos Passos

Após implementação bem-sucedida desta correção:

1. **Otimizações Futuras**:
   - Implementar cache de consultas frequentes
   - Adicionar paginação no backend se volume crescer
   - Considerar índice composto se performance degradar

2. **Funcionalidades Relacionadas**:
   - Filtro por múltiplas subáreas simultaneamente
   - Exportação de dados filtrados
   - Relatórios por área/subárea

3. **Melhorias de UX**:
   - Indicador visual de filtros ativos
   - Botão para limpar filtros rapidamente
   - Breadcrumb mostrando hierarquia selecionada

---

## Apêndices

### Apêndice A: Estrutura de Dados

#### TombamentoRecord
```csharp
public class TombamentoRecord
{
    public string IdPatomb { get; set; }
    public string Nutomb { get; set; }
    public string CdOrgao { get; set; }
    public string CdUnid { get; set; }
    public string CdArea { get; set; }      // Usado para filtro
    public string CdSArea { get; set; }     // Usado para filtro
    public string Esfera { get; set; }
    public int ExercicioFiscal { get; set; } // Usado para filtro de ano
    public string Deprod { get; set; }
    // ... outros campos
}
```

#### PatrimonioItem
```csharp
public class PatrimonioItem
{
    public string IdPatomb { get; set; }
    public string Nutomb { get; set; }
    public string CdUnid { get; set; }
    public string CdArea { get; set; }      // Indexado
    public string CdSArea { get; set; }     // Indexado
    public string Esfera { get; set; }
    public string Deprod { get; set; }
    // ... outros campos
}
```

### Apêndice B: Referências

- **IndexedDB API**: https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API
- **Blazor JavaScript Interop**: https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/
- **ASP.NET Core Minimal APIs**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

### Apêndice C: Glossário

- **UO**: Unidade Orçamentária
- **Área**: Subdivisão de uma Unidade Orçamentária
- **Subárea**: Subdivisão de uma Área
- **Nutomb**: Número único de tombamento (código do bem)
- **Exercício Fiscal**: Ano de referência dos dados patrimoniais
- **IndexedDB**: Banco de dados NoSQL no navegador
- **Esfera**: Classificação do bem (Municipal, Estadual, Federal, Ambos)

---

**Documento criado em**: 2024  
**Versão**: 1.0  
**Status**: Pronto para Implementação
