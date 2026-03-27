# Documento de Requisitos

## Introdução

Esta especificação descreve a refatoração do modelo de órgãos no sistema de inventário patrimonial municipal (PWA Blazor + API). O problema central é que o campo `dtestr` (data de início do exercício fiscal, formato YYYYMMDD) está presente no JSON de dados (`xxorga`) mas é ignorado pelo modelo `XxOrgaRecord` e pela função `BuildOrgaoHierarchy`. Isso causa ambiguidade quando o mesmo `cdorgao` aparece em múltiplos exercícios fiscais com nomes distintos.

A refatoração introduz o conceito de **Ano de Exercício** como primeiro nível de seleção no formulário de sessão, tornando a chave de identificação de órgãos composta por `(dtestr, cdorgao)` e propagando essa informação até o cliente Blazor.

## Glossário

- **Sistema**: O conjunto formado pela API (`pwa-camera-poc-api`) e pelo cliente Blazor (`pwa-camera-poc-blazor`)
- **API**: O serviço backend `pwa-camera-poc-api` responsável por ler o JSON do S3 e expor endpoints REST
- **Cliente**: A aplicação frontend Blazor PWA `pwa-camera-poc-blazor`
- **XxOrgaRecord**: Record C# na API que representa uma linha da tabela `xxorga` do JSON
- **OrgaoRecord**: Record C# na API que representa um órgão na resposta hierárquica do endpoint `/api/auth/login`
- **Orgao**: Classe C# no Cliente que representa um órgão recebido da API
- **SessionConfig**: Classe C# no Cliente que armazena a configuração de sessão selecionada pelo usuário
- **BuildOrgaoHierarchy**: Função estática na API que monta a hierarquia Órgão → UO → Área → Subárea
- **ConfiguracaoSessao**: Página Razor no Cliente que exibe o formulário de seleção de sessão
- **dtestr**: Campo inteiro no formato YYYYMMDD que representa o início do exercício fiscal (ex: `19970101` = exercício 1997)
- **AnoExercicio**: Representação do ano extraído de `dtestr` (ex: `dtestr = 19970101` → `AnoExercicio = 1997`)
- **Chave Composta**: Par `(dtestr, cdorgao)` que identifica unicamente um órgão dentro de um exercício fiscal
- **Exercício Fiscal**: Período anual de gestão orçamentária identificado pelo ano extraído de `dtestr`
- **Hierarquia de Sessão**: Sequência de seleção: Ano de Exercício → Órgão → UO → Área → Subárea

---

## Requisitos

### Requisito 1: Inclusão do campo `dtestr` no modelo de órgão da API

**User Story:** Como desenvolvedor da API, quero que o modelo `XxOrgaRecord` inclua o campo `dtestr`, para que a chave composta `(dtestr, cdorgao)` possa ser usada na construção da hierarquia de órgãos.

#### Critérios de Aceitação

1. THE `XxOrgaRecord` SHALL incluir uma propriedade inteira `DtEstr` mapeada ao campo JSON `"dtestr"` via `[JsonPropertyName("dtestr")]`
2. WHEN o JSON do S3 contiver entradas em `xxorga` com o campo `dtestr`, THE `XxOrgaRecord` SHALL desserializar o valor corretamente como inteiro
3. IF o campo `dtestr` estiver ausente em uma entrada de `xxorga`, THEN THE `XxOrgaRecord` SHALL assumir o valor padrão `0` para `DtEstr`

---

### Requisito 2: Uso da chave composta em `BuildOrgaoHierarchy`

**User Story:** Como desenvolvedor da API, quero que a função `BuildOrgaoHierarchy` use a chave composta `(dtestr, cdorgao)` para lookup de órgãos, para que órgãos com o mesmo `cdorgao` em exercícios diferentes sejam tratados como entidades distintas.

#### Critérios de Aceitação

