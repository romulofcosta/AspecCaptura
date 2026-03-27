# Documento de Requisitos

## Introdução

Esta especificação descreve a refatoração da lógica de busca de órgãos na tela de Configuração de Sessão (ConfiguracaoSessao.razor). Atualmente, o sistema utiliza uma chave composta `(DtEstr, cdorgao)` para filtrar órgãos por exercício fiscal. A refatoração simplifica essa lógica para usar apenas o `IdOrgao` como campo de busca, removendo a dependência do dropdown "Ano de Exercício" e a filtragem por `DtEstr`.

Esta mudança visa simplificar a interface do usuário e a lógica de negócio, eliminando a complexidade introduzida pela spec `orgao-exercicio-refactoring` que adicionou o conceito de exercício fiscal como primeiro nível de seleção.

## Glossário

- **ConfiguracaoSessao**: Página Razor no Cliente que exibe o formulário de seleção de sessão (Órgão, Unidade Orçamentária, Área, Subárea)
- **Orgao**: Classe C# no Cliente que representa um órgão recebido da API
- **AppState**: Serviço global que armazena o estado da aplicação, incluindo a sessão atual
- **SessionConfig**: Classe C# no Cliente que armazena a configuração de sessão selecionada pelo usuário
- **IdOrgao**: Identificador único do órgão (string), usado como chave de busca
- **DtEstr**: Campo inteiro no formato YYYYMMDD que representa o início do exercício fiscal (será removido da lógica de filtragem)
- **AnoExercicio**: Ano extraído de `DtEstr` (será removido da interface)
- **Hierarquia de Sessão**: Sequência de seleção simplificada: Órgão → UO → Área → Subárea
- **OrgaosFiltrados**: Lista de órgãos filtrados por esfera, sem considerar exercício fiscal

---

## Requisitos

### Requisito 1: Remover dropdown "Ano de Exercício" da interface

**User Story:** Como usuário do sistema, quero selecionar o órgão diretamente sem precisar escolher o ano de exercício primeiro, para que o processo de configuração seja mais rápido e simples.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL remove the "Ano de Exercício" dropdown from the form
2. THE ConfiguracaoSessao SHALL display the "Órgão" dropdown as the first field in the form
3. THE ConfiguracaoSessao SHALL enable the "Órgão" dropdown immediately after page load, without requiring prior selection
4. THE ConfiguracaoSessao SHALL maintain the existing hierarchy: Órgão → UO → Área → Subárea

---

### Requisito 2: Simplificar filtragem de órgãos

**User Story:** Como desenvolvedor, quero que a lista de órgãos seja filtrada apenas por esfera, para que a lógica de negócio seja mais simples e manutenível.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL populate the "Órgão" dropdown with all organs from the authenticated user, filtered only by sphere (Esfera)
2. THE ConfiguracaoSessao SHALL remove the filter by `DtEstr` from the `OrgaosFiltrados` property
3. WHEN the user has sphere "A" (All), THE ConfiguracaoSessao SHALL display all organs without sphere filtering
4. WHEN the user has sphere "E" (Executive) or "L" (Legislative), THE ConfiguracaoSessao SHALL filter organs by the corresponding sphere
5. THE ConfiguracaoSessao SHALL remove the `AnosExercicio` list property and related logic

---

### Requisito 3: Remover estado de seleção de ano de exercício

**User Story:** Como desenvolvedor, quero remover o estado `SelectedAnoExercicio` do componente, para que o código seja mais limpo e não mantenha dados desnecessários.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL remove the `SelectedAnoExercicio` property from the component state
2. THE ConfiguracaoSessao SHALL remove the `OnAnoExercicioChanged` event handler
3. THE ConfiguracaoSessao SHALL remove the logic that extracts distinct years from `DtEstr` in `OnInitializedAsync`
4. THE ConfiguracaoSessao SHALL maintain the existing event handlers for Órgão, UO, Área, and Subárea selection

---

### Requisito 4: Simplificar habilitação de campos do formulário

**User Story:** Como usuário, quero que os campos do formulário sejam habilitados de forma mais direta, para que a navegação seja mais intuitiva.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL enable the "Órgão" dropdown immediately after page initialization
2. THE ConfiguracaoSessao SHALL enable the "Unidade Orçamentária" dropdown when an Órgão is selected
3. THE ConfiguracaoSessao SHALL enable the "Área" dropdown when a Unidade Orçamentária is selected
4. THE ConfiguracaoSessao SHALL enable the "Subárea" dropdown when an Área is selected
5. THE ConfiguracaoSessao SHALL enable the "Confirmar Seleção" button only when all required fields (Órgão, UO, Área, Subárea) are selected

---

### Requisito 5: Remover persistência de ano de exercício na sessão

