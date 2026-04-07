# Plano de Rollback: Correção de Carregamento de Bens por Subárea

## Visão Geral

Este documento detalha os procedimentos de rollback para a correção de carregamento de bens por subárea, incluindo 3 níveis de rollback com complexidade crescente e procedimentos de detecção de problemas.

## Índice

1. [Estratégia de Rollback](#estratégia-de-rollback)
2. [Indicadores de Falha](#indicadores-de-falha)
3. [Níveis de Rollback](#níveis-de-rollback)
4. [Procedimentos Detalhados](#procedimentos-detalhados)
5. [Validação Pós-Rollback](#validação-pós-rollback)
6. [Prevenção de Problemas](#prevenção-de-problemas)

---

## Estratégia de Rollback

### Princípios

1. **Rollback Progressivo**: Começar com o nível mais simples e escalar se necessário
2. **Preservação de Dados**: Garantir que nenhum dado do usuário seja perdido
3. **Comunicação Clara**: Informar usuários sobre o status e ações necessárias
4. **Documentação**: Registrar todos os incidentes e ações tomadas

### Níveis de Rollback

| Nível | Componente | Complexidade | Tempo Estimado | Impacto |
|-------|------------|--------------|----------------|---------|
| **1** | Frontend (Items.razor) | Baixa | < 15 min | Mínimo - Funcionalidade preservada |
| **2** | IndexedDB (db-interop.js) | Média | < 30 min | Médio - Requer limpeza de cache |
| **3** | Backend (Program.cs) | Alta | < 1 hora | Alto - Payload volta a ser grande |

---

## Indicadores de Falha

### Métricas de Alerta

#### Alerta Crítico (Rollback Imediato)

**Frontend**:
- ❌ Taxa de erro > 5% em `GetPatrimonioBySubareaAsync`
- ❌ Tempo de carregamento de Items.razor > 10 segundos
- ❌ Taxa de falha de migração IndexedDB > 10%
- ❌ Dados ausentes reportados por > 5% dos usuários

**Backend**:
- ❌ Taxa de erro > 5% em `/api/auth/login`
- ❌ Tempo de resposta > 15 segundos
- ❌ Exceções relacionadas a `ExercicioFiscal` > 10/min

#### Alerta de Atenção (Investigar e Preparar Rollback)

**Frontend**:
- ⚠️ Taxa de erro > 2% em `GetPatrimonioBySubareaAsync`
- ⚠️ Tempo de carregamento de Items.razor > 5 segundos
- ⚠️ Reclamações de usuários sobre bens não aparecendo

**Backend**:
- ⚠️ Taxa de erro > 2% em `/api/auth/login`
- ⚠️ Tempo de resposta > 10 segundos
- ⚠️ Redução de payload < 50% (esperado: ≥ 70%)

### Comandos de Diagnóstico

**Verificar Taxa de Erro (Backend)**:
```bash
# Contar erros nas últimas 24h
grep "ERROR" /var/log/app.log | grep "$(date +%Y-%m-%d)" | wc -l

# Verificar erros específicos de filtro
grep "ExercicioFiscal\|Subarea" /var/log/app.log | grep "ERROR"
```

**Verificar Taxa de Erro (Frontend)**:
```javascript
// Console do navegador - verificar erros
const errors = performance.getEntriesByType('navigation')
  .filter(e => e.responseStatus >= 400);
console.log('Erros HTTP:', errors.length);
```

---

## Níveis de Rollback

### Nível 1: Rollback de Frontend (Baixo Risco)

**Quando Usar**:
- Problemas isolados no frontend
- Filtro por subárea não funciona corretamente
- Performance degradada apenas em Items.razor
- Backend funcionando normalmente

**Impacto**:
- ✅ Funcionalidade de filtro por subárea continua funcionando (filtro manual)
- ✅ Sem perda de dados
- ✅ Sem necessidade de limpeza de cache
- ⚠️ Performance pode ser ligeiramente pior (filtro em memória)

**Tempo Estimado**: < 15 minutos

---

### Nível 2: Rollback de IndexedDB (Médio Risco)

**Quando Usar**:
- Migração de IndexedDB causando problemas
- Índices corrompidos ou ausentes
- Dados inconsistentes após migração
- Nível 1 não resolveu o problema

**Impacto**:
- ⚠️ Usuários precisam limpar cache e re-sincronizar
- ⚠️ Dados locais serão perdidos (mas podem ser re-sincronizados)
- ✅ Funcionalidade volta ao estado anterior
- ⚠️ Requer comunicação com usuários

**Tempo Estimado**: < 30 minutos

---

### Nível 3: Rollback de Backend (Alto Risco)

**Quando Usar**:
- Filtro de exercício fiscal causando problemas
- Dados ausentes no payload
- Erros críticos no endpoint de login
- Níveis 1 e 2 não resolveram o problema

**Impacto**:
- ⚠️ Payload volta a ser grande (~8MB)
- ⚠️ Tempo de login volta a ser lento
- ✅ Todos os dados históricos disponíveis
- ⚠️ Performance de rede degradada

**Tempo Estimado**: < 1 hora

---

## Procedimentos Detalhados

### Nível 1: Rollback de Frontend

#### Passo 1: Reverter Código em Items.razor

**Arquivo**: `Pages/Items.razor`

**Modificação**:
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

            // ROLLBACK: Usar método antigo
            var allPatrimonio = await DbService.GetPatrimonioByUOAsync(appState.CurrentUO.IdUO);
            
            // ADICIONAR: Filtro manual por área/subárea se necessário
            if (appState.CurrentArea != null && appState.CurrentSubarea != null)
            {
                allPatrimonio = allPatrimonio
                    .Where(p => 
                        string.Equals(p.CdArea, appState.CurrentArea.IdArea, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.CdSArea, appState.CurrentSubarea.IdSubarea, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }
            
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

#### Passo 2: Build e Deploy

```bash
# Build de produção
dotnet publish -c Release -o ./publish

# Deploy (ajustar conforme seu ambiente)
# Opção A: Manual
scp -r ./publish/wwwroot/* user@server:/var/www/aspec-captura/

# Opção B: CI/CD
git add Pages/Items.razor
git commit -m "Rollback: Nível 1 - Frontend Items.razor"
git push origin main
```

#### Passo 3: Validação

```
1. Acessar aplicação em produção
2. Fazer login com usuário de teste
3. Configurar sessão com área/subárea
4. Navegar para /items
5. Verificar que filtro funciona (mesmo que mais lento)
6. Verificar logs para confirmar ausência de erros
```

---

### Nível 2: Rollback de IndexedDB

#### Passo 1: Reverter Versão do Banco

**Arquivo**: `wwwroot/js/db-interop.js`

**Modificação**:
```javascript
// Linha ~3: Reverter versão
dbVersion: 10,  // Voltar de 11 para 10

// Comentar ou remover criação de novos índices
// patrimonioStore.createIndex('cdArea', 'cdArea', { unique: false });
// patrimonioStore.createIndex('cdSArea', 'cdSArea', { unique: false });
```

#### Passo 2: Forçar Limpeza de IndexedDB nos Clientes

**Opção A: Código Automático**

Adicionar em `App.razor` ou `Program.cs`:

```csharp
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

**Opção B: Comunicação com Usuários**

Enviar notificação para usuários:

```
Título: Atualização Necessária
Mensagem: Por favor, limpe o cache do navegador e faça login novamente.

Instruções:
1. Pressione Ctrl+Shift+Delete (Windows) ou Cmd+Shift+Delete (Mac)
2. Selecione "Dados de sites" ou "Cookies e dados de sites"
3. Clique em "Limpar dados"
4. Recarregue a página e faça login novamente
```

#### Passo 3: Build e Deploy

```bash
# Build de produção
dotnet publish -c Release -o ./publish

# Deploy
git add wwwroot/js/db-interop.js App.razor
git commit -m "Rollback: Nível 2 - IndexedDB v10"
git push origin main
```

#### Passo 4: Validação

```
1. Limpar cache do navegador
2. Acessar aplicação
3. Verificar que IndexedDB v10 é criado
4. Fazer login e sincronizar dados
5. Verificar que aplicação funciona normalmente
6. Confirmar ausência de erros de migração
```

---

### Nível 3: Rollback de Backend

#### Passo 1: Reverter Filtro de Exercício Fiscal

**Arquivo**: `pwa-camera-poc-api/Program.cs`

**Modificação**:
```csharp
// Dentro do endpoint app.MapPost("/api/auth/login", ...)

var tabelas = root.Tabelas;
if (tabelas is null)
{
    log.LogWarning("Seção 'tabelas' ausente no arquivo {S3Key}.", s3Key);
    return Results.Problem("Dados de tabelas ausentes no arquivo do município.");
}

var tombamentoBase = tabelas.Tombamentos ?? new List<TombamentoRecord>();
log.LogInformation("Tombamentos encontrados em tabelas.tombamentos: {Count}", tombamentoBase.Count);

// ROLLBACK: Comentar filtro de exercício fiscal
// var exercicioCorrente = DateTime.Now.Year;
// var tombamentoFiltradoExercicio = tombamentoBase
//     .Where(p => p.ExercicioFiscal == exercicioCorrente || p.ExercicioFiscal == 0)
//     .ToList();

// Usar dados completos
var tombamentoFiltradoExercicio = tombamentoBase;

log.LogInformation("Tombamentos (sem filtro de exercício): {Count}", tombamentoFiltradoExercicio.Count);

// Filtro de esfera
var tombamentoFiltrado = user.Esfera == "A"
    ? tombamentoFiltradoExercicio
    : tombamentoFiltradoExercicio.Where(p => p.Esfera == user.Esfera).ToList();

log.LogInformation("Tombamentos filtrados para esfera {Esfera}: {Count}", user.Esfera, tombamentoFiltrado.Count);

var orgaos = BuildOrgaoHierarchy(user.Esfera, tombamentoFiltrado, tabelas);
```

#### Passo 2: Build e Deploy da API

```bash
# Navegar para projeto da API
cd pwa-camera-poc-api

# Build de produção
dotnet publish -c Release -o ./publish

# Deploy (ajustar conforme seu ambiente)
# Reiniciar serviço da API
sudo systemctl restart aspec-api
```

#### Passo 3: Validação

```bash
# Testar endpoint de login
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"ce999.admin","Senha":"senha123"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\n"

# Verificar que payload voltou ao tamanho original (~8MB)
# Verificar que todos os dados históricos estão presentes
```

#### Passo 4: Comunicação com Usuários

```
Título: Atualização de Sistema
Mensagem: O sistema foi atualizado. O primeiro login pode demorar um pouco mais devido ao carregamento de dados históricos.

Observação: Esta é uma medida temporária enquanto investigamos melhorias de performance.
```

---

## Rollback Completo (Todos os Níveis)

Se todos os níveis individuais falharem, executar rollback completo:

### Passo 1: Reverter Todos os Commits

```bash
# Identificar commits relacionados
git log --oneline | grep -i "subarea\|exercicio"

# Reverter commits (ajustar hashes conforme necessário)
git revert <commit-hash-1> <commit-hash-2> <commit-hash-3>

# Ou reverter para tag anterior
git checkout v1.0.0
git push origin main --force
```

### Passo 2: Forçar Limpeza Total

**Frontend**:
```csharp
// Adicionar flag de emergência
var forceReset = configuration.GetValue<bool>("Emergency:ForceIndexedDBReset", false);

if (forceReset)
{
    await DbService.ClearAsync("patrimonio");
    await DbService.ClearAsync("patrimonio_staging");
    await DbService.SetMetadataAsync("schema:version", "10");
    
    // Mostrar mensagem para usuário
    await ToastService.ShowWarning("Sistema atualizado. Por favor, faça login novamente.");
}
```

**Backend**:
```csharp
// Remover completamente filtro de exercício fiscal
var tombamentoFiltrado = user.Esfera == "A"
    ? tombamentoBase
    : tombamentoBase.Where(p => p.Esfera == user.Esfera).ToList();
```

### Passo 3: Deploy Completo

```bash
# Frontend
cd pwa-camera-poc-blazor
dotnet publish -c Release -o ./publish
# Deploy conforme ambiente

# Backend
cd pwa-camera-poc-api
dotnet publish -c Release -o ./publish
# Deploy conforme ambiente
```

### Passo 4: Comunicação Oficial

```
Assunto: Manutenção de Sistema - Ação Necessária

Prezados usuários,

Realizamos uma atualização de sistema que requer ação de sua parte:

1. Limpe o cache do navegador (Ctrl+Shift+Delete)
2. Selecione "Dados de sites" e clique em "Limpar dados"
3. Recarregue a página e faça login novamente
4. Sincronize seus dados novamente

Pedimos desculpas pelo inconveniente.

Equipe de Desenvolvimento
```

---

## Validação Pós-Rollback

### Checklist de Validação

- [ ] Aplicação carrega sem erros
- [ ] Login funciona normalmente
- [ ] Dados são exibidos corretamente
- [ ] Filtros de status funcionam
- [ ] Busca por código funciona
- [ ] Sincronização de dados funciona
- [ ] Sem erros nos logs
- [ ] Performance aceitável

### Testes de Regressão

**Teste 1: Login e Carregamento**
```
1. Fazer login com usuário válido
2. Verificar que dados são carregados
3. Verificar tempo de carregamento (pode ser mais lento)
4. Confirmar que todos os bens aparecem
```

**Teste 2: Filtros**
```
1. Aplicar filtro de status
2. Verificar que filtro funciona
3. Testar busca por código
4. Confirmar resultados corretos
```

**Teste 3: Sincronização**
```
1. Capturar novo item
2. Sincronizar dados
3. Verificar que sincronização funciona
4. Confirmar que item aparece após sincronização
```

---

## Prevenção de Problemas

### Testes Pré-Deploy Obrigatórios

- [ ] Testes unitários passando (100%)
- [ ] Testes de integração passando
- [ ] Teste manual em staging com dados reais
- [ ] Validação de migração IndexedDB (v10 → v11)
- [ ] Teste de performance (payload, consultas)
- [ ] Teste em múltiplos navegadores

### Deploy Gradual

**Estratégia Recomendada**:
1. **Fase 1 (10%)**: Deploy para grupo piloto, monitorar 48h
2. **Fase 2 (50%)**: Se Fase 1 bem-sucedida, monitorar 72h
3. **Fase 3 (100%)**: Se Fase 2 bem-sucedida, monitorar 7 dias

### Feature Flags

**Implementação**:
```csharp
// Permitir desabilitar funcionalidade via configuração
var enableSubareaFilter = configuration.GetValue<bool>("Features:SubareaFilter", true);

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

### Monitoramento Contínuo

**Alertas Configurados**:
- Taxa de erro > 2% → Investigar
- Taxa de erro > 5% → Rollback imediato
- Tempo de resposta > 10s → Investigar
- Tempo de resposta > 15s → Rollback imediato

---

## Documentação de Incidentes

### Template de Relatório

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
- Nível de rollback: [1/2/3/Completo]
- Tempo para resolução: [minutos]

## Causa Raiz
- [Descrição técnica da causa]

## Prevenção Futura
- [Ações para evitar recorrência]

## Lições Aprendidas
- [O que funcionou bem]
- [O que pode ser melhorado]
```

---

## Contatos de Emergência

**Equipe de Desenvolvimento**:
- Desenvolvedor Principal: [nome] - [telefone]
- DevOps: [nome] - [telefone]
- Gerente de Projeto: [nome] - [telefone]

**Canais de Comunicação**:
- Slack: #aspec-captura-emergencia
- Email: emergencia@example.com
- Telefone: [número]

**Escalação**:
1. Desenvolvedor Principal (0-30 min)
2. DevOps (30-60 min)
3. Gerente de Projeto (60+ min)

---

## Referências

- [DEPLOYMENT_SUBAREA_FILTER.md](./DEPLOYMENT_SUBAREA_FILTER.md)
- [MONITORING_SUBAREA_FILTER.md](./MONITORING_SUBAREA_FILTER.md)
- [TROUBLESHOOTING_SUBAREA_FILTER.md](./TROUBLESHOOTING_SUBAREA_FILTER.md)
- [CHANGELOG.md](../CHANGELOG.md)

---

**Documento criado em**: 2024  
**Última atualização**: 2024  
**Versão**: 1.0
