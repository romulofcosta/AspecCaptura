# Implementation Plan: Barcode Recognition Support

## Overview

This implementation plan adds 1D barcode support (CODE 128, CODE 39, EAN-13, EAN-8, UPC-A, UPC-E) to the existing PWA Blazor recognition system. The implementation follows the established Web Workers architecture and maintains full backward compatibility while introducing a new priority order: OCR > Barcodes > QR Code.

## Tasks

- [x] 1. Create Barcode Worker Infrastructure
  - Create new barcode-worker.js using ZXing-js BrowserMultiFormatReader
  - Implement initialization, frame processing, and error handling
  - Add support for all required barcode formats with configurable confidence thresholds
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

  - [ ]* 1.1 Write property test for Barcode Worker initialization
    - **Property 3: Comunicação Worker-Main Thread**
    - **Validates: Requirements 2.4, 2.5**

  - [ ]* 1.2 Write property test for Worker robustness
    - **Property 4: Robustez do Worker**
    - **Validates: Requirements 2.6**

- [x] 2. Implement BarcodeParser Component
  - [x] 2.1 Create BarcodeParser.cs with validation and extraction logic
    - Implement Parse method with format-specific validation
    - Add checksum validation for EAN-13, EAN-8, UPC-A, UPC-E formats
    - Extract patrimônio codes from detected barcodes (numeric 6-12 digits, alphanumeric [A-Z]{2,4}[0-9]{4,8})
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

  - [ ]* 2.2 Write property test for patrimônio code validation
    - **Property 5: Validação de Códigos de Patrimônio**
    - **Validates: Requirements 3.1, 3.2, 3.3**

  - [ ]* 2.3 Write property test for checksum validation
    - **Property 7: Validação de Checksum**
    - **Validates: Requirements 3.5**

  - [ ]* 2.4 Write property test for round-trip processing
    - **Property 8: Round-trip de Processamento**
    - **Validates: Requirements 3.6**

  - [ ]* 2.5 Write unit tests for BarcodeParser edge cases
    - Test invalid formats, malformed codes, and error conditions
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 3. Update RecognitionService with New Priority Logic
  - [x] 3.1 Add barcode support to RecognitionService.cs
    - Add BarcodeEnabled property and barcode-specific settings
    - Implement OnBarcodeDetectedAsync callback method
    - Update ProcessFrameAsync with new priority order (OCR > Barcode > QR)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6_

  - [ ]* 3.2 Write property test for recognition prioritization
    - **Property 9: Priorização de Reconhecimento**
    - **Validates: Requirements 4.1, 4.2, 4.3, 4.4**

  - [ ]* 3.3 Write property test for fallback chain
    - **Property 10: Cadeia de Fallback**
    - **Validates: Requirements 4.5, 4.6**

  - [ ]* 3.4 Write unit tests for priority logic scenarios
    - Test multiple detection types, priority selection, and fallback behavior
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6_

- [x] 4. Checkpoint - Core Components Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Extend Recognition Interop with Barcode Worker
  - [x] 5.1 Update recognition-interop.js to include barcode worker
    - Add barcode worker creation and initialization
    - Implement barcode worker message handlers
    - Update frame processing to send data to all three workers
    - _Requirements: 2.1, 2.2, 2.4, 2.5_

  - [ ]* 5.2 Write integration tests for worker coordination
    - Test worker initialization, message passing, and error handling
    - _Requirements: 2.1, 2.2, 2.4, 2.5, 2.6_

- [x] 6. Add Barcode Service Interface and Implementation
  - [x] 6.1 Create IBarcodeService interface and implementation
    - Define service contract for barcode detection
    - Implement BarcodeService with worker coordination
    - Add configuration support for enabled formats and confidence thresholds
    - _Requirements: 8.1, 8.2, 8.3_

  - [ ]* 6.2 Write property test for configuration handling
    - **Property 16: Configuração Dinâmica de Habilitação**
    - **Property 17: Configuração de Formatos**
    - **Property 18: Configuração de Limites de Confiança**
    - **Validates: Requirements 8.1, 8.2, 8.3, 8.6**

- [x] 7. Update Models and Enums
  - [x] 7.1 Extend RecognitionModels.cs with barcode support
    - Add Barcode to RecognitionSource enum
    - Create BarcodeResult, BarcodeFormat enum, and BarcodeDetectionOptions classes
    - Update RecognitionSettings with barcode configuration properties
    - _Requirements: 8.1, 8.2, 8.3_

  - [ ]* 7.2 Write unit tests for model validation
    - Test enum mappings, configuration validation, and data structures
    - _Requirements: 8.1, 8.2, 8.3_

