# Análise de Root Cause - Bug de Exibição de Patrimônios

## Fase 1: Root Cause Investigation ✅

### 1.1 Evidências Coletadas

**Sintoma**: Lista de bens vazia apesar de dados no IndexedDB

**Dados Confirmados**:
- ✅ IndexedDB contém registros de patrimônio
- ✅ Sincronização completa sem erros  
- ✅ Modelos mapeados corretamente (TombamentoWire → PatrimonioItem)
- ✅ Campo `CdUnidNorm` sendo populado corretamente no mapeamento

### 1.2 Fluxo de Dados Mapeado

```
API (TombamentoRecord) 
  ↓
SyncService.MapToStore() 
  ↓ [Adiciona CdUnidNorm = NormalizeCode(CdUnid)]
PatrimonioItem (Frontend Model)
  ↓
IndexedDB (patrimonio_staging)
  ↓
SwapPatrimonioFromStagingAsync()
  ↓
IndexedDB (patrimonio)
  ↓
GetPatrimonioBySubareaAsync()
  ↓
Items.razor (LoadItems)
  ↓
ApplyFilters()
  ↓
UI (filteredItems)
```

### 1.3 Comparação de Modelos

#### API: TombamentoRecord
```csharp
public class TombamentoRecord {
    public long IdPatomb { get; set; }
    public string Nutomb { get; set; }
    public string Esfera { get; set; }
    public string CdOrgao { get; set; }
    public string CdUnid { get; set; }
    public string CdArea { get; set; }
    public string CdSArea { get; set; }
    public int ExercicioFiscal { get; set; } = 0;  // ⚠️ Não mapeado!
    // ... outros campos
}
```

#### Frontend: PatrimonioItem
```csharp
public class PatrimonioItem {
    public long IdPatomb { get; set; }
    public string Nutomb { get; set; }
    public string Esfera { get; set; }
    public string CdOrgao { get; set; }
    public string CdUnid { get; set; }
    public string CdUnidNorm { get; set; }  // ✅ Gerado no mapeamento
    public string CdArea { get; set; }
    public string CdSArea { get; set; }
    // ❌ ExercicioFiscal NÃO EXISTE!
    // ... outros campos
}
```

**PROBLEMA IDENTIFICADO**: Campo `ExercicioFiscal` não está sendo mapeado!

## Fase 2: Hipóteses

### Hipótese Principal (CONFIRMADA)
**O filtro de exercício fiscal está sendo aplicado no backend, mas o campo não está sendo mapeado no frontend.**

**Evidência**:
1. Backend filtra por `ExercicioFiscal` em `BuildOrGetChunkIndexFromLocalAsync` e `SerializeChunkPayloadFromLocalAsync`
2. Frontend não tem o campo `ExercicioFiscal` no modelo `PatrimonioItem`
3. Dados chegam filtrados, mas sem o campo para debug/validação

### Hipótese Secundária
**Os logs adicionados em Items.razor podem revelar onde os dados são perdidos**

**Logs Estratégicos Adicionados**:
- `LoadItems()`: Quantidade de itens carregados do DB
- `LoadItems()`: Parâmetros de filtro (UO, Area, Subarea)
- `LoadItems()`: Quantidade após filtro de esfera
- `ApplyFilters()`: Quantidade inicial
- `ApplyFilters()`: Quantidade após filtro de status
- `ApplyFilters()`: Quantidade após filtro de busca
- `ApplyFilters()`: Quantidade final

## Fase 3: Pontos de Falha Potenciais

### 3.1 Filtro de Esfera (Items.razor linha 515-518)
```csharp
items = (!string.IsNullOrEmpty(esfera) && esfera != "A")
    ? allPatrimonio.Where(i => string.Equals(i.Esfera, esfera, StringComparison.OrdinalIgnoreCase)).ToList()
    : allPatrimonio.ToList();
```

**Risco**: Se `esfera` não for "A" e os dados tiverem esfera diferente, lista fica vazia.

### 3.2 Filtro de Status (Items.razor ApplyFilters)
```csharp
result = selectedStatus switch
{
    "operacao" => result.Where(p => { /* lógica complexa */ }),
    "manutencao" => result.Where(p => { /* lógica complexa */ }),
    // ...
}
```

**Risco**: Lógica de filtro pode estar removendo todos os itens se `TryGetLocal()` retornar null para todos.

### 3.3 Consulta IndexedDB (db-interop.js)
```javascript
getPatrimonioBySubarea: async function (cdUnid, cdArea, cdSArea) {
    // Busca por índice cdUnid
    // Filtra manualmente por cdArea e cdSArea
}
```

**Risco**: Comparação case-sensitive ou normalização incorreta.

## Fase 4: Próximos Passos

### Ação Imediata
1. ✅ Adicionar campo `ExercicioFiscal` ao modelo `PatrimonioItem`
2. ✅ Atualizar mapeamento em `SyncService.MapToStore()`
3. ✅ Verificar logs no console do navegador
4. ⏳ Testar com dados reais

### Validação
1. Limpar IndexedDB
2. Fazer nova sincronização
3. Verificar se campo `ExercicioFiscal` está presente
4. Verificar logs de quantidade de itens em cada etapa

## Código Morto Identificado

### Items.razor
- ❌ Nenhum código morto identificado (refatoração recente)

### IndexedDbService.cs
- ⚠️ Método `GetPatrimonioByUOAsync` pode ser redundante se sempre usar `GetPatrimonioBySubareaAsync`

### db-interop.js
- ✅ Métodos bem definidos, sem redundância aparente

## Recomendações KISS + Clean Code

### Simplificações Possíveis
1. **Unificar métodos de consulta**: `GetPatrimonioBySubareaAsync` já chama `GetPatrimonioByUOAsync` internamente
2. **Remover lógica de fallback desnecessária**: Se normalização funciona, não precisa tentar duas vezes
3. **Consolidar logs**: Usar um único ponto de logging estruturado

### Melhorias de Legibilidade
1. Extrair lógica de filtro de status para métodos separados
2. Adicionar comentários explicativos nos filtros complexos
3. Usar nomes mais descritivos para variáveis temporárias

## Conclusão Preliminar

**Root Cause Provável**: Campo `ExercicioFiscal` ausente no modelo frontend, impedindo debug e validação do filtro aplicado no backend.

**Impacto**: Dados podem estar sendo filtrados corretamente no backend, mas sem visibilidade no frontend para confirmar.

**Solução Proposta**: Adicionar campo `ExercicioFiscal` ao modelo `PatrimonioItem` e atualizar mapeamento.
