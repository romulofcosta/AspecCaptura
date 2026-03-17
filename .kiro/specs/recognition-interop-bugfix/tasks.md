# Implementation Plan

- [x] 1. Write bug condition exploration tests
  - **Property 1: Bug Condition** - Three Interop Bugs (Missing Method, Cleanup Loop, Duplicate Call)
  - **CRITICAL**: These tests MUST FAIL on unfixed code — failure confirms the bugs exist
  - **DO NOT attempt to fix the tests or the code when they fail**
  - **NOTE**: These tests encode the expected behavior — they will validate the fix when they pass after implementation
  - **GOAL**: Surface counterexamples that demonstrate each of the three bugs
  - **Scoped PBT Approach**: Scope each property to the concrete failing case to ensure reproducibility
  - **Bug 1 — Missing method**: Call `recognitionInterop.processBarcodeImage` via `IJSRuntime` mock that simulates JS throwing `Could not find 'recognitionInterop.processBarcodeImage'`; assert that `DetectBarcodesAsync` propagates or handles the `JSException` — on unfixed code the method does not exist in JS so the call always throws
  - **Bug 2 — Cleanup before init**: Create `BarcodeRecognitionService` without calling `InitializeAsync` (`_isInitialized = false`), invoke `CleanupResources` via reflection or a test-accessible wrapper, assert that no JS call is made — on unfixed code the JS call is made and throws `cleanupBarcodeWorker was undefined`
  - **Bug 3 — Duplicate call**: Use a counting mock of `IJSRuntime`, set `_isInitialized = true` and `_lastActivityTime` to > 30s ago, invoke `CleanupResources`, assert `InvokeVoidAsync("recognitionInterop.cleanupBarcodeWorker")` was called exactly once — on unfixed code it is called twice
  - Run tests on UNFIXED code
  - **EXPECTED OUTCOME**: Tests FAIL (this is correct — it proves the bugs exist)
  - Document counterexamples found:
    - Bug 1: `JSException: Could not find 'recognitionInterop.processBarcodeImage'`
    - Bug 2: `JSException: Could not find 'recognitionInterop.cleanupBarcodeWorker' ('cleanupBarcodeWorker' was undefined)`
    - Bug 3: mock records `InvokeVoidAsync("recognitionInterop.cleanupBarcodeWorker")` count = 2
  - Mark task complete when tests are written, run, and failures are documented
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 2. Write preservation property tests (BEFORE implementing fix)
  - **Property 2: Preservation** - Existing Recognition Behaviors Unchanged
  - **IMPORTANT**: Follow observation-first methodology — run UNFIXED code with non-buggy inputs and record actual outputs
  - Observe: `recognitionInterop.initialize()` with all workers available returns `true` and sets `isInitialized = true`
  - Observe: `recognitionInterop.startRecognition(videoElementId, 100, dotNetRef)` starts the frame interval and sets `isProcessing = true`
  - Observe: `recognitionInterop.cleanupBarcodeWorker()` when `isInitialized = true` and worker active sends `{ type: 'cleanup' }` to the worker
  - Observe: `BarcodeRecognitionService.DetectBarcodesAsync` with valid JS results parses, validates and returns results sorted by confidence descending
  - Observe: `recognitionInterop.processCurrentFrame` during active recognition captures the video frame and posts it to all three workers
  - Write property-based tests: for all non-buggy inputs (operations that are NOT `processBarcodeImage` call, NOT cleanup-before-init, NOT duplicate-cleanup), the fixed code produces the same result as the original
  - Verify tests PASS on UNFIXED code (confirms baseline behavior to preserve)
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 3. Fix recognition-interop bugs

  - [x] 3.1 Add `processBarcodeImage` method to `recognition-interop.js`
    - Add method after `cleanupBarcodeWorker` in `window.recognitionInterop`
    - Guard: if `!this.isInitialized` return `"[]"`; if `!this.barcodeWorker` return `"[]"`
    - Return a Promise that posts `{ type: 'process-image', data: { base64: base64Image, options } }` to `barcodeWorker`
    - Add a temporary `message` handler on `barcodeWorker` that resolves on `barcode-result` type
    - On success resolve with `JSON.stringify([e.data.result])`; on failure resolve with `"[]"`
    - Set a 5s timeout that resolves with `"[]"` and removes the handler
    - _Bug_Condition: isBugCondition({ operation: 'processBarcodeImage' }) — 'processBarcodeImage' NOT IN window.recognitionInterop_
    - _Expected_Behavior: method exists, returns valid JSON string, never throws_
    - _Preservation: all other methods in recognitionInterop remain unchanged_
    - _Requirements: 2.1_

  - [x] 3.2 Add `isInitialized` guard to `cleanupBarcodeWorker` in `recognition-interop.js`
    - Add `if (!this.isInitialized) return;` as the first line of `cleanupBarcodeWorker`
    - _Bug_Condition: isBugCondition({ operation: 'cleanupBarcodeWorker', state: { isInitialized: false } })_
    - _Expected_Behavior: method returns immediately without posting to worker when not initialized_
    - _Requirements: 2.2_

  - [x] 3.3 Add `_isInitialized` guard to `CleanupResources` in `BarcodeRecognitionService.cs`
    - Add `if (!_isInitialized) return;` at the start of the `CleanupResources` method body, before the inactivity check
    - _Bug_Condition: isBugCondition({ operation: 'CleanupResources', state: { _isInitialized: false } })_
    - _Expected_Behavior: CleanupResources returns immediately without invoking JS when service is not initialized_
    - _Requirements: 2.2_

  - [x] 3.4 Remove duplicate `cleanupBarcodeWorker` call in `BarcodeRecognitionService.cs`
    - Locate the two consecutive identical calls to `recognitionInterop.cleanupBarcodeWorker` around line 194
    - Remove one of the duplicate calls, keeping exactly one invocation
    - _Bug_Condition: isBugCondition({ operation: 'CleanupResources', callStack: [2x cleanupBarcodeWorker] })_
    - _Expected_Behavior: cleanupBarcodeWorker invoked exactly once per CleanupResources execution_
    - _Preservation: cleanup still occurs when inactivity > 30s and service is initialized_
    - _Requirements: 2.3_

  - [x] 3.5 Add `process-image` handler to `barcode-worker.js`
    - Add a `case 'process-image':` branch in the worker's `onmessage` handler
    - Convert `data.base64` to `ImageData` using `OffscreenCanvas` and `drawImage` with a `Blob` URL or `fetch` from data URI
    - Call the existing `processFrame(imageData, data.options)` function
    - Post back `{ type: 'barcode-result', success: true/false, result }` to the main thread
    - _Bug_Condition: isBugCondition({ operation: 'processBarcodeImage' }) — worker has no handler for 'process-image'_
    - _Expected_Behavior: worker processes base64 image and posts barcode-result back_
    - _Requirements: 2.1_

  - [x] 3.6 Verify bug condition exploration tests now pass
    - **Property 1: Expected Behavior** - Three Interop Bugs Fixed
    - **IMPORTANT**: Re-run the SAME tests from task 1 — do NOT write new tests
    - The tests from task 1 encode the expected behavior for all three bugs
    - When these tests pass, it confirms the expected behavior is satisfied
    - Run bug condition exploration tests from step 1
    - **EXPECTED OUTCOME**: Tests PASS (confirms all three bugs are fixed)
    - _Requirements: 2.1, 2.2, 2.3_

  - [x] 3.7 Verify preservation tests still pass
    - **Property 2: Preservation** - Existing Recognition Behaviors Unchanged
    - **IMPORTANT**: Re-run the SAME tests from task 2 — do NOT write new tests
    - Run preservation property tests from step 2
    - **EXPECTED OUTCOME**: Tests PASS (confirms no regressions)
    - Confirm all tests still pass after fix (no regressions)

- [x] 4. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.
