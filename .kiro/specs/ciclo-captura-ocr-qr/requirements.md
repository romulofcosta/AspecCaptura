# Requirements Document - Ciclo de Captura OCR/QR Code

## Introduction

Este documento especifica os requisitos para implementação do ciclo de captura automática de patrimônio utilizando reconhecimento de QR Code e OCR (Optical Character Recognition) no PWA Blazor Aspec Captura. O sistema deve integrar tecnologias de reconhecimento automático ao fluxo existente de captura manual, proporcionando preenchimento automático de formulários baseado em dados armazenados localmente no IndexedDB.

## Glossary

- **Sistema_Captura**: O módulo principal de captura de patrimônio do PWA Blazor
- **Reconhecedor_QR**: Componente responsável pela detecção e decodificação de códigos QR
- **Reconhecedor_OCR**: Componente responsável pela extração de texto de imagens via OCR
- **Banco_Local**: IndexedDB contendo 42MB de dados de patrimônio em chunks de 5k registros
- **Worker_Processamento**: Web Worker dedicado ao processamento de reconhecimento
- **Validador_Esfera**: Componente que valida permissões de acesso por esfera (A=Todos, E=Estadual, M=Municipal)
- **Sanitizador_Codigo**: Utilitário para limpeza e normalização de códigos detectados
- **Cache_Consulta**: Sistema de cache otimizado para consultas no IndexedDB
- **Interface_Camera**: Interface atual de captura de fotos (Camera.razor)
- **Servico_Camera**: Serviço atual de gerenciamento de câmera (CameraService.cs)
- **Interop_Camera**: JavaScript interop atual para câmera (camera-interop.js)

## Requirements

### Requirement 1: Reconhecimento Automático de QR Code

**User Story:** Como usuário do sistema de captura, eu quero que códigos QR sejam automaticamente detectados e decodificados durante a captura, para que eu possa ter preenchimento automático do formulário sem digitação manual.

#### Acceptance Criteria

1. WHEN a câmera está ativa para captura, THE Reconhecedor_QR SHALL detectar códigos QR em tempo real no feed de vídeo
2. WHEN um código QR válido é detectado, THE Reconhecedor_QR SHALL decodificar o conteúdo em menos de 500ms
3. THE Reconhecedor_QR SHALL utilizar a biblioteca ZXing-js para decodificação
4. WHEN múltiplos códigos QR estão visíveis, THE Reconhecedor_QR SHALL priorizar o código com maior área de detecção
5. THE Reconhecedor_QR SHALL funcionar com códigos QR em diferentes orientações e condições de iluminação

### Requirement 2: Reconhecimento Automático de OCR

**User Story:** Como usuário do sistema de captura, eu quero que textos sejam automaticamente extraídos de placas e etiquetas durante a captura, para que eu possa identificar códigos de patrimônio mesmo quando não há QR Code disponível.

#### Acceptance Criteria

1. WHEN a câmera está ativa para captura, THE Reconhecedor_OCR SHALL extrair texto do feed de vídeo em tempo real
2. THE Reconhecedor_OCR SHALL utilizar a biblioteca Tesseract.js para extração de texto
3. WHEN texto é detectado, THE Reconhecedor_OCR SHALL processar a extração em menos de 2 segundos
4. THE Reconhecedor_OCR SHALL focar na detecção de códigos alfanuméricos típicos de patrimônio
5. THE Reconhecedor_OCR SHALL funcionar com diferentes fontes e tamanhos de texto em placas metálicas

### Requirement 3: Busca Automática no Banco Local

**User Story:** Como usuário do sistema de captura, eu quero que códigos detectados sejam automaticamente pesquisados no banco de dados local, para que eu possa verificar se o patrimônio já está cadastrado no sistema.

#### Acceptance Criteria

1. WHEN um código é detectado via QR ou OCR, THE Sistema_Captura SHALL buscar o código no Banco_Local
2. THE Sistema_Captura SHALL pesquisar pelos campos 'code' e 'nutomb' na store 'patrimonio'
3. WHEN a busca é realizada, THE Cache_Consulta SHALL otimizar consultas para evitar múltiplas buscas do mesmo código
4. THE Sistema_Captura SHALL completar a busca em menos de 100ms para códigos já em cache
5. WHEN nenhum resultado é encontrado, THE Sistema_Captura SHALL permitir captura como novo item

