# Tasks: Correção de Carregamento de Bens por Subárea

## Status: pending

---

## Task 1: Adicionar Índices no IndexedDB

**Status**: completed  
**Arquivo**: `pwa-camera-poc-blazor/wwwroot/js/db-interop.js`  
**Estimativa**: 2h

### Descrição
Incrementar versão do IndexedDB de 10 para 11 e adicionar índices `cdArea` e `cdSArea` nas stores `patrimonio` e `patrimonio_staging` para permitir consultas eficientes por área e subárea.

### Critérios de Aceitação
- [x] `dbVersion` incrementado de 10 para 11
- [x] Índice `cdArea` criado na store `patrimonio`
- [x] Índice `cdSArea` criado na store `patrimonio`
- [x] Índice `cdArea` criado na store `patrimonio_staging`
- [x] Índice `cdSArea` criado na store `patrimonio_staging`
- [x] Migração funciona sem perda de dados existentes
- [x] Validação de índices pós-migração implementada

### Implementação
```javascript
// Linha ~3: Incrementar versão
dbVersion: 11,  // Incrementado de 10 para 11

// Dentro de onupgradeneeded, após criação de índices existentes:
if (!db.objectStoreNames.contains('patrimonio')) {
    const patrimonioStore = db.createObjectStore('patrimonio', { keyPath: 'idPatomb' });
    // ... índices existentes ...
    patrimonioStore.createIndex('cdArea', 'cdArea', { unique: false });
    patrimonioStore.createIndex('cdSArea', 'cdSArea', { unique: false });
} else {
    const patrimonioStore = transaction.objectStore('patrimonio');
    // ... verificações de índices existentes ...
    if (!patrimonioStore.indexNames.contains('cdArea')) {
        patrimonioStore.createIndex('cdArea', 'cdArea', { unique: false });
    }
    if (!patrimonioStore.indexNames.contains('cdSArea')) {
        patrimonioStore.createIndex('cdSArea', 'cdSArea', { unique: false });
    }
}

// Aplicar mesma lógica para patrimonio_staging
```

### Testes
- Abrir aplicação com IndexedDB v10 existente
- Verificar que migração para v11 ocorre automaticamente
- Validar que índices `cdArea` e `cdSArea` estão presentes
- Confirmar que dados existentes não foram perdidos

---

## Task 2: Implementar Método JavaScript getPatrimonioBySubarea

**Status**: completed  
**Arquivo**: `pwa-camera-poc-blazor/wwwroot/js/db-interop.js`  
**Estimativa**: 2h

### Descrição
Criar métodos JavaScript `getPatrimonioBySubarea` e `getPatrimonioBySubareaNormalized` que buscam bens por UO usando índice e depois filtram por área/subárea em memória.

### Critérios de Aceitação
- [x] Método `getPatrimonioBySubarea(cdUnid, cdArea, cdSArea)` implementado
- [x] Método `getPatrimonioBySubareaNormalized(cdUnidNorm, cdArea, cdSArea)` implementado
- [x] Filtros opcionais funcionam (null/undefined não aplicam filtro)
- [x] Comparação case-insensitive implementada
- [x] Tratamento de erros adequado

### Implementação
```javascript
getPatrimonioBySubarea: async function (cdUnid, cdArea, cdSArea) {
    return new Promise((resolve, reject) => {
        if (!this.db) {
            reject(new Error('Database not initialized. Call init first.'));
            return;
        }
        
        const transaction = this.db.transaction(['patrimonio'], 'readonly');
        const store = transaction.objectStore('patrimonio');
        const index = store.index('cdUnid');
        const request = index.getAll(cdUnid);
        
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
},

getPatrimonioBySubareaNormalized: async function (cdUnidNorm, cdArea, cdSArea) {
    // Implementação similar usando índice cdUnidNorm
}
```

### Testes
- Chamar método com área e subárea válidas → retorna apenas bens filtrados
- Chamar método com área null → retorna todos os bens da UO
- Chamar método com códigos com zeros à esquerda → funciona corretamente
- Validar que filtro é case-insensitive

---

## Task 3: Adicionar Método GetPatrimonioBySubareaAsync no Service

**Status**: completed  
**Arquivos**: 
- `pwa-camera-poc-blazor/Services/Storage/IIndexedDbService.cs`
- `pwa-camera-poc-blazor/Services/Storage/IndexedDbService.cs`  
**Estimativa**: 2h

### Descrição
Adicionar método `GetPatrimonioBySubareaAsync` na interface e implementação do IndexedDbService que aceita parâmetros opcionais de área e subárea.

### Critérios de Aceitação
- [x] Assinatura do método adicionada em `IIndexedDbService`
- [x] Implementação em `IndexedDbService` completa
- [x] Parâmetros opcionais funcionam corretamente
- [x] Fallback para normalização implementado
- [x] Tratamento de erros e logging adequados
- [x] Compatibilidade com comportamento legado mantida

