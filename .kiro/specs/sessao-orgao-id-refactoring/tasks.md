# Plano de Implementação: Refatoração de Busca de Órgãos por IdOrgao

## Visão Geral

Esta refatoração simplifica a interface de seleção de sessão no componente `ConfiguracaoSessao.razor`, removendo o dropdown "Ano de Exercício" e a dependência do campo `DtEstr` como critério de filtragem. A hierarquia de seleção passa de 5 níveis (Ano → Órgão → UO → Área → Subárea) para 4 níveis (Órgão → UO → Área → Subárea), mantendo apenas a filtragem por esfera de acesso.

## Tarefas

- [x] 1. Remover estado e lógica relacionados ao Ano de Exercício
  - [x] 1.1 Remover propriedades de estado relacionadas ao exercício fiscal
    - Remover `AnosExercicio` (List<int>)
    - Remover `SelectedAnoExercicio` (int?)
    - Remover lógica de extração de anos em `OnInitializedAsync`
    - _Requisitos: 3.1, 3.3_
  
  - [x] 1.2 Remover event handler `OnAnoExercicioChanged`
    - Deletar método completo
    - _Requisitos: 3.2_
  
  - [x] 1.3 Simplificar propriedade computada de filtragem de órgãos
    - Renomear `OrgaosFiltrados` para `Orgaos`
    - Remover filtro por `DtEstr`
    - Implementar filtro apenas por esfera (E, L, A)
    - Usar `appState.EsferaAtual ?? _usuario.Esfera` como fonte de esfera
    - _Requisitos: 2.1, 2.2, 2.3, 2.4, 8.1, 8.2, 8.3, 8.4_

- [x] 2. Atualizar markup Razor da interface
  - [x] 2.1 Remover dropdown "Ano de Exercício" do formulário
    - Deletar `<MudItem>` completo que contém o `MudSelect` de ano
    - _Requisitos: 1.1, 9.1, 9.2_
  
  - [x] 2.2 Modificar dropdown de Órgão
    - Remover atributo `Disabled="@(SelectedAnoExercicio == null)"`
    - Alterar referência de `OrgaosFiltrados` para `Orgaos`
    - _Requisitos: 1.2, 1.3, 4.1_
  
  - [x] 2.3 Atualizar condição de habilitação do botão "Confirmar Seleção"
    - Remover verificação `SelectedAnoExercicio == null` da condição `Disabled`
    - Manter apenas `SelectedSubarea == null`
    - _Requisitos: 4.5_

- [x] 3. Remover persistência de campos legados
  - [x] 3.1 Atualizar método `ConfirmarSessao`
    - Remover linha `appState.DtEstr = SelectedOrgao.DtEstr;`
    - Remover linha `appState.AnoExercicio = SelectedOrgao.DtEstr / 10000;`
    - Manter população de outros campos (OrgaoId, OrgaoName, etc.)
    - _Requisitos: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 4. Checkpoint - Verificar compilação e testes básicos
  - Executar build do projeto para verificar erros de compilação
  - Testar fluxo básico de seleção manualmente
  - Verificar que não há referências a "Ano de Exercício" na UI
  - Perguntar ao usuário se há dúvidas ou problemas

- [ ]* 5. Implementar testes unitários para casos específicos
  - [ ]* 5.1 Testar filtragem por esfera "A" (todos os órgãos)
    - Criar usuário com esfera "A"
    - Verificar que `Orgaos` retorna todos os órgãos do usuário
    - _Requisitos: 2.3, 8.2_
  
  - [ ]* 5.2 Testar filtragem por esfera "E" (Executivo)
    - Criar usuário com esfera "E"
    - Verificar que `Orgaos` retorna apenas órgãos com subareas contendo "Executivo"
    - _Requisitos: 2.4, 8.3_
  
  - [ ]* 5.3 Testar filtragem por esfera "L" (Legislativo)
    - Criar usuário com esfera "L"
    - Verificar que `Orgaos` retorna apenas órgãos com subareas contendo "Legislativo"
    - _Requisitos: 2.4, 8.4_
  
  - [ ]* 5.4 Testar compatibilidade com DtEstr = 0
    - Criar órgão com `DtEstr = 0`
    - Verificar que órgão aparece na lista sem exceções
    - _Requisitos: 6.1, 6.4_
  
  - [ ]* 5.5 Testar redirecionamento para login quando não autenticado
    - Simular usuário não autenticado
    - Verificar redirecionamento para "/login"
    - _Requisitos: 10.2_
  
  - [ ]* 5.6 Testar reset de campos dependentes
    - Selecionar hierarquia completa
    - Mudar Órgão e verificar reset de UO, Área, Subárea
    - Mudar UO e verificar reset de Área, Subárea
    - Mudar Área e verificar reset de Subárea
    - _Requisitos: 7.1, 7.2, 7.3_