### Requirement 4: Preenchimento Automático de Formulário

**User Story:** Como usuário do sistema de captura, eu quero que o formulário seja automaticamente preenchido quando um patrimônio é encontrado no banco local, para que eu possa confirmar ou editar as informações sem digitação manual.

#### Acceptance Criteria

1. WHEN um patrimônio é encontrado no Banco_Local, THE Sistema_Captura SHALL preencher automaticamente os campos do formulário
2. THE Sistema_Captura SHALL mapear os dados do PatrimonioItem para os campos do InventoryItem
3. WHEN o formulário é preenchido automaticamente, THE Sistema_Captura SHALL destacar visualmente os campos preenchidos
4. THE Sistema_Captura SHALL permitir edição manual de todos os campos preenchidos automaticamente
5. WHEN dados são preenchidos automaticamente, THE Sistema_Captura SHALL manter a foto capturada associada

### Requirement 5: Validação de Esfera de Acesso

**User Story:** Como usuário com esfera específica de acesso, eu quero que o sistema valide se tenho permissão para acessar um patrimônio encontrado, para que eu seja impedido de acessar itens fora da minha esfera de atuação.

#### Acceptance Criteria

1. WHEN um patrimônio é encontrado no Banco_Local, THE Validador_Esfera SHALL verificar a esfera do item contra a esfera do usuário
2. IF o patrimônio pertence a esfera diferente da permitida, THEN THE Sistema_Captura SHALL exibir mensagem impeditiva
3. THE Validador_Esfera SHALL permitir acesso quando esfera do usuário é 'A' (Todos)
4. THE Validador_Esfera SHALL permitir acesso quando esfera do item corresponde à esfera do usuário
5. WHEN acesso é negado por esfera, THE Sistema_Captura SHALL impedir continuação do processo de captura

### Requirement 6: Sanitização e Normalização de Códigos

**User Story:** Como usuário do sistema de captura, eu quero que códigos detectados sejam automaticamente limpos e normalizados, para que variações comuns de digitação e OCR sejam tratadas adequadamente.

#### Acceptance Criteria

1. WHEN um código é detectado, THE Sanitizador_Codigo SHALL remover espaços em branco no início e fim
2. THE Sanitizador_Codigo SHALL converter caracteres similares (O para 0, I para 1) quando apropriado
3. THE Sanitizador_Codigo SHALL normalizar códigos para formato padrão (maiúsculas, sem caracteres especiais)
4. WHEN múltiplas variações do mesmo código são detectadas, THE Sanitizador_Codigo SHALL usar a versão mais confiável
5. THE Sanitizador_Codigo SHALL manter histórico de códigos detectados para evitar reprocessamento

### Requirement 7: Feedback Visual e Tátil

**User Story:** Como usuário do sistema de captura, eu quero receber feedback visual e tátil quando códigos são detectados, para que eu saiba quando o reconhecimento está funcionando e quando um código foi identificado.

#### Acceptance Criteria

1. WHEN um código QR é detectado, THE Interface_Camera SHALL exibir moldura verde ao redor do código
2. WHEN texto OCR é detectado, THE Interface_Camera SHALL exibir overlay destacando a área de texto
3. WHEN um código é decodificado com sucesso, THE Sistema_Captura SHALL vibrar o dispositivo por 200ms
4. WHEN um patrimônio é encontrado no banco, THE Sistema_Captura SHALL emitir som de confirmação
5. WHEN reconhecimento está ativo, THE Interface_Camera SHALL exibir indicador visual de status

### Requirement 8: Performance e Otimização

**User Story:** Como usuário do sistema de captura, eu quero que o reconhecimento automático não afete a performance da aplicação, para que eu possa usar o sistema de forma fluida mesmo em dispositivos com recursos limitados.

#### Acceptance Criteria

1. THE Worker_Processamento SHALL executar reconhecimento QR e OCR em Web Worker separado
2. THE Sistema_Captura SHALL processar frames de vídeo a no máximo 10 FPS para reconhecimento
3. WHEN processamento está ativo, THE Sistema_Captura SHALL manter interface responsiva
4. THE Cache_Consulta SHALL armazenar resultados de busca por até 5 minutos
5. WHEN memória está baixa, THE Sistema_Captura SHALL reduzir frequência de processamento automaticamente