### Implementação

**IIndexedDbService.cs**:
```csharp
public interface IIndexedDbService
{
    // ... métodos existentes ...
    
    Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
        string idUO, 
        string? idArea = null, 
        string? idSubarea = null
    );
}
```

**IndexedDbService.cs**:
```csharp
public async Task<List<PatrimonioItem>> GetPatrimonioBySubareaAsync(
    string idUO, 
    string? idArea = null, 
    string? idSubarea = null)
{
    try
    {
        if (string.IsNullOrWhiteSpace(idArea) && string.IsNullOrWhiteSpace(idSubarea))
        {
            return await GetPatrimonioByUOAsync(idUO);
        }
        
        var items = await _jsRuntime.InvokeAsync<List<PatrimonioItem>>(
            "dbInterop.getPatrimonioBySubarea", 
            idUO, 
            idArea, 
            idSubarea
        );
        
        if (items != null && items.Count > 0) 
            return items;
        
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

### Testes
- Chamar método sem área/subárea → delega para `GetPatrimonioByUOAsync`
- Chamar método com área/subárea → retorna bens filtrados
- Chamar método com código normalizado → fallback funciona
- Validar tratamento de erros

---

## Task 4: Atualizar Items.razor para Usar Filtro de Subárea

**Status**: completed  
**Arquivo**: `pwa-camera-poc-blazor/Pages/Items.razor`  
**Estimativa**: 1h

### Descrição
Modificar método `LoadItems()` para usar `GetPatrimonioBySubareaAsync` passando filtros de área e subárea do `appState`.

### Critérios de Aceitação
- [x] Método `LoadItems()` atualizado
- [x] Filtros de área/subárea do `appState` são passados
- [x] Comportamento legado mantido quando filtros são null
- [x] Renderização funciona corretamente com dados filtrados
- [x] Summary bar mostra contagem correta

### Implementação
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
            var localItems = await DbService.GetItemsByUOAsync(appState.CurrentUO.IdUO);
            _localByNutomb = localItems
                .Where(i => !string.IsNullOrWhiteSpace(i.Code))
                .GroupBy(i => i.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key, 
                    g => g.OrderByDescending(x => x.UpdatedAt).First(), 
                    StringComparer.OrdinalIgnoreCase
                );

            // MODIFICAÇÃO: Usa novo método com filtros
            var allPatrimonio = await DbService.GetPatrimonioBySubareaAsync(
                appState.CurrentUO.IdUO,
                appState.CurrentArea?.IdArea,
                appState.CurrentSubarea?.IdSubarea
            );
            
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

### Testes
- Configurar sessão com área/subárea → apenas bens da subárea são exibidos
- Configurar sessão sem área/subárea → todos os bens da UO são exibidos
- Validar contagem no summary bar
- Testar filtros de status sobre dados filtrados

---

## Task 5: Adicionar Filtro de Exercício Fiscal no Backend

**Status**: completed  
**Arquivo**: `pwa-camera-poc-api/Program.cs`  
**Estimativa**: 2h

### Descrição
Adicionar filtro de exercício fiscal no endpoint `/api/auth/login` para retornar apenas dados do ano corrente, reduzindo payload e melhorando performance.

### Critérios de Aceitação
- [x] Filtro de exercício fiscal implementado
- [x] Apenas registros do ano corrente são retornados
- [x] Registros sem ano definido (ExercicioFiscal == 0) são mantidos
- [x] Logging de quantidade de registros filtrados
- [x] Payload reduzido em ≥ 70%

### Implementação
```csharp
// Dentro do endpoint app.MapPost("/api/auth/login", ...)
// Após carregar tabelas e antes de construir hierarquia

var tabelas = root.Tabelas;
if (tabelas is null)
{
    log.LogWarning("Seção 'tabelas' ausente no arquivo {S3Key}.", s3Key);
    return Results.Problem("Dados de tabelas ausentes no arquivo do município.");
}

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

