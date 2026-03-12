# Checklist de Atualização de Versão

Este documento lista todos os arquivos que precisam ser atualizados ao incrementar a versão do projeto.

## 📋 Arquivos a Atualizar

### 1. Configuração do Projeto
- [ ] **pwa-camera-poc-blazor.csproj**
  - `<Version>0.2.4</Version>`
  - `<AssemblyVersion>0.2.4.0</AssemblyVersion>`
  - `<FileVersion>0.2.4.0</FileVersion>`
  - `<InformationalVersion>0.2.4</InformationalVersion>`

### 2. PWA e Service Worker
- [ ] **wwwroot/manifest.json**
  - `"version": "0.2.4"`

- [ ] **wwwroot/service-worker.js**
  - `const APP_VERSION = '0.2.4';`
  - Comentário no topo: `// Version: 0.2.4`

### 3. Frontend (UI)
- [ ] **Services/AppInfo.cs**
  - `public string Version { get; private set; } = "0.2.4";`
  - `public DateTime BuildDate { get; } = new DateTime(2026, 3, 12);`

- [ ] **Components/Layout/AuthMinimalLayout.razor**
  - Fallback: `@(AppInfo?.Version ?? "0.2.4")`
  - Log: `{AppInfo?.Version ?? "0.2.4"}`

### 4. Documentação
- [ ] **docs/CHANGELOG.md**
  - Adicionar nova seção `## [0.2.4] - YYYY-MM-DD`
  - Listar mudanças em categorias (Adicionado, Alterado, Corrigido, Removido)

- [ ] **docs/RELEASE_NOTES_vX.X.X.md**
  - Criar novo arquivo com release notes detalhadas
  - Incluir instruções de atualização
  - Listar breaking changes (se houver)

## 🔄 Processo Recomendado

### Passo 1: Decidir o Tipo de Versão
Seguindo [Semantic Versioning](https://semver.org/):

- **PATCH** (0.2.4 → 0.2.5): Correções de bugs
- **MINOR** (0.2.4 → 0.3.0): Novas funcionalidades compatíveis
- **MAJOR** (0.2.4 → 1.0.0): Mudanças incompatíveis

### Passo 2: Atualizar Todos os Arquivos
Use este checklist para garantir que nenhum arquivo foi esquecido.

### Passo 3: Atualizar Documentação
- Atualizar CHANGELOG.md
- Criar RELEASE_NOTES_vX.X.X.md
- Atualizar README.md se necessário

### Passo 4: Compilar e Testar
```bash
dotnet build
dotnet run
```

### Passo 5: Commit e Tag
```bash
git add .
git commit -m "chore: bump version to vX.X.X"
git tag -a vX.X.X -m "Release vX.X.X - Description"
git push origin branch-name
git push origin vX.X.X
```

## 🔍 Verificação Rápida

Execute este comando para encontrar todas as referências à versão antiga:

```bash
# Windows PowerShell
Get-ChildItem -Recurse -Include *.cs,*.razor,*.json,*.js,*.md | Select-String "0\.2\.3" | Select-Object Path, LineNumber, Line

# Linux/Mac
grep -r "0.2.3" --include="*.cs" --include="*.razor" --include="*.json" --include="*.js" --include="*.md"
```

## 📝 Notas

- **AppInfo.cs** é a fonte central da versão exibida na UI
- **manifest.json** é usado pelo PWA para detectar atualizações
- **service-worker.js** usa a versão para gerenciar cache
- Sempre atualize a data de build em **AppInfo.cs**

## ⚠️ Cuidados

1. Não reutilize números de versão
2. Mantenha consistência entre todos os arquivos
3. Documente todas as mudanças no CHANGELOG
4. Teste a aplicação após atualizar a versão
5. Crie tag Git para cada release

---

**Última Atualização**: 2026-03-12  
**Versão Atual**: 0.2.4
