# Documento de Requisitos - Suporte a Códigos de Barras

## Introdução

Esta especificação define os requisitos para adicionar suporte a códigos de barras lineares (1D) ao sistema de reconhecimento automático existente no PWA Blazor Aspec Captura. O sistema atual já possui reconhecimento de QR Code e OCR, e esta expansão manterá a mesma arquitetura e qualidade de performance, alterando a priorização para OCR > Códigos de Barras > QR Code.

## Glossário

- **Sistema_Reconhecimento**: O sistema completo de reconhecimento automático que processa frames de vídeo
- **Worker_Barcode**: Web Worker dedicado ao processamento de códigos de barras
- **Parser_Barcode**: Componente responsável por extrair e validar códigos de barras detectados
- **Codigo_Barras_1D**: Códigos de barras lineares incluindo CODE 128, CODE 39, EAN-13, EAN-8, UPC-A, UPC-E
- **Prioridade_Reconhecimento**: Ordem de processamento definida como OCR > Códigos de Barras > QR Code
- **Biblioteca_ZXing**: Biblioteca JavaScript ZXing-js utilizada para detecção de códigos
- **Interface_Camera**: Componente Camera.razor que exibe a câmera e overlays visuais

## Requisitos

### Requisito 1: Detecção de Códigos de Barras 1D

**User Story:** Como usuário do sistema de captura, quero que o sistema detecte códigos de barras lineares automaticamente, para que eu possa identificar patrimônios codificados em formatos 1D.

#### Critérios de Aceitação

1. O Sistema_Reconhecimento DEVE detectar códigos CODE 128 com confiança mínima de 0.7
2. O Sistema_Reconhecimento DEVE detectar códigos CODE 39 com confiança mínima de 0.7
3. O Sistema_Reconhecimento DEVE detectar códigos EAN-13 com confiança mínima de 0.8
4. O Sistema_Reconhecimento DEVE detectar códigos EAN-8 com confiança mínima de 0.8
5. O Sistema_Reconhecimento DEVE detectar códigos UPC-A com confiança mínima de 0.8
6. O Sistema_Reconhecimento DEVE detectar códigos UPC-E com confiança mínima de 0.8
7. QUANDO um código de barras válido é detectado, O Sistema_Reconhecimento DEVE retornar o código extraído e sua posição no frame

### Requisito 2: Integração com Arquitetura de Web Workers

**User Story:** Como desenvolvedor do sistema, quero que o reconhecimento de códigos de barras utilize a mesma arquitetura de Web Workers, para que a performance seja mantida sem bloquear a UI.

#### Critérios de Aceitação

1. O Worker_Barcode DEVE ser criado como um Web Worker independente
2. O Worker_Barcode DEVE utilizar a Biblioteca_ZXing para processamento
3. QUANDO o Worker_Barcode é inicializado, ELE DEVE carregar as dependências necessárias em até 10 segundos
4. O Worker_Barcode DEVE processar frames de ImageData recebidos via postMessage
5. QUANDO um código de barras é detectado, O Worker_Barcode DEVE enviar o resultado via postMessage
6. O Worker_Barcode DEVE implementar tratamento de erros sem interromper o processamento contínuo

### Requisito 3: Parser Específico para Códigos de Barras

**User Story:** Como desenvolvedor do sistema, quero um parser dedicado para códigos de barras, para que a validação e extração de dados seja consistente com os parsers existentes.

#### Critérios de Aceitação

1. O Parser_Barcode DEVE validar se o código detectado é um código de patrimônio válido
2. O Parser_Barcode DEVE extrair códigos numéricos de 6 a 12 dígitos
3. O Parser_Barcode DEVE extrair códigos alfanuméricos no formato [A-Z]{2,4}[0-9]{4,8}
4. QUANDO um código de barras é processado, O Parser_Barcode DEVE retornar confiança, formato detectado e código sanitizado
5. O Parser_Barcode DEVE implementar validação de checksum para códigos EAN e UPC
6. PARA TODOS os códigos válidos, processar e depois formatar e depois processar novamente DEVE produzir resultado equivalente (propriedade round-trip)

### Requisito 4: Nova Priorização de Reconhecimento

**User Story:** Como usuário do sistema, quero que o OCR tenha prioridade sobre códigos de barras e QR codes, para que textos impressos sejam detectados primeiro.

#### Critérios de Aceitação

1. O Sistema_Reconhecimento DEVE processar OCR com Prioridade_Reconhecimento 1
2. O Sistema_Reconhecimento DEVE processar códigos de barras com Prioridade_Reconhecimento 2
3. O Sistema_Reconhecimento DEVE processar QR codes com Prioridade_Reconhecimento 3
4. QUANDO múltiplos tipos são detectados simultaneamente, O Sistema_Reconhecimento DEVE retornar apenas o resultado de maior prioridade
5. SE OCR não detectar código válido, ENTÃO O Sistema_Reconhecimento DEVE processar códigos de barras
6. SE códigos de barras não forem detectados, ENTÃO O Sistema_Reconhecimento DEVE processar QR codes

