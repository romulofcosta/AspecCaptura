# Plano de Implementação: orgao-exercicio-refactoring

## Visão Geral

Refatoração incremental para introduzir o campo `dtestr` (exercício fiscal) nos modelos de órgão da API e do cliente Blazor, adotando a chave composta `(DtEstr, CdOrgao)` na hierarquia e expondo o "Ano de Exercício" como primeiro nível de seleção no formulário de sessão.

## Tarefas

- [x] 1. Adicionar `DtEstr` ao `XxOrgaRecord` na API
  - Incluir a propriedade `int DtEstr` com `[JsonPropertyName("dtestr")]` no record posicional `XxOrgaRecord` em `pwa-camera-poc-api/Models/ApiModels.cs`
  - Garantir que a ausência do campo no JSON resulte em `DtEstr = 0` (comportamento padrão do desserializador)
  - _Requisitos: 1.1, 1.2, 1.3_

  - [ ]* 1.1 Escrever teste de propriedade para round-trip de `XxOrgaRecord`
    - **Propriedade 1: Round-trip de desserialização do XxOrgaRecord**
    - **Valida: Requisitos 1.1, 1.2, 10.1**
    - Usar FsCheck; gerar `int` arbitrário para `DtEstr`, strings para `CdOrgao`/`NmOrgao`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 1`

  - [ ]* 1.2 Escrever teste unitário para `XxOrgaRecord` sem `dtestr`
    - Verificar que JSON sem campo `dtestr` resulta em `DtEstr == 0`
    - _Requisitos: 1.3_

- [x] 2. Adicionar `DtEstr` ao `OrgaoRecord` na API
  - Incluir a propriedade `int DtEstr` no record posicional `OrgaoRecord` em `pwa-camera-poc-api/Models/ApiModels.cs`
  - _Requisitos: 3.1, 3.3_

  - [ ]* 2.1 Escrever teste de propriedade para round-trip de `OrgaoRecord`
    - **Propriedade 4: Round-trip de OrgaoRecord preserva DtEstr**
    - **Valida: Requisitos 3.1, 3.2, 3.3, 10.2**
    - Usar FsCheck; gerar `XxOrgaRecord` com `DtEstr` arbitrário
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 4`

- [x] 3. Atualizar `BuildOrgaoHierarchy` para usar chave composta `(DtEstr, CdOrgao)`
  - Modificar `BuildOrgaoHierarchy` em `pwa-camera-poc-api/Program.cs` para:
    - Esfera `"A"`: extrair chaves `(DtEstr, CdOrgao)` diretamente de `tabelas.XxOrga`
    - Outras esferas: obter `DtEstr` via `Join` entre tombamentos filtrados e `tabelas.XxOrga`
  - Atualizar o lookup de `orgInfo` para usar `o.DtEstr == dtEstr && o.CdOrgao == orgCode`
  - Atualizar a criação de `OrgaoRecord` para incluir `dtEstr` como último argumento
  - _Requisitos: 2.1, 2.2, 2.3, 3.2_

  - [ ]* 3.1 Escrever teste de propriedade: chave composta distingue órgãos por exercício
    - **Propriedade 2: Chave composta distingue órgãos por exercício**
    - **Valida: Requisitos 2.1, 2.2**
    - Gerar dois `XxOrgaRecord` com mesmo `CdOrgao` e `DtEstr` distintos; verificar dois `OrgaoRecord` na saída
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 2`

  - [ ]* 3.2 Escrever teste de propriedade: filtragem por esfera é preservada
    - **Propriedade 3: Filtragem por esfera é preservada**
    - **Valida: Requisito 2.3**
    - Gerar esfera aleatória (não `"A"`) e lista de tombamentos; verificar que todos os `OrgaoRecord` retornados têm `IdOrgao` presente nos tombamentos
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 3`

  - [ ]* 3.3 Escrever teste unitário: JSON legado sem `dtestr` constrói hierarquia normalmente
    - Verificar que `BuildOrgaoHierarchy` com `DtEstr = 0` em todos os registros não lança exceção e retorna hierarquia válida
    - _Requisitos: 9.1_

