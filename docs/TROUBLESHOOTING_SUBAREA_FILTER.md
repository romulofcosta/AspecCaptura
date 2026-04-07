# Guia de Troubleshooting: Correção de Carregamento de Bens por Subárea

## Visão Geral

Este documento fornece soluções para problemas comuns relacionados à correção de carregamento de bens por subárea, incluindo diagnóstico, resolução e prevenção.

## Índice

1. [Problemas de Migração IndexedDB](#problemas-de-migração-indexeddb)
2. [Problemas de Filtro por Subárea](#problemas-de-filtro-por-subárea)
3. [Problemas de Performance](#problemas-de-performance)
4. [Problemas de Dados](#problemas-de-dados)
5. [Problemas de Backend](#problemas-de-backend)
6. [Problemas de Navegador](#problemas-de-navegador)

---

## Problemas de Migração IndexedDB

### Problema 1: Migração de v10 para v11 Falha

**Sintomas**:
- Erro no console: "Database upgrade failed"
- Aplicação não carrega dados
- Página fica em branco ou com loading infinito

**Diagnóstico**:
```javascript
// Console do navegador
const db = await indexedDB.open('aspec-captura-db');
console.log('Versão atual:', db.version);
// Se versão ainda é 10, migração falhou
```

**Causas Comuns**:
1. Transação de migração foi abortada
2. Erro ao criar índices
3. Dados corrompidos no IndexedDB
4. Navegador não suporta operação

**Solução 1: Forçar Limpeza e Re-sincronização**
```javascript
// Console do navegador
// 1. Fechar todas as conexões
indexedDB.deleteDatabase('aspec-captura-db');

// 2. Recarregar página
window.location.reload();

// 3. Fazer login novamente
// 4. Sincronizar dados
```

**Solução 2: Limpar Cache do Navegador**
```
1. Pressione Ctrl+Shift+Delete (Windows) ou Cmd+Shift+Delete (Mac)
2. Selecione "Dados de sites" ou "Cookies e dados de sites"
3. Clique em "Limpar dados"
4. Recarregue a página
5. Faça login novamente
```

**Solução 3: Usar Modo Anônimo para Testar**
```
1. Abra janela anônima (Ctrl+Shift+N no Chrome)
2. Acesse a aplicação
3. Faça login
4. Verifique se migração funciona
5. Se funcionar, problema é com cache/dados locais
```

**Prevenção**:
- Implementar validação de migração
- Adicionar tratamento de erros robusto
- Testar migração em múltiplos navegadores

---

### Problema 2: Índices cdArea/cdSArea Ausentes

**Sintomas**:
- Filtro por subárea não funciona
- Erro no console: "Index 'cdArea' not found"
- Todos os bens da UO são exibidos

**Diagnóstico**:
```javascript
// Console do navegador
const db = await indexedDB.open('aspec-captura-db', 11);
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
const indexes = Array.from(store.indexNames);
console.log('Índices disponíveis:', indexes);
// Esperado: ['nutomb', 'cdUnid', 'cdUnidNorm', 'esfera', 'cdArea', 'cdSArea']
```

**Causas Comuns**:
1. Migração não completou
2. Versão do banco ainda é 10
3. Código de migração não foi executado

**Solução**:
```javascript
// Forçar re-migração
indexedDB.deleteDatabase('aspec-captura-db');
window.location.reload();
```

**Prevenção**:
- Adicionar validação de índices após migração
- Implementar fallback para criar índices se ausentes

---

### Problema 3: Dados Perdidos Após Migração

**Sintomas**:
- IndexedDB v11 criado com sucesso
- Mas dados anteriores não aparecem
- Contagem de bens é zero

**Diagnóstico**:
```javascript
// Verificar quantidade de registros
const db = await indexedDB.open('aspec-captura-db', 11);
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
const countRequest = store.count();
countRequest.onsuccess = () => console.log('Total de registros:', countRequest.result);
```

**Causas Comuns**:
1. Migração abortou e dados foram perdidos
2. Transação de migração falhou
3. Erro ao copiar dados

**Solução**:
```
1. Limpar IndexedDB
2. Fazer login novamente
3. Sincronizar dados do servidor
4. Verificar que dados foram carregados
```

**Prevenção**:
- Implementar backup antes de migração
- Testar migração extensivamente
- Adicionar validação de integridade de dados

---

## Problemas de Filtro por Subárea

### Problema 4: Filtro por Subárea Não Funciona

**Sintomas**:
- Área e subárea configuradas na sessão
- Mas todos os bens da UO são exibidos
- Contagem não corresponde à subárea

**Diagnóstico**:
```javascript
// Console do navegador
// 1. Verificar AppState
console.log('CurrentArea:', appState.CurrentArea);
console.log('CurrentSubarea:', appState.CurrentSubarea);

// 2. Testar método diretamente
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log('Itens filtrados:', items.length);

// 3. Verificar dados
const allItems = await dbInterop.getPatrimonioByUO("09");
console.log('Total na UO:', allItems.length);
console.log('Amostra:', allItems.slice(0, 3));
```

**Causas Comuns**:
1. AppState não está configurado corretamente
2. Método JavaScript não está sendo chamado
3. Campos cdArea/cdSArea ausentes nos dados
4. Comparação de strings falhando (case-sensitive)

**Solução 1: Verificar Configuração de Sessão**
```
1. Navegar para /configuracao-sessao
2. Verificar que área e subárea estão selecionadas
3. Salvar configuração novamente
4. Navegar para /items
5. Verificar que filtro funciona
```

**Solução 2: Verificar Dados**
```javascript
// Verificar se campos existem
const sample = await dbInterop.getPatrimonioByUO("09");
console.log('Campos disponíveis:', Object.keys(sample[0] || {}));
console.log('cdArea:', sample[0]?.cdArea);
console.log('cdSArea:', sample[0]?.cdSArea);
```

**Solução 3: Forçar Re-sincronização**
```
1. Limpar IndexedDB
2. Fazer login novamente
3. Sincronizar dados
4. Verificar que campos cdArea/cdSArea estão presentes
```

**Prevenção**:
- Validar dados no backend antes de enviar
- Adicionar tratamento de campos ausentes
- Implementar logging detalhado

---

### Problema 5: Filtro Retorna Resultados Incorretos

**Sintomas**:
- Filtro funciona, mas retorna bens errados
- Bens de outras subáreas aparecem
- Contagem não bate com esperado

**Diagnóstico**:
```javascript
// Verificar lógica de filtro
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log('Itens retornados:', items.length);
items.forEach(item => {
    console.log(`${item.nutomb}: cdArea=${item.cdArea}, cdSArea=${item.cdSArea}`);
});
```

**Causas Comuns**:
1. Comparação case-sensitive
2. Espaços em branco nos códigos
3. Zeros à esquerda ("001" vs "1")
4. Lógica de filtro incorreta

**Solução**:
```javascript
// Verificar normalização
const items = await dbInterop.getPatrimonioByUO("09");
const areas = [...new Set(items.map(i => i.cdArea))];
const subareas = [...new Set(items.map(i => i.cdSArea))];
console.log('Áreas únicas:', areas);
console.log('Subáreas únicas:', subareas);
```

**Prevenção**:
- Normalizar códigos antes de comparar
- Usar comparação case-insensitive
- Remover espaços em branco

---

### Problema 6: Comportamento Legado Quebrado

**Sintomas**:
- Quando área/subárea não são configuradas
- Nenhum bem é exibido
- Ou erro ocorre

**Diagnóstico**:
```javascript
// Verificar comportamento sem filtros
console.log('CurrentArea:', appState.CurrentArea); // Deve ser null
console.log('CurrentSubarea:', appState.CurrentSubarea); // Deve ser null

// Testar método legado
const items = await dbInterop.getPatrimonioByUO("09");
console.log('Itens sem filtro:', items.length);
```

**Causas Comuns**:
1. Método `GetPatrimonioByUOAsync` não está sendo chamado
2. Lógica de fallback não funciona
3. Parâmetros opcionais não tratados corretamente

**Solução**:
```csharp
// Verificar lógica em Items.razor
if (string.IsNullOrWhiteSpace(idArea) && string.IsNullOrWhiteSpace(idSubarea))
{
    // Deve usar método legado
    return await GetPatrimonioByUOAsync(idUO);
}
```

**Prevenção**:
- Testar cenário legado extensivamente
- Adicionar testes de regressão
- Validar parâmetros opcionais

---

## Problemas de Performance

### Problema 7: Carregamento de Items.razor Muito Lento

**Sintomas**:
- Página /items demora > 5 segundos para carregar
- Loading infinito
- Navegador trava

**Diagnóstico**:
```javascript
// Medir tempo de carregamento
console.time('LoadItems');
// Navegar para /items
// Aguardar carregamento completo
console.timeEnd('LoadItems');

// Verificar quantidade de dados
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log('Itens carregados:', items.length);
```

**Causas Comuns**:
1. Volume de dados muito grande
2. Filtro não está sendo aplicado
3. Renderização de muitos componentes
4. Consulta IndexedDB lenta

**Solução 1: Verificar Filtro**
```javascript
// Confirmar que filtro está ativo
const allUO = await dbInterop.getPatrimonioByUO("09");
const filtered = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log(`Redução: ${allUO.length} → ${filtered.length} (${Math.round((1 - filtered.length/allUO.length) * 100)}%)`);
```

**Solução 2: Otimizar Consulta**
```javascript
// Benchmark de consulta
console.time('query');
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.timeEnd('query');
// Esperado: < 50ms
```

**Solução 3: Implementar Paginação**
```csharp
// Em Items.razor
private int pageSize = 20;
private int currentPage = 1;

private List<PatrimonioItem> GetPagedItems()
{
    return items
        .Skip((currentPage - 1) * pageSize)
        .Take(pageSize)
        .ToList();
}
```

**Prevenção**:
- Implementar paginação desde o início
- Monitorar performance de consultas
- Otimizar renderização de componentes

---

### Problema 8: Consulta IndexedDB Lenta

**Sintomas**:
- Método `getPatrimonioBySubarea` demora > 100ms
- Performance pior que antes
- Navegador trava durante consulta

**Diagnóstico**:
```javascript
// Benchmark detalhado
const times = [];
for (let i = 0; i < 10; i++) {
    const start = performance.now();
    await dbInterop.getPatrimonioBySubarea("09", "001", "001");
    const end = performance.now();
    times.push(end - start);
}
console.log('Tempos (ms):', times);
console.log('Média:', times.reduce((a, b) => a + b) / times.length);
console.log('Máximo:', Math.max(...times));
```

**Causas Comuns**:
1. Índice não está sendo usado
2. Volume de dados muito grande
3. Filtro em memória ineficiente
4. Navegador com recursos limitados

**Solução 1: Verificar Uso de Índice**
```javascript
// Confirmar que índice cdUnid está sendo usado
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
const index = store.index('cdUnid');
console.log('Índice cdUnid existe:', index !== undefined);
```

**Solução 2: Otimizar Filtro**
```javascript
// Usar índice composto se disponível
// Ou otimizar filtro em memória
const filtered = allFromUO.filter(item => {
    // Comparação rápida
    return item.cdArea === cdArea && item.cdSArea === cdSArea;
});
```

**Prevenção**:
- Monitorar performance de consultas
- Implementar cache se necessário
- Considerar índice composto para volumes grandes

---

## Problemas de Dados

### Problema 9: Dados Ausentes ou Incompletos

**Sintomas**:
- Bens que deveriam aparecer não aparecem
- Contagem menor que esperado
- Campos ausentes nos dados

**Diagnóstico**:
```javascript
// Verificar dados brutos
const allItems = await dbInterop.getAll('patrimonio');
console.log('Total de registros:', allItems.length);
console.log('Amostra:', allItems.slice(0, 5));

// Verificar campos
const sample = allItems[0];
console.log('Campos disponíveis:', Object.keys(sample || {}));
console.log('cdArea:', sample?.cdArea);
console.log('cdSArea:', sample?.cdSArea);
```

**Causas Comuns**:
1. Dados não foram sincronizados
2. Filtro de exercício fiscal muito restritivo
3. Campos ausentes no backend
4. Erro na sincronização

**Solução 1: Re-sincronizar Dados**
```
1. Navegar para /tombamentos-sync
2. Clicar em "Sincronizar Dados"
3. Aguardar conclusão
4. Verificar que dados foram carregados
```

**Solução 2: Verificar Backend**
```bash
# Verificar logs do backend
grep "Tombamentos filtrados" /var/log/app.log | tail -10

# Verificar se filtro de exercício está muito restritivo
grep "ExercicioFiscal" /var/log/app.log
```

**Solução 3: Verificar Dados no S3**
```bash
# Listar arquivos no bucket
aws s3 ls s3://bucket-name/municipio/

# Baixar arquivo de dados
aws s3 cp s3://bucket-name/municipio/CE999.json /tmp/

# Verificar conteúdo
jq '.tabelas.tombamentos | length' /tmp/CE999.json
jq '.tabelas.tombamentos[0]' /tmp/CE999.json
```

**Prevenção**:
- Validar dados no backend
- Implementar logging detalhado
- Adicionar verificação de integridade

---

### Problema 10: Códigos com Zeros à Esquerda

**Sintomas**:
- Filtro não funciona com códigos "001", "009"
- Mas funciona com "1", "9"
- Ou vice-versa

**Diagnóstico**:
```javascript
// Testar ambas as formas
const result1 = await dbInterop.getPatrimonioBySubarea("009", "001", "001");
const result2 = await dbInterop.getPatrimonioBySubarea("9", "1", "1");
console.log('Com zeros:', result1.length);
console.log('Sem zeros:', result2.length);
console.log('Iguais?', result1.length === result2.length);
```

**Causas Comuns**:
1. Normalização não está funcionando
2. Dados armazenados com formato inconsistente
3. Comparação não normaliza códigos

**Solução**:
```javascript
// Implementar normalização consistente
function normalizeCode(code) {
    if (!code) return '';
    return code.trim().toUpperCase().replace(/^0+/, '');
}

// Usar em comparações
const matchArea = normalizeCode(item.cdArea) === normalizeCode(cdArea);
```

**Prevenção**:
- Normalizar códigos no backend antes de armazenar
- Usar normalização consistente em todas as comparações
- Documentar formato esperado de códigos

---

## Problemas de Backend

### Problema 11: Payload Ainda Grande

**Sintomas**:
- Tempo de login ainda lento
- Payload > 1MB
- Redução < 50%

**Diagnóstico**:
```bash
# Medir tamanho de payload
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"ce999.admin","Senha":"senha123"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\n" \
  -o /dev/null -s

# Verificar logs do backend
grep "Tombamentos filtrados para exercício" /var/log/app.log | tail -10
```

**Causas Comuns**:
1. Filtro de exercício fiscal não está ativo
2. Dados não têm campo `ExercicioFiscal`
3. Filtro está incorreto
4. Compressão não está funcionando

**Solução 1: Verificar Filtro no Código**
```csharp
// Confirmar que filtro está ativo
var exercicioCorrente = DateTime.Now.Year;
var tombamentoFiltrado = tombamentoBase
    .Where(p => p.ExercicioFiscal == exercicioCorrente || p.ExercicioFiscal == 0)
    .ToList();

log.LogInformation(
    "Tombamentos filtrados: {Count} de {Total}",
    tombamentoFiltrado.Count,
    tombamentoBase.Count
);
```

**Solução 2: Verificar Dados**
```bash
# Baixar arquivo de dados
aws s3 cp s3://bucket-name/municipio/CE999.json /tmp/

# Verificar campo ExercicioFiscal
jq '.tabelas.tombamentos[0].ExercicioFiscal' /tmp/CE999.json

# Contar registros por ano
jq '.tabelas.tombamentos | group_by(.ExercicioFiscal) | map({ano: .[0].ExercicioFiscal, count: length})' /tmp/CE999.json
```

**Solução 3: Habilitar Compressão**
```csharp
// Em Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

app.UseResponseCompression();
```

**Prevenção**:
- Validar dados no S3
- Implementar logging de tamanho de payload
- Monitorar redução de payload

---

### Problema 12: Erro no Endpoint de Login

**Sintomas**:
- Erro 500 no endpoint `/api/auth/login`
- Exceção relacionada a `ExercicioFiscal`
- Login não funciona

**Diagnóstico**:
```bash
# Verificar logs de erro
grep "api/auth/login" /var/log/app.log | grep "ERROR"

# Verificar exceções
grep "ExercicioFiscal" /var/log/app.log | grep "Exception"
```

**Causas Comuns**:
1. Campo `ExercicioFiscal` é null
2. Tipo de dados incorreto
3. Filtro LINQ com erro
4. Dados corrompidos

**Solução 1: Tratamento Defensivo**
```csharp
// Adicionar tratamento de null
var tombamentoFiltrado = tombamentoBase
    .Where(p => 
        (p.ExercicioFiscal.HasValue && p.ExercicioFiscal.Value == exercicioCorrente) ||
        !p.ExercicioFiscal.HasValue ||
        p.ExercicioFiscal == 0
    )
    .ToList();
```

**Solução 2: Validar Dados**
```csharp
// Adicionar validação antes de filtrar
if (tombamentoBase == null || !tombamentoBase.Any())
{
    log.LogWarning("Nenhum tombamento encontrado");
    return Results.Ok(new { orgaos = new List<Orgao>() });
}
```

**Prevenção**:
- Adicionar validação de dados
- Implementar tratamento de erros robusto
- Testar com dados diversos

---

## Problemas de Navegador

### Problema 13: Funciona no Chrome mas não no Firefox/Safari

**Sintomas**:
- Aplicação funciona perfeitamente no Chrome
- Mas falha no Firefox ou Safari
- Erros específicos de navegador

**Diagnóstico**:
```javascript
// Verificar suporte do navegador
console.log('IndexedDB:', 'indexedDB' in window);
console.log('Service Worker:', 'serviceWorker' in navigator);
console.log('User Agent:', navigator.userAgent);
```

**Causas Comuns**:
1. API não suportada no navegador
2. Diferenças de implementação de IndexedDB
3. Políticas de segurança diferentes
4. Bugs específicos do navegador

**Solução 1: Polyfills**
```html
<!-- Adicionar polyfills se necessário -->
<script src="https://cdn.jsdelivr.net/npm/indexeddb-polyfill@1.0.0/dist/indexeddb.min.js"></script>
```

**Solução 2: Detecção de Navegador**
```javascript
// Implementar fallback para navegadores não suportados
if (!('indexedDB' in window)) {
    console.error('IndexedDB não suportado');
    alert('Seu navegador não suporta esta aplicação. Por favor, use Chrome, Edge ou Firefox atualizado.');
}
```

**Prevenção**:
- Testar em múltiplos navegadores
- Usar APIs padronizadas
- Implementar fallbacks

---

### Problema 14: Modo Privado/Anônimo Não Funciona

**Sintomas**:
- Aplicação não funciona em modo privado
- IndexedDB não persiste dados
- Erro ao criar banco

**Diagnóstico**:
```javascript
// Testar criação de IndexedDB
try {
    const db = await indexedDB.open('test-db', 1);
    console.log('IndexedDB funciona em modo privado');
} catch (error) {
    console.error('IndexedDB não funciona em modo privado:', error);
}
```

**Causas Comuns**:
1. Navegador bloqueia IndexedDB em modo privado
2. Quota de armazenamento zero
3. Políticas de privacidade

**Solução**:
```javascript
// Detectar modo privado e avisar usuário
async function isPrivateMode() {
    try {
        const db = await indexedDB.open('test', 1);
        return false;
    } catch {
        return true;
    }
}

if (await isPrivateMode()) {
    alert('Esta aplicação não funciona em modo privado. Por favor, use uma janela normal.');
}
```

**Prevenção**:
- Documentar limitações
- Adicionar detecção de modo privado
- Mostrar mensagem clara para usuário

---

## Comandos Úteis de Diagnóstico

### Console do Navegador

```javascript
// Verificar estado geral
console.log('Versão IndexedDB:', (await indexedDB.open('aspec-captura-db')).version);
console.log('AppState:', appState);
console.log('Usuário:', await AuthService.GetCurrentUserAsync());

// Testar consultas
const allUO = await dbInterop.getPatrimonioByUO("09");
const filtered = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log(`Total UO: ${allUO.length}, Filtrado: ${filtered.length}`);

// Verificar performance
console.time('query');
await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.timeEnd('query');
```

### Backend

```bash
# Logs gerais
tail -100 /var/log/app.log

# Logs de erro
grep "ERROR" /var/log/app.log | tail -50

# Logs de login
grep "api/auth/login" /var/log/app.log | tail -50

# Logs de filtro
grep "Tombamentos filtrados" /var/log/app.log | tail -20

# Monitoramento em tempo real
tail -f /var/log/app.log | grep "ERROR\|WARNING\|Subarea\|ExercicioFiscal"
```

---

## Contatos de Suporte

**Equipe de Desenvolvimento**:
- Desenvolvedor Principal: [nome] - [email]
- DevOps: [nome] - [email]
- QA: [nome] - [email]

**Canais de Comunicação**:
- Slack: #aspec-captura-support
- Email: support@example.com
- Telefone: [número]

**Horário de Suporte**:
- Segunda a Sexta: 9h-18h
- Emergências: 24/7 (telefone)

---

## Referências

- [DEPLOYMENT_SUBAREA_FILTER.md](./DEPLOYMENT_SUBAREA_FILTER.md)
- [ROLLBACK_SUBAREA_FILTER.md](./ROLLBACK_SUBAREA_FILTER.md)
- [MONITORING_SUBAREA_FILTER.md](./MONITORING_SUBAREA_FILTER.md)
- [CHANGELOG.md](../CHANGELOG.md)
- [Design Document](./.kiro/specs/subarea-filter-loading-fix/design.md)

---

**Documento criado em**: 2024  
**Última atualização**: 2024  
**Versão**: 1.0
