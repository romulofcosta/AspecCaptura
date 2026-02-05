using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for model transformation completeness
/// Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
/// </summary>
[TestFixture]
public class ModelTransformationTests
{
    private readonly string _projectRoot;

    public ModelTransformationTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_NoOldModelClassNames()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // No files should contain references to old English model class names
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests")) // Exclude test files
            .ToArray();
        
        var oldModelNames = new[] { "InventoryItem", "OrganizationalUnit" };
        
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);
            
            foreach (var oldName in oldModelNames)
            {
                // Check for class declarations, property types, method parameters, etc.
                var pattern = $@"\b{oldName}\b";
                var matches = Regex.Matches(content, pattern);
                
                Assert.That(matches.Count, Is.EqualTo(0), 
                    $"File {fileName} should not contain references to old model name '{oldName}'");
            }
        }
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_NewPortugueseModelClassesExist()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // New Portuguese model classes should exist and be properly defined
        
        var modelsDir = Path.Combine(_projectRoot, "Models");
        Assert.That(Directory.Exists(modelsDir), "Models directory should exist");

        var expectedModels = new[]
        {
            ("ItemPatrimonio.cs", "ItemPatrimonio"),
            ("Usuario.cs", "Usuario"),
            ("UnidadeGestora.cs", "UnidadeGestora")
        };

        foreach (var (fileName, className) in expectedModels)
        {
            var filePath = Path.Combine(modelsDir, fileName);
            Assert.That(File.Exists(filePath), $"Model file {fileName} should exist");

            var content = File.ReadAllText(filePath);
            Assert.That(content.Contains($"public class {className}"), 
                $"File {fileName} should contain class definition for {className}");
        }
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_FileNamesMatchClassNames()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // File names should match their contained class names (C# convention)
        
        var modelsDir = Path.Combine(_projectRoot, "Models");
        var csFiles = Directory.GetFiles(modelsDir, "*.cs", SearchOption.TopDirectoryOnly);

        foreach (var file in csFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(_projectRoot, file);

            // Extract class names from the file
            var classMatches = Regex.Matches(content, @"public class (\w+)");
            
            if (classMatches.Count > 0)
            {
                // For files with multiple classes, at least one should match the filename
                var classNames = classMatches.Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
                var hasMatchingClass = classNames.Contains(fileName);
                
                Assert.That(hasMatchingClass, 
                    $"File {relativePath} should contain a class with the same name as the file. " +
                    $"File: {fileName}, Classes found: {string.Join(", ", classNames)}");
            }
        }
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_PortuguesePropertyNames()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // Model properties should use Portuguese names without accents
        
        var modelsDir = Path.Combine(_projectRoot, "Models");
        var csFiles = Directory.GetFiles(modelsDir, "*.cs", SearchOption.TopDirectoryOnly);

        var expectedPropertyMappings = new Dictionary<string, string>
        {
            { "Name", "Nome" },
            { "Code", "Codigo" },
            { "Description", "Descricao" },
            { "CreatedDate", "DataCriacao" },
            { "ModifiedDate", "DataModificacao" },
            { "UserId", "UsuarioId" },
            { "OrganizationalUnitId", "UnidadeGestoraId" },
            { "IsActive", "EstaAtivo" },
            { "Timestamp", "DataHora" },
            { "Synced", "Sincronizado" },
            { "CreatedBy", "CriadoPor" },
            { "Location", "Localizacao" },
            { "Observations", "Observacoes" }
        };

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Check that old English property names are not used
            foreach (var (oldName, newName) in expectedPropertyMappings)
            {
                var oldPropertyPattern = $@"public\s+\w+\s+{oldName}\s*\{{";
                var hasOldProperty = Regex.IsMatch(content, oldPropertyPattern);
                
                Assert.That(!hasOldProperty, 
                    $"File {fileName} should not contain old English property name '{oldName}'. Use '{newName}' instead.");
            }
        }
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_AllReferencesUpdated()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // All references to models throughout the codebase should use new Portuguese names
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests")) // Exclude test files
            .ToArray();
        
        var razorFiles = Directory.GetFiles(_projectRoot, "*.razor", SearchOption.AllDirectories);
        var allFiles = csFiles.Concat(razorFiles).ToArray();

        foreach (var file in allFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Check for usage of new Portuguese model names
            if (content.Contains("ItemPatrimonio") || content.Contains("Usuario") || content.Contains("UnidadeGestora"))
            {
                // If file uses new model names, ensure it doesn't also use old names
                var oldReferences = new[] { "InventoryItem", "OrganizationalUnit" };
                
                foreach (var oldRef in oldReferences)
                {
                    var pattern = $@"\b{oldRef}\b";
                    var hasOldReference = Regex.IsMatch(content, pattern);
                    
                    Assert.That(!hasOldReference, 
                        $"File {fileName} uses new model names but still contains reference to old name '{oldRef}'");
                }
            }
        }
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_UserModelPropertiesLocalized()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // User model should have all properties properly localized
        
        var userModelPath = Path.Combine(_projectRoot, "Models", "Usuario.cs");
        Assert.That(File.Exists(userModelPath), "Usuario.cs model file should exist");

        var content = File.ReadAllText(userModelPath);
        
        var expectedUserProperties = new[]
        {
            "NomeUsuario",
            "PrimeiroNome", 
            "UltimoNome",
            "HashSenha",
            "IdsUnidadesGestoras",
            "UnidadeGestoraAtualId",
            "DataCriacao",
            "UltimoLogin"
        };

        foreach (var property in expectedUserProperties)
        {
            var propertyPattern = $@"public\s+\w+\s+{property}\s*\{{";
            var hasProperty = Regex.IsMatch(content, propertyPattern);
            
            Assert.That(hasProperty, 
                $"Usuario model should contain property '{property}'");
        }

        // Ensure old English property names are not present
        var oldUserProperties = new[] { "Username", "FirstName", "LastName", "PasswordHash", "UnitIds", "CurrentUnitId", "CreatedAt" };
        
        foreach (var oldProperty in oldUserProperties)
        {
            var oldPropertyPattern = $@"public\s+\w+\s+{oldProperty}\s*\{{";
            var hasOldProperty = Regex.IsMatch(content, oldPropertyPattern);
            
            Assert.That(!hasOldProperty, 
                $"Usuario model should not contain old English property name '{oldProperty}'");
        }
    }

    [Test]
    public void Property5_ModelTransformationCompleteness_ItemPatrimonioPropertiesLocalized()
    {
        // Feature: aspec-capture-refactoring, Property 5: Model Transformation Completeness
        // ItemPatrimonio model should have all properties properly localized
        
        var itemModelPath = Path.Combine(_projectRoot, "Models", "ItemPatrimonio.cs");
        Assert.That(File.Exists(itemModelPath), "ItemPatrimonio.cs model file should exist");

        var content = File.ReadAllText(itemModelPath);
        
        var expectedItemProperties = new[]
        {
            "Nome",
            "Codigo", 
            "Localizacao",
            "Observacoes",
            "DataHora",
            "Sincronizado",
            "UnidadeGestoraId",
            "CriadoPor"
        };

        foreach (var property in expectedItemProperties)
        {
            var propertyPattern = $@"public\s+\w+\s+{property}\s*\{{";
            var hasProperty = Regex.IsMatch(content, propertyPattern);
            
            Assert.That(hasProperty, 
                $"ItemPatrimonio model should contain property '{property}'");
        }

        // Ensure old English property names are not present
        var oldItemProperties = new[] { "Name", "Code", "Location", "Observations", "Timestamp", "Synced", "UnitId", "CreatedBy" };
        
        foreach (var oldProperty in oldItemProperties)
        {
            var oldPropertyPattern = $@"public\s+\w+\s+{oldProperty}\s*\{{";
            var hasOldProperty = Regex.IsMatch(content, oldPropertyPattern);
            
            Assert.That(!hasOldProperty, 
                $"ItemPatrimonio model should not contain old English property name '{oldProperty}'");
        }
    }
}