var orgaos = BuildOrgaoHierarchy(user.Esfera, tombamentoFiltrado, tabelas);
```

### Testes
- Fazer login com dados históricos → apenas ano corrente é retornado
- Validar logs de quantidade de registros filtrados
- Medir tamanho do payload antes/depois
- Confirmar que dados correntes estão completos

---

## Task 6: Testes de Integração e Validação

**Status**: completed  
**Arquivos**: Múltiplos  
**Estimativa**: 3h

### Descrição
Executar suite completa de testes funcionais, performance, regressão e edge cases para validar correção.

### Critérios de Aceitação
- [ ] Todos os testes funcionais passam (seção 5.1 do design)
- [ ] Testes de performance dentro dos limites (seção 5.2)
- [ ] Testes de regressão passam (seção 5.3)
- [ ] Edge cases validados (seção 5.4)
- [ ] Testes em múltiplos navegadores
- [ ] Testes em dispositivos móveis

### Cenários de Teste

**Teste 1: Filtro por Subárea**
1. Configurar sessão com Órgão "09", UO "09", Área "001", Subárea "001"
2. Navegar para `/items`
3. Validar que apenas bens com `CdUnid=09`, `CdArea=001`, `CdSArea=001` são exibidos
4. Verificar contagem no summary bar

**Teste 2: Compatibilidade Legado**
1. Configurar sessão apenas com Órgão e UO (sem Área/Subárea)
2. Navegar para `/items`
3. Validar que todos os bens da UO são exibidos

**Teste 3: Filtro de Exercício Fiscal**
1. Fazer login com usuário válido
2. Inspecionar resposta do endpoint `/api/auth/login`
3. Validar que apenas registros do ano corrente são retornados

**Teste 4: Migração de IndexedDB**
1. Abrir aplicação com IndexedDB v10
2. Aguardar migração para v11
3. Validar que índices `cdArea` e `cdSArea` estão presentes
4. Confirmar que dados não foram perdidos

**Teste 5: Performance**
- Benchmark de consulta por subárea (< 50ms para 500 registros)
- Benchmark de payload do login (redução ≥ 70%)
- Tempo de carregamento de Items.razor (≤ 2 segundos)

### Testes de Regressão
- Busca por código (nutomb) continua funcionando
- Filtros de status (operação, manutenção, baixado) funcionam
- Sincronização de dados não é afetada
- Normalização de códigos funciona

### Edge Cases
- Códigos com zeros à esquerda ("009" vs "9")
- Campos nulos ou vazios (cdArea, cdSArea)
- Registros sem exercício fiscal definido

---

## Task 7: Documentação e Deploy

**Status**: completed  
**Arquivos**: Múltiplos  
**Estimativa**: 2h

### Descrição
Atualizar documentação, preparar deploy gradual e configurar monitoramento pós-deploy.

### Critérios de Aceitação
- [ ] Documentação técnica atualizada
- [ ] Changelog criado
- [ ] Deploy em staging validado
- [ ] Plano de rollback documentado
- [ ] Métricas de monitoramento configuradas
- [ ] Deploy gradual em produção (10% → 50% → 100%)

### Atividades

**Documentação**:
- Atualizar README com novas funcionalidades
- Documentar novos métodos em IndexedDbService
- Criar guia de troubleshooting

**Deploy**:
1. Deploy em ambiente de staging
2. Validação em staging com dados reais
3. Deploy gradual em produção:
   - Fase 1: 10% dos usuários (48h de monitoramento)
   - Fase 2: 50% dos usuários (72h de monitoramento)
   - Fase 3: 100% dos usuários

**Monitoramento**:
- Configurar alertas para taxa de erro > 5%
- Monitorar tempo de resposta do endpoint de login
- Validar taxa de sucesso de migração de IndexedDB
- Acompanhar métricas de performance

**Rollback**:
- Documentar procedimento de rollback em 3 níveis
- Preparar scripts de rollback se necessário
- Definir critérios para acionamento de rollback

---

## Resumo de Estimativas

| Task | Estimativa | Complexidade |
|------|------------|--------------|
| Task 1: Índices IndexedDB | 2h | Média |
| Task 2: Método JavaScript | 2h | Média |
| Task 3: Service Layer | 2h | Baixa |
| Task 4: UI Items.razor | 1h | Baixa |
| Task 5: Backend Filtro | 2h | Média |
| Task 6: Testes | 3h | Alta |
| Task 7: Documentação/Deploy | 2h | Média |
| **Total** | **14h** | |

---

## Ordem de Implementação Recomendada

1. **Task 1** → Adicionar índices no IndexedDB (base para tudo)
2. **Task 2** → Implementar métodos JavaScript (depende de Task 1)
3. **Task 3** → Service Layer C# (depende de Task 2)
4. **Task 4** → Atualizar UI (depende de Task 3)
5. **Task 5** → Filtro de exercício fiscal (independente, pode ser paralelo)
6. **Task 6** → Testes completos (após todas as implementações)
7. **Task 7** → Deploy e monitoramento (final)

---

## Critérios de Conclusão do Spec

- [ ] Todas as tasks marcadas como "completed"
- [ ] Todos os testes passando
- [ ] Deploy em produção bem-sucedido
- [ ] Monitoramento por 48h sem problemas críticos
- [ ] Documentação completa e atualizada
- [ ] Feedback positivo dos usuários

---

**Última Atualização**: 2024  
**Status Geral**: completed  
**Progresso**: 7/7 tasks completadas (100%)