- [x] 8. Implement Performance Optimizations
  - [x] 8.1 Add debounce logic and performance monitoring
    - Implement 2-second debounce for duplicate detections
    - Add performance metrics and memory management
    - Implement worker resource cleanup after inactivity
    - _Requirements: 7.4, 7.5, 7.6_

  - [ ]* 8.2 Write property test for debounce behavior
    - **Property 15: Debounce de Detecções**
    - **Validates: Requirements 7.5**

  - [ ]* 8.3 Write property test for confidence-based selection
    - **Property 14: Seleção por Maior Confiança**
    - **Validates: Requirements 7.4**

  - [ ]* 8.4 Write performance tests
    - Test processing time limits, memory usage, and FPS maintenance
    - _Requirements: 7.1, 7.2, 7.3, 7.6_

- [x] 9. Add Error Handling and Logging
  - [x] 9.1 Implement comprehensive error handling
    - Add graceful degradation when ZXing library fails to load
    - Implement circuit breaker pattern for barcode detection failures
    - Add detailed logging for detection attempts and failures
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_

  - [ ]* 9.2 Write property tests for error handling
    - **Property 23: Fallback Gracioso para Falha de Biblioteca**
    - **Property 24: Notificação de Falhas de Inicialização**
    - **Property 26: Fallback para Worker Indisponível**
    - **Validates: Requirements 9.3, 9.4, 9.6**

  - [ ]* 9.3 Write unit tests for error scenarios
    - Test library loading failures, worker errors, and recovery mechanisms
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_

- [x] 10. Checkpoint - Error Handling Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Update Camera Interface for Barcode Visual Feedback
  - [x] 11.1 Extend Camera.razor with barcode overlay support
    - Add barcode detection overlays (green for valid, yellow for invalid)
    - Display detected barcode format (CODE 128, EAN-13, etc.)
    - Draw bounding boxes around detected barcodes
    - Maintain compatibility with existing QR and OCR overlays
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [ ]* 11.2 Write integration tests for visual feedback
    - Test overlay display, format indication, and bounding box rendering
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [x] 12. Add Configuration Management
  - [x] 12.1 Implement barcode configuration persistence
    - Add localStorage persistence for barcode settings
    - Create configuration UI components for barcode options
    - Implement dynamic configuration updates without restart
    - _Requirements: 8.4, 8.5, 8.6_

  - [ ]* 12.2 Write property test for configuration persistence
    - **Property 20: Persistência de Configuração**
    - **Validates: Requirements 8.5**

  - [ ]* 12.3 Write property test for disabled configuration behavior
    - **Property 19: Comportamento com Configuração Desabilitada**
    - **Validates: Requirements 8.4**

- [x] 13. Comprehensive Testing Suite
  - [ ]* 13.1 Write property tests for universal barcode detection
    - **Property 1: Detecção Universal de Códigos de Barras**
    - **Property 2: Completude de Resultado de Detecção**
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7**

  - [ ]* 13.2 Write property tests for backward compatibility
    - **Property 12: Não-regressão de Funcionalidades Existentes**
    - **Property 13: Comportamento com Códigos de Barras Desabilitados**
    - **Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5, 6.6**

  - [ ]* 13.3 Write property tests for logging and monitoring
    - **Property 21: Logging de Tentativas**
    - **Property 22: Logging de Erros sem Interrupção**
    - **Property 25: Logging de Códigos Rejeitados**
    - **Validates: Requirements 9.1, 9.2, 9.5**

  - [ ]* 13.4 Write regression tests for existing functionality
    - Test that all existing QR Code and OCR functionality remains unchanged
    - Use existing test cases as baseline for comparison
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.6_

  - [ ]* 13.5 Write integration tests with real barcode images
    - Test detection accuracy with actual barcode images for all supported formats
    - Validate confidence thresholds and format recognition
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_

- [x] 14. Final Integration and Wiring
  - [x] 14.1 Wire all components together
    - Register BarcodeService in dependency injection
    - Update service registrations and configurations
    - Ensure proper initialization order and dependencies
    - _Requirements: All requirements integration_

  - [ ]* 14.2 Write end-to-end integration tests
    - Test complete barcode recognition flow from camera to patrimônio search
    - Validate priority ordering with mixed detection scenarios
    - _Requirements: All requirements integration_

- [x] 15. Final Checkpoint - Complete System Validation
  - Ensure all tests pass, ask the user if questions arise.
  - Verify backward compatibility with existing QR and OCR functionality
  - Confirm new priority order (OCR > Barcode > QR) is working correctly
  - Validate performance requirements are met on mobile devices

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Property-based tests use FsCheck with minimum 100 iterations per test
- All 26 correctness properties from the design document are covered
- Comprehensive regression testing ensures no breaking changes to existing functionality
- Performance requirements include <200ms processing time and 5+ FPS maintenance
- Error handling includes graceful degradation and circuit breaker patterns