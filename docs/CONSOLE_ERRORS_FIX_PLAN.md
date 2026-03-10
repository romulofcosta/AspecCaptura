# Plano de Correção de Erros de Console - Aspec Captura

## Análise Realizada

### 1. Problemas Identificados

#### Login.razor
- ✅ LoginModel inicializado corretamente com `new()`
- ✅ Tratamento de nulidade com `result.Success && result.User != null`
- ✅ Operador seguro `?.` usado em `appState.CurrentUser?.NomeCompleto`
- ✅ Try/catch implementado em HandleLogin

#### Settings.razor
- ⚠️ `appState.CurrentUser?.NomeCompleto` pode retornar null
- ⚠️ `appState.CurrentUser?.UsuarioNome` pode retornar null
- ⚠️ Sem verificação de inicialização em OnInitialized
- ⚠️ Sem carregamento de dados do usuário em OnInitializedAsync

#### AuthService.cs
- ✅ Tratamento de exceções implementado
- ✅ Validação de entrada (username/password)
- ✅ Brute force protection
- ✅ Logging de erros com Console.Error.WriteLine

#### AppState.cs
- ✅ Inicialização de propriedades com valores padrão
- ✅ Implementação de INotifyPropertyChanged
- ✅ Métodos de limpeza de estado

### 2. Problemas Críticos a Resolver

1. **Settings.razor**: Falta carregamento de dados do usuário em OnInitializedAsync
2. **Settings.razor**: Versão hardcoded como "1.0.0" em vez de usar a versão do build
3. **Modelos**: Falta inicialização em alguns cenários
4. **Logging**: Falta logging estruturado com ILogger

## Plano de Ação

### Fase 1: Corrigir Settings.razor
- [ ] Adicionar OnInitializedAsync para carregar dados do usuário
- [ ] Adicionar verificações de nulidade com operadores seguros
- [ ] Usar versão dinâmica do meta tag
- [ ] Adicionar tratamento de erros

### Fase 2: Melhorar AuthService
- [ ] Adicionar logging estruturado com ILogger
- [ ] Melhorar tratamento de exceções
- [ ] Adicionar validação de estado

### Fase 3: Adicionar ILogger ao Program.cs
- [ ] Registrar ILogger como serviço
- [ ] Usar logging em todos os serviços críticos

### Fase 4: Testes
- [ ] Testar login com credenciais válidas
- [ ] Testar login com credenciais inválidas
- [ ] Testar Settings sem dados carregados
- [ ] Verificar console para erros

## Checklist Final
- [ ] Nenhum NullReferenceException
- [ ] Nenhum erro de inicialização
- [ ] Nenhum erro de renderização
- [ ] Logging claro para depuração
- [ ] Experiência do usuário estável
