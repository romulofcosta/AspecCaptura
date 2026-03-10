namespace pwa_camera_poc_blazor.Services;

public class ToastService
{
    private readonly Queue<ToastMessage> _toastQueue = new();
    private const int MAX_QUEUE_SIZE = 5;

    public event Action<string, string>? OnShow;

    public void Show(string message, string type = "info")
    {
        // Add to queue if not at max
        if (_toastQueue.Count >= MAX_QUEUE_SIZE)
        {
            _toastQueue.Dequeue();
        }

        _toastQueue.Enqueue(new ToastMessage { Message = message, Type = type, Timestamp = DateTime.UtcNow });
        OnShow?.Invoke(message, type);
    }

    public void ShowSuccess(string message) => Show(message, "success");
    public void ShowError(string message) => Show(message, "error");
    public void ShowWarning(string message) => Show(message, "warning");
    public void ShowInfo(string message) => Show(message, "info");

    public IEnumerable<ToastMessage> GetRecentToasts() => _toastQueue.ToList();
}

public class ToastMessage
{
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info";
    public DateTime Timestamp { get; set; }
}
