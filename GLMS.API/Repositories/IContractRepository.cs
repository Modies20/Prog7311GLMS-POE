using GLMS.API.Models;

namespace GLMS.API.Repositories
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllContractsAsync(string? status = null, string? clientName = null);
        Task<Contract?> GetContractByIdAsync(int id);
        Task<Contract> CreateContractAsync(Contract contract);
        Task<bool> UpdateContractStatusAsync(int id, string status);
        Task<bool> ContractExistsAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}