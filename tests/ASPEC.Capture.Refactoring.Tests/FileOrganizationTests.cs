using NUnit.Framework;
using FsCheck;
using FsCheck.NUnit;
using System.IO;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for file organization consistency
/// Feature: aspec-capture-refactoring, Property 1: File Organization Consistency
/// </summary>
[TestFixture]
public class FileOrganizationTests
{
    private readonly string _projectRoot;

    public FileOrganizationTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property1_FileOrganizationConsistency_BuildScriptsInScriptsFolder()
    {
        // Feature: aspec-capture-refactoring, Property 1: File Organization Consistency
        // For any project directory structure, after refactoring, all build scripts should be located in the scripts/ folder
        
        var scriptsDir = Path.Combine(_projectRoot, "scripts");
        Assert.That(Directory.Exists(scriptsDir), "Scripts directory should exist");

        // Check that build scripts are in scripts/ folder
        var buildScript = Path.Combine(scriptsDir, "build.sh");
        var installScript = Path.Combine(scriptsDir, "dotnet-install.sh");
        
        Assert.That(File.Exists(buildScript), "build.sh should be in scripts/ folder");
        Assert.That(File.Exists(installScript), "dotnet-install.sh should be in scripts/ folder");
    }

    [Test]
    public void Property1_FileOrganizationConsistency_ProjectRootOnlyEssentialFiles()
    {
        // Feature: aspec-capture-refactoring, Property 1: File Organization Consistency
        // Project root should contain only essential configuration files
        
        var rootFiles = Directory.GetFiles(_projectRoot);
        var essentialExtensions = new[] { ".csproj", ".sln", ".razor", ".cs", ".md", ".gitignore" };
        
        foreach (var file in rootFiles)
        {
            var fileName = Path.GetFileName(file);
            var extension = Path.GetExtension(file);
            
            // Allow essential configuration files
            var isEssential = essentialExtensions.Contains(extension) || 
                             fileName.Equals("Program.cs", StringComparison.OrdinalIgnoreCase) ||
                             fileName.Equals("App.razor", StringComparison.OrdinalIgnoreCase) ||
                             fileName.Equals("_Imports.razor", StringComparison.OrdinalIgnoreCase) ||
                             fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase) ||
                             fileName.Equals(".gitignore", StringComparison.OrdinalIgnoreCase);
            
            Assert.That(isEssential, $"File {fileName} should not be in project root - move to appropriate subfolder");
        }
    }

    [Test]
    public void Property1_FileOrganizationConsistency_NoTemporaryFilesInRoot()
    {
        // Feature: aspec-capture-refactoring, Property 1: File Organization Consistency
        // Temporary files should not exist in project root
        
        var temporaryPatterns = new[] { "build_", "error", "output", "comp_def", "test-payload" };
        var rootFiles = Directory.GetFiles(_projectRoot);
        
        foreach (var file in rootFiles)
        {
            var fileName = Path.GetFileName(file).ToLowerInvariant();
            var hasTemporaryPattern = temporaryPatterns.Any(pattern => fileName.Contains(pattern));
            
            if (hasTemporaryPattern)
            {
                Assert.That(!File.Exists(file), $"Temporary file {fileName} should not exist in project root");
            }
        }
    }
}