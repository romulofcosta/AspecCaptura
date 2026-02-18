# Design Document: ASPEC Capture Refactoring

## Overview

This design document outlines the comprehensive refactoring approach for the ASPEC Capture .NET Blazor PWA application. The refactoring focuses on four main areas: project structure cleanup, semantic renaming to Portuguese domain language (without accents), naming convention compliance, and functional validation.

The refactoring will be executed in phases to minimize risk and ensure incremental validation. Each phase builds upon the previous one, allowing for rollback if issues are encountered.

## Architecture

### Current Architecture
The ASPEC Capture application follows a typical .NET Blazor PWA architecture:

```
ASPEC.Capture/
├── Services/
│   ├── UG/                    # Unidades Gestoras (to be renamed)
│   ├── Camera/                # Camera capture services
│   ├── OCR/                   # Text recognition services
│   ├── Barcode/               # Barcode scanning services
│   ├── Auth/                  # Authentication services
│   └── Sync/                  # AWS synchronization services
├── Models/
│   ├── InventoryItem.cs       # Main domain model (to be renamed)
│   └── Other domain models
├── Pages/
│   ├── Camera.razor           # Camera capture page
│   ├── Sync.razor             # Synchronization page
│   └── Other pages
├── Components/
└── wwwroot/
```

### Target Architecture
After refactoring, the architecture will align with Portuguese domain terminology and .NET best practices:

```
ASPEC.Capture/
├── scripts/                   # Build scripts (moved from root)
├── Services/
│   ├── UnidadesGestoras/      # Renamed from UG/
│   ├── Captura/               # Consolidated capture services
│   ├── Autenticacao/          # Authentication services
│   └── Sincronizacao/         # AWS synchronization services
├── Models/
│   ├── ItemPatrimonio.cs      # Renamed from InventoryItem
│   └── Other domain models (Portuguese names)
├── Pages/
│   ├── Captura.razor          # Renamed from Camera.razor
│   ├── Sincronizacao.razor    # Renamed from Sync.razor
│   └── Other pages
├── Components/
└── wwwroot/
```

## Components and Interfaces

### 1. Project Structure Manager
**Responsibility**: Manages file and directory organization

**Key Operations**:
- Move build scripts to scripts/ folder
- Update .gitignore patterns
- Verify ignored directories

**Interface**:
```csharp
public interface IEstruturaProjetoManager
{
    Task OrganizarScriptsBuildAsync();
    Task AtualizarGitIgnoreAsync();
    Task VerificarDiretoriosIgnoradosAsync();
}
```

### 2. Namespace Refactoring Engine
**Responsibility**: Handles namespace and using statement updates

**Key Operations**:
- Rename directory Services/UG/ → Services/UnidadesGestoras/
- Update all namespace declarations
- Update all using statements

**Interface**:
```csharp
public interface INamespaceRefactoringEngine
{
    Task RenomearDiretorioAsync(string caminhoAntigo, string caminhoNovo);
    Task AtualizarNamespacesAsync(string namespaceAntigo, string namespaceNovo);
    Task AtualizarUsingStatementsAsync(string usingAntigo, string usingNovo);
}
```

### 3. Model Localization Service
**Responsibility**: Converts English model names to Portuguese equivalents

**Key Operations**:
- Rename classes (InventoryItem → ItemPatrimonio)
- Rename properties (Name → Nome, Code → Codigo)
- Update all references

**Interface**:
```csharp
public interface IModelLocalizationService
{
    Task LocalizarModeloAsync(string nomeClasse, Dictionary<string, string> mapeamentoPropriedades);
    Task AtualizarReferenciasModeloAsync(string nomeAntigo, string nomeNovo);
}
```

### 4. Service Method Localizer
**Responsibility**: Localizes service method names to Portuguese

**Key Operations**:
- Rename authentication methods (LoginAsync → AutenticarAsync)
- Rename camera methods (StartCameraAsync → IniciarCameraAsync)
- Rename OCR methods (RecognizeTextAsync → ReconhecerTextoAsync)
- Rename sync methods (SyncAsync → SincronizarAsync)

