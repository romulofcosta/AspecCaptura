namespace pwa_camera_poc_blazor.Services.Ocr
{
    /// <summary>
    /// Estados possíveis para a máquina de estados do Worker OCR.
    /// Garante gerenciamento determinístico do ciclo de vida.
    /// </summary>
    public enum OcrState
    {
        Idle,           // Worker não existe ou foi destruído
        Initializing,   // Worker está sendo criado/carregado
        Ready,          // Worker carregado e pronto para uso
        Recognizing,    // Processando uma imagem (ocupado)
        Disposed        // Serviço foi descartado, não pode ser reutilizado
    }
}