**User Story:** Como desenvolvedor, quero que o `SessionConfig` e `AppState` não armazenem mais `AnoExercicio` e `DtEstr`, para que o modelo de dados seja mais simples.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL not populate `SessionConfig.AnoExercicio` when confirming the session
2. THE ConfiguracaoSessao SHALL not populate `SessionConfig.DtEstr` when confirming the session
3. THE ConfiguracaoSessao SHALL not populate `AppState.AnoExercicio` when confirming the session
4. THE ConfiguracaoSessao SHALL not populate `AppState.DtEstr` when confirming the session
5. THE ConfiguracaoSessao SHALL maintain the existing population of `OrgaoId`, `OrgaoName`, `UnidadeId`, `UnidadeName`, `AreaId`, `AreaName`, `SubareaId`, and `SubareaName`

---

### Requisito 6: Manter compatibilidade com dados existentes

**User Story:** Como operador do sistema, quero que a refatoração não quebre o comportamento para usuários que já possuem órgãos com `DtEstr` preenchido, para que a transição seja transparente.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL display all organs regardless of their `DtEstr` value
2. WHEN multiple organs have the same `IdOrgao` but different `DtEstr` values, THE ConfiguracaoSessao SHALL display all of them in the dropdown
3. THE ConfiguracaoSessao SHALL use `IdOrgao` as the unique identifier for organ selection
4. THE ConfiguracaoSessao SHALL not throw exceptions when `DtEstr` is 0 or absent

---

### Requisito 7: Simplificar lógica de reset de campos dependentes

**User Story:** Como desenvolvedor, quero que a lógica de reset de campos dependentes seja mais simples, para que o código seja mais fácil de entender e manter.

#### Critérios de Aceitação

1. WHEN the user selects an Órgão, THE ConfiguracaoSessao SHALL reset UO, Área, and Subárea to null
2. WHEN the user selects a UO, THE ConfiguracaoSessao SHALL reset Área and Subárea to null
3. WHEN the user selects an Área, THE ConfiguracaoSessao SHALL reset Subárea to null
4. THE ConfiguracaoSessao SHALL not reset any fields when the page is initialized
5. THE ConfiguracaoSessao SHALL remove the reset logic related to `SelectedAnoExercicio`

---

### Requisito 8: Manter filtragem por esfera

**User Story:** Como usuário com acesso restrito a uma esfera específica, quero que o sistema continue filtrando órgãos pela minha esfera, para que eu veja apenas os dados relevantes.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL maintain the existing sphere filtering logic in the `Orgaos` property
2. WHEN `appState.EsferaAtual` is "A" or null, THE ConfiguracaoSessao SHALL display all organs from the user
3. WHEN `appState.EsferaAtual` is "E", THE ConfiguracaoSessao SHALL filter organs to show only those with "Executivo" in subarea names
4. WHEN `appState.EsferaAtual` is "L", THE ConfiguracaoSessao SHALL filter organs to show only those with "Legislativo" in subarea names
5. THE ConfiguracaoSessao SHALL apply the same sphere filtering to Unidades Orçamentárias

---

### Requisito 9: Remover referências a exercício fiscal na UI

**User Story:** Como usuário, quero que a interface não mencione "Ano de Exercício", para que a experiência seja consistente com a nova lógica simplificada.

#### Critérios de Aceitação

1. THE ConfiguracaoSessao SHALL remove all UI text references to "Ano de Exercício"
2. THE ConfiguracaoSessao SHALL remove all UI text references to "Exercício Fiscal"
3. THE ConfiguracaoSessao SHALL maintain the existing labels for Órgão, Unidade Orçamentária, Área, and Subárea
4. THE ConfiguracaoSessao SHALL maintain the existing title "Configuração da Sessão"
5. THE ConfiguracaoSessao SHALL maintain the existing subtitle "Selecione as informações contábeis para iniciar o trabalho"

---

### Requisito 10: Garantir inicialização correta do estado

**User Story:** Como desenvolvedor, quero que o estado inicial do componente seja consistente, para que não haja erros de referência nula ou comportamento inesperado.

#### Critérios de Aceitação

1. WHEN the page is initialized, THE ConfiguracaoSessao SHALL load the authenticated user from `AuthService`
2. IF the user is not authenticated, THEN THE ConfiguracaoSessao SHALL redirect to "/login"
3. WHEN the user is authenticated, THE ConfiguracaoSessao SHALL ensure `appState.EsferaAtual` is populated with the user's sphere if it's null
4. THE ConfiguracaoSessao SHALL call `StateHasChanged()` after loading user data to force UI update
5. THE ConfiguracaoSessao SHALL initialize all selection properties (SelectedOrgao, SelectedUO, SelectedArea, SelectedSubarea) as null

