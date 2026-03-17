# Design: Suíte de Testes Abrangente

## Visão Geral da Arquitetura

A suíte de testes será estruturada em camadas hierárquicas, cobrindo desde testes unitários até testes end-to-end, com foco especial em interoperabilidade JavaScript/C# e componentes Blazor.

## 1. Arquitetura de Testes

### 1.1 Pirâmide de Testes
```
                    E2E Tests (5%)
                 ┌─────────────────┐
                 │  Integration    │
                ┌┴─────────────────┴┐
                │  Component Tests  │ (25%)
               ┌┴───────────────────┴┐
               │    Service Tests    │ (35%)
              ┌┴─────────────────────┴┐
              │     Unit Tests        │ (35%)
             └───────────────────────┘
```

### 1.2 Estrutura de Diretórios
```
tests/
├── Unit/                          # Testes unitários
│   ├── Services/                  # Testes de serviços
│   │   ├── Recognition/           # Serviços de reconhecimento
│   │   ├── Camera/               # Serviços de câmera
│   │   ├── Auth/                 # Serviços de autenticação
│   │   └── Sync/                 # Serviços de sincronização
│   ├── Models/                   # Testes de modelos
│   ├── Parsers/                  # Testes de parsers
│   └── Validators/               # Testes de validadores
├── Component/                     # Testes de componentes Blazor
│   ├── Pages/                    # Testes de páginas
│   ├── Layouts/                  # Testes de layouts
│   └── Shared/                   # Componentes compartilhados
├── Integration/                   # Testes de integração
│   ├── JavaScript/               # Interoperabilidade JS/C#
│   ├── Workers/                  # Testes de Web Workers
│   ├── Camera/                   # Integração com câmera
│   └── Recognition/              # Fluxo de reconhecimento
├── Performance/                   # Testes de performance
│   ├── Memory/                   # Uso de memória
│   ├── Throughput/               # Taxa de processamento
│   └── Responsiveness/           # Responsividade da UI
├── Security/                      # Testes de segurança
│   ├── Input/                    # Validação de entrada
│   ├── Auth/                     # Autenticação/autorização
│   └── XSS/                      # Proteção contra XSS
├── E2E/                          # Testes end-to-end
│   ├── Scenarios/                # Cenários completos
│   └── Regression/               # Testes de regressão
├── Fixtures/                     # Dados de teste
│   ├── Images/                   # Imagens para teste
│   ├── QRCodes/                  # QR codes de teste
│   └── MockData/                 # Dados mock
└── Helpers/                      # Utilitários de teste
    ├── Mocks/                    # Mocks e stubs
    ├── Builders/                 # Test data builders
    └── Extensions/               # Extensões de teste
```

## 2. Estratégias de Teste por Camada

### 2.1 Testes Unitários

#### 2.1.1 Serviços de Reconhecimento
```csharp
// Exemplo: RecognitionServiceTests.cs
public class RecognitionServiceTests
{
    private readonly Mock<IQRCodeService> _qrServiceMock;
    private readonly Mock<IOCRService> _ocrServiceMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly RecognitionService _service;

    [Fact]
    public async Task StartRecognitionAsync_ShouldInitializeServices()
    {
        // Arrange
        _qrServiceMock.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
        _ocrServiceMock.Setup(x => x.InitializeAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.StartRecognitionAsync("video-element");

        // Assert
        result.Should().BeTrue();
        _qrServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
        _ocrServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
    }
}
```

#### 2.1.2 Parsers e Validadores
```csharp
// Exemplo: QRParserTests.cs
public class QRParserTests
{
    [Theory]
    [InlineData("123456789", true)]
    [InlineData("ABC123DEF", true)]
    [InlineData("12-34-56", false)]
    public void IsValidPatrimonioCode_ShouldValidateCorrectly(string code, bool expected)
    {
        // Act
        var result = QRParser.IsValidPatrimonioCode(code);

        // Assert
        result.Should().Be(expected);
    }
}
```

