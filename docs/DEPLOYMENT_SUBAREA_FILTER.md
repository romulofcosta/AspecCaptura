# Guia de Deploy: Correção de Carregamento de Bens por Subárea

## Visão Geral

Este documento detalha o procedimento de deploy para a correção de carregamento de bens por subárea, incluindo checklist de validação, estratégia de deploy gradual e procedimentos de monitoramento.

## Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Checklist Pré-Deploy](#checklist-pré-deploy)
3. [Deploy em Staging](#deploy-em-staging)
4. [Deploy Gradual em Produção](#deploy-gradual-em-produção)
5. [Validação Pós-Deploy](#validação-pós-deploy)
6. [Monitoramento](#monitoramento)
7. [Troubleshooting](#troubleshooting)

---

## Pré-requisitos

### Ambiente

- [ ] Ambiente de staging configurado e funcional
- [ ] Ambiente de produção acessível
- [ ] Acesso ao sistema de logs (console, Application Insights, etc.)
- [ ] Ferramentas de monitoramento configuradas
- [ ] Backup do banco de dados (se aplicável)

### Código

- [ ] Todos os testes unitários passando (100%)
- [ ] Todos os testes de integração passando
- [ ] Code review aprovado
- [ ] Branch principal atualizada (`main` ou `master`)
- [ ] Versão taggeada no Git (ex: `v1.1.0-subarea-filter`)

### Documentação

- [ ] CHANGELOG.md atualizado
- [ ] README.md atualizado com novas funcionalidades
- [ ] Documentação técnica completa (este documento)
- [ ] Plano de rollback documentado

---

## Checklist Pré-Deploy

### 1. Validação de Código

```bash
# Executar testes
dotnet test

# Build de produção
dotnet publish -c Release

# Verificar tamanho do bundle
ls -lh bin/Release/net8.0/publish/wwwroot/_framework/
```

**Critérios de Aceitação**:
- ✅ Todos os testes passando
- ✅ Build sem erros ou warnings críticos
- ✅ Tamanho do bundle aceitável (< 5MB para framework)

### 2. Validação de Migração IndexedDB

**Teste Manual**:
1. Abrir aplicação com IndexedDB v10 existente
2. Abrir DevTools → Application → IndexedDB → aspec-captura-db
3. Verificar versão atual: `10`
4. Atualizar código para versão com migração
5. Recarregar aplicação
6. Verificar nova versão: `11`
7. Verificar índices em `patrimonio`:
   - `nutomb`
   - `cdUnid`
   - `cdUnidNorm`
   - `esfera`
   - `cdArea` ✨ **NOVO**
   - `cdSArea` ✨ **NOVO**

**Console de Validação**:
```javascript
// Abrir console do navegador
const db = await indexedDB.open('aspec-captura-db', 11);
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
console.log('Índices disponíveis:', Array.from(store.indexNames));
// Esperado: ['nutomb', 'cdUnid', 'cdUnidNorm', 'esfera', 'cdArea', 'cdSArea']
```

### 3. Validação de Funcionalidade

**Cenário 1: Filtro por Subárea**
```
1. Login com usuário válido
2. Navegar para /configuracao-sessao
3. Selecionar: Órgão "09" → UO "09" → Área "001" → Subárea "001"
4. Salvar configuração
5. Navegar para /items
6. Verificar que apenas bens com CdUnid=09, CdArea=001, CdSArea=001 são exibidos
7. Verificar contagem no summary bar (ex: "45 BENS")
```

**Cenário 2: Compatibilidade Legado**
```
1. Login com usuário válido
2. Navegar para /configuracao-sessao
3. Selecionar apenas Órgão e UO (sem Área/Subárea)
4. Salvar configuração
5. Navegar para /items
6. Verificar que todos os bens da UO são exibidos
7. Verificar contagem total (ex: "500 BENS")
```

**Cenário 3: Filtro de Exercício Fiscal**
```
1. Fazer login com usuário válido
2. Abrir DevTools → Network
3. Inspecionar resposta de /api/auth/login
4. Verificar que apenas registros do ano corrente (2024) são retornados
5. Confirmar redução de payload (≥ 70%)
```

### 4. Validação de Performance

**Benchmark de Consulta**:
```javascript
// Console do navegador
console.time('getPatrimonioBySubarea');
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.timeEnd('getPatrimonioBySubarea');
console.log('Registros retornados:', items.length);
// Esperado: < 50ms para 500 registros na UO
```

**Benchmark de Payload**:
```bash
# Medir tamanho da resposta do login
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"ce999.admin","Senha":"senha123"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\n"
# Esperado: < 500KB (antes: ~8MB)
```

---

## Deploy em Staging

### Passo 1: Build e Publicação

```bash
# Navegar para o diretório do projeto
cd pwa-camera-poc-blazor

# Limpar builds anteriores
dotnet clean

# Build de produção
dotnet publish -c Release -o ./publish

# Verificar arquivos publicados
ls -la ./publish/wwwroot/
```

### Passo 2: Deploy para Staging

**Opção A: Deploy Manual**
```bash
# Copiar arquivos para servidor de staging
scp -r ./publish/wwwroot/* user@staging-server:/var/www/aspec-captura/

# Reiniciar servidor (se necessário)
ssh user@staging-server "sudo systemctl restart nginx"
```

**Opção B: Deploy via CI/CD**
```bash
# Fazer push para branch de staging
git checkout staging
git merge main
git push origin staging

# CI/CD pipeline executará automaticamente
```

### Passo 3: Validação em Staging

**Checklist de Validação**:
- [ ] Aplicação carrega sem erros
- [ ] Login funciona corretamente
- [ ] Migração de IndexedDB ocorre automaticamente
- [ ] Filtro por subárea funciona
- [ ] Comportamento legado preservado
- [ ] Payload do login reduzido
- [ ] Sem erros no console do navegador
- [ ] Sem erros nos logs do servidor

**Teste com Dados Reais**:
```
1. Usar credenciais de usuário de teste em staging
2. Configurar sessão com área/subárea real
3. Verificar que bens corretos são exibidos
4. Testar sincronização de dados
5. Verificar que filtros de status funcionam
6. Testar busca por código
```

### Passo 4: Aprovação para Produção

**Critérios de Aprovação**:
- ✅ Todos os testes de validação passaram
- ✅ Performance dentro dos limites esperados
- ✅ Sem erros críticos em 24h de staging
- ✅ Feedback positivo de usuários de teste
- ✅ Plano de rollback documentado e testado

---

## Deploy Gradual em Produção

### Estratégia de Deploy

O deploy será realizado em **3 fases** com monitoramento entre cada fase:

| Fase | Usuários | Duração de Monitoramento | Critério de Avanço |
|------|----------|--------------------------|-------------------|
| **Fase 1** | 10% | 48 horas | Taxa de erro < 2%, sem problemas críticos |
| **Fase 2** | 50% | 72 horas | Taxa de erro < 2%, performance estável |
| **Fase 3** | 100% | 7 dias | Monitoramento contínuo |

### Fase 1: Deploy para 10% dos Usuários (Grupo Piloto)

**Implementação com Feature Flag**:

```csharp
// Em Program.cs ou configuração de serviços
builder.Services.AddScoped<IFeatureManager, FeatureManager>();

// Em Items.razor
@inject IFeatureManager FeatureManager

private async Task LoadItems()
{
    var user = await AuthService.GetCurrentUserAsync();
    if (user == null) return;

    // Feature flag baseada em usuário
    var enableSubareaFilter = await FeatureManager.IsEnabledAsync("SubareaFilter", user);

    if (enableSubareaFilter && appState.CurrentArea != null)
    {
        // Nova funcionalidade
        items = await DbService.GetPatrimonioBySubareaAsync(
            appState.CurrentUO.IdUO,
            appState.CurrentArea?.IdArea,
            appState.CurrentSubarea?.IdSubarea
        );
    }
    else
    {
        // Funcionalidade legado
        items = await DbService.GetPatrimonioByUOAsync(appState.CurrentUO.IdUO);
    }
}
```

**Configuração de Grupo Piloto**:
```json
// appsettings.Production.json
{
  "FeatureManagement": {
    "SubareaFilter": {
      "EnabledFor": [
        {
          "Name": "Percentage",
          "Parameters": {
            "Value": 10
          }
        }
      ]
    }
  }
}
```

**Monitoramento Fase 1 (48h)**:
- [ ] Taxa de sucesso de login ≥ 98%
- [ ] Tempo médio de carregamento de Items.razor ≤ 2s
- [ ] Taxa de erro < 2%
- [ ] Nenhum relatório de dados ausentes
- [ ] Migração de IndexedDB bem-sucedida em ≥ 95% dos clientes

### Fase 2: Deploy para 50% dos Usuários

**Atualizar Feature Flag**:
```json
{
  "FeatureManagement": {
    "SubareaFilter": {
      "EnabledFor": [
        {
          "Name": "Percentage",
          "Parameters": {
            "Value": 50
          }
        }
      ]
    }
  }
}
```

**Monitoramento Fase 2 (72h)**:
- [ ] Métricas de Fase 1 mantidas
- [ ] Volume de requisições estável
- [ ] Sem degradação de performance
- [ ] Feedback positivo de usuários

### Fase 3: Deploy para 100% dos Usuários

**Atualizar Feature Flag**:
```json
{
  "FeatureManagement": {
    "SubareaFilter": {
      "EnabledFor": [
        {
          "Name": "AlwaysOn"
        }
      ]
    }
  }
}
```

**Monitoramento Fase 3 (7 dias)**:
- [ ] Todas as métricas estáveis
- [ ] Sem problemas reportados
- [ ] Performance dentro dos limites
- [ ] Preparar para remover feature flag

### Remoção de Feature Flag (Após 7 dias)

Após validação completa, remover código de feature flag:

```csharp
// Simplificar para usar sempre a nova funcionalidade
private async Task LoadItems()
{
    var user = await AuthService.GetCurrentUserAsync();
    if (user == null) return;

    if (appState.CurrentUO != null)
    {
        var allPatrimonio = await DbService.GetPatrimonioBySubareaAsync(
            appState.CurrentUO.IdUO,
            appState.CurrentArea?.IdArea,
            appState.CurrentSubarea?.IdSubarea
        );
        
        // ... resto do código
    }
}
```

---

## Validação Pós-Deploy

### Checklist de Validação Imediata (Primeiras 2 horas)

- [ ] Aplicação carrega sem erros em produção
- [ ] Login funciona para múltiplos usuários
- [ ] Migração de IndexedDB ocorre automaticamente
- [ ] Filtro por subárea funciona corretamente
- [ ] Comportamento legado preservado
- [ ] Sem erros críticos nos logs
- [ ] Métricas de performance dentro dos limites

### Checklist de Validação Estendida (Primeiras 24 horas)

- [ ] Taxa de sucesso de login ≥ 98%
- [ ] Tempo médio de carregamento ≤ 2 segundos
- [ ] Taxa de erro < 2%
- [ ] Nenhum relatório de dados ausentes
- [ ] Migração de IndexedDB bem-sucedida em ≥ 95% dos clientes
- [ ] Redução de payload confirmada (≥ 70%)
- [ ] Feedback positivo de usuários

### Testes de Validação Manual

**Teste 1: Novo Usuário**
```
1. Limpar cache e dados do navegador
2. Fazer login com novo usuário
3. Verificar que IndexedDB v11 é criado diretamente
4. Configurar sessão com área/subárea
5. Verificar filtro funcionando
```

**Teste 2: Usuário Existente**
```
1. Fazer login com usuário que já usava a aplicação
2. Verificar migração automática de v10 para v11
3. Verificar que dados existentes foram preservados
4. Configurar sessão com área/subárea
5. Verificar filtro funcionando
```

**Teste 3: Múltiplos Navegadores**
```
- [ ] Chrome/Edge (Desktop)
- [ ] Firefox (Desktop)
- [ ] Safari (Desktop)
- [ ] Chrome (Android)
- [ ] Safari (iOS)
```

---

## Monitoramento

### Métricas Críticas

**Backend (API)**:
```
- Taxa de sucesso de /api/auth/login: ≥ 98%
- Tempo de resposta de /api/auth/login: ≤ 3s (p95)
- Tamanho médio de payload: ~270KB (redução de 70%)
- Taxa de erro: < 2%
```

**Frontend (Blazor)**:
```
- Tempo de carregamento de Items.razor: ≤ 2s (p95)
- Taxa de sucesso de migração IndexedDB: ≥ 95%
- Taxa de erro em GetPatrimonioBySubareaAsync: < 2%
- Tempo de consulta IndexedDB: ≤ 50ms (p95)
```

### Alertas Configurados

**Alerta Crítico** (Ação Imediata):
- Taxa de erro > 5% em qualquer endpoint
- Tempo de resposta > 10s (p95)
- Taxa de falha de migração > 10%

**Alerta de Atenção** (Investigar):
- Taxa de erro > 2% em qualquer endpoint
- Tempo de resposta > 5s (p95)
- Redução de payload < 50%

### Comandos de Monitoramento

**Logs do Backend**:
```bash
# Filtrar logs de login
grep "api/auth/login" /var/log/app.log | tail -100

# Filtrar logs de erro
grep "ERROR" /var/log/app.log | tail -50

# Monitorar em tempo real
tail -f /var/log/app.log | grep "Subarea\|ExercicioFiscal"
```

**Logs do Frontend (Console do Navegador)**:
```javascript
// Verificar versão do IndexedDB
const db = await indexedDB.open('aspec-captura-db');
console.log('Versão do DB:', db.version);

// Verificar índices
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
console.log('Índices:', Array.from(store.indexNames));
```

### Dashboard de Monitoramento

**Métricas Recomendadas**:
1. **Taxa de Sucesso de Login** (linha do tempo)
2. **Tempo de Resposta de Login** (histograma)
3. **Tamanho de Payload** (linha do tempo)
4. **Taxa de Erro por Endpoint** (gráfico de barras)
5. **Tempo de Carregamento de Items.razor** (histograma)
6. **Taxa de Sucesso de Migração IndexedDB** (percentual)

---

## Troubleshooting

### Problema: Migração de IndexedDB Falha

**Sintomas**:
- Erro no console: "Database upgrade failed"
- Aplicação não carrega dados

**Solução**:
```javascript
// Forçar limpeza e re-sincronização
await dbInterop.clear('patrimonio');
await dbInterop.clear('patrimonio_staging');
// Recarregar página
window.location.reload();
```

### Problema: Filtro por Subárea Não Funciona

**Sintomas**:
- Todos os bens da UO são exibidos mesmo com área/subárea configuradas

**Diagnóstico**:
```javascript
// Verificar AppState
console.log('CurrentArea:', appState.CurrentArea);
console.log('CurrentSubarea:', appState.CurrentSubarea);

// Testar método diretamente
const items = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log('Itens filtrados:', items.length);
```

**Solução**:
- Verificar se área/subárea estão realmente configuradas no AppState
- Verificar se método JavaScript está sendo chamado corretamente
- Verificar logs do console para erros

### Problema: Payload Ainda Grande

**Sintomas**:
- Tempo de login ainda lento
- Payload > 1MB

**Diagnóstico**:
```bash
# Verificar logs do backend
grep "Tombamentos filtrados para exercício" /var/log/app.log
```

**Solução**:
- Verificar se filtro de exercício fiscal está ativo
- Verificar se dados no S3 contêm campo `ExercicioFiscal`
- Verificar logs para confirmar quantidade de registros filtrados

### Problema: Dados Ausentes

**Sintomas**:
- Usuários reportam que bens não aparecem

**Diagnóstico**:
```javascript
// Verificar dados no IndexedDB
const allItems = await dbInterop.getAll('patrimonio');
console.log('Total de itens:', allItems.length);

// Verificar filtros
const filtered = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.log('Itens filtrados:', filtered.length);
```

**Solução**:
- Verificar se dados foram sincronizados corretamente
- Verificar se campos `cdArea` e `cdSArea` estão presentes nos dados
- Forçar re-sincronização se necessário

---

## Comandos Úteis

### Build e Deploy

```bash
# Build de produção
dotnet publish -c Release -o ./publish

# Verificar tamanho do bundle
du -sh ./publish/wwwroot/_framework/

# Criar tag de versão
git tag -a v1.1.0-subarea-filter -m "Deploy: Correção de carregamento por subárea"
git push origin v1.1.0-subarea-filter
```

### Validação

```bash
# Testar endpoint de login
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"ce999.admin","Senha":"senha123"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\nTempo: %{time_total}s\n"

# Verificar logs em tempo real
tail -f /var/log/app.log | grep "ERROR\|WARNING"
```

### Rollback

```bash
# Reverter para versão anterior
git revert HEAD
git push origin main

# Ou reverter para tag específica
git checkout v1.0.0
git push origin main --force
```

---

## Contatos e Suporte

**Equipe de Desenvolvimento**:
- Desenvolvedor Principal: [nome]
- DevOps: [nome]
- QA: [nome]

**Canais de Comunicação**:
- Slack: #aspec-captura-deploy
- Email: dev@example.com
- Telefone de Emergência: [número]

---

## Referências

- [CHANGELOG.md](../CHANGELOG.md)
- [ROLLBACK_SUBAREA_FILTER.md](./ROLLBACK_SUBAREA_FILTER.md)
- [MONITORING_SUBAREA_FILTER.md](./MONITORING_SUBAREA_FILTER.md)
- [TROUBLESHOOTING_SUBAREA_FILTER.md](./TROUBLESHOOTING_SUBAREA_FILTER.md)
- [Design Document](./.kiro/specs/subarea-filter-loading-fix/design.md)

---

**Documento criado em**: 2024  
**Última atualização**: 2024  
**Versão**: 1.0
