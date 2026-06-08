using System.Text;
using System.Text.Json;
using GLMS.Data.Entities;

namespace GLMS.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _authToken;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            // Get token from session or cookie
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                _authToken = token;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var loginData = new { username, password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(json);
                _authToken = result?.token;

                // Store token in session
                _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", _authToken ?? "");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

                return _authToken;
            }
            return null;
        }

        // GetContractsAsync with status and clientName (original)
        public async Task<List<Contract>> GetContractsAsync(string? status = null, string? clientName = null)
        {
            var url = "api/contracts";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrEmpty(clientName)) queryParams.Add($"clientName={Uri.EscapeDataString(clientName)}");
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Contract>>(json) ?? new List<Contract>();
        }

        // GetContractsAsync with status, startDate, endDate (new overload)
        public async Task<List<Contract>> GetContractsAsync(string? status, DateTime? startDate, DateTime? endDate)
        {
            var url = "api/contracts";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Contract>>(json) ?? new List<Contract>();
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json);
        }

        public async Task<Contract> CreateContractAsync(Contract contract)
        {
            var content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/contracts", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json) ?? contract;
        }

        public async Task<bool> UpdateContractAsync(Contract contract)
        {
            // Assuming API has PUT /api/contracts/{id}
            var content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/contracts/{contract.ContractId}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateContractStatusAsync(int id, string status)
        {
            var statusUpdate = new { status };
            var content = new StringContent(JsonSerializer.Serialize(statusUpdate), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"api/contracts/{id}/status", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId)
        {
            var response = await _httpClient.GetAsync($"api/servicerequests/contract/{contractId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ServiceRequest>>(json) ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/servicerequests", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json) ?? request;
        }

        public async Task<List<Client>> GetClientsAsync()
        {
            var response = await _httpClient.GetAsync("api/clients");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Client>>(json) ?? new List<Client>();
        }

        private class LoginResponse
        {
            public string? token { get; set; }
            public string? message { get; set; }
        }
    }
}