### 2.2 Testes de Componentes

#### 2.2.1 Componentes Blazor com bUnit
```csharp
// Exemplo: CameraComponentTests.cs
public class CameraComponentTests : TestContext
{
    [Fact]
    public void Camera_ShouldRenderWithoutPermissions()
    {
        // Arrange
        var cameraServiceMock = new Mock<ICameraService>();
        cameraServiceMock.Setup(x => x.HasPermission).Returns(false);
        Services.AddSingleton(cameraServiceMock.Object);

        // Act
        var component = RenderComponent<Camera>();

        // Assert
        component.Find(".permission-request").Should().NotBeNull();
        component.Find(".camera-preview").Should().BeNull();
    }

    [Fact]
    public void Camera_ShouldShowRecognitionOverlay_WhenEnabled()
    {
        // Arrange
        var recognitionServiceMock = new Mock<IRecognitionService>();
        recognitionServiceMock.Setup(x => x.IsActive).Returns(true);
        Services.AddSingleton(recognitionServiceMock.Object);

        // Act
        var component = RenderComponent<Camera>();

        // Assert
        component.Find(".recognition-overlay").Should().NotBeNull();
    }
}
```

### 2.3 Testes de Integração

#### 2.3.1 Interoperabilidade JavaScript
```csharp
// Exemplo: JavaScriptInteropTests.cs
public class JavaScriptInteropTests
{
    [Fact]
    public async Task JSInvokable_OnQRDetected_ShouldProcessCorrectly()
    {
        // Arrange
        var service = new RecognitionService(/* dependencies */);
        var qrData = """{"code": "123456789", "confidence": 0.95}""";

        // Act
        await service.OnQRDetectedAsync(qrData);

        // Assert
        // Verificar se o evento foi disparado
        // Verificar se o código foi processado
    }
}
```

#### 2.3.2 Web Workers
```csharp
// Exemplo: WorkerIntegrationTests.cs
public class WorkerIntegrationTests
{
    [Fact]
    public async Task QRWorker_ShouldInitialize_WithExternalLibrary()
    {
        // Arrange
        var jsRuntime = new MockJSRuntime();
        jsRuntime.Setup("recognitionInterop.initialize").ReturnsAsync(true);

        // Act
        var result = await jsRuntime.InvokeAsync<bool>("recognitionInterop.initialize");

        // Assert
        result.Should().BeTrue();
    }
}
```

### 2.4 Testes de Performance

#### 2.4.1 Testes de Memória
```csharp
// Exemplo: MemoryUsageTests.cs
public class MemoryUsageTests
{
    [Fact]
    public void RecognitionService_ShouldNotLeakMemory_DuringLongSession()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var service = new RecognitionService(/* dependencies */);

        // Act
        for (int i = 0; i < 1000; i++)
        {
            // Simular processamento de frames
        }

        // Assert
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(10 * 1024 * 1024); // < 10MB
    }
}
```

### 2.5 Testes End-to-End

#### 2.5.1 Cenários Completos
```csharp
// Exemplo: QRRecognitionE2ETests.cs
public class QRRecognitionE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CompleteQRFlow_ShouldWork_EndToEnd()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        // 1. Navegar para página da câmera
        // 2. Simular detecção de QR code
        // 3. Verificar preenchimento automático
        // 4. Salvar formulário
        // 5. Verificar sincronização

        // Assert
        // Verificar que o item foi salvo corretamente
    }
}
```

## 3. Mocking e Fixtures

### 3.1 Mocks de JavaScript APIs
```csharp
// MockJSRuntime.cs
public class MockJSRuntime : IJSRuntime
{
    private readonly Dictionary<string, object> _setupMethods = new();

    public void Setup(string identifier, object returnValue)
    {
        _setupMethods[identifier] = returnValue;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
    {
        if (_setupMethods.TryGetValue(identifier, out var value))
        {
            return ValueTask.FromResult((TValue)value);
        }
        return ValueTask.FromResult(default(TValue));
    }
}
```

