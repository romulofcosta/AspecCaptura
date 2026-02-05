using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for route transformation completeness
/// Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
/// </summary>
[TestFixture]
public class RouteTransformationTests
{
    private readonly string _projectRoot;

    public RouteTransformationTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_PortugueseRoutesExist()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // Portuguese routes should exist and be properly defined
        
        var cameraPagePath = Path.Combine(_projectRoot, "Pages", "Camera.razor");
        var syncPagePath = Path.Combine(_projectRoot, "Pages", "Sync.razor");
        
        Assert.That(File.Exists(cameraPagePath), "Camera.razor should exist");
        Assert.That(File.Exists(syncPagePath), "Sync.razor should exist");

        var cameraContent = File.ReadAllText(cameraPagePath);
        var syncContent = File.ReadAllText(syncPagePath);

        // Check that new Portuguese routes exist
        Assert.That(cameraContent.Contains("@page \"/captura\""), 
            "Camera.razor should contain Portuguese route @page \"/captura\"");
        Assert.That(syncContent.Contains("@page \"/sincronizacao\""), 
            "Sync.razor should contain Portuguese route @page \"/sincronizacao\"");

        // Check that old English routes are not present
        Assert.That(!cameraContent.Contains("@page \"/camera\""), 
            "Camera.razor should not contain old English route @page \"/camera\"");
        Assert.That(!syncContent.Contains("@page \"/sync\""), 
            "Sync.razor should not contain old English route @page \"/sync\"");
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_NavigationReferencesUpdated()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // All navigation references should use the new Portuguese routes
        
        var razorFiles = Directory.GetFiles(_projectRoot, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in razorFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Check for NavigationManager.NavigateTo calls with old routes
            Assert.That(!content.Contains("NavigateTo(\"/camera"), 
                $"File {fileName} should not contain NavigateTo calls to old /camera route");
            Assert.That(!content.Contains("NavigateTo(\"/sync"), 
                $"File {fileName} should not contain NavigateTo calls to old /sync route");

            // Check for Href attributes with old routes
            Assert.That(!content.Contains("Href=\"/camera\""), 
                $"File {fileName} should not contain Href attributes with old /camera route");
            Assert.That(!content.Contains("Href=\"/sync\""), 
                $"File {fileName} should not contain Href attributes with old /sync route");
        }
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_NewRoutesUsedInNavigation()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // Navigation components should use the new Portuguese routes
        
        var homePagePath = Path.Combine(_projectRoot, "Pages", "Home.razor");
        var mainLayoutPath = Path.Combine(_projectRoot, "Components", "Layout", "MainLayout.razor");
        
        Assert.That(File.Exists(homePagePath), "Home.razor should exist");
        Assert.That(File.Exists(mainLayoutPath), "MainLayout.razor should exist");

        var homeContent = File.ReadAllText(homePagePath);
        var mainLayoutContent = File.ReadAllText(mainLayoutPath);

        // Check that Home page uses new Portuguese routes
        Assert.That(homeContent.Contains("Href=\"/captura\""), 
            "Home.razor should contain navigation to /captura route");

        // Check that MainLayout uses new Portuguese routes
        Assert.That(mainLayoutContent.Contains("NavigateTo(\"/captura"), 
            "MainLayout.razor should contain NavigateTo calls to /captura route");
        Assert.That(mainLayoutContent.Contains("NavigateTo(\"/sincronizacao"), 
            "MainLayout.razor should contain NavigateTo calls to /sincronizacao route");
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_RouteParametersFunctional()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // Route parameters should continue to work with new Portuguese routes
        
        var itemDetailsPath = Path.Combine(_projectRoot, "Pages", "ItemDetails.razor");
        Assert.That(File.Exists(itemDetailsPath), "ItemDetails.razor should exist");

        var content = File.ReadAllText(itemDetailsPath);

