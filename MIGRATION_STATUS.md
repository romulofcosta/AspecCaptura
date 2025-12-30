# Status da Migração para MudBlazor

## ✅ Concluído

### Configuração
- [x] MudBlazor 7.20.0 instalado
- [x] FluentUI removido do projeto
- [x] Program.cs configurado com MudServices
- [x] _Imports.razor atualizado
- [x] index.html atualizado (CSS)
- [x] Snackbar configurado

### Componentes Migrados
- [x] **Pages/Login.razor**: Migrado para MudCard, MudTextField, MudButton, e Ícones Material.
- [x] **Pages/Home.razor**: Migrado para MudTextField (Busca), MudIconButton, MudSelect, MudPagination.
- [x] **Pages/Camera.razor**: Migrado formulário para MudGrid, MudTextField, MudSelect.
- [x] **Layout/MainLayout.razor**: Adicionados Providers (Theme, Dialog, Snackbar) e migrado lógica de notificação para ISnackbar.

## ⚠️ Pendente / A Fazer

### Componentes a Revisar
- [ ] **Pages/Register.razor**: Ainda utiliza componentes HTML padrão (`InputText`, `select`). Funcional, mas pode ser atualizado para `MudTextField` para consistência visual total.
- [ ] **Layouts/Header.razor** e **Footer.razor**: Verificar se há classes CSS legadas que podem ser substituídas por utilitários MudBlazor.

## 🐛 Problemas Conhecidos
- Avisos de build sobre casing de atributos (ex: `Title` vs HTML `title`) podem aparecer, mas não impedem a execução.
