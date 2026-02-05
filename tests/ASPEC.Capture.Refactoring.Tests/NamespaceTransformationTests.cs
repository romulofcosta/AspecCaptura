using NUnit.Framework;
using System.IO;
using System.Linq;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for namespace transformation consistency
/// Feature: aspec-capture-refactoring, Property 3: Namespace Transformation Consistency
/// </summary>
[TestFixture]
public class NamespaceTransformationTests
{
    private readonly string _projectRoot;

    public NamespaceTransformationTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property3_NamespaceTransformationConsistency_NoOldUGNamespaceReferences()
    {
        // Feature: aspec-capture-refactoring, Property 3: Namespace Transformation Consistency
        // No files should contain references to the old Services.UG namespace
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests")) // Exclude test files
            .ToArray();
        
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);
            
            Assert.That(!content.Contains("Services.UG"), 
                $"File {fileName} should not contain references to old namespace 'Services.UG'");
        }
    }

    [Test]
    public void Property3_NamespaceTransformationConsistency_NewUnidadesGestorasNamespaceExists()
    {
        // Feature: aspec-capture-refactoring, Property 3: Namespace Transformation Consistency
        // The new Services.UnidadesGestoras namespace should exist and be used
        
        var unidadesGestorasDir = Path.Combine(_projectRoot, "Services", "UnidadesGestoras");
        Assert.That(Directory.Exists(unidadesGestorasDir), 
            "Services/UnidadesGestoras directory should exist");

        var csFiles = Directory.GetFiles(unidadesGestorasDir, "*.cs", SearchOption.AllDirectories);
        Assert.That(csFiles.Length, Is.GreaterThan(0), 
            "Services/UnidadesGestoras directory should contain C# files");

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);
            
            Assert.That(content.Contains("namespace pwa_camera_poc_blazor.Services.UnidadesGestoras"), 
                $"File {fileName} should use the new namespace 'Services.UnidadesGestoras'");
        }
    }

    [Test]
    public void Property3_NamespaceTransformationConsistency_UsingStatementsUpdated()
    {
        // Feature: aspec-capture-refactoring, Property 3: Namespace Transformation Consistency
        // All using statements should reference the new namespace
        
        var csFiles = Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests")) // Exclude test files
            .ToArray();
        
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);
            
            // Check for old using statements
            Assert.That(!content.Contains("using pwa_camera_poc_blazor.Services.UG"), 
                $"File {fileName} should not contain old using statement 'using pwa_camera_poc_blazor.Services.UG'");
            
            // If file references UGStateService and is NOT the UGStateService file itself, it should use the new namespace
            if (content.Contains("UGStateService") && content.Contains("using") && !fileName.EndsWith("UGStateService.cs"))
            {
                var hasCorrectUsing = content.Contains("using pwa_camera_poc_blazor.Services.UnidadesGestoras") ||
                                    content.Contains("pwa_camera_poc_blazor.Services.UnidadesGestoras.UGStateService");
                
                Assert.That(hasCorrectUsing, 
                    $"File {fileName} references UGStateService but doesn't use the correct namespace");
            }
        }
    }

    [Test]
    public void Property3_NamespaceTransformationConsistency_DirectoryStructureMatchesNamespace()
    {
        // Feature: aspec-capture-refactoring, Property 3: Namespace Transformation Consistency
        // Directory structure should match namespace hierarchy
        
        var servicesDir = Path.Combine(_projectRoot, "Services");
        var subdirectories = Directory.GetDirectories(servicesDir);
        
        foreach (var dir in subdirectories)
        {
            var dirName = Path.GetFileName(dir);
            var csFiles = Directory.GetFiles(dir, "*.cs", SearchOption.TopDirectoryOnly);
            
            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetRelativePath(_projectRoot, file);
                
                // Extract namespace from file
                var lines = content.Split('\n');
                var namespaceLine = lines.FirstOrDefault(l => l.Trim().StartsWith("namespace"));
                
                if (namespaceLine != null)
                {
                    var expectedNamespace = $"pwa_camera_poc_blazor.Services.{dirName}";
                    Assert.That(namespaceLine.Contains(expectedNamespace), 
                        $"File {fileName} namespace should match directory structure. Expected: {expectedNamespace}");
                }
            }
        }
    }

    [Test]
    public void Property3_NamespaceTransformationConsistency_OldUGDirectoryRemoved()
    {
        // Feature: aspec-capture-refactoring, Property 3: Namespace Transformation Consistency
        // The old Services/UG directory should no longer exist
        
        var oldUGDir = Path.Combine(_projectRoot, "Services", "UG");
        Assert.That(!Directory.Exists(oldUGDir), 
            "Old Services/UG directory should be removed");
    }
}