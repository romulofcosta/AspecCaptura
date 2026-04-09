# Plano de Renomeação de Projetos

## 🎯 Objetivo

Renomear os projetos de forma sistemática:
- `pwa-camera-poc-blazor` → `AspecCaptura`
- `pwa-camera-poc-api` → `AspecCapturaApi`

## 📋 Checklist de Renomeação

### Fase 1: Preparação
- [ ] Fazer backup dos projetos
- [ ] Commitar todas as mudanças pendentes
- [ ] Fechar Visual Studio / IDE

### Fase 2: Renomeação de Arquivos (Blazor)
- [ ] `pwa-camera-poc-blazor.csproj` → `AspecCaptura.csproj`
- [ ] `pwa-camera-poc-blazor.sln` → `AspecCaptura.sln`
- [ ] Atualizar `RootNamespace` no .csproj: `pwa_camera_poc_blazor` → `AspecCaptura`
- [ ] Atualizar `TrimmerRootAssembly` no .csproj

### Fase 3: Renomeação de Arquivos (API)
- [ ] `pwa-camera-poc-api.csproj` → `AspecCapturaApi.csproj`
- [ ] `pwa-camera-poc-api.sln` → `AspecCapturaApi.sln`
- [ ] Atualizar `RootNamespace` no .csproj: `pwa_camera_poc_api` → `AspecCapturaApi`

### Fase 4: Atualização de Namespaces (Blazor)
Arquivos a atualizar:
- [ ] `_Imports.razor` - Todos os `@using pwa_camera_poc_blazor.*`
- [ ] `Program.cs` - Namespace e usings
- [ ] Todos os arquivos `.cs` em:
  - [ ] `Models/`
  - [ ] `Services/`
  - [ ] `Components/`
  - [ ] `Layouts/`

### Fase 5: Atualização de Namespaces (API)
Arquivos a atualizar:
- [ ] `Program.cs` - Namespace e usings
- [ ] Todos os arquivos `.cs` em:
  - [ ] `Models/`

### Fase 6: Atualização de Referências em Arquivos de Configuração
- [ ] `linker.xml` - Atualizar assembly name
- [ ] `package.json` - Atualizar name
- [ ] `README.md` - Atualizar referências
- [ ] Arquivos de documentação (*.md)

### Fase 7: Build e Testes
- [ ] Limpar solução: `dotnet clean`
- [ ] Restaurar pacotes: `dotnet restore`
- [ ] Build Blazor: `dotnet build AspecCaptura.csproj`
- [ ] Build API: `dotnet build AspecCapturaApi.csproj`
- [ ] Verificar erros de compilação
- [ ] Corrigir erros encontrados
- [ ] Build final com sucesso

### Fase 8: Verificação
- [ ] Testar aplicação localmente
- [ ] Verificar se todas as funcionalidades funcionam
- [ ] Commit das mudanças

## 🔍 Arquivos que Precisam de Atenção Especial

### Blazor
1. **_Imports.razor** - Muitos usings
2. **Program.cs** - Namespace principal
3. **Todos os Services/** - Namespaces
4. **Todos os Models/** - Namespaces
5. **Todos os Components/** - Namespaces

### API
1. **Program.cs** - Namespace e usings
2. **Models/** - Namespaces

## ⚠️ Cuidados

1. **Não renomear pastas** - Apenas arquivos de projeto e namespaces
2. **Manter estrutura de pastas** - Não mover arquivos
3. **Testar após cada fase** - Garantir que nada quebrou
4. **Usar find/replace com cuidado** - Verificar cada ocorrência

## 🚀 Ordem de Execução

1. Renomear arquivos .csproj e .sln
2. Atualizar RootNamespace nos .csproj
3. Atualizar _Imports.razor (Blazor)
4. Atualizar Program.cs (ambos)
5. Atualizar todos os arquivos .cs
6. Build e corrigir erros
7. Testar aplicação

## 📝 Comandos Úteis

```bash
# Limpar build
dotnet clean

# Restaurar pacotes
dotnet restore

# Build
dotnet build

# Procurar referências
grep -r "pwa_camera_poc_blazor" .
grep -r "pwa-camera-poc-blazor" .
grep -r "pwa_camera_poc_api" .
grep -r "pwa-camera-poc-api" .
```

## ✅ Critérios de Sucesso

- [ ] Build sem erros
- [ ] Todos os namespaces atualizados
- [ ] Aplicação funciona localmente
- [ ] Nenhuma referência ao nome antigo no código
