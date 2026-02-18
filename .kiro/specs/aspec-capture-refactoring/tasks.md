# Implementation Plan: ASPEC Capture Refactoring

## Overview

This implementation plan executes the comprehensive refactoring of the ASPEC Capture .NET Blazor PWA application in four distinct phases. Each phase builds incrementally on the previous one, with validation checkpoints to ensure stability and rollback capability if issues arise.

## Tasks

- [x] 1. Phase 1: Project Structure Cleanup
  - [x] 1.1 Create scripts directory and move build files
    - Create scripts/ folder in project root
    - Move all .ps1, .bat, and build-related files to scripts/
    - Update any references to moved scripts in project files
    - _Requirements: 1.1_

  - [x] 1.2 Update .gitignore for comprehensive exclusions
    - Add patterns for build logs (*.log, *.binlog)
    - Add patterns for agent tools and temporary files
    - Verify bin/ and obj/ patterns are present
    - Add patterns for IDE temporary files
    - _Requirements: 1.2, 1.3_

  - [x] 1.3 Write property test for file organization
    - **Property 1: File Organization Consistency**
    - **Validates: Requirements 1.1, 1.4**

  - [x] 1.4 Write property test for gitignore completeness
    - **Property 2: GitIgnore Pattern Completeness**
    - **Validates: Requirements 1.2, 1.3**

- [x] 2. Phase 1 Checkpoint - Verify clean structure
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Phase 2: Directory and Namespace Refactoring
  - [x] 3.1 Rename Services/UG directory to Services/UnidadesGestoras
    - Rename the physical directory
    - Update all namespace declarations in affected files
    - Update all using statements throughout the project
    - _Requirements: 2.1, 2.2, 2.3_

  - [x] 3.2 Validate namespace hierarchy consistency
    - Ensure all namespaces match directory structure
    - Verify no orphaned namespace references remain
    - _Requirements: 2.4_

  - [x] 3.3 Write property test for namespace transformation
    - **Property 3: Namespace Transformation Consistency**
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4**

  - [x] 3.4 Evaluate and consolidate capture services
    - Analyze Camera, OCR, and Barcode services for consolidation opportunities
    - If beneficial, create unified CapturaService with all functionality
    - Preserve all existing public methods and interfaces
    - _Requirements: 3.1, 3.2, 3.3_

  - [x] 3.5 Write property test for service consolidation
    - **Property 4: Service Consolidation Preservation**
    - **Validates: Requirements 3.2, 3.3**

- [x] 4. Phase 2 Checkpoint - Verify namespace consistency
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Phase 3: Model and Service Localization
  - [x] 5.1 Refactor domain models to Portuguese naming
    - Rename InventoryItem class to ItemPatrimonio
    - Rename properties: Name → Nome, Code → Codigo, Description → Descricao
    - Rename properties: CreatedDate → DataCriacao, ModifiedDate → DataModificacao
    - Rename properties: UserId → UsuarioId, OrganizationalUnitId → UnidadeGestoraId
    - Rename properties: IsActive → EstaAtivo
    - Update all references throughout the codebase
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

  - [x] 5.2 Write property test for model transformation
    - **Property 5: Model Transformation Completeness**
    - **Validates: Requirements 4.1, 4.2, 4.3, 4.4**

  - [x] 5.3 Refactor authentication service methods
    - Rename LoginAsync to AutenticarAsync
    - Rename LogoutAsync to DesconectarAsync
    - Rename ValidateUserAsync to ValidarUsuarioAsync
    - Update all method calls throughout the application
    - _Requirements: 5.1, 5.6_

  - [x] 5.4 Refactor camera service methods
    - Rename StartCameraAsync to IniciarCameraAsync
    - Rename StopCameraAsync to PararCameraAsync
    - Rename CaptureImageAsync to CapturarImagemAsync
    - Update all method calls throughout the application
    - _Requirements: 5.2, 5.6_

  - [x] 5.5 Refactor OCR service methods
    - Rename RecognizeTextAsync to ReconhecerTextoAsync
    - Rename ProcessImageAsync to ProcessarImagemAsync
    - Rename ExtractTextAsync to ExtrairTextoAsync
    - Update all method calls throughout the application
    - _Requirements: 5.3, 5.6_

  - [x] 5.6 Refactor synchronization service methods
    - Rename SyncAsync to SincronizarAsync
    - Rename SyncDataAsync to SincronizarDadosAsync
    - Rename GetSyncStatusAsync to ObterStatusSincronizacaoAsync
    - Update all method calls throughout the application
    - _Requirements: 5.4, 5.6_

  - [x] 5.7 Write property test for service method localization
    - **Property 6: Service Method Localization Consistency**
    - **Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5, 5.6**

- [x] 6. Phase 3 Checkpoint - Verify model and service changes
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Phase 4: Route and Component Localization
  - [x] 7.1 Update page routes to Portuguese
    - Change @page "/camera" to @page "/captura" in Camera.razor
    - Change @page "/sync" to @page "/sincronizacao" in Sync.razor
    - Update any other English routes to Portuguese equivalents
    - _Requirements: 6.1, 6.2, 6.4_

  - [x] 7.2 Update navigation references throughout application
    - Update NavigationManager.NavigateTo calls to use new routes
    - Update NavLink href attributes to use new routes
    - Update any hardcoded route strings in components
    - _Requirements: 6.3_

  - [x] 7.3 Write property test for route transformation
    - **Property 7: Route Transformation Completeness**
    - **Validates: Requirements 6.1, 6.2, 6.3, 6.4**

  - [x] 7.4 Write unit test for route parameter functionality
    - Test that route parameters continue to work with new Portuguese routes
    - _Requirements: 6.5_

