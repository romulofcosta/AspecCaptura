using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Unit tests for route parameter functionality with Portuguese routes
/// Feature: aspec-capture-refactoring, Property 7: Route Transformation Completeness
/// </summary>
[TestFixture]
public class RouteParameterFunctionalityTests
{
    private readonly string _projectRoot;

    public RouteParameterFunctionalityTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void RouteParameters_ItemDetailsPage_ShouldMaintainParameterBinding()
    {
        // Test that route parameters continue to work with new Portuguese routes
        // This validates requirement 6.5 from the design document
        
        var itemDetailsPath = Path.Combine(_projectRoot, "Pages", "ItemDetails.razor");
        Assert.That(File.Exists(itemDetailsPath), "ItemDetails.razor should exist");

        var content = File.ReadAllText(itemDetailsPath);

        // Verify route parameter definition
        Assert.That(content.Contains("@page \"/item/{Id}\""), 
            "ItemDetails should have route parameter definition");

        // Verify parameter property binding
        var parameterPattern = @"\[Parameter\]\s+public\s+string\s+Id\s*\{\s*get;\s*set;\s*\}";
        var hasParameterBinding = Regex.IsMatch(content, parameterPattern);
        
        Assert.That(hasParameterBinding, 
            "ItemDetails should have proper parameter binding for Id");

        // Verify parameter is used in the component
        Assert.That(content.Contains("Id"), 
            "ItemDetails should use the Id parameter in its logic");
    }

    [Test]
    public void RouteParameters_CameraPage_ShouldSupportQueryParameters()
    {
        // Test that query parameters work correctly with the new /captura route
        
        var cameraPagePath = Path.Combine(_projectRoot, "Pages", "Camera.razor");
        Assert.That(File.Exists(cameraPagePath), "Camera.razor should exist");

        var content = File.ReadAllText(cameraPagePath);

        // Verify the page has the new Portuguese route
        Assert.That(content.Contains("@page \"/captura\""), 
            "Camera page should use Portuguese route /captura");

        // Check if the page supports query parameters (mode parameter)
        if (content.Contains("SupplyParameterFromQuery"))
        {
            var queryParameterPattern = @"\[Parameter,\s*SupplyParameterFromQuery\]\s+public\s+string\?\s+mode";
            var hasQueryParameter = Regex.IsMatch(content, queryParameterPattern);
            
            Assert.That(hasQueryParameter, 
                "Camera page should properly bind query parameters");
        }
    }