### 3.2 Test Data Builders
```csharp
// InventoryItemBuilder.cs
public class InventoryItemBuilder
{
    private InventoryItem _item = new();

    public InventoryItemBuilder WithCode(string code)
    {
        _item.Code = code;
        return this;
    }

    public InventoryItemBuilder WithName(string name)
    {
        _item.Name = name;
        return this;
    }

    public InventoryItemBuilder AsSynced()
    {
        _item.Synced = true;
        return this;
    }

    public InventoryItem Build() => _item;
}
```

### 3.3 Fixtures de Dados
```csharp
// TestFixtures.cs
public static class TestFixtures
{
    public static class QRCodes
    {
        public const string ValidPatrimonio = "123456789";
        public const string InvalidFormat = "ABC-DEF-GHI";
        public const string TooShort = "123";
    }

    public static class Images
    {
        public static byte[] QRCodeImage => LoadTestImage("qr-code-sample.png");
        public static byte[] TextImage => LoadTestImage("text-sample.png");
        public static byte[] NoiseImage => LoadTestImage("noise-sample.png");
    }
}
```

## 4. Configuração de Ambiente

### 4.1 Projeto de Testes Atualizado
```xml
<!-- Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Testing Frameworks -->
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    
    <!-- Blazor Testing -->
    <PackageReference Include="bunit" Version="1.24.10" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    
    <!-- Mocking and Assertions -->
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    
    <!-- Performance Testing -->
    <PackageReference Include="NBomber" Version="5.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../pwa-camera-poc-blazor.csproj" />
  </ItemGroup>
</Project>
```

### 4.2 Configuração de CI/CD
```yaml
# .github/workflows/tests.yml
name: Test Suite
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Run tests
      run: dotnet test --collect:"XPlat Code Coverage"
    
    - name: Generate coverage report
      run: |
        dotnet tool install -g dotnet-reportgenerator-globaltool
        reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
    
    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

## 5. Métricas e Relatórios

### 5.1 Cobertura de Código
- **Meta**: > 80% para código crítico
- **Ferramentas**: Coverlet + ReportGenerator
- **Relatórios**: HTML + XML para CI/CD

### 5.2 Métricas de Qualidade
- **Complexidade Ciclomática**: < 10 por método
- **Duplicação de Código**: < 5%
- **Débito Técnico**: Tracking contínuo
- **Performance**: Tempo de execução < 5min

### 5.3 Dashboards
- **Cobertura por Módulo**: Gráficos de tendência
- **Falhas por Categoria**: Análise de padrões
- **Performance de Testes**: Tempo de execução
- **Qualidade de Código**: Métricas SonarQube

## 6. Estratégia de Execução

### 6.1 Desenvolvimento Local
```bash
# Executar todos os testes
dotnet test

# Executar categoria específica
dotnet test --filter Category=Unit

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes de performance
dotnet test --filter Category=Performance
```

### 6.2 Pipeline de CI/CD
1. **Commit**: Testes unitários rápidos
2. **PR**: Testes de integração + cobertura
3. **Merge**: Suíte completa + E2E
4. **Deploy**: Smoke tests + monitoramento

### 6.3 Estratégia de Paralelização
- **Testes Unitários**: Paralelos por classe
- **Testes de Integração**: Sequenciais por recurso
- **Testes E2E**: Isolados por cenário
- **Testes de Performance**: Dedicados

## 7. Manutenção e Evolução

### 7.1 Revisão Contínua
- **Semanal**: Análise de cobertura
- **Mensal**: Revisão de testes lentos
- **Trimestral**: Refatoração de mocks
- **Anual**: Atualização de frameworks

### 7.2 Documentação Viva
- **README**: Guia de execução
- **Wiki**: Estratégias e padrões
- **Comentários**: Casos complexos
- **Exemplos**: Novos desenvolvedores

Esta arquitetura garante cobertura completa, execução eficiente e manutenibilidade a longo prazo da suíte de testes.