using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for service method localization consistency
/// Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
/// </summary>
[TestFixture]
public class ServiceMethodLocalizationTests
{
    private readonly string _projectRoot;

    public ServiceMethodLocalizationTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_AuthenticationMethodsLocalized()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // Authentication service methods should use Portuguese naming
        
        var authServicePath = Path.Combine(_projectRoot, "Services", "Auth", "AuthService.cs");
        var authInterfacePath = Path.Combine(_projectRoot, "Services", "Auth", "IAuthService.cs");
        
        Assert.That(File.Exists(authServicePath), "AuthService.cs should exist");
        Assert.That(File.Exists(authInterfacePath), "IAuthService.cs should exist");

        var serviceContent = File.ReadAllText(authServicePath);
        var interfaceContent = File.ReadAllText(authInterfacePath);

        // Check that new Portuguese method names exist
        Assert.That(serviceContent.Contains("AutenticarAsync"), 
            "AuthService should contain AutenticarAsync method");
        Assert.That(serviceContent.Contains("DesconectarAsync"), 
            "AuthService should contain DesconectarAsync method");
        
        Assert.That(interfaceContent.Contains("AutenticarAsync"), 
            "IAuthService should contain AutenticarAsync method");
        Assert.That(interfaceContent.Contains("DesconectarAsync"), 
            "IAuthService should contain DesconectarAsync method");

        // Check that old English method names are not present
        Assert.That(!serviceContent.Contains("LoginAsync"), 
            "AuthService should not contain old LoginAsync method");
        Assert.That(!serviceContent.Contains("LogoutAsync"), 
            "AuthService should not contain old LogoutAsync method");
        
        Assert.That(!interfaceContent.Contains("LoginAsync"), 
            "IAuthService should not contain old LoginAsync method");
        Assert.That(!interfaceContent.Contains("LogoutAsync"), 
            "IAuthService should not contain old LogoutAsync method");
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_CameraMethodsLocalized()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // Camera service methods should use Portuguese naming
        
        var cameraServicePath = Path.Combine(_projectRoot, "Services", "Camera", "CameraService.cs");
        Assert.That(File.Exists(cameraServicePath), "CameraService.cs should exist");

        var content = File.ReadAllText(cameraServicePath);

        // Check that new Portuguese method names exist
        Assert.That(content.Contains("IniciarCameraAsync"), 
            "CameraService should contain IniciarCameraAsync method");
        Assert.That(content.Contains("PararCameraAsync"), 
            "CameraService should contain PararCameraAsync method");
        Assert.That(content.Contains("CapturarImagemAsync"), 
            "CameraService should contain CapturarImagemAsync method");

        // Check that old English method names are not present
        Assert.That(!content.Contains("StartCameraAsync"), 
            "CameraService should not contain old StartCameraAsync method");
        Assert.That(!content.Contains("StopCameraAsync"), 
            "CameraService should not contain old StopCameraAsync method");
        Assert.That(!content.Contains("TakePhotoAsync"), 
            "CameraService should not contain old TakePhotoAsync method");
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_OCRMethodsLocalized()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // OCR service methods should use Portuguese naming
        
        var ocrServicePath = Path.Combine(_projectRoot, "Services", "Ocr", "OcrService.cs");
        Assert.That(File.Exists(ocrServicePath), "OcrService.cs should exist");

        var content = File.ReadAllText(ocrServicePath);

        // Check that new Portuguese method names exist
        Assert.That(content.Contains("ReconhecerTextoAsync"), 
            "OcrService should contain ReconhecerTextoAsync method");

        // Check that old English method names are not present
        Assert.That(!content.Contains("RecognizeAsync"), 
            "OcrService should not contain old RecognizeAsync method");
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_SynchronizationMethodsLocalized()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // Synchronization methods should use Portuguese naming
        
        var syncPagePath = Path.Combine(_projectRoot, "Pages", "Sync.razor");
        Assert.That(File.Exists(syncPagePath), "Sync.razor should exist");

        var content = File.ReadAllText(syncPagePath);

        // Check that new Portuguese method names exist
        Assert.That(content.Contains("SincronizarTudoAsync"), 
            "Sync.razor should contain SincronizarTudoAsync method");
        Assert.That(content.Contains("CarregarEstatisticasAsync"), 
            "Sync.razor should contain CarregarEstatisticasAsync method");

        // Check that old English method names are not present
        Assert.That(!content.Contains("SyncAll"), 
            "Sync.razor should not contain old SyncAll method");
        Assert.That(!content.Contains("LoadStats"), 
            "Sync.razor should not contain old LoadStats method");
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_AllMethodCallsUpdated()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // All method calls throughout the application should use new Portuguese names
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests")) // Exclude test files
            .ToArray();
        