- [x] 4. Checkpoint — Garantir que todos os testes da API passem
  - Garantir que todos os testes passem; perguntar ao usuário se houver dúvidas.

- [x] 5. Adicionar `DtEstr` à classe `Orgao` no cliente
  - Incluir a propriedade `public int DtEstr { get; set; } = 0;` na classe `Orgao` em `pwa-camera-poc-blazor/Models/Usuario.cs`
  - Manter todas as propriedades existentes sem alteração
  - _Requisitos: 4.1, 4.3_

  - [ ]* 5.1 Escrever teste de propriedade: mapeamento AuthService preserva DtEstr
    - **Propriedade 5: Mapeamento AuthService preserva DtEstr no Orgao do cliente**
    - **Valida: Requisitos 4.2, 10.2**
    - Gerar lista de `OrgaoRecord` com `DtEstr` arbitrários; verificar que `Orgao.DtEstr` é igual ao valor recebido da API
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 5`

- [x] 6. Adicionar `AnoExercicio` e `DtEstr` ao `SessionConfig`
  - Incluir as propriedades `public int AnoExercicio { get; set; } = 0;` e `public int DtEstr { get; set; } = 0;` em `pwa-camera-poc-blazor/Models/SessionConfig.cs`
  - Manter todas as propriedades existentes sem alteração
  - _Requisitos: 5.1, 5.2, 5.3_

  - [ ]* 6.1 Escrever teste de propriedade: round-trip de persistência do `SessionConfig`
    - **Propriedade 14: Round-trip de persistência do SessionConfig**
    - **Valida: Requisito 10.3**
    - Gerar `SessionConfig` com `AnoExercicio` e `DtEstr` arbitrários; serializar e desserializar; verificar igualdade
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 14`

- [x] 7. Atualizar `ConfiguracaoSessao.razor` — adicionar dropdown "Ano de Exercício"
  - Adicionar campos de estado `AnosExercicio` e `SelectedAnoExercicio` na seção `@code`
  - Em `OnInitializedAsync`, extrair anos distintos: `_usuario.Orgaos.Select(o => o.DtEstr / 10000).Distinct().OrderByDescending(a => a).ToList()`
  - Adicionar propriedade computada `OrgaosFiltrados` que filtra por `DtEstr / 10000 == SelectedAnoExercicio`
  - Adicionar handler `OnAnoExercicioChanged` que reseta `SelectedOrgao`, `SelectedUO`, `SelectedArea` e `SelectedSubarea` para `null`
  - Inserir `MudSelect` de "Ano de Exercício" como primeiro campo do formulário, antes do dropdown "Órgão"
  - Atualizar o dropdown "Órgão" para usar `OrgaosFiltrados` e `Disabled="@(SelectedAnoExercicio == null)"`
  - Atualizar o botão "Confirmar Seleção" para incluir `SelectedAnoExercicio == null` na condição `Disabled`
  - _Requisitos: 6.1, 6.2, 6.3, 6.4, 7.1, 7.2, 7.3, 7.4, 7.5, 7.6_

  - [ ]* 7.1 Escrever teste de propriedade: extração de anos distintos é completa e sem duplicatas
    - **Propriedade 6: Extração de anos distintos é completa e sem duplicatas**
    - **Valida: Requisito 6.1**
    - Gerar lista de `Orgao` com `DtEstr` variados; verificar que `AnosExercicio` contém exatamente os valores distintos de `DtEstr / 10000`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 6`

  - [ ]* 7.2 Escrever teste de propriedade: derivação de `AnoExercicio` é correta para qualquer `DtEstr`
    - **Propriedade 7: Derivação de AnoExercicio é correta para qualquer DtEstr**
    - **Valida: Requisito 6.2**
    - Gerar `int` positivo no formato YYYYMMDD; verificar `AnoExercicio == DtEstr / 10000`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 7`

  - [ ]* 7.3 Escrever teste de propriedade: lista de anos está em ordem decrescente
    - **Propriedade 8: Lista de anos está em ordem decrescente**
    - **Valida: Requisito 6.3**
    - Gerar lista de `Orgao` com `DtEstr` distintos; verificar que cada elemento é maior ou igual ao próximo
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 8`

  - [ ]* 7.4 Escrever teste de propriedade: filtragem de órgãos por exercício é precisa
    - **Propriedade 9: Filtragem de órgãos por exercício é precisa**
    - **Valida: Requisito 7.2**
    - Gerar `AnoExercicio` arbitrário e lista mista de `Orgao`; verificar que `OrgaosFiltrados` contém apenas órgãos com `DtEstr / 10000 == AnoExercicio`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 9`

  - [ ]* 7.5 Escrever teste de propriedade: mudança de exercício reseta campos dependentes
    - **Propriedade 10: Mudança de exercício reseta campos dependentes**
    - **Valida: Requisito 7.3**
    - Gerar estado de formulário com seleções arbitrárias; invocar `OnAnoExercicioChanged`; verificar que `SelectedOrgao`, `SelectedUO`, `SelectedArea` e `SelectedSubarea` são `null`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 10`

  - [ ]* 7.6 Escrever teste de propriedade: campos desabilitados sem exercício selecionado
    - **Propriedade 11: Sem exercício selecionado, campos dependentes ficam desabilitados**
    - **Valida: Requisitos 7.4, 7.5**
    - Verificar que com `SelectedAnoExercicio == null`, o dropdown de Órgão e o botão Confirmar estão desabilitados
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 11`

