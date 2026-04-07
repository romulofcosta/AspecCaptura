# Guia de Monitoramento: Correção de Carregamento de Bens por Subárea

## Visão Geral

Este documento detalha as métricas, alertas e procedimentos de monitoramento para a correção de carregamento de bens por subárea, garantindo detecção precoce de problemas e manutenção da qualidade do serviço.

## Índice

1. [Métricas Críticas](#métricas-críticas)
2. [Alertas e Thresholds](#alertas-e-thresholds)
3. [Dashboard de Monitoramento](#dashboard-de-monitoramento)
4. [Comandos de Diagnóstico](#comandos-de-diagnóstico)
5. [Procedimentos de Resposta](#procedimentos-de-resposta)
6. [Relatórios e Análises](#relatórios-e-análises)

---

## Métricas Críticas

### Backend (API)

#### 1. Taxa de Sucesso de Login
**Métrica**: Percentual de requisições bem-sucedidas em `/api/auth/login`

**Thresholds**:
- ✅ **Normal**: ≥ 98%
- ⚠️ **Atenção**: 95-98%
- ❌ **Crítico**: < 95%

**Query (exemplo com Application Insights)**:
```kusto
requests
| where name == "POST /api/auth/login"
| summarize 
    Total = count(),
    Success = countif(resultCode == 200),
    SuccessRate = (countif(resultCode == 200) * 100.0) / count()
| project SuccessRate
```

**Comando de Log**:
```bash
# Contar requisições de login nas últimas 24h
grep "api/auth/login" /var/log/app.log | grep "$(date +%Y-%m-%d)" | wc -l

# Contar sucessos
grep "api/auth/login" /var/log/app.log | grep "$(date +%Y-%m-%d)" | grep "200" | wc -l

# Calcular taxa de sucesso
echo "scale=2; $(grep "api/auth/login" /var/log/app.log | grep "$(date +%Y-%m-%d)" | grep "200" | wc -l) * 100 / $(grep "api/auth/login" /var/log/app.log | grep "$(date +%Y-%m-%d)" | wc -l)" | bc
```

#### 2. Tempo de Resposta de Login
**Métrica**: Tempo de resposta (p50, p95, p99) do endpoint `/api/auth/login`

**Thresholds**:
- ✅ **Normal**: p95 ≤ 3s
- ⚠️ **Atenção**: p95 3-5s
- ❌ **Crítico**: p95 > 5s

**Query**:
```kusto
requests
| where name == "POST /api/auth/login"
| summarize 
    p50 = percentile(duration, 50),
    p95 = percentile(duration, 95),
    p99 = percentile(duration, 99)
```

#### 3. Tamanho de Payload
**Métrica**: Tamanho médio da resposta do endpoint de login

**Thresholds**:
- ✅ **Normal**: ~270KB (redução de 70%)
- ⚠️ **Atenção**: 500KB-1MB
- ❌ **Crítico**: > 1MB (indica filtro não funcionando)

**Comando**:
```bash
# Testar tamanho de payload
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"ce999.admin","Senha":"senha123"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\n" \
  -o /dev/null -s
```

#### 4. Registros Filtrados por Exercício Fiscal
**Métrica**: Quantidade de registros antes e depois do filtro

**Thresholds**:
- ✅ **Normal**: Redução de 70-96%
- ⚠️ **Atenção**: Redução de 50-70%
- ❌ **Crítico**: Redução < 50%

**Log Pattern**:
```
Tombamentos filtrados para exercício 2024: 500 (de 15000)
```

**Comando**:
```bash
# Extrair métricas de filtro
grep "Tombamentos filtrados para exercício" /var/log/app.log | tail -10
```

### Frontend (Blazor)

#### 5. Tempo de Carregamento de Items.razor
**Métrica**: Tempo desde navegação até renderização completa

**Thresholds**:
- ✅ **Normal**: p95 ≤ 2s
- ⚠️ **Atenção**: p95 2-5s
- ❌ **Crítico**: p95 > 5s

**Medição (Console do Navegador)**:
```javascript
// Adicionar em Items.razor
console.time('LoadItems');
await LoadItems();
console.timeEnd('LoadItems');
```

#### 6. Taxa de Sucesso de Migração IndexedDB
**Métrica**: Percentual de migrações bem-sucedidas de v10 para v11

**Thresholds**:
- ✅ **Normal**: ≥ 95%
- ⚠️ **Atenção**: 90-95%
- ❌ **Crítico**: < 90%

**Validação (Console do Navegador)**:
```javascript
// Verificar versão do IndexedDB
const db = await indexedDB.open('aspec-captura-db');
console.log('Versão do DB:', db.version);
// Esperado: 11

// Verificar índices
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
const indexes = Array.from(store.indexNames);
console.log('Índices:', indexes);
// Esperado: ['nutomb', 'cdUnid', 'cdUnidNorm', 'esfera', 'cdArea', 'cdSArea']
```

#### 7. Taxa de Erro em GetPatrimonioBySubareaAsync
**Métrica**: Percentual de chamadas que resultam em erro

**Thresholds**:
- ✅ **Normal**: < 1%
- ⚠️ **Atenção**: 1-2%
- ❌ **Crítico**: > 2%

**Log Pattern**:
```
Error getting patrimonio by subarea (UO: 09, Area: 001, Subarea: 001): [mensagem]
```

**Comando**:
```bash
# Contar erros de GetPatrimonioBySubarea
grep "Error getting patrimonio by subarea" /var/log/app.log | wc -l
```

#### 8. Tempo de Consulta IndexedDB
**Métrica**: Tempo de execução de `getPatrimonioBySubarea`

**Thresholds**:
- ✅ **Normal**: p95 ≤ 50ms
- ⚠️ **Atenção**: p95 50-100ms
- ❌ **Crítico**: p95 > 100ms

**Benchmark (Console do Navegador)**:
```javascript
// Executar múltiplas vezes para obter média
const times = [];
for (let i = 0; i < 10; i++) {
    const start = performance.now();
    await dbInterop.getPatrimonioBySubarea("09", "001", "001");
    const end = performance.now();
    times.push(end - start);
}
console.log('Tempos (ms):', times);
console.log('Média:', times.reduce((a, b) => a + b) / times.length);
console.log('p95:', times.sort((a, b) => a - b)[Math.floor(times.length * 0.95)]);
```

---

## Alertas e Thresholds

### Configuração de Alertas

#### Alerta Crítico (Ação Imediata)

**Condições**:
- Taxa de erro > 5% em qualquer endpoint
- Tempo de resposta > 10s (p95)
- Taxa de falha de migração > 10%
- Payload > 2MB (indica filtro não funcionando)

**Ação**:
1. Notificar equipe de desenvolvimento imediatamente
2. Iniciar procedimento de rollback (ver [ROLLBACK_SUBAREA_FILTER.md](./ROLLBACK_SUBAREA_FILTER.md))
3. Investigar causa raiz
4. Documentar incidente

**Exemplo de Configuração (Application Insights)**:
```json
{
  "name": "Subarea Filter - Critical Error Rate",
  "condition": {
    "allOf": [
      {
        "metricName": "requests/failed",
        "operator": "GreaterThan",
        "threshold": 5,
        "timeAggregation": "Percentage",
        "dimensions": [
          {
            "name": "request/name",
            "operator": "Include",
            "values": ["POST /api/auth/login"]
          }
        ]
      }
    ]
  },
  "actions": [
    {
      "actionGroupId": "/subscriptions/.../actionGroups/dev-team-critical"
    }
  ]
}
```

#### Alerta de Atenção (Investigar)

**Condições**:
- Taxa de erro > 2% em qualquer endpoint
- Tempo de resposta > 5s (p95)
- Redução de payload < 50%
- Tempo de carregamento > 5s

**Ação**:
1. Notificar equipe de desenvolvimento
2. Investigar logs e métricas
3. Preparar plano de rollback se necessário
4. Monitorar de perto nas próximas horas

---

## Dashboard de Monitoramento

### Métricas Recomendadas para Dashboard

#### Painel 1: Saúde Geral

**Widgets**:
1. **Taxa de Sucesso de Login** (gauge)
   - Verde: ≥ 98%
   - Amarelo: 95-98%
   - Vermelho: < 95%

2. **Tempo de Resposta de Login** (linha do tempo)
   - p50, p95, p99
   - Últimas 24 horas

3. **Taxa de Erro Geral** (gauge)
   - Verde: < 1%
   - Amarelo: 1-2%
   - Vermelho: > 2%

4. **Usuários Ativos** (contador)
   - Últimas 24 horas

#### Painel 2: Performance

**Widgets**:
1. **Tamanho de Payload** (linha do tempo)
   - Média, mínimo, máximo
   - Últimas 24 horas

2. **Tempo de Carregamento de Items.razor** (histograma)
   - Distribuição de tempos
   - Últimas 24 horas

3. **Tempo de Consulta IndexedDB** (linha do tempo)
   - p50, p95, p99
   - Últimas 24 horas

4. **Redução de Payload** (gauge)
   - Verde: ≥ 70%
   - Amarelo: 50-70%
   - Vermelho: < 50%

#### Painel 3: Migração IndexedDB

**Widgets**:
1. **Taxa de Sucesso de Migração** (gauge)
   - Verde: ≥ 95%
   - Amarelo: 90-95%
   - Vermelho: < 90%

2. **Versões de IndexedDB** (gráfico de pizza)
   - v10, v11, outros

3. **Erros de Migração** (lista)
   - Últimos 10 erros
   - Timestamp, mensagem

4. **Índices Ausentes** (contador)
   - Clientes sem índices cdArea/cdSArea

#### Painel 4: Filtros e Dados

**Widgets**:
1. **Registros Filtrados por Exercício** (linha do tempo)
   - Total antes do filtro
   - Total depois do filtro
   - Percentual de redução

2. **Uso de Filtro por Subárea** (contador)
   - Requisições com filtro
   - Requisições sem filtro

3. **Distribuição de Bens por Subárea** (gráfico de barras)
   - Top 10 subáreas mais acessadas

4. **Erros de Filtro** (lista)
   - Últimos 10 erros
   - Timestamp, UO, Área, Subárea

### Exemplo de Dashboard (Grafana)

```json
{
  "dashboard": {
    "title": "Aspec Captura - Subarea Filter Monitoring",
    "panels": [
      {
        "title": "Taxa de Sucesso de Login",
        "type": "gauge",
        "targets": [
          {
            "expr": "sum(rate(http_requests_total{endpoint=\"/api/auth/login\",status=\"200\"}[5m])) / sum(rate(http_requests_total{endpoint=\"/api/auth/login\"}[5m])) * 100"
          }
        ],
        "thresholds": [
          { "value": 95, "color": "red" },
          { "value": 98, "color": "yellow" },
          { "value": 100, "color": "green" }
        ]
      },
      {
        "title": "Tempo de Resposta de Login (p95)",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket{endpoint=\"/api/auth/login\"}[5m])) by (le))"
          }
        ]
      }
    ]
  }
}
```

---

## Comandos de Diagnóstico

### Backend

#### Verificar Logs de Login
```bash
# Últimos 100 logins
grep "api/auth/login" /var/log/app.log | tail -100

# Logins com erro
grep "api/auth/login" /var/log/app.log | grep "ERROR"

# Logins lentos (> 5s)
grep "api/auth/login" /var/log/app.log | grep "duration" | awk '$NF > 5000'
```

#### Verificar Filtro de Exercício Fiscal
```bash
# Logs de filtro
grep "Tombamentos filtrados para exercício" /var/log/app.log | tail -20

# Calcular redução média
grep "Tombamentos filtrados para exercício" /var/log/app.log | \
  awk -F'[()]' '{split($2, a, " de "); print (1 - a[1]/a[2]) * 100}' | \
  awk '{sum+=$1; count++} END {print "Redução média:", sum/count "%"}'
```

#### Verificar Erros Gerais
```bash
# Erros nas últimas 24h
grep "ERROR" /var/log/app.log | grep "$(date +%Y-%m-%d)" | wc -l

# Top 10 erros mais frequentes
grep "ERROR" /var/log/app.log | grep "$(date +%Y-%m-%d)" | \
  awk -F'ERROR' '{print $2}' | sort | uniq -c | sort -rn | head -10
```

### Frontend

#### Verificar Versão do IndexedDB
```javascript
// Console do navegador
const db = await indexedDB.open('aspec-captura-db');
console.log('Versão:', db.version);
console.log('Stores:', Array.from(db.objectStoreNames));

const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
console.log('Índices:', Array.from(store.indexNames));
```

#### Verificar Performance de Consultas
```javascript
// Benchmark de consulta por UO
console.time('getPatrimonioByUO');
const allUO = await dbInterop.getPatrimonioByUO("09");
console.timeEnd('getPatrimonioByUO');
console.log('Registros:', allUO.length);

// Benchmark de consulta por subárea
console.time('getPatrimonioBySubarea');
const filtered = await dbInterop.getPatrimonioBySubarea("09", "001", "001");
console.timeEnd('getPatrimonioBySubarea');
console.log('Registros:', filtered.length);
```

#### Verificar Dados no IndexedDB
```javascript
// Contar registros
const tx = db.transaction(['patrimonio'], 'readonly');
const store = tx.objectStore('patrimonio');
const countRequest = store.count();
countRequest.onsuccess = () => console.log('Total de registros:', countRequest.result);

// Verificar amostra de dados
const getAllRequest = store.getAll(null, 10);
getAllRequest.onsuccess = () => {
    console.log('Amostra de dados:', getAllRequest.result);
    console.log('Campos presentes:', Object.keys(getAllRequest.result[0] || {}));
};
```

---

## Procedimentos de Resposta

### Resposta a Alerta Crítico

**Passo 1: Verificação Inicial (0-5 min)**
```bash
# Verificar se serviço está rodando
systemctl status aspec-api

# Verificar logs recentes
tail -100 /var/log/app.log | grep "ERROR\|CRITICAL"

# Verificar uso de recursos
top -b -n 1 | head -20
df -h
```

**Passo 2: Diagnóstico (5-15 min)**
```bash
# Identificar tipo de problema
# - Taxa de erro alta?
grep "ERROR" /var/log/app.log | grep "$(date +%Y-%m-%d)" | wc -l

# - Tempo de resposta alto?
grep "api/auth/login" /var/log/app.log | grep "duration" | tail -20

# - Payload grande?
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Usuario":"test","Senha":"test"}' \
  --compressed -w "\nTamanho: %{size_download} bytes\n" \
  -o /dev/null -s
```

**Passo 3: Decisão (15-20 min)**
- Se problema é crítico e afeta > 10% dos usuários → **Iniciar rollback**
- Se problema é isolado → **Investigar causa raiz**
- Se problema é intermitente → **Monitorar de perto**

**Passo 4: Ação (20-30 min)**
- Executar rollback conforme [ROLLBACK_SUBAREA_FILTER.md](./ROLLBACK_SUBAREA_FILTER.md)
- Ou aplicar correção específica
- Comunicar usuários se necessário

**Passo 5: Validação (30-45 min)**
- Verificar que problema foi resolvido
- Confirmar métricas voltaram ao normal
- Documentar incidente

### Resposta a Alerta de Atenção

**Passo 1: Coleta de Informações (0-10 min)**
```bash
# Coletar métricas atuais
grep "api/auth/login" /var/log/app.log | tail -50 > /tmp/login-metrics.txt
grep "ERROR" /var/log/app.log | tail -50 > /tmp/errors.txt
```

**Passo 2: Análise (10-30 min)**
- Identificar padrão de erro
- Verificar se problema está crescendo
- Determinar impacto em usuários

**Passo 3: Preparação (30-60 min)**
- Preparar plano de rollback se necessário
- Notificar equipe
- Aumentar frequência de monitoramento

**Passo 4: Monitoramento Contínuo**
- Verificar métricas a cada 15 minutos
- Escalar para alerta crítico se piorar
- Documentar observações

---

## Relatórios e Análises

### Relatório Diário

**Template**:
```markdown
# Relatório Diário - Subarea Filter
Data: [YYYY-MM-DD]

## Métricas Gerais
- Taxa de sucesso de login: [X]%
- Tempo médio de resposta: [X]s
- Tamanho médio de payload: [X]KB
- Usuários ativos: [X]

## Performance
- Tempo de carregamento Items.razor (p95): [X]s
- Tempo de consulta IndexedDB (p95): [X]ms
- Redução de payload: [X]%

## Migração IndexedDB
- Taxa de sucesso: [X]%
- Clientes em v11: [X]
- Clientes em v10: [X]
- Erros de migração: [X]

## Problemas Identificados
- [Lista de problemas, se houver]

## Ações Tomadas
- [Lista de ações, se houver]

## Observações
- [Observações gerais]
```

### Relatório Semanal

**Análise de Tendências**:
- Evolução de métricas ao longo da semana
- Comparação com semana anterior
- Identificação de padrões
- Recomendações de otimização

### Relatório Mensal

**Análise Estratégica**:
- Impacto geral da correção
- ROI (redução de custos de infraestrutura)
- Satisfação de usuários
- Planos de melhoria contínua

---

## Ferramentas Recomendadas

### Monitoramento de Aplicação
- **Application Insights** (Azure)
- **New Relic**
- **Datadog**
- **Elastic APM**

### Monitoramento de Infraestrutura
- **Prometheus + Grafana**
- **CloudWatch** (AWS)
- **Azure Monitor**

### Logs
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Splunk**
- **Graylog**

### Alertas
- **PagerDuty**
- **Opsgenie**
- **Slack/Teams** (para alertas não críticos)

---

## Referências

- [DEPLOYMENT_SUBAREA_FILTER.md](./DEPLOYMENT_SUBAREA_FILTER.md)
- [ROLLBACK_SUBAREA_FILTER.md](./ROLLBACK_SUBAREA_FILTER.md)
- [TROUBLESHOOTING_SUBAREA_FILTER.md](./TROUBLESHOOTING_SUBAREA_FILTER.md)
- [CHANGELOG.md](../CHANGELOG.md)

---

**Documento criado em**: 2024  
**Última atualização**: 2024  
**Versão**: 1.0
