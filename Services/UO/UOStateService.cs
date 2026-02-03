using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using pwa_camera_poc_blazor.Models;
using pwa_camera_poc_blazor.Services.Auth;

namespace pwa_camera_poc_blazor.Services.UO
{
    /// <summary>
    /// Gatekeeper responsável por validar se a Unidade Organizadora (UO) está pronta para operação.
    /// Sprint 2 - v1.4 Arquitetura Defensiva
    /// </summary>
    public class UOStateService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        // Estado interno
        private List<UnitInventoryItem>? _currentInventory;
        private int _currentUnitId;
        private bool _isLoaded;

        public bool IsReady => _isLoaded && _currentInventory != null && _currentInventory.Count > 0;
        public int CurrentUnitId => _currentUnitId;

        // Cache do inventário oficial para acesso rápido
        public List<UnitInventoryItem> OfficialInventory => _currentInventory ?? new List<UnitInventoryItem>();

        public UOStateService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        /// <summary>
        /// Valida e carrega os dados da UO atual.
        /// Deve ser chamado antes de permitir acesso à câmera.
        /// </summary>
        public async Task ValidateAndLoadAsync()
        {
            _isLoaded = false;
            _currentInventory = null;

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Usuário não autenticado.");
            }

            _currentUnitId = user.CurrentUnitId.GetValueOrDefault();
            if (_currentUnitId <= 0)
            {
                throw new InvalidOperationException("Unidade Organizadora não selecionada.");
            }

            try
            {
                // Tenta carregar o arquivo de inventário da UO
                // Em produção, isso viria de uma API ou IndexDB cacheado
                var inventoryPath = $"sample-data/unit-{_currentUnitId}-inventory.json";
                var json = await _httpClient.GetStringAsync(inventoryPath);

                if (string.IsNullOrEmpty(json))
                {
                    throw new InvalidOperationException("Arquivo de inventário vazio ou corrompido.");
                }

                _currentInventory = JsonSerializer.Deserialize<List<UnitInventoryItem>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (_currentInventory == null)
                {
                    throw new InvalidOperationException("Falha ao deserializar inventário da UO.");
                }

                _isLoaded = true;
            }
            catch (HttpRequestException)
            {
                // Erro específico para arquivo não encontrado (404) ou erro de rede
                throw new InvalidOperationException($"Inventário da Unidade {_currentUnitId} não encontrado. (Verifique se unit-{_currentUnitId}-inventory.json existe)");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UOStateService] Erro ao carregar unidade: {ex.Message}");
                throw new InvalidOperationException($"Erro ao carregar dados da Unidade: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica mandatoriamente se está pronto. Se não, lança exceção.
        /// </summary>
        public void EnsureReady()
        {
            if (!IsReady)
            {
                throw new InvalidOperationException("ERR_UO_NOT_LOADED: A Unidade Organizadora não foi carregada corretamente.");
            }
        }
    }
}
