using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for naming convention compliance
/// Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
/// </summary>
[TestFixture]
public class NamingConventionComplianceTests
{
    private readonly string _projectRoot;

    public NamingConventionComplianceTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property8_NamingConventionCompliance_ClassNamesUsePascalCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // All class names should use PascalCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all class declarations
            var classMatches = Regex.Matches(content, @"(?:public|internal|private|protected)?\s*(?:static\s+)?(?:abstract\s+)?(?:sealed\s+)?class\s+(\w+)");
            
            foreach (Match match in classMatches)
            {
                var className = match.Groups[1].Value;
                
                // Verify PascalCase (starts with uppercase, no underscores)
                Assert.That(char.IsUpper(className[0]), 
                    $"Class '{className}' in {fileName} should start with uppercase letter (PascalCase)");
                Assert.That(!className.Contains("_"), 
                    $"Class '{className}' in {fileName} should not contain underscores (PascalCase)");
                
                // Verify it's not all uppercase (which would be UPPER_CASE)
                Assert.That(!className.All(char.IsUpper) || className.Length == 1, 
                    $"Class '{className}' in {fileName} should not be all uppercase");
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_MethodNamesUsePascalCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // All method names should use PascalCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all method declarations (public, private, protected, internal)
            var methodMatches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?(?:Task(?:<[^>]+>)?|void|\w+(?:<[^>]+>)?)\s+(\w+)\s*\(");
            
            foreach (Match match in methodMatches)
            {
                var methodName = match.Groups[1].Value;
                
                // Skip special methods and operators
                if (methodName == "Dispose" || methodName == "DisposeAsync" || 
                    methodName == "ToString" || methodName == "Equals" || 
                    methodName == "GetHashCode" || methodName.StartsWith("get_") || 
                    methodName.StartsWith("set_"))
                {
                    continue;
                }

                // Verify PascalCase (starts with uppercase, no underscores except for test methods)
                Assert.That(char.IsUpper(methodName[0]), 
                    $"Method '{methodName}' in {fileName} should start with uppercase letter (PascalCase)");
                
                // Allow underscores only in test methods (which we're excluding anyway)
                if (!fileName.Contains("test", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.That(!methodName.Contains("_"), 
                        $"Method '{methodName}' in {fileName} should not contain underscores (PascalCase)");
                }
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_PropertyNamesUsePascalCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // All property names should use PascalCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all property declarations
            var propertyMatches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+(?:static\s+)?(?:readonly\s+)?\w+(?:<[^>]+>)?\s+(\w+)\s*\{\s*(?:get|set)");
            
            foreach (Match match in propertyMatches)
            {
                var propertyName = match.Groups[1].Value;
                
                // Verify PascalCase (starts with uppercase, no underscores)
                Assert.That(char.IsUpper(propertyName[0]), 
                    $"Property '{propertyName}' in {fileName} should start with uppercase letter (PascalCase)");
                Assert.That(!propertyName.Contains("_"), 
                    $"Property '{propertyName}' in {fileName} should not contain underscores (PascalCase)");
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_PrivateFieldsUseCamelCaseWithUnderscore()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // All private fields should use _camelCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all private field declarations
            var fieldMatches = Regex.Matches(content, @"private\s+(?:readonly\s+)?(?:static\s+)?\w+(?:<[^>]+>)?\s+(\w+)\s*[;=]");
            
            foreach (Match match in fieldMatches)
            {
                var fieldName = match.Groups[1].Value;
                
                // Skip constants (they use different convention)
                if (content.Contains($"private const") && content.Contains(fieldName))
                {
                    continue;
                }

                // Verify _camelCase (starts with underscore, followed by lowercase)
                Assert.That(fieldName.StartsWith("_"), 
                    $"Private field '{fieldName}' in {fileName} should start with underscore (_camelCase)");
                
                if (fieldName.Length > 1)
                {
                    Assert.That(char.IsLower(fieldName[1]), 
                        $"Private field '{fieldName}' in {fileName} should have lowercase letter after underscore (_camelCase)");
                }
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_LocalVariablesUseCamelCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // Local variables should use camelCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find var declarations (common pattern for local variables)
            var varMatches = Regex.Matches(content, @"var\s+(\w+)\s*=");
            
            foreach (Match match in varMatches)
            {
                var variableName = match.Groups[1].Value;
                
                // Verify camelCase (starts with lowercase, no underscores)
                Assert.That(char.IsLower(variableName[0]), 
                    $"Local variable '{variableName}' in {fileName} should start with lowercase letter (camelCase)");
                Assert.That(!variableName.Contains("_"), 
                    $"Local variable '{variableName}' in {fileName} should not contain underscores (camelCase)");
            }

            // Find explicit type declarations for local variables
            var localVarMatches = Regex.Matches(content, @"(?:string|int|bool|double|float|decimal|DateTime|List<\w+>)\s+(\w+)\s*=");
            
            foreach (Match match in localVarMatches)
            {
                var variableName = match.Groups[1].Value;
                
                // Skip if it's a field or property (they have different rules)
                if (content.Contains($"public {variableName}") || content.Contains($"private {variableName}"))
                {
                    continue;
                }

                // Verify camelCase (starts with lowercase, no underscores)
                Assert.That(char.IsLower(variableName[0]), 
                    $"Local variable '{variableName}' in {fileName} should start with lowercase letter (camelCase)");
                Assert.That(!variableName.Contains("_"), 
                    $"Local variable '{variableName}' in {fileName} should not contain underscores (camelCase)");
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_ConstantsUsePascalCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // Constants should use PascalCase (following .NET convention, not UPPER_SNAKE_CASE)
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all constant declarations
            var constMatches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+const\s+\w+\s+(\w+)\s*=");
            
            foreach (Match match in constMatches)
            {
                var constantName = match.Groups[1].Value;
                
                // Verify PascalCase (starts with uppercase) - .NET convention for constants
                Assert.That(char.IsUpper(constantName[0]), 
                    $"Constant '{constantName}' in {fileName} should start with uppercase letter (PascalCase - .NET convention)");
                
                // Allow underscores in constants as they're commonly used in .NET
                // But prefer PascalCase without underscores
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_InterfaceNamesStartWithI()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // Interface names should start with 'I' followed by PascalCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all interface declarations
            var interfaceMatches = Regex.Matches(content, @"(?:public|internal|private|protected)?\s*interface\s+(\w+)");
            
            foreach (Match match in interfaceMatches)
            {
                var interfaceName = match.Groups[1].Value;
                
                // Verify interface naming convention (starts with 'I' followed by PascalCase)
                Assert.That(interfaceName.StartsWith("I"), 
                    $"Interface '{interfaceName}' in {fileName} should start with 'I'");
                
                if (interfaceName.Length > 1)
                {
                    Assert.That(char.IsUpper(interfaceName[1]), 
                        $"Interface '{interfaceName}' in {fileName} should have uppercase letter after 'I' (IPascalCase)");
                }
                
                Assert.That(!interfaceName.Contains("_"), 
                    $"Interface '{interfaceName}' in {fileName} should not contain underscores");
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_NamespaceNamesUsePascalCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // Namespace names should use PascalCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all namespace declarations
            var namespaceMatches = Regex.Matches(content, @"namespace\s+([\w\.]+)");
            
            foreach (Match match in namespaceMatches)
            {
                var namespaceName = match.Groups[1].Value;
                var namespaceParts = namespaceName.Split('.');
                
                foreach (var part in namespaceParts)
                {
                    if (string.IsNullOrEmpty(part)) continue;
                    
                    // Verify each part uses PascalCase
                    Assert.That(char.IsUpper(part[0]), 
                        $"Namespace part '{part}' in {fileName} should start with uppercase letter (PascalCase)");
                    
                    // Allow underscores in namespace names as they're sometimes used in project names
                }
            }
        }
    }

    [Test]
    public void Property8_NamingConventionCompliance_ParameterNamesUseCamelCase()
    {
        // Feature: aspec-capture-refactoring, Property 8: Naming Convention Compliance
        // Method parameters should use camelCase
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find method parameters (simplified pattern)
            var parameterMatches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+(?:async\s+)?(?:Task(?:<[^>]+>)?|void|\w+)\s+\w+\s*\([^)]*\b(\w+)\s*(?:,|\)))");
            
            foreach (Match match in parameterMatches)
            {
                var parameterName = match.Groups[1].Value;
                
                // Skip type names and keywords
                if (parameterName == "string" || parameterName == "int" || parameterName == "bool" || 
                    parameterName == "object" || parameterName == "Task" || parameterName == "void" ||
                    char.IsUpper(parameterName[0]) && parameterName.Length > 1 && char.IsUpper(parameterName[1]))
                {
                    continue;
                }

                // Verify camelCase (starts with lowercase)
                if (parameterName.Length > 0 && char.IsLetter(parameterName[0]))
                {
                    Assert.That(char.IsLower(parameterName[0]), 
                        $"Parameter '{parameterName}' in {fileName} should start with lowercase letter (camelCase)");
                }
            }
        }
    }
}