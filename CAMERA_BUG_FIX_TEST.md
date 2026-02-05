# Camera Functionality Bug Fix - Test Instructions

## Bug Description
The "Escanear Placa" and "Começar Inventário" buttons on the Home page were not activating the camera properly. They were using simple navigation (`Href="/captura"`) instead of triggering camera functionality directly.

## Fix Implementation
1. **Home.razor Changes**:
   - Changed buttons from `Href="/captura"` to `OnClick` handlers
   - Added `IniciarEscaneamento()` method that navigates to `/captura?mode=scan`
   - Added `IniciarInventario()` method that navigates to `/captura?mode=photo`

2. **Camera.razor Changes**:
   - Enhanced `OnInitializedAsync()` to properly handle mode parameters
   - Added logic to set `useOcrFlow` based on mode parameter:
     - `mode=scan` → starts in scan mode (OCR/barcode scanning)
     - `mode=photo` → starts in photo mode (manual photo capture)
     - Default → starts in scan mode

3. **JavaScript Enhancements**:
   - Added `toggleFlash()` function to camera-interop.js for flash control

## Test Scenarios

### Test 1: Escanear Placa Button
1. Navigate to Home page (`/home`)
2. Click "Escanear Placa" button
3. **Expected**: Camera page opens in scan mode with OCR scanning active
4. **Verify**: 
   - Camera feed is visible
   - "SCAN" mode is selected in the toggle
   - ROI (Region of Interest) guide is visible
   - "ESCANEAR AGORA" button is present

### Test 2: Começar Inventário Button
1. Navigate to Home page (`/home`)
2. Click "Começar Inventário" button (appears when no items exist)
3. **Expected**: Camera page opens in photo mode
4. **Verify**:
   - Camera feed is visible
   - "FOTO" mode is selected in the toggle
   - Circular capture button is present (not scan button)

### Test 3: Mode Toggle Functionality
1. Open camera page in any mode
2. Toggle between "FOTO" and "SCAN" modes
3. **Expected**: UI changes appropriately between modes
4. **Verify**:
   - FOTO mode: Shows circular capture button
   - SCAN mode: Shows "ESCANEAR AGORA" button and ROI guide

### Test 4: Flash Toggle
1. Open camera page
2. Click flash icon in header
3. **Expected**: Flash icon toggles between on/off states
4. **Note**: Actual flash functionality depends on device support

### Test 5: Camera Permissions
1. Open camera page on fresh browser/incognito
2. **Expected**: Browser requests camera permission
3. **Verify**: Proper error messages if permission denied

## Browser Compatibility
- **Chrome/Edge Mobile**: Full functionality expected
- **Chrome/Edge Desktop**: Full functionality expected
- **Safari Mobile**: Flash may not be supported
- **Firefox**: Basic functionality expected

## PWA Compatibility
- Camera functionality should work when app is installed as PWA
- Permissions should persist across PWA sessions

## Status: ✅ FIXED
The camera functionality bug has been resolved. Both buttons now properly trigger camera activation with the correct mode parameters.