### Requirement 9: Priorização QR Code sobre OCR

**User Story:** Como usuário do sistema de captura, eu quero que códigos QR tenham prioridade sobre texto OCR quando ambos são detectados, para que eu tenha maior precisão na identificação de patrimônio.

#### Acceptance Criteria

1. WHEN QR Code e OCR detectam códigos simultaneamente, THE Sistema_Captura SHALL priorizar resultado do QR Code
2. THE Sistema_Captura SHALL usar resultado OCR apenas quando QR Code não detecta nenhum código
3. WHEN QR Code detecta código inválido, THE Sistema_Captura SHALL tentar usar resultado OCR como fallback
4. THE Sistema_Captura SHALL indicar visualmente qual método de reconhecimento foi utilizado
5. WHEN ambos métodos falham, THE Sistema_Captura SHALL permitir entrada manual do código

### Requirement 10: Integração com Funcionalidades Existentes

**User Story:** Como usuário do sistema de captura, eu quero que todas as funcionalidades existentes continuem funcionando normalmente, para que eu possa usar tanto captura manual quanto automática conforme necessário.

#### Acceptance Criteria

1. THE Sistema_Captura SHALL manter compatibilidade total com fluxo de captura manual existente
2. WHEN reconhecimento automático está desabilitado, THE Sistema_Captura SHALL funcionar exatamente como antes
3. THE Sistema_Captura SHALL permitir alternar entre modo manual e automático durante a captura
4. THE Servico_Camera SHALL manter todas as APIs existentes para compatibilidade
5. WHEN funcionalidades automáticas falham, THE Sistema_Captura SHALL degradar graciosamente para modo manual

### Requirement 11: Configuração e Controle do Usuário

**User Story:** Como usuário do sistema de captura, eu quero poder controlar quando o reconhecimento automático está ativo, para que eu possa desabilitar a funcionalidade quando não necessária ou quando está causando problemas.

#### Acceptance Criteria

1. THE Sistema_Captura SHALL fornecer toggle para habilitar/desabilitar reconhecimento automático
2. THE Sistema_Captura SHALL permitir configurar sensibilidade do reconhecimento OCR
3. WHEN reconhecimento está desabilitado, THE Sistema_Captura SHALL parar todo processamento automático
4. THE Sistema_Captura SHALL salvar preferências do usuário no localStorage
5. THE Sistema_Captura SHALL exibir status atual do reconhecimento na interface

### Requirement 12: Parser de Códigos QR

**User Story:** Como desenvolvedor do sistema, eu quero um parser robusto para códigos QR, para que diferentes formatos de QR Code sejam interpretados corretamente.

#### Acceptance Criteria

1. WHEN um código QR é decodificado, THE Parser_QR SHALL extrair informações estruturadas do conteúdo
2. THE Parser_QR SHALL suportar formatos de QR Code específicos de patrimônio público
3. THE Pretty_Printer_QR SHALL formatar dados de patrimônio em QR Code válido
4. FOR ALL códigos QR válidos de patrimônio, parsing então printing então parsing SHALL produzir objeto equivalente (round-trip property)
5. WHEN QR Code contém dados inválidos, THE Parser_QR SHALL retornar erro descritivo

### Requirement 13: Parser de Texto OCR

**User Story:** Como desenvolvedor do sistema, eu quero um parser robusto para texto extraído via OCR, para que códigos de patrimônio sejam identificados corretamente mesmo com ruído na extração.

#### Acceptance Criteria

1. WHEN texto é extraído via OCR, THE Parser_OCR SHALL identificar padrões de códigos de patrimônio
2. THE Parser_OCR SHALL filtrar ruído e caracteres irrelevantes do texto extraído
3. THE Parser_OCR SHALL aplicar regex patterns para identificar códigos alfanuméricos válidos
4. WHEN múltiplos códigos são detectados no texto, THE Parser_OCR SHALL retornar o mais provável
5. THE Parser_OCR SHALL validar formato de códigos contra padrões conhecidos de patrimônio
