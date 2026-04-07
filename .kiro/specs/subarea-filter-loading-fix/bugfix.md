# Bugfix Requirements Document

## Introdução

Após configurar a sessão (órgão/unidade/área/subárea) na página ConfiguracaoSessao.razor, os bens da subárea selecionada não estão sendo carregados corretamente na página Items.razor. O sistema atualmente filtra bens apenas por Unidade Orçamentária (UO), ignorando completamente os filtros de Área e Subárea configurados pelo usuário. Adicionalmente, o sistema não filtra por exercício fiscal corrente, carregando dados históricos de múltiplos anos desnecessariamente.

Este bug impacta diretamente a experiência do usuário, que não consegue visualizar apenas os bens da subárea selecionada, resultando em performance degradada e confusão ao exibir bens de outras áreas/subáreas.

## Análise do Bug

### Comportamento Atual (Defeito)

1.1 QUANDO o usuário seleciona uma subárea específica na ConfiguracaoSessao.razor ENTÃO o sistema carrega TODOS os bens da Unidade Orçamentária, ignorando os filtros de Área e Subárea

1.2 QUANDO o método `GetPatrimonioByUOAsync` é chamado em Items.razor ENTÃO o sistema usa apenas o índice `cdUnid` do IndexedDB, retornando bens de todas as áreas e subáreas da unidade

1.3 QUANDO o endpoint `/api/auth/login` carrega dados do S3 ENTÃO o sistema retorna dados históricos de múltiplos exercícios fiscais (1993-2026) sem filtrar pelo ano corrente

1.4 QUANDO o IndexedDB é consultado para buscar bens ENTÃO não existem índices para `cdArea` e `cdSArea`, impossibilitando filtros eficientes por área/subárea

1.5 QUANDO o usuário navega para Items.razor após configurar a sessão ENTÃO a página exibe bens de outras áreas/subáreas além da selecionada, causando confusão

### Comportamento Esperado (Correto)

2.1 QUANDO o usuário seleciona uma subárea específica na ConfiguracaoSessao.razor ENTÃO o sistema DEVERÁ carregar e exibir apenas os bens dessa subárea específica

2.2 QUANDO o método `GetPatrimonioByUOAsync` é chamado em Items.razor ENTÃO o sistema DEVERÁ aceitar filtros opcionais de área e subárea, aplicando-os na consulta ao IndexedDB

2.3 QUANDO o endpoint `/api/auth/login` carrega dados do S3 ENTÃO o sistema DEVERÁ filtrar órgãos e unidades apenas pelo exercício fiscal corrente (ano atual)

2.4 QUANDO o IndexedDB é inicializado ENTÃO o sistema DEVERÁ criar índices para `cdArea` e `cdSArea` na store `patrimonio`, permitindo consultas eficientes

2.5 QUANDO o usuário navega para Items.razor após configurar a sessão ENTÃO a página DEVERÁ exibir apenas os bens que correspondem à hierarquia completa: Órgão → Unidade → Área → Subárea

2.6 QUANDO o sistema JavaScript consulta bens por subárea ENTÃO DEVERÁ existir um método `getPatrimonioBySubarea(cdUnid, cdArea, cdSArea)` que retorne apenas bens da subárea especificada

### Comportamento Inalterado (Prevenção de Regressão)

3.1 QUANDO o usuário não seleciona filtros de área/subárea (cenário legado) ENTÃO o sistema DEVERÁ CONTINUAR A carregar todos os bens da Unidade Orçamentária como antes

3.2 QUANDO o sistema sincroniza dados do S3 para IndexedDB ENTÃO o processo de sincronização DEVERÁ CONTINUAR A funcionar sem alterações na estrutura de dados

3.3 QUANDO o usuário busca bens por código (nutomb) ou descrição ENTÃO a funcionalidade de busca DEVERÁ CONTINUAR A funcionar independentemente dos filtros de área/subárea

3.4 QUANDO o sistema carrega bens já capturados localmente (store `items`) ENTÃO a consulta por `idUO` DEVERÁ CONTINUAR A funcionar sem alterações

3.5 QUANDO o usuário filtra bens por status (operação, manutenção, baixado) ENTÃO os filtros de status DEVERÃO CONTINUAR A funcionar corretamente sobre o conjunto filtrado por subárea

3.6 QUANDO o sistema normaliza códigos de unidade (remove zeros à esquerda) ENTÃO a lógica de normalização DEVERÁ CONTINUAR A funcionar para garantir compatibilidade com dados legados

## Condição do Bug

A condição do bug pode ser expressa através da seguinte função:

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type SessionConfig
  OUTPUT: boolean
  
  // Retorna true quando a condição do bug é atendida
  RETURN (X.CurrentSubarea IS NOT NULL) AND 
         (X.CurrentArea IS NOT NULL) AND
         (X.CurrentUO IS NOT NULL)
END FUNCTION
```

**Propriedade: Fix Checking**
```pascal
// Propriedade: Verificação de Correção - Filtro de Subárea
FOR ALL X WHERE isBugCondition(X) DO
  bens ← LoadPatrimonio'(X)
  ASSERT ALL(bens, bem => 
    bem.CdUnid = X.CurrentUO.IdUO AND
    bem.CdArea = X.CurrentArea.IdArea AND
    bem.CdSArea = X.CurrentSubarea.IdSubarea
  )
END FOR
```

**Propriedade: Preservation Checking**
```pascal
// Propriedade: Verificação de Preservação - Comportamento Legado
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT LoadPatrimonio(X) = LoadPatrimonio'(X)
END FOR
```

Onde:
- **F (LoadPatrimonio)**: Função original que carrega bens apenas por UO
- **F' (LoadPatrimonio')**: Função corrigida que carrega bens por UO + Área + Subárea quando disponíveis
- **X**: Configuração de sessão contendo órgão, unidade, área e subárea selecionados

**Contraexemplo Concreto:**

```
Entrada:
  SessionConfig {
    CurrentOrgao: { IdOrgao: "09", NomeOrgao: "Secretaria de Cultura" }
    CurrentUO: { IdUO: "09", NomeUO: "Secretaria de Cultura" }
    CurrentArea: { IdArea: "001", NomeArea: "Deposito" }
    CurrentSubarea: { IdSubarea: "001", NomeSubarea: "Deposito Principal" }
  }

Comportamento Atual (Buggy):
  GetPatrimonioByUOAsync("09") retorna:
    - 150 bens da UO "09" (todas as áreas e subáreas)
    - Inclui bens de "Área 002 - Almoxarifado"
    - Inclui bens de "Subárea 002 - Depósito Secundário"

Comportamento Esperado (Fixed):
  GetPatrimonioBySubareaAsync("09", "001", "001") retorna:
    - 45 bens da UO "09", Área "001", Subárea "001"
    - Exclui bens de outras áreas/subáreas
    - Apenas bens do "Deposito Principal"
```