1. WHEN `BuildOrgaoHierarchy` for executada, THE `BuildOrgaoHierarchy` SHALL usar o par `(DtEstr, CdOrgao)` como chave de lookup ao buscar informações de órgão em `tabelas.XxOrga`
2. WHEN dois registros em `xxorga` possuírem o mesmo `cdorgao` mas `dtestr` diferentes, THE `BuildOrgaoHierarchy` SHALL tratá-los como órgãos distintos na hierarquia resultante
3. THE `BuildOrgaoHierarchy` SHALL preservar o comportamento atual de filtragem por esfera (`"A"` vs outros valores) ao iterar sobre os códigos de órgão

---

### Requisito 3: Propagação do `dtestr` no `OrgaoRecord` da API

**User Story:** Como desenvolvedor da API, quero que o `OrgaoRecord` retornado pelo endpoint `/api/auth/login` inclua o campo `dtestr`, para que o Cliente possa filtrar órgãos por exercício fiscal.

#### Critérios de Aceitação

1. THE `OrgaoRecord` SHALL incluir uma propriedade inteira `DtEstr` representando o exercício fiscal do órgão
2. WHEN `BuildOrgaoHierarchy` construir um `OrgaoRecord`, THE `BuildOrgaoHierarchy` SHALL preencher `DtEstr` com o valor de `DtEstr` do `XxOrgaRecord` correspondente
3. THE `AuthResponse` SHALL serializar o campo `DtEstr` de cada `OrgaoRecord` na resposta JSON do endpoint `/api/auth/login`

---

### Requisito 4: Inclusão do campo `DtEstr` no modelo `Orgao` do Cliente

**User Story:** Como desenvolvedor do Cliente, quero que a classe `Orgao` inclua o campo `DtEstr`, para que o formulário de sessão possa filtrar órgãos pelo exercício fiscal selecionado.

#### Critérios de Aceitação

1. THE `Orgao` SHALL incluir uma propriedade inteira `DtEstr` com valor padrão `0`
2. WHEN o `AuthService` mapear a `LoginResponse` para o modelo `Usuario`, THE `AuthService` SHALL preencher `DtEstr` em cada `Orgao` com o valor recebido da API
3. THE `Orgao` SHALL manter todas as propriedades existentes (`IdOrgao`, `NomeOrgao`, `UnidadesOrcamentarias`) sem alteração de comportamento

---

### Requisito 5: Inclusão de `AnoExercicio` e `DtEstr` no `SessionConfig`

**User Story:** Como desenvolvedor do Cliente, quero que o `SessionConfig` armazene o ano de exercício selecionado, para que a sessão de inventário seja associada ao exercício fiscal correto.

#### Critérios de Aceitação

1. THE `SessionConfig` SHALL incluir uma propriedade inteira `AnoExercicio` com valor padrão `0`, representando o ano extraído de `dtestr` (ex: `19970101` → `1997`)
2. THE `SessionConfig` SHALL incluir uma propriedade inteira `DtEstr` com valor padrão `0`, armazenando o valor bruto de `dtestr` do órgão selecionado
3. THE `SessionConfig` SHALL manter todas as propriedades existentes sem alteração de comportamento

---

### Requisito 6: Extração dos anos de exercício disponíveis

**User Story:** Como usuário do sistema, quero ver uma lista dos anos de exercício disponíveis antes de selecionar o órgão, para que eu possa filtrar os órgãos pelo exercício fiscal correto.

#### Critérios de Aceitação

1. WHEN a página `ConfiguracaoSessao` for inicializada, THE `ConfiguracaoSessao` SHALL extrair os valores distintos de `DtEstr` presentes nos `Orgaos` do usuário autenticado
2. THE `ConfiguracaoSessao` SHALL derivar o `AnoExercicio` de cada `DtEstr` extraindo os quatro primeiros dígitos (ex: `19970101` → `1997`)
3. THE `ConfiguracaoSessao` SHALL exibir a lista de anos de exercício disponíveis em ordem decrescente
4. IF nenhum órgão estiver disponível para o usuário, THEN THE `ConfiguracaoSessao` SHALL exibir a lista de anos de exercício vazia e manter os demais dropdowns desabilitados

---

### Requisito 7: Novo dropdown "Ano de Exercício" no formulário de sessão

**User Story:** Como usuário do sistema, quero selecionar o Ano de Exercício como primeiro passo no formulário de sessão, para que os órgãos exibidos sejam apenas os do exercício selecionado.

