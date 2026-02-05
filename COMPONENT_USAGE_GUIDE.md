# 📦 Component Usage Guide
**ASPEC Capture PWA - Camera Components**

---

## 📋 Overview

This guide provides examples and best practices for using the refactored camera components. All components are located in `Components/Camera/` directory.

---

## 🎯 Component Catalog

### 1. PhotoPreview.razor
**Purpose:** Display current photo with swipe navigation

**Usage:**
```razor
<PhotoPreview 
    Photos="@capturedPhotos" 
    CurrentIndex="@currentPreviewIndex"
    CurrentIndexChanged="@((index) => currentPreviewIndex = index)" />
```

**Props:**
| Prop | Type | Required | Description |
|------|------|----------|-------------|
| Photos | List<string> | Yes | List of photo URLs (base64 or URLs) |
| CurrentIndex | int | Yes | Index of currently displayed photo |
| CurrentIndexChanged | EventCallback<int> | Yes | Callback when index changes |

**Features:**
- Automatic swipe indicators (chevrons)
- Photo counter badge
- Empty state handling
- Accessibility labels

**Example:**
```csharp
private List<string> capturedPhotos = new();
private int currentPreviewIndex = 0;

// In markup:
<PhotoPreview 
    Photos="@capturedPhotos" 
    CurrentIndex="@currentPreviewIndex"
    CurrentIndexChanged="@OnPhotoIndexChanged" />

// In code:
private void OnPhotoIndexChanged(int newIndex)
{
    currentPreviewIndex = newIndex;
    StateHasChanged();
}
```

---

### 2. PhotoThumbnails.razor
**Purpose:** Display photo thumbnails with add/remove functionality

**Usage:**
```razor
<PhotoThumbnails 
    Photos="@capturedPhotos"
    CurrentIndex="@currentPreviewIndex"
    OnAddPhoto="@OpenAddPhotoModal"
    OnThumbnailSelected="@((index) => currentPreviewIndex = index)"
    OnPhotoRemoved="@RemovePhoto" />
```

**Props:**
| Prop | Type | Required | Description |
|------|------|----------|-------------|
| Photos | List<string> | Yes | List of photo URLs |
| CurrentIndex | int | Yes | Index of selected thumbnail |
| OnAddPhoto | EventCallback | Yes | Callback when add button clicked |
| OnThumbnailSelected | EventCallback<int> | Yes | Callback when thumbnail clicked |
| OnPhotoRemoved | EventCallback<int> | Yes | Callback when remove button clicked |

**Features:**
- Horizontal scrollable list
- Active thumbnail highlighting
- Remove button per thumbnail
- Scale-in animation on selection

**Example:**
```csharp
private void OpenAddPhotoModal()
{
    isPhotoModalOpen = true;
}

private void RemovePhoto(int index)
{
    capturedPhotos.RemoveAt(index);
    if (currentPreviewIndex >= capturedPhotos.Count)
        currentPreviewIndex = Math.Max(0, capturedPhotos.Count - 1);
}
```

---

### 3. CameraViewport.razor
**Purpose:** Camera feed display with controls and ROI guide

**Usage:**
```razor
<CameraViewport 
    VideoElementId="camera-feed"
    CanvasElementId="camera-canvas"
    BackUrl="/home"
    IsFlashOn="@isFlashOn"
    IsScanMode="@useOcrFlow"
    IsProcessing="@(processingStatus == "Analisando...")"
    StatusMessage="@processingStatus"
    OnFlashToggle="@ToggleFlash"
    OnModeChange="@SetMode"
    OnCapture="@CaptureAndProcess" />
```

**Props:**
| Prop | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| VideoElementId | string | No | "camera-feed" | Video element ID |
| CanvasElementId | string | No | "camera-canvas" | Canvas element ID |
| BackUrl | string | No | "/home" | Back navigation URL |
| IsFlashOn | bool | Yes | - | Flash state |
| IsScanMode | bool | Yes | - | Current mode (true=scan, false=photo) |
| IsProcessing | bool | Yes | - | Processing state |
| StatusMessage | string | Yes | - | Status message to display |
| OnFlashToggle | EventCallback | Yes | - | Flash toggle callback |
| OnModeChange | EventCallback<bool> | Yes | - | Mode change callback |
| OnCapture | EventCallback | Yes | - | Capture callback |

**Features:**
- Video feed with canvas overlay
- Header actions (back, flash)
- ROI guide with animated corners
- Mode toggle (Photo/Scan)
- Capture button (changes based on mode)
- Processing status display

**Example:**
```csharp
private bool isFlashOn = false;
private bool useOcrFlow = true;
private string processingStatus = "";

private async Task ToggleFlash()
{
    isFlashOn = !isFlashOn;
    await JSRuntime.InvokeVoidAsync("cameraInterop.toggleFlash", "camera-feed", isFlashOn);
}

private void SetMode(bool scanMode)
{
    useOcrFlow = scanMode;
    StateHasChanged();
}

private async Task CaptureAndProcess()
{
    if (useOcrFlow)
    {
        processingStatus = "Analisando...";
        // Scan logic
    }
    else
    {
        // Photo capture logic
    }
}
```

