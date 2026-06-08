using GLMS.API.Models;

namespace GLMS.API.Repositories
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetRequestsByContractIdAsync(int contractId);
        Task<ServiceRequest?> GetRequestByIdAsync(int id);
        Task<ServiceRequest> CreateRequestAsync(ServiceRequest request);
        Task<bool> UpdateRequestStatusAsync(int id, string status);
    }
}
