using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Storage;
using pwa_camera_poc_blazor.Services.UnidadesGestoras;

namespace pwa_camera_poc_blazor.Services.Scanning
{
    /// <summary>
    /// Orquestrador de decisão centralizado para o processo de scanner.
    /// Define se o que foi lido é válido, se existe e o que fazer a seguir.
    /// Sprint 3 - v1.4 Arquitetura Defensiva
    /// </summary>
    public class ScanOrchestrator
    {
        private readonly UGStateService _ugStateService;
        private readonly IIndexedDbService _dbService;

        public ScanOrchestrator(UGStateService ugStateService, IIndexedDbService dbService)
        {
            _ugStateService = ugStateService;
            _dbService = dbService;
        }

        /// <summary>
        /// Processa o texto extraído (seja via OCR ou Barcode) e retorna uma decisão de negócio.
        /// </summary>
        public async Task<ScanDecision> ProcessScanAsync(string rawText, double confidence = 100)
        {
            // 1. Validação Básica de Leitura
            if (string.IsNullOrWhiteSpace(rawText) || confidence < 60)
            {
                return new ScanDecision
                {
                    ResultType = ScanResultType.ReadError,
                    Message = "Não foi possível fazer a leitura da imagem"
                };
            }

            // 2. Normalização e Regex (Formato do Código)
            var code = NormalizeCode(rawText);
            if (string.IsNullOrEmpty(code))
            {
                return new ScanDecision
                {
                    ResultType = ScanResultType.ValidationFailed,
                    Message = "Código inválido, tente novamente"
                };
            }

            // 3. Validação Contra Inventário Local (Já bipado?)
            var localItems = await _dbService.GetAllAsync<ItemPatrimonio>("items");
            var existingItem = localItems.FirstOrDefault(i => i.Codigo == code);

            if (existingItem != null)
            {
                return new ScanDecision
                {
                    ResultType = ScanResultType.AlreadyRegistered,
                    Message = $"Patrimônio {code} encontrado!",
                    LocalItem = existingItem,
                    ExtractedCode = code
                };
            }

            // 4. Validação Contra Inventário da UG (Gatekeeper de Negócio)
            // UGStateService já deve ter garantido que OfficialInventory está populado
            var officialItem = _ugStateService.OfficialInventory
                .FirstOrDefault(i => i.Codigo.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (officialItem == null)
            {
                return new ScanDecision
                {
                    ResultType = ScanResultType.ItemNotFoundInUg,
                    Message = "Item não identificado ou não pertence a esta Unidade",
                    ExtractedCode = code
                };
            }

            // 5. Sucesso - Item Novo Validado
            return new ScanDecision
            {
                ResultType = ScanResultType.Success,
                Message = $"Patrimônio {code} identificado: {officialItem.Nome}",
                OfficialItem = officialItem,
                ExtractedCode = code
            };
        }

        private string NormalizeCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            // Regex para extrair apenas números e caracteres permitidos (mínimo 4 caracteres)
            var match = Regex.Match(text, @"[0-9A-Z\-\.\/]{4,}");
            return match.Success ? match.Value : string.Empty;
        }
    }
}
