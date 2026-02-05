using NUnit.Framework;
using FsCheck;
using FsCheck.NUnit;
using System.IO;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for .gitignore pattern completeness
/// Feature: aspec-capture-refactoring, Property 2: GitIgnore Pattern Completeness
/// </summary>
[TestFixture]
public class GitIgnoreTests
{
    private readonly string _projectRoot;
    private readonly string _gitIgnorePath;

    public GitIgnoreTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
        _gitIgnorePath = Path.Combine(_projectRoot, ".gitignore");
    }

    [Test]
    public void Property2_GitIgnorePatternCompleteness_ContainsBuildLogPatterns()
    {
        // Feature: aspec-capture-refactoring, Property 2: GitIgnore Pattern Completeness
        // .gitignore should contain patterns that exclude build logs
        
        Assert.That(File.Exists(_gitIgnorePath), ".gitignore file should exist");
        
        var gitIgnoreContent = File.ReadAllText(_gitIgnorePath);
        
        var requiredBuildLogPatterns = new[]
        {
            "*.log",
            "*.binlog", 
            "scripts/logs/",
            "msbuild.log"
        };
        
        foreach (var pattern in requiredBuildLogPatterns)
        {
            Assert.That(gitIgnoreContent.Contains(pattern), 
                $".gitignore should contain build log pattern: {pattern}");
        }
    }

    [Test]
    public void Property2_GitIgnorePatternCompleteness_ContainsAgentToolPatterns()
    {
        // Feature: aspec-capture-refactoring, Property 2: GitIgnore Pattern Completeness
        // .gitignore should contain patterns that exclude agent tools
        
        var gitIgnoreContent = File.ReadAllText(_gitIgnorePath);
        
        var requiredAgentPatterns = new[]
        {
            ".agent/",
            ".kiro/",
            ".trae/"
        };
        
        foreach (var pattern in requiredAgentPatterns)
        {
            Assert.That(gitIgnoreContent.Contains(pattern), 
                $".gitignore should contain agent tool pattern: {pattern}");
        }
    }

    [Test]
    public void Property2_GitIgnorePatternCompleteness_ContainsBinObjPatterns()
    {
        // Feature: aspec-capture-refactoring, Property 2: GitIgnore Pattern Completeness
        // .gitignore should contain patterns that exclude bin/ and obj/ folders
        
        var gitIgnoreContent = File.ReadAllText(_gitIgnorePath);
        
        var requiredBuildPatterns = new[]
        {
            "[Bb]in/",
            "[Oo]bj/"
        };
        
        foreach (var pattern in requiredBuildPatterns)
        {
            Assert.That(gitIgnoreContent.Contains(pattern), 
                $".gitignore should contain build folder pattern: {pattern}");
        }
    }

    [Test]
    public void Property2_GitIgnorePatternCompleteness_ContainsTemporaryFilePatterns()
    {
        // Feature: aspec-capture-refactoring, Property 2: GitIgnore Pattern Completeness
        // .gitignore should contain patterns that exclude temporary files
        
        var gitIgnoreContent = File.ReadAllText(_gitIgnorePath);
        
        var requiredTempPatterns = new[]
        {
            "comp_def.txt",
            "test-payload.json",
            "*.swp",
            "*.swo"
        };
        
        foreach (var pattern in requiredTempPatterns)
        {
            Assert.That(gitIgnoreContent.Contains(pattern), 
                $".gitignore should contain temporary file pattern: {pattern}");
        }
    }

    [Test]
    public void Property2_GitIgnorePatternCompleteness_ExcludesGeneratedFiles()
    {
        // Feature: aspec-capture-refactoring, Property 2: GitIgnore Pattern Completeness
        // For any generated file pattern, it should be excluded by .gitignore
        
        var gitIgnoreContent = File.ReadAllText(_gitIgnorePath);
        
        // Test common generated file patterns
        var generatedPatterns = new Dictionary<string, string[]>
        {
            { ".log", new[] { "*.log" } },
            { ".binlog", new[] { "*.binlog" } },
            { "build_", new[] { "build_*.txt", "build*.txt" } },
            { "error", new[] { "error*.txt", "errors.log" } }
        };
        
        foreach (var (pattern, ignorePatterns) in generatedPatterns)
        {
            var shouldBeIgnored = ignorePatterns.Any(ignorePattern => 
                gitIgnoreContent.Contains(ignorePattern));
            
            Assert.That(shouldBeIgnored, 
                $"Generated file pattern '{pattern}' should be covered by .gitignore patterns");
        }
    }
}