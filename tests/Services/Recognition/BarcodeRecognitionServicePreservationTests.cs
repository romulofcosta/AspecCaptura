using Microsoft.JSInterop;
using Xunit;
using FluentAssertions;
using AspecCaptura.Services.Recognition;
using Tests.Mocks;
using System.Reflection;

namespace Tests.Services.Recognition;

/// <summary>
/// Preservation property tests for BarcodeRecognitionService.
///
/// These tests verify that existing (non-buggy) behaviors are NOT broken by the fix.
/// They MUST PASS on both unfixed and fixed code.
///
/// Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5
/// </summary>
[Trait("Category", "Preservation")]
[Trait("Service", "BarcodeRecognitionService")]
public class BarcodeRecognitionServicePreservationTests
{
    // -------------------------------------------------------------------------
    // P1 — DetectBarcodesAsync with valid JS results parses and returns sorted results
    // -------------------------------------------------------------------------

    /// <summary>
    /// P1: When JS returns valid JSON with multiple barcode results, DetectBarcodesAsync
    /// must parse them, validate via BarcodeParser, and return sorted by confidence descending.
    ///
    /// BarcodeParser recalculates confidence internally:
    ///   "123456" (numeric, CODE_128, no checksum) → 0.75
    ///   "PA0001" (alphanumeric [A-Z]{2,4}\d{4,8}, CODE_128, checksum valid) → 0.95
    /// So PA0001 must appear first.
    ///
    /// Validates: Requirements 3.1, 3.4
    /// </summary>
    [Fact]
    [Trait("Preservation", "P1-SortedResults")]
    public async Task P1_DetectBarcodesAsync_WithValidJsResults_ReturnsSortedByConfidenceDescending()
    {
        // Arrange
        var mockJSRuntime = new MockJSRuntime();

        // Two valid patrimonio codes with different parser-computed confidences:
        //   "123456" — numeric 6-digit, CODE_128, checksumValid=false → 0.7 (base) + 0.05 (len 6-10) = 0.75
        //   "PA0001" — alphanumeric [A-Z]{2,4}\d{4,8}, CODE_128, checksumValid=true → 0.7 + 0.1 + 0.05 + 0.1 = 0.95
        // BarcodeParser recalculates confidence internally, so PA0001 must come first.
        var json = """
            [
              {"Code":"123456","Confidence":0.5,"Format":"CODE_128","BoundingBox":{"X":0,"Y":0,"Width":100,"Height":50},"ChecksumValid":false,"Timestamp":0},
              {"Code":"PA0001","Confidence":0.5,"Format":"CODE_128","BoundingBox":{"X":0,"Y":0,"Width":100,"Height":50},"ChecksumValid":true,"Timestamp":0}
            ]
            """;

        mockJSRuntime.Setup("recognitionInterop.processBarcodeImage", json);

        var service = new BarcodeRecognitionService(mockJSRuntime);
        SetPrivateField(service, "_isInitialized", true);

        // Act
        var results = await service.DetectBarcodesAsync(new byte[] { 1, 2, 3 });

        // Assert — results must be sorted by confidence descending (PA0001 first, higher parser confidence)
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThan(0, "valid patrimonio codes should be parsed successfully");
        results[0].Code.Should().Be("PA0001", "highest confidence result must come first");
        results.Should().BeInDescendingOrder(r => r.Confidence, "results must be sorted by confidence descending");
    }

    // -------------------------------------------------------------------------
    // P2 — CleanupResources when initialized and inactive sends cleanup message
    // -------------------------------------------------------------------------

    /// <summary>
    /// P2: When the service is initialized and has been inactive for more than 30s,
    /// CleanupResources must invoke recognitionInterop.cleanupBarcodeWorker at least once.
    ///
    /// This verifies the cleanup path still works correctly for the non-buggy case
    /// (initialized + inactive).
    ///
    /// Validates: Requirements 3.2, 3.5
    /// </summary>
    [Fact]
    [Trait("Preservation", "P2-CleanupWhenInitializedAndInactive")]
    public async Task P2_CleanupResources_WhenInitializedAndInactive_SendsCleanupMessage()
    {
        // Arrange
        var mockJSRuntime = new MockJSRuntime();
        mockJSRuntime.Setup<object?>("recognitionInterop.cleanupBarcodeWorker", null);

        var service = new BarcodeRecognitionService(mockJSRuntime);
        SetPrivateField(service, "_isInitialized", true);
        SetPrivateField(service, "_lastActivityTime", DateTime.Now.AddSeconds(-31));

        // Act — invoke CleanupResources via reflection
        await InvokeCleanupResourcesAsync(service);

        // Assert — cleanup must have been called at least once
        mockJSRuntime.GetCallCount("recognitionInterop.cleanupBarcodeWorker")
            .Should().BeGreaterThanOrEqualTo(1,
                "CleanupResources should invoke cleanupBarcodeWorker when initialized and inactive > 30s");
    }

    // -------------------------------------------------------------------------
    // P3 — DetectBarcodesAsync when not initialized calls InitializeAsync first
    // -------------------------------------------------------------------------

    /// <summary>
    /// P3: When DetectBarcodesAsync is called without prior initialization,
    /// it must call recognitionInterop.initialize before processing.
    ///
    /// Validates: Requirements 3.3
    /// </summary>
    [Fact]
    [Trait("Preservation", "P3-AutoInitialize")]
    public async Task P3_DetectBarcodesAsync_WhenNotInitialized_CallsInitializeFirst()
    {
        // Arrange
        var mockJSRuntime = new MockJSRuntime();
        mockJSRuntime.Setup("recognitionInterop.initialize", true);
        mockJSRuntime.Setup("recognitionInterop.processBarcodeImage", "[]");

        var service = new BarcodeRecognitionService(mockJSRuntime);
        // _isInitialized is false by default — do NOT set it

        // Act
        await service.DetectBarcodesAsync(new byte[] { 1, 2, 3 });

        // Assert — initialize must have been called
        mockJSRuntime.WasCalled("recognitionInterop.initialize")
            .Should().BeTrue("DetectBarcodesAsync must call InitializeAsync when not yet initialized");
    }

    // -------------------------------------------------------------------------
    // P4 — DetectBarcodesAsync returns empty when JS returns null/empty
    // -------------------------------------------------------------------------

    /// <summary>
    /// P4: When JS returns an empty string for processBarcodeImage,
    /// DetectBarcodesAsync must return an empty array without throwing.
    ///
    /// Validates: Requirements 3.1
    /// </summary>
    [Fact]
    [Trait("Preservation", "P4-EmptyResultOnEmptyJson")]
    public async Task P4_DetectBarcodesAsync_WhenJsReturnsEmpty_ReturnsEmptyArray()
    {
        // Arrange
        var mockJSRuntime = new MockJSRuntime();
        mockJSRuntime.Setup("recognitionInterop.processBarcodeImage", "");

        var service = new BarcodeRecognitionService(mockJSRuntime);
        SetPrivateField(service, "_isInitialized", true);

        // Act
        var results = await service.DetectBarcodesAsync(new byte[] { 1, 2, 3 });

        // Assert
        results.Should().NotBeNull();
        results.Should().BeEmpty("empty JS response should yield an empty result array");
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