---

### 4. ItemForm.razor
**Purpose:** Form for entering item details

**Usage:**
```razor
<ItemForm 
    Item="@itemModel"
    EditContext="@editContext"
    IsSaving="@isSaving"
    OnSubmit="@HandleSave"
    OnCancel="@CancelForm" />
```

**Props:**
| Prop | Type | Required | Description |
|------|------|----------|-------------|
| Item | ItemPatrimonio | Yes | Item model to bind |
| EditContext | EditContext | Yes | Form edit context |
| IsSaving | bool | Yes | Saving state |
| OnSubmit | EventCallback | Yes | Submit callback |
| OnCancel | EventCallback | Yes | Cancel callback |

**Features:**
- Two sections (Identification, Location)
- Form validation
- Category dropdown
- Save/Cancel buttons
- Accessibility labels

**Example:**
```csharp
private ItemPatrimonio itemModel = new();
private EditContext? editContext;
private bool isSaving = false;

protected override void OnInitialized()
{
    editContext = new EditContext(itemModel);
}

private async Task HandleSave()
{
    if (isSaving) return;
    isSaving = true;
    
    try
    {
        // Save logic
        await DbService.AddAsync("items", itemModel);
        Navigation.NavigateTo("/home");
    }
    finally
    {
        isSaving = false;
    }
}

private void CancelForm()
{
    Navigation.NavigateTo("/home");
}
```

---

### 5. AddPhotoModal.razor
**Purpose:** Modal for adding photos (camera or upload)

**Usage:**
```razor
<AddPhotoModal 
    @bind-IsVisible="isPhotoModalOpen"
    OnTakePhoto="@TakePhotoFromModal"
    OnFileUpload="@HandleFileUpload" />
```

**Props:**
| Prop | Type | Required | Description |
|------|------|----------|-------------|
| IsVisible | bool | Yes | Modal visibility (two-way binding) |
| IsVisibleChanged | EventCallback<bool> | Yes | Visibility change callback |
| OnTakePhoto | EventCallback | Yes | Take photo callback |
| OnFileUpload | EventCallback<InputFileChangeEventArgs> | Yes | File upload callback |

**Features:**
- Take photo button
- Upload file button
- Cancel button
- Scale-in animation
- Auto-close on action

**Example:**
```csharp
private bool isPhotoModalOpen = false;

private async Task TakePhotoFromModal()
{
    isPhotoModalOpen = false;
    isCameraInModalActive = true;
    await Task.Delay(100);
    await _cameraService.IniciarCameraAsync("modal-camera-feed", false);
}

private async Task HandleFileUpload(InputFileChangeEventArgs e)
{
    var file = e.File;
    if (file != null)
    {
        var buffer = new byte[file.Size];
        await file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 5).ReadAsync(buffer);
        var base64 = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
        capturedPhotos.Add(base64);
    }
}
```

---

### 6. CameraModal.razor
**Purpose:** Full-screen camera modal for additional photos

**Usage:**
```razor
<CameraModal 
    IsActive="@isCameraInModalActive"
    VideoElementId="modal-camera-feed"
    OnClose="@StopModalCamera"
    OnCapture="@CapturePhotoFromModal" />
```

**Props:**
| Prop | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| IsActive | bool | Yes | - | Modal active state |
| VideoElementId | string | No | "modal-camera-feed" | Video element ID |
| OnClose | EventCallback | Yes | - | Close callback |
| OnCapture | EventCallback | Yes | - | Capture callback |

**Features:**
- Full-screen video feed
- Close button
- Capture button
- Fade-in animation

**Example:**
```csharp
private bool isCameraInModalActive = false;

private async Task StopModalCamera()
{
    await _cameraService.PararCameraAsync("modal-camera-feed");
    isCameraInModalActive = false;
    isPhotoModalOpen = true; // Return to add photo modal
}

private async Task CapturePhotoFromModal()
{
    var photoData = await _cameraService.CapturarImagemAsync("modal-camera-feed");
    if (!string.IsNullOrEmpty(photoData))
    {
        capturedPhotos.Add(photoData);
        currentPreviewIndex = capturedPhotos.Count - 1;
        await StopModalCamera();
    }
}
```

---

## 🎨 Complete Example

Here's a complete example showing how to use all components together:

```razor
@page "/captura"
@using pwa_camera_poc_blazor.Models
@using pwa_camera_poc_blazor.Components.Camera

<PageTitle>Capturar - ASPEC Capture</PageTitle>

@if (isFormVisible)
{
    <div class="d-flex flex-column" style="height: 100%; position: relative;">
        <div class="flex-grow-1 overflow-y-auto pa-4">
            <MudCard Elevation="4" Class="mb-4">
                <MudCardHeader>
                    <CardHeaderContent>
                        <MudText Typo="Typo.h6">Adicionar Informações</MudText>
                    </CardHeaderContent>
                    <CardHeaderActions>
                        <MudIconButton Icon="@Icons.Material.Filled.Close" OnClick="CancelForm" />
                    </CardHeaderActions>
                </MudCardHeader>
                <MudCardContent>
                    <!-- Photo Preview with Swipe -->
                    <PhotoPreview 
                        Photos="@capturedPhotos" 
                        CurrentIndex="@currentPreviewIndex"
                        CurrentIndexChanged="@((index) => currentPreviewIndex = index)" />

                    <!-- Photo Thumbnails -->
                    <PhotoThumbnails 
                        Photos="@capturedPhotos"
                        CurrentIndex="@currentPreviewIndex"
                        OnAddPhoto="@(() => isPhotoModalOpen = true)"
                        OnThumbnailSelected="@((index) => currentPreviewIndex = index)"
                        OnPhotoRemoved="@RemovePhoto" />

                    <!-- Item Form -->
                    <ItemForm 
                        Item="@itemModel"
                        EditContext="@editContext"
                        IsSaving="@isSaving"
                        OnSubmit="@HandleSave"
                        OnCancel="@CancelForm" />
                </MudCardContent>
            </MudCard>
        </div>
    </div>
}
else
{
    <!-- Camera Viewport -->
    <CameraViewport 
        IsFlashOn="@isFlashOn"
        IsScanMode="@useOcrFlow"
        IsProcessing="@(processingStatus == "Analisando...")"
        StatusMessage="@processingStatus"
        OnFlashToggle="@ToggleFlash"
        OnModeChange="@SetMode"
        OnCapture="@CaptureAndProcess" />
}

<!-- Add Photo Modal -->
<AddPhotoModal 
    @bind-IsVisible="isPhotoModalOpen"
    OnTakePhoto="@TakePhotoFromModal"
    OnFileUpload="@HandleFileUpload" />

<!-- Camera Modal -->
<CameraModal 
    IsActive="@isCameraInModalActive"
    OnClose="@StopModalCamera"
    OnCapture="@CapturePhotoFromModal" />

@code {
    // State
    private bool isFormVisible = false;
    private List<string> capturedPhotos = new();
    private int currentPreviewIndex = 0;
    private ItemPatrimonio itemModel = new();
    private EditContext? editContext;
    private bool isSaving = false;
    private bool isFlashOn = false;
    private bool useOcrFlow = true;
    private string processingStatus = "";
    private bool isPhotoModalOpen = false;
    private bool isCameraInModalActive = false;

    // Methods
    protected override void OnInitialized()
    {
        editContext = new EditContext(itemModel);
    }

    private async Task ToggleFlash()
    {
        isFlashOn = !isFlashOn;
        // Flash toggle logic
    }

    private void SetMode(bool scanMode)
    {
        useOcrFlow = scanMode;
    }

    private async Task CaptureAndProcess()
    {
        // Capture logic
    }

    private void RemovePhoto(int index)
    {
        capturedPhotos.RemoveAt(index);
        if (currentPreviewIndex >= capturedPhotos.Count)
            currentPreviewIndex = Math.Max(0, capturedPhotos.Count - 1);
    }

    private async Task HandleSave()
    {
        // Save logic
    }

    private void CancelForm()
    {
        Navigation.NavigateTo("/home");
    }

    private async Task TakePhotoFromModal()
    {
        // Take photo logic
    }

    private async Task HandleFileUpload(InputFileChangeEventArgs e)
    {
        // File upload logic
    }

    private async Task StopModalCamera()
    {
        // Stop camera logic
    }

    private async Task CapturePhotoFromModal()
    {
        // Capture from modal logic
    }
}
```

---

## 💡 Best Practices

### 1. State Management
- Keep state in parent component
- Pass data down via props
- Handle events in parent

### 2. Event Handling
- Use EventCallback for parent-child communication
- Don't mutate props in child components
- Always call StateHasChanged() after state changes

### 3. Accessibility
- All components have ARIA labels
- Use semantic HTML
- Support keyboard navigation

### 4. Performance
- Use EventCallback instead of Action
- Avoid unnecessary re-renders
- Use CSS animations (GPU accelerated)

### 5. Error Handling
- Validate props in child components
- Handle null/empty states gracefully
- Provide meaningful error messages

---

## 🐛 Troubleshooting

### Component Not Rendering
**Problem:** Component doesn't appear
**Solution:** Check that all required props are provided

### Events Not Firing
**Problem:** EventCallback not triggering
**Solution:** Ensure EventCallback is awaited in parent

### State Not Updating
**Problem:** UI doesn't reflect state changes
**Solution:** Call StateHasChanged() after state modifications

### Styling Issues
**Problem:** Component styles not applying
**Solution:** Check that CSS files are included in index.html

---

## 📚 Additional Resources

### Documentation
- [Blazor Component Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/)
- [EventCallback Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling)
- [MudBlazor Documentation](https://mudblazor.com/)

### Code References
- `Components/Camera/` - All camera components
- `Pages/Camera.razor` - Parent component example
- `SPRINT3_PHASE2_IMPLEMENTATION_REPORT.md` - Refactoring details

---

*Guide version: 1.0*  
*Last updated: February 5, 2026*  
*ASPEC Capture PWA v1.6.0*
