# Requirements Document

## Introduction

This document specifies the requirements for the ASPEC Capture refactoring project, which involves executing structural and naming refactoring to align the .NET Blazor PWA codebase with best practices and Portuguese domain language (without accents).

## Glossary

- **ASPEC_System**: The ASPEC Capture .NET Blazor PWA application
- **Build_Scripts**: PowerShell and batch files used for project compilation and deployment
- **UnidadeGestora**: Organizational unit (formerly "Unidade Organizadora")
- **ItemPatrimonio**: Inventory item in the patrimony system
- **Captura_Service**: Service responsible for camera, OCR, and barcode capture functionality
- **Sincronizacao_Service**: Service responsible for data synchronization with AWS
- **Autenticacao_Service**: Service responsible for user authentication
- **Portuguese_Domain**: Business logic components using Portuguese naming without accents
- **English_Technical**: Framework and technical components using English naming
- **UTF8_Encoding**: Unicode text encoding standard for proper Portuguese character support

## Requirements

### Requirement 1: Project Structure Cleanup

**User Story:** As a developer, I want a clean and organized project structure, so that the codebase follows .NET Blazor best practices and is easier to maintain.

#### Acceptance Criteria

1. WHEN build scripts exist in the project root, THE ASPEC_System SHALL move them to a scripts/ folder
2. WHEN the .gitignore file is updated, THE ASPEC_System SHALL exclude build logs, agent tools, and temporary files
3. WHEN checking ignored folders, THE ASPEC_System SHALL verify bin/ and obj/ folders are properly excluded from version control
4. THE ASPEC_System SHALL maintain a clean project root with only essential configuration files

### Requirement 2: Semantic Directory Renaming

**User Story:** As a developer, I want directory names to reflect the correct Portuguese domain terminology, so that the code structure aligns with business language.

#### Acceptance Criteria

1. WHEN the Services/UG/ directory exists, THE ASPEC_System SHALL rename it to Services/UnidadesGestoras/
2. WHEN directory renaming occurs, THE ASPEC_System SHALL update all corresponding namespace declarations
3. WHEN directory renaming occurs, THE ASPEC_System SHALL update all using statements that reference the old namespace
4. THE ASPEC_System SHALL maintain consistent namespace hierarchy after renaming

### Requirement 3: Service Consolidation and Organization

**User Story:** As a developer, I want capture-related services properly organized, so that related functionality is grouped logically.

#### Acceptance Criteria

1. WHEN multiple capture services exist (Camera, OCR, Barcode), THE ASPEC_System SHALL evaluate consolidation opportunities
2. IF consolidation is beneficial, THE ASPEC_System SHALL merge related capture services into a unified Captura_Service
3. WHEN services are consolidated, THE ASPEC_System SHALL preserve all existing functionality
4. THE ASPEC_System SHALL maintain clear separation of concerns between different service types

### Requirement 4: Model Localization to Portuguese

**User Story:** As a developer, I want business domain models to use Portuguese naming without accents, so that the code reflects the business language naturally.

#### Acceptance Criteria

1. WHEN the InventoryItem model exists, THE ASPEC_System SHALL rename it to ItemPatrimonio
2. WHEN model properties use English names, THE ASPEC_System SHALL rename them to Portuguese equivalents (Name → Nome, Code → Codigo)
3. WHEN model renaming occurs, THE ASPEC_System SHALL update all references throughout the codebase
4. THE ASPEC_System SHALL ensure all Portuguese names exclude accented characters
5. THE ASPEC_System SHALL maintain PascalCase naming convention for all model classes and properties

### Requirement 5: Service Method Localization

**User Story:** As a developer, I want business logic methods to use Portuguese naming, so that the code is more intuitive for Portuguese-speaking developers.

#### Acceptance Criteria

1. WHEN authentication methods exist, THE ASPEC_System SHALL rename LoginAsync to AutenticarAsync
2. WHEN camera methods exist, THE ASPEC_System SHALL rename StartCameraAsync to IniciarCameraAsync
3. WHEN OCR methods exist, THE ASPEC_System SHALL rename text recognition methods to ReconhecerTextoAsync
4. WHEN synchronization methods exist, THE ASPEC_System SHALL rename sync methods to use Sincronizar prefix
5. THE ASPEC_System SHALL maintain async method naming patterns with Async suffix
6. THE ASPEC_System SHALL update all method calls throughout the application

### Requirement 6: Page and Component Route Localization