- [x] 8. Phase 4: Naming Convention and Language Boundary Enforcement
  - [x] 8.1 Validate and fix naming conventions across codebase
    - Ensure all class names use PascalCase (OrquestradorCaptura)
    - Ensure all method names use PascalCase (ReconhecerTextoAsync)
    - Ensure all property names use PascalCase (UnidadeGestoraId)
    - Ensure all private fields use _camelCase (_servicoAutenticacao)
    - Ensure all local variables use camelCase (resultadoOperacao)
    - Ensure all constants use UPPER_SNAKE_CASE (TIMEOUT_PADRAO)
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6_

  - [x] 8.2 Write property test for naming convention compliance
    - **Property 8: Naming Convention Compliance**
    - **Validates: Requirements 7.1, 7.2, 7.3, 7.4, 7.5, 7.6**

  - [x] 8.3 Enforce language boundaries throughout codebase
    - Verify business logic uses Portuguese naming without accents
    - Verify framework/technical code uses English naming
    - Verify external API interfaces use English naming
    - Ensure consistent language choice within each module
    - _Requirements: 8.1, 8.2, 8.3, 8.4_

  - [x] 8.4 Write property test for language boundary enforcement
    - **Property 9: Language Boundary Enforcement**
    - **Validates: Requirements 8.1, 8.2, 8.3, 8.4**

- [x] 9. Phase 4: Domain Terminology Standardization
  - [x] 9.1 Standardize organizational unit terminology
    - Replace all "OrganizationalUnit" references with "UnidadeGestora"
    - Update comments, documentation, and string literals
    - _Requirements: 12.1_

  - [x] 9.2 Standardize inventory item terminology
    - Replace all "InventoryItem" references with "ItemPatrimonio"
    - Update comments, documentation, and string literals
    - _Requirements: 12.2_

  - [x] 9.3 Standardize scanning operation terminology
    - Replace "scan" references with "capturar" or "escanear"
    - Update comments, documentation, and string literals
    - _Requirements: 12.3_

  - [x] 9.4 Standardize synchronization terminology
    - Replace "sync" references with "sincronizar"
    - Update comments, documentation, and string literals
    - _Requirements: 12.4_

  - [x] 9.5 Standardize user and authentication terminology
    - Replace "user" references with "usuario"
    - Replace "auth" references with "autenticar"
    - Update comments, documentation, and string literals
    - _Requirements: 12.5, 12.6_

  - [x] 9.6 Write property test for domain terminology standardization
    - **Property 12: Domain Terminology Standardization**
    - **Validates: Requirements 12.1, 12.2, 12.3, 12.4, 12.5, 12.6**

- [x] 10. Phase 5: Encoding and Build Validation
  - [x] 10.1 Ensure UTF-8 encoding for all text files
    - Convert all .cs, .razor, .json, .md files to UTF-8 encoding
    - Verify proper handling of Portuguese characters
    - _Requirements: 11.1, 11.4_

  - [x] 10.2 Write property test for UTF-8 encoding consistency
    - **Property 11: UTF-8 Encoding Consistency**
    - **Validates: Requirements 11.1, 11.4**

  - [x] 10.3 Validate clean compilation
    - Build the project and ensure zero compilation errors
    - Build the project and ensure zero compilation warnings
    - Verify all namespace references are resolved
    - Verify all method signatures are consistent
    - _Requirements: 9.1, 9.2, 9.3, 9.4_

  - [x] 10.4 Write property test for build success
    - **Property 10: Build Success Guarantee**
    - **Validates: Requirements 9.1, 9.2, 9.3, 9.4**

- [x] 11. Phase 6: Functional Validation
  - [x] 11.1 Write functional test for navigation with new routes
    - Test that navigation works correctly with Portuguese routes
    - _Requirements: 10.1_

  - [x] 11.2 Write functional test for OCR functionality
    - Test that OCR works correctly with renamed methods
    - _Requirements: 10.2_

  - [x] 11.3 Write functional test for barcode scanning
    - Test that barcode scanning works correctly with renamed methods
    - _Requirements: 10.3_

  - [x] 11.4 Write functional test for AWS synchronization
    - Test that sync works correctly with renamed service methods
    - _Requirements: 10.4_

  - [x] 11.5 Write functional test for authentication flow
    - Test that authentication works correctly with renamed methods
    - _Requirements: 10.5_

  - [x] 11.6 Write functional test for Portuguese text handling
    - Test that Portuguese text is processed and displayed correctly
    - _Requirements: 11.2, 11.3_

- [x] 12. Final Checkpoint - Complete validation
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- All tasks are required for comprehensive refactoring validation
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation and provide rollback points
- Property tests validate universal correctness properties across the entire codebase
- Unit tests validate specific examples and edge cases
- The refactoring is designed to be executed in phases to minimize risk
- Each phase can be rolled back independently if issues are encountered