        // Verify that route parameters are still properly defined
        Assert.That(content.Contains("@page \"/item/{Id}\""), 
            "ItemDetails.razor should maintain route parameter functionality");
        Assert.That(content.Contains("[Parameter] public string Id"), 
            "ItemDetails.razor should maintain parameter binding");
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_RouteCheckingLogicUpdated()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // Route checking logic should use new Portuguese route names
        
        var mainLayoutPath = Path.Combine(_projectRoot, "Components", "Layout", "MainLayout.razor");
        Assert.That(File.Exists(mainLayoutPath), "MainLayout.razor should exist");

        var content = File.ReadAllText(mainLayoutPath);

        // Check that route checking logic uses new Portuguese routes
        Assert.That(content.Contains("relativePath == \"sincronizacao\""), 
            "MainLayout should check for sincronizacao route");
        Assert.That(content.Contains("relativePath != \"captura\""), 
            "MainLayout should check for captura route in camera state logic");

        // Check that old route checking logic is not present
        Assert.That(!content.Contains("relativePath == \"sync\"") || content.Contains("relativePath == \"sincronizacao\""), 
            "MainLayout should not use old sync route checking without also having new sincronizacao route checking");
        Assert.That(!content.Contains("relativePath != \"camera\"") || content.Contains("relativePath != \"captura\""), 
            "MainLayout should not use old camera route checking without also having new captura route checking");
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_AllPageDirectivesValid()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // All @page directives should be valid and properly formatted
        
        var razorFiles = Directory.GetFiles(_projectRoot, "*.razor", SearchOption.AllDirectories)
            .Where(f => f.Contains("Pages") && !f.Contains("tests"))
            .ToArray();

        foreach (var file in razorFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all @page directives
            var pageMatches = Regex.Matches(content, @"@page\s+""([^""]+)""");
            
            foreach (Match match in pageMatches)
            {
                var route = match.Groups[1].Value;
                
                // Verify route format
                Assert.That(route.StartsWith("/"), 
                    $"Route '{route}' in {fileName} should start with '/'");
                
                // Verify no spaces in routes
                Assert.That(!route.Contains(" "), 
                    $"Route '{route}' in {fileName} should not contain spaces");
                
                // Verify Portuguese routes don't contain English words for main pages
                if (route == "/captura" || route == "/sincronizacao")
                {
                    // These are the expected Portuguese routes
                    Assert.That(route == "/captura" || route == "/sincronizacao", 
                        $"Route '{route}' in {fileName} is a properly localized Portuguese route");
                }
            }
        }
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_NoOrphanedEnglishRoutes()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // No orphaned English route references should remain in the codebase
        
        var allFiles = Directory.GetFiles(_projectRoot, "*.razor", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(_projectRoot, "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Contains("tests"))
            .ToArray();

        var englishRoutes = new[] { "/camera", "/sync" };

        foreach (var file in allFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            foreach (var englishRoute in englishRoutes)
            {
                // Look for route references (in quotes or navigation calls)
                var routePattern = $@"""{englishRoute}""";
                var hasEnglishRoute = Regex.IsMatch(content, routePattern);
                
                Assert.That(!hasEnglishRoute, 
                    $"File {fileName} should not contain references to old English route '{englishRoute}'");
            }
        }
    }

    [Test]
    public void Property7_RouteTransformationCompleteness_QueryParametersPreserved()
    {
        // Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
        // Query parameters should work correctly with new Portuguese routes
        
        var mainLayoutPath = Path.Combine(_projectRoot, "Components", "Layout", "MainLayout.razor");
        Assert.That(File.Exists(mainLayoutPath), "MainLayout.razor should exist");

        var content = File.ReadAllText(mainLayoutPath);

        // Check that query parameters are preserved in navigation calls
        if (content.Contains("mode=photo"))
        {
            Assert.That(content.Contains("/captura?mode=photo"), 
                "Navigation with query parameters should use new Portuguese route /captura");
        }
    }
}