    [Test]
    public void RouteParameters_NavigationWithParameters_ShouldUsePortugueseRoutes()
    {
        // Test that navigation calls with parameters use the new Portuguese routes
        
        var allRazorFiles = Directory.GetFiles(_projectRoot, "*.razor", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();

        foreach (var file in allRazorFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Look for navigation calls with item parameters
            var itemNavigationPattern = @"NavigateTo\s*\(\s*[""']/item/";
            var hasItemNavigation = Regex.IsMatch(content, itemNavigationPattern);

            if (hasItemNavigation)
            {
                // Verify it's using the correct route format
                Assert.That(content.Contains("/item/"), 
                    $"File {fileName} should use /item/ route for item details navigation");
                
                // Ensure it's not using old English routes
                Assert.That(!content.Contains("NavigateTo(\"/camera/"), 
                    $"File {fileName} should not use old /camera/ route for navigation");
                Assert.That(!content.Contains("NavigateTo(\"/sync/"), 
                    $"File {fileName} should not use old /sync/ route for navigation");
            }
        }
    }

    [Test]
    public void RouteParameters_QueryStringHandling_ShouldWorkWithPortugueseRoutes()
    {
        // Test that query string parameters work correctly with Portuguese routes
        
        var mainLayoutPath = Path.Combine(_projectRoot, "Components", "Layout", "MainLayout.razor");
        Assert.That(File.Exists(mainLayoutPath), "MainLayout.razor should exist");

        var content = File.ReadAllText(mainLayoutPath);

        // Check for navigation with query parameters using Portuguese routes
        if (content.Contains("mode=photo"))
        {
            Assert.That(content.Contains("/captura?mode=photo"), 
                "Navigation with query parameters should use Portuguese route /captura");
            
            // Ensure old English route with query parameters is not present
            Assert.That(!content.Contains("/camera?mode=photo"), 
                "Navigation should not use old English route /camera with query parameters");
        }
    }

    [Test]
    public void RouteParameters_RouteConstraints_ShouldBePreserved()
    {
        // Test that any route constraints are preserved with the new Portuguese routes
        
        var pageFiles = Directory.GetFiles(Path.Combine(_projectRoot, "Pages"), "*.razor", SearchOption.TopDirectoryOnly);

        foreach (var file in pageFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Find all @page directives with parameters
            var pageMatches = Regex.Matches(content, @"@page\s+""([^""]+)""");
            
            foreach (Match match in pageMatches)
            {
                var route = match.Groups[1].Value;
                
                // If route has parameters, verify they're properly formatted
                if (route.Contains("{") && route.Contains("}"))
                {
                    // Verify parameter syntax is correct
                    var parameterPattern = @"\{[^}]+\}";
                    var parameterMatches = Regex.Matches(route, parameterPattern);
                    
                    foreach (Match paramMatch in parameterMatches)
                    {
                        var parameter = paramMatch.Value;
                        
                        // Verify parameter format (should be {ParameterName} or {ParameterName:constraint})
                        Assert.That(parameter.Length > 2, 
                            $"Route parameter '{parameter}' in {fileName} should have valid format");
                        Assert.That(!parameter.Contains(" "), 
                            $"Route parameter '{parameter}' in {fileName} should not contain spaces");
                    }
                }
            }
        }
    }

    [Test]
    public void RouteParameters_CaseInsensitiveRouting_ShouldWorkCorrectly()
    {
        // Test that route matching works correctly with Portuguese routes
        // This is more of a validation that our routes follow proper conventions
        
        var cameraPagePath = Path.Combine(_projectRoot, "Pages", "Camera.razor");
        var syncPagePath = Path.Combine(_projectRoot, "Pages", "Sync.razor");
        
        var cameraContent = File.ReadAllText(cameraPagePath);
        var syncContent = File.ReadAllText(syncPagePath);

        // Verify routes use lowercase (ASP.NET Core convention)
        Assert.That(cameraContent.Contains("@page \"/captura\""), 
            "Camera route should use lowercase /captura");
        Assert.That(syncContent.Contains("@page \"/sincronizacao\""), 
            "Sync route should use lowercase /sincronizacao");

        // Verify routes don't have mixed case that could cause routing issues
        Assert.That(!cameraContent.Contains("@page \"/Captura\""), 
            "Camera route should not use mixed case /Captura");
        Assert.That(!syncContent.Contains("@page \"/Sincronizacao\""), 
            "Sync route should not use mixed case /Sincronizacao");
    }

    [Test]
    public void RouteParameters_OptionalParameters_ShouldBeHandledCorrectly()
    {
        // Test that optional parameters work correctly with Portuguese routes
        
        var pageFiles = Directory.GetFiles(Path.Combine(_projectRoot, "Pages"), "*.razor", SearchOption.TopDirectoryOnly);

        foreach (var file in pageFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);

            // Look for optional parameters in route definitions
            var optionalParamPattern = @"@page\s+""[^""]*\{[^}]*\?[^}]*\}[^""]*""";
            var hasOptionalParams = Regex.IsMatch(content, optionalParamPattern);

            if (hasOptionalParams)
            {
                // If optional parameters exist, verify they're properly formatted
                var pageMatches = Regex.Matches(content, @"@page\s+""([^""]+)""");
                
                foreach (Match match in pageMatches)
                {
                    var route = match.Groups[1].Value;
                    
                    if (route.Contains("?"))
                    {
                        // Verify optional parameter syntax
                        Assert.That(route.Contains("{") && route.Contains("}"), 
                            $"Optional parameter route '{route}' in {fileName} should have proper parameter syntax");
                    }
                }
            }
        }
    }
}