**Interface**:
```csharp
public interface IServiceMethodLocalizer
{
    Task LocalizarMetodosAutenticacaoAsync();
    Task LocalizarMetodosCameraAsync();
    Task LocalizarMetodosOCRAsync();
    Task LocalizarMetodosSincronizacaoAsync();
}
```

### 5. Route Localization Manager
**Responsibility**: Updates page routes and navigation to Portuguese

**Key Operations**:
- Update @page directives (/camera → /captura, /sync → /sincronizacao)
- Update navigation references
- Maintain route parameter functionality

**Interface**:
```csharp
public interface IRouteLocalizationManager
{
    Task AtualizarRotasPaginasAsync();
    Task AtualizarReferenciaNavegacaoAsync();
    Task ValidarParametrosRotaAsync();
}
```

### 6. Naming Convention Enforcer
**Responsibility**: Ensures consistent naming conventions throughout codebase

**Key Operations**:
- Validate PascalCase for classes, methods, properties
- Validate _camelCase for private fields
- Validate camelCase for local variables
- Validate UPPER_SNAKE_CASE for constants

**Interface**:
```csharp
public interface INamingConventionEnforcer
{
    Task ValidarConvencaoClassesAsync();
    Task ValidarConvencaoMetodosAsync();
    Task ValidarConvencaoPropriedadesAsync();
    Task ValidarConvencaoCamposPrivadosAsync();
    Task ValidarConvencaoConstantesAsync();
}
```

### 7. Build Validation Service
**Responsibility**: Ensures clean compilation after refactoring

**Key Operations**:
- Compile project and check for errors
- Validate namespace resolution
- Check method signature consistency

**Interface**:
```csharp
public interface IBuildValidationService
{
    Task<BuildResult> CompilarProjetoAsync();
    Task<bool> ValidarNamespacesAsync();
    Task<bool> ValidarAssinaturasMetodosAsync();
}
```

### 8. Functional Test Orchestrator
**Responsibility**: Validates application functionality after refactoring

**Key Operations**:
- Test navigation with new routes
- Test OCR functionality with renamed methods
- Test barcode scanning with renamed methods
- Test synchronization with AWS
- Test authentication flow

**Interface**:
```csharp
public interface IFunctionalTestOrchestrator
{
    Task TestarNavegacaoAsync();
    Task TestarFuncionalidadeOCRAsync();
    Task TestarEscaneamentoCodigoBarrasAsync();
    Task TestarSincronizacaoAWSAsync();
    Task TestarFluxoAutenticacaoAsync();
}
```

## Data Models

### Domain Model Mappings

**Current → Target Model Names**:
- `InventoryItem` → `ItemPatrimonio`
- `OrganizationalUnit` → `UnidadeGestora`
- `User` → `Usuario`
- `SyncResult` → `ResultadoSincronizacao`
- `CameraSettings` → `ConfiguracoesCamera`
- `OCRResult` → `ResultadoOCR`
- `BarcodeData` → `DadosCodigoBarras`