#### Critérios de Aceitação

1. THE `ConfiguracaoSessao` SHALL exibir o dropdown "Ano de Exercício" como primeiro campo do formulário, antes do dropdown "Órgão"
2. WHEN o usuário selecionar um Ano de Exercício, THE `ConfiguracaoSessao` SHALL filtrar a lista de órgãos para exibir apenas os `Orgao` cujo `DtEstr` corresponda ao `dtestr` do exercício selecionado
3. WHEN o usuário selecionar um Ano de Exercício, THE `ConfiguracaoSessao` SHALL redefinir os campos Órgão, UO, Área e Subárea para o estado não selecionado
4. WHILE nenhum Ano de Exercício estiver selecionado, THE `ConfiguracaoSessao` SHALL manter o dropdown "Órgão" desabilitado
5. WHILE nenhum Ano de Exercício estiver selecionado, THE `ConfiguracaoSessao` SHALL manter o botão "Confirmar Seleção" desabilitado
6. THE `ConfiguracaoSessao` SHALL manter a hierarquia de dependência existente: Órgão → UO → Área → Subárea, agora precedida por Ano de Exercício

---

### Requisito 8: Persistência do Ano de Exercício na confirmação da sessão

**User Story:** Como usuário do sistema, quero que o Ano de Exercício selecionado seja salvo na sessão, para que as operações de inventário sejam realizadas no contexto do exercício fiscal correto.

#### Critérios de Aceitação

1. WHEN o usuário confirmar a sessão, THE `ConfiguracaoSessao` SHALL preencher `SessionConfig.AnoExercicio` com o ano derivado do `DtEstr` do órgão selecionado (ex: `19970101` → `1997`)
2. WHEN o usuário confirmar a sessão, THE `ConfiguracaoSessao` SHALL preencher `SessionConfig.DtEstr` com o valor bruto de `DtEstr` do `Orgao` selecionado
3. WHEN o usuário confirmar a sessão, THE `ConfiguracaoSessao` SHALL manter o preenchimento existente dos campos `OrgaoId`, `OrgaoName`, `UnidadeId`, `UnidadeName`, `AreaId`, `AreaName`, `SubareaId` e `SubareaName`

---

### Requisito 9: Compatibilidade retroativa com dados sem `dtestr`

**User Story:** Como operador do sistema, quero que a refatoração não quebre o comportamento para arquivos JSON que não possuam o campo `dtestr` em `xxorga`, para que a transição seja segura e incremental.

#### Critérios de Aceitação

1. IF o campo `dtestr` estiver ausente em todas as entradas de `xxorga` de um arquivo JSON, THEN THE `BuildOrgaoHierarchy` SHALL construir a hierarquia normalmente usando `DtEstr = 0` como valor padrão
2. IF todos os `Orgao` do usuário possuírem `DtEstr = 0`, THEN THE `ConfiguracaoSessao` SHALL exibir um único ano de exercício representado como `"0"` ou equivalente, permitindo a seleção e continuidade do fluxo
3. THE `Sistema` SHALL garantir que nenhuma exceção de referência nula seja lançada durante o fluxo de login e configuração de sessão em decorrência da ausência do campo `dtestr` no JSON

---

### Requisito 10: Consistência do round-trip de serialização dos modelos alterados

**User Story:** Como desenvolvedor do sistema, quero garantir que os modelos alterados (`XxOrgaRecord`, `OrgaoRecord`, `Orgao`, `SessionConfig`) mantenham consistência de serialização/desserialização, para que nenhum dado seja perdido ou corrompido no ciclo API → Cliente.

#### Critérios de Aceitação

1. FOR ALL `XxOrgaRecord` válidos com `dtestr` presente, desserializar o JSON e reserializar SHALL produzir um objeto equivalente (propriedade de round-trip)
2. FOR ALL `OrgaoRecord` retornados pela API com `DtEstr` preenchido, desserializar no Cliente como `Orgao` SHALL preservar o valor de `DtEstr` sem perda
3. THE `SessionConfig` SHALL serializar e desserializar `AnoExercicio` e `DtEstr` corretamente via o mecanismo de persistência existente (`SaveStateAsync`)