**User Story:** As a user, I want application routes to use Portuguese terminology, so that the URLs are more intuitive for Portuguese speakers.

#### Acceptance Criteria

1. WHEN the /camera route exists, THE ASPEC_System SHALL rename it to /captura
2. WHEN the /sync route exists, THE ASPEC_System SHALL rename it to /sincronizacao
3. WHEN route changes occur, THE ASPEC_System SHALL update all navigation references
4. WHEN route changes occur, THE ASPEC_System SHALL update all @page directives in components
5. THE ASPEC_System SHALL ensure all route parameters continue to function correctly

### Requirement 7: Naming Convention Compliance

**User Story:** As a developer, I want consistent naming conventions throughout the codebase, so that the code follows established .NET standards.

#### Acceptance Criteria

1. THE ASPEC_System SHALL apply PascalCase to all class names (OrquestradorCaptura)
2. THE ASPEC_System SHALL apply PascalCase to all method names (ReconhecerTextoAsync)
3. THE ASPEC_System SHALL apply PascalCase to all property names (UnidadeGestoraId)
4. THE ASPEC_System SHALL apply _camelCase to all private field names
5. THE ASPEC_System SHALL apply camelCase to all local variable names
6. THE ASPEC_System SHALL apply UPPER_SNAKE_CASE to all constant names

### Requirement 8: Language Boundary Enforcement

**User Story:** As a developer, I want clear language boundaries in the codebase, so that Portuguese is used for business logic and English for technical/framework code.

#### Acceptance Criteria

1. WHEN code represents business logic, THE ASPEC_System SHALL use Portuguese_Domain naming without accents
2. WHEN code represents framework or technical functionality, THE ASPEC_System SHALL use English_Technical naming
3. WHEN code interfaces with external APIs, THE ASPEC_System SHALL use English_Technical naming
4. THE ASPEC_System SHALL maintain consistent language choice within each code module

### Requirement 9: Build and Compilation Validation

**User Story:** As a developer, I want the refactored code to compile cleanly, so that no regressions are introduced during refactoring.

#### Acceptance Criteria

1. WHEN the refactoring is complete, THE ASPEC_System SHALL compile with zero build errors
2. WHEN the refactoring is complete, THE ASPEC_System SHALL compile with zero build warnings
3. WHEN compilation occurs, THE ASPEC_System SHALL verify all namespace references are resolved
4. WHEN compilation occurs, THE ASPEC_System SHALL verify all method signatures are consistent

### Requirement 10: Functional Validation

**User Story:** As a user, I want all application functionality to work correctly after refactoring, so that the user experience is preserved.

#### Acceptance Criteria

1. WHEN navigation occurs, THE ASPEC_System SHALL route to the correct pages using new Portuguese routes
2. WHEN OCR functionality is used, THE ASPEC_System SHALL recognize text correctly with renamed methods
3. WHEN barcode scanning is used, THE ASPEC_System SHALL capture barcodes correctly with renamed methods
4. WHEN synchronization occurs, THE ASPEC_System SHALL sync data with AWS using renamed service methods
5. WHEN authentication occurs, THE ASPEC_System SHALL authenticate users correctly with renamed methods

### Requirement 11: Character Encoding Compliance

**User Story:** As a developer, I want proper character encoding support, so that Portuguese text is displayed and processed correctly.

#### Acceptance Criteria

1. THE ASPEC_System SHALL use UTF8_Encoding for all text files
2. WHEN Portuguese text is processed, THE ASPEC_System SHALL handle characters correctly without corruption
3. WHEN Portuguese text is displayed, THE ASPEC_System SHALL render characters correctly in the UI
4. THE ASPEC_System SHALL maintain encoding consistency across all project files

### Requirement 12: Domain Terminology Standardization

**User Story:** As a business stakeholder, I want consistent domain terminology throughout the application, so that the code reflects our business language accurately.

#### Acceptance Criteria

1. WHEN organizational units are referenced, THE ASPEC_System SHALL use UnidadeGestora terminology consistently
2. WHEN inventory items are referenced, THE ASPEC_System SHALL use ItemPatrimonio terminology consistently
3. WHEN scanning operations are referenced, THE ASPEC_System SHALL use Capturar or Escanear terminology consistently
4. WHEN synchronization operations are referenced, THE ASPEC_System SHALL use Sincronizar terminology consistently
5. WHEN user operations are referenced, THE ASPEC_System SHALL use Usuario terminology consistently
6. WHEN authentication operations are referenced, THE ASPEC_System SHALL use Autenticar terminology consistently