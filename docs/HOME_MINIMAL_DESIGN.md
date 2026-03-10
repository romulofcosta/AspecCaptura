# Home Minimalista - Design System

## Data: 2026-03-09

## Visão Geral

Criada nova versão minimalista da página Home baseada em design moderno e limpo, inspirado em aplicativos mobile nativos.

## Características do Design

### 1. Layout Minimalista
- **Fundo branco puro** (#FFFFFF)
- **Bordas sutis** (#E2E8F0)
- **Sem sombras pesadas** - apenas elevação sutil no hover
- **Espaçamento generoso** - respiração visual

### 2. Tipografia
- **Fonte**: Inter (sans-serif moderna)
- **Hierarquia clara**:
  - Títulos: 18px, peso 600
  - Subtítulos: 14px, peso 600
  - Labels: 12px, peso 500
  - Micro-texto: 10px, peso 700 (uppercase)

### 3. Cores

#### Primárias
- **Corporate Blue**: #003366 (botões, títulos principais)
- **Background**: #FFFFFF (fundo principal)
- **Surface**: #F8FAFC (cards, inputs)

#### Status
- **Success**: #2e7d32 (itens sincronizados)
- **Warning**: #ed6c02 (itens pendentes)
- **Info**: #0066CC (links, ações secundárias)

#### Neutras
- **Text Primary**: #1E293B
- **Text Secondary**: #64748B
- **Text Tertiary**: #94A3B8
- **Border**: #E2E8F0

### 4. Componentes

#### Header Sticky
```css
- Posição: sticky top
- Background: branco com borda inferior sutil
- Padding: 56px top (safe area) + 24px lateral
- Avatar circular com ícone
- Nome do usuário + contexto
- Botão de notificações
```

#### Cards de Estatísticas
```css
- Grid 3 colunas
- Bordas sutis, sem sombra
- Border-radius: 16px
- Labels uppercase (10px, bold, tracking wide)
- Valores grandes (20px, bold)
- Cores por status
```

#### Busca Minimalista
```css
- Background: #F8FAFC
- Border-radius: 9999px (pill)
- Padding: 12px 20px 12px 44px
- Ícone posicionado absolutamente
- Sem borda, focus ring sutil
```

#### Botão de Captura
```css
- Background: #003366
- Color: white
- Border-radius: 16px
- Padding: 16px 24px
- Ícone + texto centralizado
- Active state: scale(0.98)
- Width: 100%
```

#### Cards de Itens
```css
- Border: 1px solid #E2E8F0
- Border-radius: 16px
- Padding: 16px
- Hover: sombra sutil
- Layout: flex com ícone + info + status
- Ícone em box arredondado (#F8FAFC)
```

## Estrutura da Página

### 1. Header (Sticky)
- Avatar do usuário
- Saudação personalizada
- Nome da aplicação
- Contexto atual (UO/Órgão)
- Botão "Alterar"
- Botão de notificações

### 2. Estatísticas
- Total de itens
- Itens sincronizados
- Itens pendentes

### 3. Ações Principais
- Campo de busca
- Botão "Iniciar Captura"

### 4. Lista de Itens Recentes
- Título da seção
- Link "Ver todos"
- Cards de itens (máximo 10)
- Cada card mostra:
  - Ícone de inventário
  - Nome do item
  - Código + data/hora
  - Status de sincronização

## Responsividade

### Mobile First
- Design otimizado para telas pequenas
- Touch targets mínimos de 44px
- Espaçamento adequado para dedos
- Scroll suave

### Adaptações
- Grid de estatísticas: 3 colunas fixas
- Cards de itens: largura total
- Padding lateral: 24px
- Padding inferior: 120px (espaço para bottom nav)

## Acessibilidade

- Contraste adequado (WCAG AA)
- Touch targets mínimos
- Focus states visíveis
- Hierarquia semântica
- Labels descritivos

## Performance

- CSS inline para carregamento rápido
- Sem dependências externas de CSS
- Componentes MudBlazor apenas para ícones
- Renderização otimizada (máximo 10 itens)

## Comparação com Versão Anterior

### Antes (MudBlazor Padrão)
- Layout mais pesado
- Muitos componentes MudBlazor
- Sombras e elevações pronunciadas
- Cores mais saturadas
- Filtros e ordenação visíveis

### Depois (Minimalista)
- Layout limpo e leve
- CSS customizado minimalista
- Bordas sutis, sem sombras pesadas
- Cores corporativas suaves
- Foco no essencial

## Arquivos

### Criados
- `Pages/HomeMinimal.razor` - Nova página minimalista

### Mantidos
- `Pages/Home.razor` - Versão original com filtros e paginação

## Rotas

- `/home` - Versão original (com filtros completos)
- `/home-minimal` - Versão minimalista (nova)

## Próximos Passos

1. ✅ Criar página minimalista
2. ⏳ Testar em dispositivos móveis
3. ⏳ Validar acessibilidade
4. ⏳ Medir performance
5. ⏳ Coletar feedback dos usuários
6. ⏳ Decidir qual versão usar como padrão

## Recomendação

Sugerimos usar a versão minimalista como padrão para:
- Melhor experiência mobile
- Carregamento mais rápido
- Visual mais moderno
- Foco nas ações principais

A versão original pode ser mantida como "visualização avançada" para usuários que precisam de filtros e ordenação complexa.
