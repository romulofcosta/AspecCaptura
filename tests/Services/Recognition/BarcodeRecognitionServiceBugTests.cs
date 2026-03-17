using Microsoft.JSInterop;
using Xunit;
using FluentAssertions;
using pwa_camera_poc_blazor.Services.Recognition;
using Tests.Mocks;
using System.Reflection;

namespace Tests.Services.Recognition;

/// <summary>
/// Bug condition exploration tests for BarcodeRecognitionService.
///
/// These tests encode the EXPECTED (fixed) behavior.
/// They MUST FAIL on unfixed code — failure confirms the bugs exist.
/// DO NOT fix the code when these tests fail.
///
/// Validates: Requirements 1.1, 1.2, 1.3
/// </summary>
[Trait("Category", "BugExploration")]
[Trait("Service", "BarcodeRecognitionService")]
public class BarcodeRecognitionServiceBugTests
{
    // -------------------------------------------------------------------------
    // Bug 1 — processBarcodeImage missing in recognition-interop.js
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bug 1: BarcodeRecognitionService.DetectBarcodesAsync calls
    /// recognitionInterop.processBarcodeImage via IJSRuntime, but this method
    /// does NOT exist in recognition-interop.js.
    ///
    /// Expected (fixed) behavior: the service catches the JSException and returns
    /// Array.Empty&lt;BarcodeResult&gt;() instead of propagating the exception.
    ///
    /// On UNFIXED code: the MockJSRuntime in strict mode throws
    /// JSException("Could not find 'recognitionInterop.processBarcodeImage'")
    /// and the service does NOT handle it gracefully — the test fails because
    /// the exception propagates out of DetectBarcodesAsync.
    ///
    /// Validates: Requirements 1.1
    /// </summary>
    [Fact]
    [Trait("Bug", "1-MissingMethod")]
    public async Task Bug1_DetectBarcodesAsync_WhenProcessBarcodeImageMissing_ShouldReturnEmptyNotThrow()
    {
        // Arrange
        var mockJSRuntime = new MockJSRuntime();
        mockJSRuntime.SetStrictMode(true);

        // Allow initialize to succeed so _isInitialized becomes true
        mockJSRuntime.Setup("recognitionInterop.initialize", true);

        var service = new BarcodeRecognitionService(mockJSRuntime);

        // Force _isInitialized = true via reflection so we skip the init path
        // and go straight to the processBarcodeImage call
        SetPrivateField(service, "_isInitialized", true);

        // Act — processBarcodeImage is NOT in _setupMethods, strict mode will throw
        var result = await service.DetectBarcodesAsync(new byte[] { 1, 2, 3 });

        // Assert — fixed code catches the JSException and returns empty array
        result.Should().NotBeNull();
        result.Should().BeEmpty("the service should catch the JSException and return an empty array");
    }

    // -------------------------------------------------------------------------
    // Bug 2 — cleanupBarcodeWorker called before initialization
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bug 2: The Timer calls CleanupResources every 30s. CleanupResources calls
    /// recognitionInterop.cleanupBarcodeWorker even when _isInitialized = false.
    ///
    /// Expected (fixed) behavior: when _isInitialized = false, CleanupResources
    /// returns immediately without making any JS call.
    ///
    /// On UNFIXED code: the JS call IS made (count > 0), so the assertion
    /// GetCallCount == 0 fails — confirming the bug.
    ///
    /// Validates: Requirements 1.2
    /// </summary>
    [Fact]
    [Trait("Bug", "2-CleanupBeforeInit")]
    public async Task Bug2_CleanupResources_WhenNotInitialized_ShouldNotCallJS()
    {
        // Arrange — service created WITHOUT calling InitializeAsync (_isInitialized = false)
        var mockJSRuntime = new MockJSRuntime();
        // Allow cleanupBarcodeWorker to be called without throwing (so we can count calls)
        mockJSRuntime.Setup<object?>("recognitionInterop.cleanupBarcodeWorker", null);

        var service = new BarcodeRecognitionService(mockJSRuntime);
        // _isInitialized is false by default — do NOT call InitializeAsync

        // Set _lastActivityTime far in the past so the inactivity condition is met
        SetPrivateField(service, "_lastActivityTime", DateTime.Now.AddSeconds(-31));

        // Act — invoke CleanupResources directly via reflection
        await InvokeCleanupResourcesAsync(service);

        // Assert — no JS call should have been made when not initialized
        mockJSRuntime.GetCallCount("recognitionInterop.cleanupBarcodeWorker")
            .Should().Be(0, "CleanupResources should be a no-op when _isInitialized = false");
    }

    // -------------------------------------------------------------------------
    // Bug 3 — duplicate cleanupBarcodeWorker call in CleanupResources
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bug 3: CleanupResources calls recognitionInterop.cleanupBarcodeWorker
    /// TWICE consecutively — a duplicate call.
    ///
    /// Expected (fixed) behavior: the method is called exactly once per
    /// CleanupResources execution.
    ///
    /// On UNFIXED code: the mock records 2 invocations, so the assertion
    /// count == 1 fails — confirming the bug.
    ///
    /// Validates: Requirements 1.3
    /// </summary>
    [Fact]
    [Trait("Bug", "3-DuplicateCall")]
    public async Task Bug3_CleanupResources_WhenInactive_ShouldCallCleanupBarcodeWorkerExactlyOnce()
    {
        // Arrange
        var mockJSRuntime = new MockJSRuntime();
        mockJSRuntime.Setup<object?>("recognitionInterop.cleanupBarcodeWorker", null);

        var service = new BarcodeRecognitionService(mockJSRuntime);

        // Set _isInitialized = true so the guard (once fixed) allows the call
        SetPrivateField(service, "_isInitialized", true);

        // Set _lastActivityTime > 30s ago so the inactivity condition is met
        SetPrivateField(service, "_lastActivityTime", DateTime.Now.AddSeconds(-31));

        // Act
        await InvokeCleanupResourcesAsync(service);

        // Assert — must be called exactly once, not twice
        mockJSRuntime.GetCallCount("recognitionInterop.cleanupBarcodeWorker")
            .Should().Be(1, "cleanupBarcodeWorker should be invoked exactly once per CleanupResources execution");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        field.Should().NotBeNull($"field '{fieldName}' must exist on {target.GetType().Name}");
        field!.SetValue(target, value);
    }

    /// <summary>
    /// Invokes CleanupResources(null) via reflection and waits for any async
    /// Task.Run work it spawns to complete.
    /// </summary>
    private static async Task InvokeCleanupResourcesAsync(BarcodeRecognitionService service)
    {
        var method = typeof(BarcodeRecognitionService).GetMethod(
            "CleanupResources",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method.Should().NotBeNull("CleanupResources method must exist on BarcodeRecognitionService");
        method!.Invoke(service, new object?[] { null });

        // CleanupResources fires a Task.Run internally — give it time to complete
        await Task.Delay(200);
    }
}