### Requisito 5: Atualização da Interface Visual

**User Story:** Como usuário do sistema, quero ver indicações visuais quando códigos de barras são detectados, para que eu saiba que o sistema está funcionando corretamente.

#### Critérios de Aceitação

1. A Interface_Camera DEVE exibir overlay verde quando um código de barras é detectado com sucesso
2. A Interface_Camera DEVE exibir overlay amarelo quando um código de barras é detectado mas não validado
3. A Interface_Camera DEVE mostrar o tipo de código detectado (CODE 128, EAN-13, etc.)
4. QUANDO um código de barras é detectado, A Interface_Camera DEVE desenhar bounding box ao redor da área detectada
5. A Interface_Camera DEVE manter compatibilidade visual com overlays de QR e OCR existentes

### Requisito 6: Compatibilidade com Sistema Existente

**User Story:** Como usuário atual do sistema, quero que todas as funcionalidades existentes continuem funcionando, para que não haja regressão nas capacidades atuais.

#### Critérios de Aceitação

1. O Sistema_Reconhecimento DEVE manter compatibilidade total com detecção de QR codes existente
2. O Sistema_Reconhecimento DEVE manter compatibilidade total com OCR existente
3. O Sistema_Reconhecimento DEVE manter integração com IndexedDB para busca de patrimônio
4. O Sistema_Reconhecimento DEVE manter validação por esfera de acesso
5. QUANDO códigos de barras são desabilitados via configuração, O Sistema_Reconhecimento DEVE funcionar como antes da implementação
6. O Sistema_Reconhecimento DEVE manter os mesmos eventos e callbacks para componentes consumidores

### Requisito 7: Performance e Otimização

**User Story:** Como usuário do sistema, quero que o reconhecimento de códigos de barras seja rápido e eficiente, para que não impacte a experiência de uso.

#### Critérios de Aceitação

1. O Worker_Barcode DEVE processar um frame em menos de 200ms em dispositivos móveis médios
2. O Sistema_Reconhecimento DEVE manter taxa de processamento de pelo menos 5 FPS com códigos de barras habilitados
3. O Worker_Barcode DEVE usar no máximo 50MB de memória durante operação normal
4. QUANDO múltiplos códigos de barras estão presentes, O Sistema_Reconhecimento DEVE retornar o de maior confiança em menos de 300ms
5. O Sistema_Reconhecimento DEVE implementar debounce de 2 segundos para evitar detecções duplicadas
6. O Worker_Barcode DEVE liberar recursos automaticamente após 30 segundos de inatividade

### Requisito 8: Configuração e Controle

**User Story:** Como administrador do sistema, quero controlar quais tipos de códigos de barras são detectados, para que eu possa otimizar para casos de uso específicos.

#### Critérios de Aceitação

1. O Sistema_Reconhecimento DEVE permitir habilitar/desabilitar detecção de códigos de barras via configuração
2. O Sistema_Reconhecimento DEVE permitir configurar quais formatos de código de barras são processados
3. O Sistema_Reconhecimento DEVE permitir ajustar limites de confiança por tipo de código
4. ONDE configuração de códigos de barras está desabilitada, O Sistema_Reconhecimento DEVE pular processamento de códigos de barras
5. O Sistema_Reconhecimento DEVE persistir configurações de códigos de barras no localStorage
6. O Sistema_Reconhecimento DEVE aplicar configurações sem necessidade de reinicialização

### Requisito 9: Tratamento de Erros e Logging

**User Story:** Como desenvolvedor do sistema, quero logs detalhados e tratamento de erros robusto, para que eu possa diagnosticar problemas de reconhecimento.

#### Critérios de Aceitação

1. O Worker_Barcode DEVE registrar tentativas de detecção com timestamp e resultado
2. O Worker_Barcode DEVE registrar erros de processamento sem interromper operação
3. SE a Biblioteca_ZXing falhar ao carregar, ENTÃO O Sistema_Reconhecimento DEVE continuar funcionando sem códigos de barras
4. O Sistema_Reconhecimento DEVE notificar falhas de inicialização do Worker_Barcode via evento de erro
5. O Parser_Barcode DEVE registrar códigos rejeitados com motivo da rejeição
6. O Sistema_Reconhecimento DEVE implementar fallback gracioso quando Worker_Barcode não está disponível

### Requisito 10: Testes e Validação

**User Story:** Como desenvolvedor do sistema, quero testes abrangentes para códigos de barras, para que eu possa garantir qualidade e confiabilidade.

#### Critérios de Aceitação

1. O Sistema_Reconhecimento DEVE incluir testes unitários para Parser_Barcode
2. O Sistema_Reconhecimento DEVE incluir testes de integração para Worker_Barcode
3. O Sistema_Reconhecimento DEVE incluir testes de performance para processamento de códigos de barras
4. O Sistema_Reconhecimento DEVE incluir testes com imagens de códigos de barras reais
5. PARA TODOS os formatos suportados, testes DEVEM validar detecção correta com confiança adequada
6. O Sistema_Reconhecimento DEVE incluir testes de regressão para funcionalidades QR e OCR existentes