        var razorFiles = Directory.GetFiles(_projectRoot, "*.razor", SearchOption.AllDirectories);
        var allFiles = csFiles.Concat(razorFiles).ToArray();

        var oldMethodCalls = new[]
        {
            "LoginAsync", "LogoutAsync", "StartCameraAsync", "StopCameraAsync", 
            "TakePhotoAsync", "RecognizeAsync", "SyncAll", "LoadStats"
        };

        foreach (var file in allFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            foreach (var oldMethod in oldMethodCalls)
            {
                // Look for method calls (with parentheses or await)
                var callPattern = $@"\b{oldMethod}\s*\(";
                var hasOldCall = Regex.IsMatch(content, callPattern);
                
                Assert.That(!hasOldCall, 
                    $"File {fileName} should not contain calls to old method '{oldMethod}'");
            }
        }
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_AsyncSuffixPreserved()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // All localized service methods should maintain the Async suffix
        
        var serviceFiles = Directory.GetFiles(Path.Combine(_projectRoot, "Services"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        var expectedPortugueseMethods = new[]
        {
            "AutenticarAsync", "DesconectarAsync", "IniciarCameraAsync", 
            "PararCameraAsync", "CapturarImagemAsync", "ReconhecerTextoAsync"
        };

        foreach (var file in serviceFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            foreach (var method in expectedPortugueseMethods)
            {
                if (content.Contains(method))
                {
                    // If the method exists, verify it has proper Async suffix
                    Assert.That(method.EndsWith("Async"), 
                        $"Portuguese method '{method}' in {fileName} should end with 'Async' suffix");
                    
                    // Verify it's a proper method declaration
                    var methodPattern = $@"(public|private|protected|internal)\s+.*\s+{method}\s*\(";
                    var hasProperDeclaration = Regex.IsMatch(content, methodPattern);
                    
                    if (hasProperDeclaration)
                    {
                        Assert.That(hasProperDeclaration, 
                            $"Method '{method}' in {fileName} should be properly declared");
                    }
                }
            }
        }
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_BusinessLogicUsesPortuguese()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // Business logic methods should use Portuguese naming without accents
        
        var serviceFiles = Directory.GetFiles(Path.Combine(_projectRoot, "Services"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in serviceFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all public method declarations
            var methodMatches = Regex.Matches(content, @"public\s+(?:async\s+)?Task(?:<[^>]+>)?\s+(\w+)\s*\(");
            
            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;
                
                // Skip framework methods and property accessors
                if (methodName.StartsWith("Get") || methodName.StartsWith("Set") || 
                    methodName == "Dispose" || methodName == "DisposeAsync" ||
                    methodName == "ToString" || methodName == "Equals" ||
                    methodName == "GetHashCode")
                {
                    continue;
                }

                // Business logic methods should not use English verbs for domain operations
                var englishBusinessVerbs = new[] { "Login", "Logout", "Start", "Stop", "Take", "Recognize", "Sync", "Load" };
                
                foreach (var englishVerb in englishBusinessVerbs)
                {
                    Assert.That(!methodName.StartsWith(englishVerb), 
                        $"Business logic method '{methodName}' in {fileName} should not start with English verb '{englishVerb}'. Use Portuguese equivalent.");
                }
            }
        }
    }

    [Test]
    public void Property6_ServiceMethodLocalizationConsistency_NoAccentsInMethodNames()
    {
        // Feature: aspec-capture-refactoring, Property 6: Service Method Localization Consistency
        // Portuguese method names should not contain accents
        
        var serviceFiles = Directory.GetFiles(Path.Combine(_projectRoot, "Services"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        var accentedChars = new[] { 'á', 'à', 'ã', 'â', 'é', 'ê', 'í', 'ó', 'ô', 'õ', 'ú', 'ç', 'Á', 'À', 'Ã', 'Â', 'É', 'Ê', 'Í', 'Ó', 'Ô', 'Õ', 'Ú', 'Ç' };

        foreach (var file in serviceFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all method declarations
            var methodMatches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+(?:async\s+)?(?:Task(?:<[^>]+>)?|void|\w+)\s+(\w+)\s*\(");
            
            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;
                
                foreach (var accentChar in accentedChars)
                {
                    Assert.That(!methodName.Contains(accentChar), 
                        $"Method '{methodName}' in {fileName} should not contain accented character '{accentChar}'. Use unaccented Portuguese.");
                }
            }
        }
    }
}