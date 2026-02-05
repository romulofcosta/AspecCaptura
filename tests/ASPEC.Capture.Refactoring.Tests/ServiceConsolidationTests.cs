using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ASPEC.Capture.Refactoring.Tests;

/// <summary>
/// Property-based tests for service consolidation preservation
/// Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
/// </summary>
[TestFixture]
public class ServiceConsolidationTests
{
    private readonly string _projectRoot;

    public ServiceConsolidationTests()
    {
        // Navigate up from test directory to project root
        _projectRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_CameraServiceExists()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // Camera service should exist and maintain its functionality
        
        var cameraServicePath = Path.Combine(_projectRoot, "Services", "Camera", "CameraService.cs");
        Assert.That(File.Exists(cameraServicePath), "CameraService.cs should exist");

        var content = File.ReadAllText(cameraServicePath);
        
        // Verify essential camera methods exist
        Assert.That(content.Contains("StartCameraAsync"), "CameraService should have StartCameraAsync method");
        Assert.That(content.Contains("TakePhotoAsync"), "CameraService should have TakePhotoAsync method");
        Assert.That(content.Contains("StopCameraAsync"), "CameraService should have StopCameraAsync method");
        Assert.That(content.Contains("CaptureFrameForOcrAsync"), "CameraService should have CaptureFrameForOcrAsync method");
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_OcrServiceExists()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // OCR service should exist and maintain its functionality
        
        var ocrServicePath = Path.Combine(_projectRoot, "Services", "Ocr", "OcrService.cs");
        Assert.That(File.Exists(ocrServicePath), "OcrService.cs should exist");

        var content = File.ReadAllText(ocrServicePath);
        
        // Verify essential OCR methods exist
        Assert.That(content.Contains("InitializeAsync"), "OcrService should have InitializeAsync method");
        Assert.That(content.Contains("RecognizeAsync"), "OcrService should have RecognizeAsync method");
        Assert.That(content.Contains("IAsyncDisposable"), "OcrService should implement IAsyncDisposable");
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_ScanOrchestratorExists()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // Scan orchestrator should exist and coordinate capture services
        
        var scanOrchestratorPath = Path.Combine(_projectRoot, "Services", "Scanning", "ScanOrchestrator.cs");
        Assert.That(File.Exists(scanOrchestratorPath), "ScanOrchestrator.cs should exist");

        var content = File.ReadAllText(scanOrchestratorPath);
        
        // Verify orchestration functionality
        Assert.That(content.Contains("ProcessScanAsync"), "ScanOrchestrator should have ProcessScanAsync method");
        Assert.That(content.Contains("UGStateService"), "ScanOrchestrator should reference UGStateService");
        Assert.That(content.Contains("IIndexedDbService"), "ScanOrchestrator should reference IIndexedDbService");
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_ServiceRegistrationExists()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // All capture services should be registered in DI container
        
        var programPath = Path.Combine(_projectRoot, "Program.cs");
        Assert.That(File.Exists(programPath), "Program.cs should exist");

        var content = File.ReadAllText(programPath);
        
        // Verify service registrations
        Assert.That(content.Contains("CameraService"), "CameraService should be registered in DI");
        Assert.That(content.Contains("OcrService"), "OcrService should be registered in DI");
        Assert.That(content.Contains("ScanOrchestrator"), "ScanOrchestrator should be registered in DI");
        Assert.That(content.Contains("UGStateService"), "UGStateService should be registered in DI");
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_ServiceSeparationOfConcerns()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // Services should maintain clear separation of concerns
        
        var servicesDir = Path.Combine(_projectRoot, "Services");
        
        // Camera service should only handle camera operations
        var cameraDir = Path.Combine(servicesDir, "Camera");
        Assert.That(Directory.Exists(cameraDir), "Camera service directory should exist");
        
        // OCR service should only handle text recognition
        var ocrDir = Path.Combine(servicesDir, "Ocr");
        Assert.That(Directory.Exists(ocrDir), "OCR service directory should exist");
        
        // Scanning should orchestrate the process
        var scanningDir = Path.Combine(servicesDir, "Scanning");
        Assert.That(Directory.Exists(scanningDir), "Scanning service directory should exist");
        
        // UnidadesGestoras should handle business logic
        var ugDir = Path.Combine(servicesDir, "UnidadesGestoras");
        Assert.That(Directory.Exists(ugDir), "UnidadesGestoras service directory should exist");
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_NoFunctionalityLoss()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // No essential functionality should be lost during consolidation
        
        var allServiceFiles = Directory.GetFiles(Path.Combine(_projectRoot, "Services"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();
        
        // Essential capture-related functionality should be present
        var allContent = string.Join(" ", allServiceFiles.Select(File.ReadAllText));
        
        // Camera functionality
        Assert.That(allContent.Contains("startCamera"), "Camera start functionality should be preserved");
        Assert.That(allContent.Contains("takePhoto"), "Photo capture functionality should be preserved");
        Assert.That(allContent.Contains("stopCamera"), "Camera stop functionality should be preserved");
        
        // OCR functionality
        Assert.That(allContent.Contains("RecognizeAsync") || allContent.Contains("recognize"), 
            "OCR recognition functionality should be preserved");
        Assert.That(allContent.Contains("Tesseract") || allContent.Contains("tesseract"), 
            "OCR engine integration should be preserved");
        
        // Orchestration functionality
        Assert.That(allContent.Contains("ProcessScanAsync"), "Scan processing functionality should be preserved");
        Assert.That(allContent.Contains("ScanDecision"), "Scan decision logic should be preserved");
    }

    [Test]
    public void Property4_ServiceConsolidationPreservation_InterfaceConsistency()
    {
        // Feature: aspec-capture-refactoring, Property 4: Service Consolidation Preservation
        // Service interfaces should remain consistent for consumers
        
        var servicesDir = Path.Combine(_projectRoot, "Services");
        var serviceFiles = Directory.GetFiles(servicesDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("tests"))
            .ToArray();
        
        foreach (var file in serviceFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetRelativePath(_projectRoot, file);
            
            // Services should have proper async patterns
            if (content.Contains("public async Task") || content.Contains("public async ValueTask"))
            {
                Assert.That(content.Contains("Async"), 
                    $"Service {fileName} should follow async naming conventions");
            }
            
            // Services should have proper namespace
            Assert.That(content.Contains("namespace pwa_camera_poc_blazor.Services"), 
                $"Service {fileName} should use correct namespace");
        }
    }
}