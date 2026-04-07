# Bug: Lista de Bens Não Exibe Patrimônios Carregados no IndexedDB

## Sintoma
Os registros de patrimônio estão sendo salvos corretamente no IndexedDB, mas não aparecem na lista de bens na interface do usuário.

## Evidências
- ✅ IndexedDB contém os registros de patrimônio
- ✅ Sincronização completa sem erros
- ❌ Lista de bens vazia ou não exibe os itens

## Hipótese Inicial
Problema no fluxo de recuperação e exibição dos dados no frontend, possivelmente:
1. Inconsistência entre modelos API/Frontend
2. Código morto de refatorações anteriores
3. Lógica de filtro removendo todos os itens

## Áreas de Investigação

### 1. Consistência de Modelos (API ↔ Frontend)
- [ ] Verificar classe/entidade correspondente aos tombamentos
- [ ] Comparar nomes de classes entre API e Frontend
- [ ] Comparar campos/propriedades entre API e Frontend
- [ ] Verificar mapeamento de dados na sincronização

### 2. Código Morto e Refatorações
- [ ] Identificar variáveis não utilizadas
- [ ] Identificar métodos sem chamadas
- [ ] Identificar métodos ambíguos ou redundantes
- [ ] Remover código sem referências

### 3. Fluxo de Dados (KISS + Clean Code)
- [ ] Traçar fluxo: IndexedDB → Service → Component → UI
- [ ] Verificar cada transformação de dados
- [ ] Simplificar lógica complexa
- [ ] Adicionar logs estratégicos

## Arquivos Críticos
- `pwa-camera-poc-blazor/Pages/Items.razor` - Componente de exibição
- `pwa-camera-poc-blazor/Services/Storage/IndexedDbService.cs` - Serviço de acesso ao IndexedDB
- `pwa-camera-poc-blazor/wwwroot/js/db-interop.js` - Interop JavaScript
- `pwa-camera-poc-api/Models/ApiModels.cs` - Modelos da API
- `pwa-camera-poc-blazor/Models/*.cs` - Modelos do Frontend

## Próximos Passos
1. Mapear modelos de dados (API vs Frontend)
2. Analisar fluxo de recuperação de dados
3. Identificar ponto de falha
4. Implementar correção mínima
5. Verificar solução
