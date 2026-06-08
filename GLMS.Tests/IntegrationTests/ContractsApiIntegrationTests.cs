using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GLMS.Tests.IntegrationTests
{
    public class ContractsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ContractsApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var loginData = new { username = "admin", password = "Admin123!" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return result?["token"] ?? "";
            }
            catch
            {
                // Fallback: try to extract token substring if response is not valid JSON
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var marker = "\"token\":\"";
                    var idx = json.IndexOf(marker);
                    if (idx >= 0)
                    {
                        var start = idx + marker.Length;
                        var end = json.IndexOf('"', start);
                        if (end > start)
                            return json.Substring(start, end - start);
                    }
                }

                return string.Empty;
            }
        }

        [Fact]
        public async Task GET_Contracts_ReturnsSuccessStatusCode()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthTokenAsync());

            // Act
            var response = await _client.GetAsync("/api/contracts");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contracts_ReturnsNonEmptyList()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthTokenAsync());

            // Act
            var response = await _client.GetAsync("/api/contracts");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(content));
            Assert.Contains("[", content);
        }

        [Fact]
        public async Task POST_Contract_WithValidData_ReturnsCreated()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthTokenAsync());
            var newContract = new
            {
                ClientName = "Integration Test Client",
                ContractNumber = $"TEST-{DateTime.Now.Ticks}",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                Status = "Draft",
                ContractValue = 50000m
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/contracts", newContract);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task PATCH_ContractStatus_WithValidId_ReturnsOk()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthTokenAsync());

            // First create a contract
            var newContract = new
            {
                ClientName = "Status Test Client",
                ContractNumber = $"STATUS-{DateTime.Now.Ticks}",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                Status = "Draft",
                ContractValue = 10000m
            };

            var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
            var createJson = await createResponse.Content.ReadAsStringAsync();
            var createdContract = JsonSerializer.Deserialize<Dictionary<string, object>>(createJson);
            var contractId = Convert.ToInt32(createdContract?["contractId"]?.ToString());

            var statusUpdate = new { status = "Active" };
            var content = new StringContent(JsonSerializer.Serialize(statusUpdate), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"/api/contracts/{contractId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_Contract_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAuthTokenAsync());

            // Act
            var response = await _client.GetAsync("/api/contracts/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Unauthorized_Access_ReturnsUnauthorized()
        {
            // Arrange - No auth token
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/contracts");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}