- [x] 8. Atualizar `ConfirmarSessao` para persistir `AnoExercicio` e `DtEstr`
  - Modificar o método `ConfirmarSessao` em `ConfiguracaoSessao.razor` para preencher `SessionConfig.AnoExercicio` e `SessionConfig.DtEstr` a partir do `SelectedOrgao.DtEstr`
  - Manter o preenchimento existente de todos os demais campos do `SessionConfig`
  - _Requisitos: 8.1, 8.2, 8.3_

  - [ ]* 8.1 Escrever teste de propriedade: confirmação da sessão preenche `AnoExercicio` e `DtEstr` corretamente
    - **Propriedade 12: Confirmação da sessão preenche AnoExercicio e DtEstr corretamente**
    - **Valida: Requisitos 8.1, 8.2, 8.3**
    - Gerar `Orgao` com `DtEstr` arbitrário e hierarquia completa; verificar `SessionConfig.AnoExercicio == DtEstr / 10000` e `SessionConfig.DtEstr == DtEstr`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 12`

- [x] 9. Verificar compatibilidade retroativa com dados sem `dtestr`
  - Escrever teste de integração que simula o fluxo completo com JSON sem campo `dtestr` em `xxorga`
  - Verificar que nenhuma exceção é lançada e que `DtEstr = 0` aparece como único ano disponível
  - _Requisitos: 9.1, 9.2, 9.3_

  - [ ]* 9.1 Escrever teste de propriedade: ausência de `dtestr` não causa exceção
    - **Propriedade 13: Ausência de dtestr não causa exceção**
    - **Valida: Requisitos 9.1, 9.2, 9.3**
    - Gerar JSON sem campo `dtestr`; verificar que o fluxo completo de login e configuração de sessão é concluído sem exceções com `DtEstr = 0`
    - Tag: `// Feature: orgao-exercicio-refactoring, Property 13`

- [x] 10. Checkpoint final — Garantir que todos os testes passem
  - Garantir que todos os testes passem; perguntar ao usuário se houver dúvidas.

## Notas

- Tarefas marcadas com `*` são opcionais e podem ser puladas para um MVP mais rápido
- Cada tarefa referencia requisitos específicos para rastreabilidade
- Os testes de propriedade usam FsCheck (.NET); configuração mínima de 100 iterações por propriedade
- Tag de referência nos testes: `// Feature: orgao-exercicio-refactoring, Property {N}: {texto}`
- A propagação do `DtEstr` pelo `AuthService` ocorre automaticamente via desserialização, sem alteração de código adicional no serviço
