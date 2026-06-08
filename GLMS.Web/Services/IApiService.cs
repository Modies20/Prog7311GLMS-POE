using GLMS.Data.Entities;

namespace GLMS.Services
{
    public interface IApiService
    {
        // Authentication
        Task<string?> AuthenticateAsync(string username, string password);

        // Contracts
        Task<List<Contract>> GetContractsAsync(string? status = null, string? clientName = null);
        Task<List<Contract>> GetContractsAsync(string? status, DateTime? startDate, DateTime? endDate);
        Task<Contract?> GetContractByIdAsync(int id);
        Task<Contract> CreateContractAsync(Contract contract);
        Task<bool> UpdateContractAsync(Contract contract);
        Task<bool> UpdateContractStatusAsync(int id, string status);
        Task<bool> DeleteContractAsync(int id);



        // Service Requests
        Task<List<ServiceRequest>> GetServiceRequestsByContractAsync(int contractId);
        Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request);

        // Clients
        Task<List<Client>> GetClientsAsync();
    }
}