- [ ]* 6. Implementar testes baseados em propriedades (Property-Based Tests)
  - [ ]* 6.1 Teste de propriedade: Filtragem por esfera aplica-se a todos os usuários
    - **Property 1: Sphere filtering applies to all users**
    - **Valida: Requisitos 2.1, 2.4, 8.3, 8.4**
    - Gerar usuários aleatórios com esferas E/L
    - Verificar que todos os órgãos retornados têm subareas correspondentes
    - Usar CsCheck ou FsCheck com mínimo 100 iterações
  
  - [ ]* 6.2 Teste de propriedade: Esfera "A" mostra todos os órgãos
    - **Property 2: Sphere "A" shows all organs**
    - **Valida: Requisitos 2.3, 8.2**
    - Gerar usuários aleatórios com esfera "A"
    - Verificar que todos os órgãos do usuário são retornados
  
  - [ ]* 6.3 Teste de propriedade: Cascata de dropdowns habilita corretamente
    - **Property 3: Dropdown cascade enables correctly**
    - **Valida: Requisitos 4.2, 4.3, 4.4**
    - Gerar estados de seleção aleatórios
    - Verificar que dropdowns são habilitados conforme a cascata
  
  - [ ]* 6.4 Teste de propriedade: Botão confirmar requer todas as seleções
    - **Property 4: Confirm button requires all selections**
    - **Valida: Requisitos 4.5**
    - Gerar combinações aleatórias de seleções
    - Verificar que botão só está habilitado quando todos os campos estão preenchidos
  
  - [ ]* 6.5 Teste de propriedade: Campos legados não são populados
    - **Property 5: Legacy fields not populated on confirmation**
    - **Valida: Requisitos 5.1, 5.2, 5.3, 5.4**
    - Gerar seleções aleatórias válidas
    - Confirmar sessão e verificar que AnoExercicio e DtEstr permanecem 0
  
  - [ ]* 6.6 Teste de propriedade: Campos de sessão corretamente populados
    - **Property 6: Session fields correctly populated**
    - **Valida: Requisitos 5.5**
    - Gerar seleções aleatórias válidas
    - Confirmar sessão e verificar que todos os campos são populados corretamente
  
  - [ ]* 6.7 Teste de propriedade: Todos os órgãos exibidos independente de DtEstr
    - **Property 7: All organs displayed regardless of DtEstr**
    - **Valida: Requisitos 6.1**
    - Gerar órgãos com valores aleatórios de DtEstr (incluindo 0)
    - Verificar que todos aparecem na lista (sujeito a filtragem por esfera)
  
  - [ ]* 6.8 Teste de propriedade: Seleção reseta campos dependentes
    - **Property 8: Selection resets dependent fields**
    - **Valida: Requisitos 7.1, 7.2, 7.3**
    - Gerar mudanças aleatórias de seleção
    - Verificar que campos dependentes são resetados
  
  - [ ]* 6.9 Teste de propriedade: Filtragem de UO por esfera é consistente
    - **Property 9: UO sphere filtering matches organ filtering**
    - **Valida: Requisitos 8.5**
    - Gerar órgãos e UOs aleatórios
    - Verificar que filtragem por esfera é consistente entre órgãos e UOs

- [ ]* 7. Implementar testes de componente Blazor (bUnit)
  - [ ]* 7.1 Testar renderização inicial do componente
    - Verificar que dropdown de Órgão está presente e habilitado
    - Verificar que dropdown de "Ano de Exercício" NÃO está presente
    - Verificar que outros dropdowns estão desabilitados inicialmente
    - _Requisitos: 1.1, 4.1_
  
  - [ ]* 7.2 Testar interações do usuário com dropdowns
    - Simular seleção de Órgão → verificar que UO é habilitado
    - Simular seleção de UO → verificar que Área é habilitado
    - Simular seleção de Área → verificar que Subárea é habilitado
    - Simular seleção de Subárea → verificar que botão é habilitado
    - _Requisitos: 4.2, 4.3, 4.4, 4.5_
  
  - [ ]* 7.3 Testar reset de campos ao mudar seleção
    - Selecionar hierarquia completa
    - Mudar Órgão → verificar que UO, Área, Subárea são resetados
    - Mudar UO → verificar que Área, Subárea são resetados
    - Mudar Área → verificar que Subárea é resetado
    - _Requisitos: 7.1, 7.2, 7.3_

- [x] 8. Checkpoint final - Validação completa
  - Executar todos os testes (unitários, propriedades, componente)
  - Verificar cobertura de código (mínimo 80% linhas, 75% branches)
  - Testar fluxo completo manualmente em navegador
  - Testar com diferentes esferas (A, E, L)
  - Testar com dados legados (DtEstr = 0)
  - Verificar que não há regressões em funcionalidades existentes
  - Perguntar ao usuário se há dúvidas ou se está pronto para deploy

## Notas

- Tarefas marcadas com `*` são opcionais e podem ser puladas para um MVP mais rápido
- Cada tarefa referencia requisitos específicos para rastreabilidade
- Checkpoints garantem validação incremental
- Testes de propriedades validam propriedades universais de correção
- Testes unitários validam exemplos específicos e casos extremos
- A refatoração mantém campos legados (`AnoExercicio`, `DtEstr`) nas classes de modelo para compatibilidade, mas não os popula mais