**Property Name Mappings**:
- `Name` → `Nome`
- `Code` → `Codigo`
- `Description` → `Descricao`
- `CreatedDate` → `DataCriacao`
- `ModifiedDate` → `DataModificacao`
- `UserId` → `UsuarioId`
- `OrganizationalUnitId` → `UnidadeGestoraId`
- `IsActive` → `EstaAtivo`
- `Status` → `Status` (remains English as it's often an enum)

### Example Refactored Model

**Before**:
```csharp
public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public int OrganizationalUnitId { get; set; }
    public bool IsActive { get; set; }
}
```

**After**:
```csharp
public class ItemPatrimonio
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Codigo { get; set; }
    public string Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    public int UnidadeGestoraId { get; set; }
    public bool EstaAtivo { get; set; }
}
```

## Error Handling

### Refactoring Error Categories

1. **Compilation Errors**
   - Missing namespace references
   - Unresolved method calls
   - Type mismatch after renaming

2. **Runtime Errors**
   - Route not found after URL changes
   - Method not found after service renaming
   - Serialization issues with renamed properties

3. **Functional Errors**
   - OCR not working after method renaming
   - Camera not starting after service changes
   - AWS sync failing after model changes

### Error Recovery Strategy

**Phase-by-Phase Rollback**:
- Each refactoring phase creates a backup point
- If errors occur, rollback to previous stable state
- Fix issues before proceeding to next phase

**Validation Gates**:
- Compilation validation after each major change
- Functional testing after each service refactoring
- End-to-end testing after route changes

**Error Logging**:
```csharp
public class RefactoringErrorLogger
{
    public void LogCompilationError(string arquivo, string erro);
    public void LogRuntimeError(string componente, Exception excecao);
    public void LogFunctionalError(string funcionalidade, string detalhes);
}
```

## Testing Strategy

### Dual Testing Approach

The testing strategy combines unit testing for specific scenarios with property-based testing for comprehensive validation of refactoring rules.

**Unit Testing Focus**:
- Specific refactoring transformations (e.g., "InventoryItem" → "ItemPatrimonio")
- Edge cases in file processing (empty files, special characters)
- Integration points between refactoring components
- Error conditions and recovery scenarios

**Property-Based Testing Focus**:
- Universal refactoring properties that must hold across all code files
- Naming convention compliance across the entire codebase
- Namespace consistency after bulk transformations
- Route mapping correctness for all page components

### Property-Based Testing Configuration

**Testing Library**: Use **FsCheck** for .NET property-based testing
**Test Configuration**: Minimum 100 iterations per property test
**Test Tagging**: Each property test references its design document property using format:
`// Feature: aspec-capture-refactoring, Property {number}: {property_text}`

### Unit Test Examples

```csharp
[Test]
public void DeveRenomearInventoryItemParaItemPatrimonio()
{
    // Test specific model renaming
    var resultado = modelLocalizer.LocalizarModelo("InventoryItem", "ItemPatrimonio");
    Assert.That(resultado.NovoNome, Is.EqualTo("ItemPatrimonio"));
}

[Test]
public void DeveManterFuncionalidadeAposRenomearMetodo()
{
    // Test that LoginAsync → AutenticarAsync preserves functionality
    var servicoAuth = new ServicoAutenticacao();
    var resultado = servicoAuth.AutenticarAsync("usuario", "senha");
    Assert.That(resultado, Is.Not.Null);
}
```

### Integration Testing

**Route Testing**:
- Verify all old routes redirect to new Portuguese routes
- Test navigation components use updated route references
- Validate route parameters continue to work

**Service Integration Testing**:
- Test renamed services integrate correctly with dependency injection
- Verify AWS services work with renamed models
- Test OCR and camera services function with new method names

**End-to-End Testing**:
- Complete user workflows (login → capture → sync)
- Cross-browser compatibility with Portuguese routes
- PWA functionality with updated service worker

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: File Organization Consistency
*For any* project directory structure, after refactoring, all build scripts should be located in the scripts/ folder and the project root should contain only essential configuration files
**Validates: Requirements 1.1, 1.4**

### Property 2: GitIgnore Pattern Completeness  
*For any* .gitignore file after refactoring, it should contain patterns that exclude build logs, agent tools, temporary files, bin/ folders, and obj/ folders
**Validates: Requirements 1.2, 1.3**

### Property 3: Namespace Transformation Consistency
*For any* C# file in the project, if it was in the Services/UG/ namespace, it should now be in the Services/UnidadesGestoras/ namespace, and all using statements should reference the new namespace
**Validates: Requirements 2.1, 2.2, 2.3, 2.4**

### Property 4: Service Consolidation Preservation
*For any* consolidated capture service, all public methods from the original Camera, OCR, and Barcode services should be available and functional
**Validates: Requirements 3.2, 3.3**

### Property 5: Model Transformation Completeness
*For any* domain model class, if it was named in English (like InventoryItem), it should be renamed to Portuguese without accents (like ItemPatrimonio), and all references throughout the codebase should use the new name
**Validates: Requirements 4.1, 4.2, 4.3, 4.4**

### Property 6: Service Method Localization Consistency
*For any* service method that represents business logic, it should use Portuguese naming without accents (LoginAsync → AutenticarAsync), maintain the Async suffix, and all method calls should use the new names
**Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5, 5.6**

### Property 7: Route Transformation Completeness
*For any* page component with routes, English routes should be transformed to Portuguese (/camera → /captura), all @page directives should be updated, and all navigation references should use the new routes
**Validates: Requirements 6.1, 6.2, 6.3, 6.4**

### Property 8: Naming Convention Compliance
*For any* C# identifier in the codebase, it should follow the appropriate naming convention: PascalCase for classes/methods/properties, _camelCase for private fields, camelCase for local variables, and UPPER_SNAKE_CASE for constants
**Validates: Requirements 7.1, 7.2, 7.3, 7.4, 7.5, 7.6**

### Property 9: Language Boundary Enforcement
*For any* code module, business logic should use Portuguese naming without accents, while framework/technical code and external API interfaces should use English naming, with consistent language choice within each module
**Validates: Requirements 8.1, 8.2, 8.3, 8.4**

### Property 10: Build Success Guarantee
*For any* refactored codebase, compilation should complete with zero errors and zero warnings, with all namespace references resolved
**Validates: Requirements 9.1, 9.2, 9.3, 9.4**

### Property 11: UTF-8 Encoding Consistency
*For any* text file in the project, it should use UTF-8 encoding to ensure proper handling of Portuguese characters
**Validates: Requirements 11.1, 11.4**

### Property 12: Domain Terminology Standardization
*For any* reference to domain concepts in the codebase, it should use consistent Portuguese terminology: UnidadeGestora for organizational units, ItemPatrimonio for inventory items, Capturar/Escanear for scanning, Sincronizar for synchronization, Usuario for users, and Autenticar for authentication
**Validates: Requirements 12.1, 12.2, 12.3, 12.4, 12.5, 12.6**

### Testing Strategy

**Dual Testing Approach**:
The refactoring validation uses both unit testing for specific transformations and property-based testing for comprehensive rule verification across the entire codebase.

**Unit Testing Focus**:
- Specific refactoring transformations (e.g., "InventoryItem" → "ItemPatrimonio")
- Edge cases in file processing (empty files, special characters, binary files)
- Integration points between refactoring components
- Error conditions and recovery scenarios during refactoring
- Functional validation examples (navigation works, OCR functions, authentication succeeds)

**Property-Based Testing Focus**:
- Universal refactoring properties that must hold across all code files
- Naming convention compliance across the entire codebase  
- Namespace consistency after bulk transformations
- Route mapping correctness for all page components
- Domain terminology consistency across all references

**Property-Based Testing Configuration**:
- **Testing Library**: FsCheck for .NET property-based testing
- **Test Configuration**: Minimum 100 iterations per property test to ensure comprehensive coverage
- **Test Tagging**: Each property test must include a comment referencing its design document property:
  ```csharp
  // Feature: aspec-capture-refactoring, Property 1: File Organization Consistency
  ```

**Testing Phases**:

1. **Pre-Refactoring Validation**:
   - Baseline compilation success
   - Current functionality verification
   - Code structure analysis

2. **Incremental Refactoring Testing**:
   - After each phase: compilation validation
   - After service changes: functional testing
   - After route changes: navigation testing

3. **Post-Refactoring Validation**:
   - Complete property-based test suite execution
   - End-to-end functional testing
   - Performance regression testing

**Test Data Generation**:
- Generate various C# code patterns for naming convention testing
- Generate different file structures for organization testing
- Generate Portuguese text samples for encoding testing
- Generate route configurations for navigation testing

**Rollback Testing**:
- Verify rollback procedures work at each phase
- Test recovery from partial refactoring states
- Validate backup and restore functionality