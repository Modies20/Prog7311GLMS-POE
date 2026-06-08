using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.API.Models;

namespace GLMS.API.Repositories
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetRequestsByContractIdAsync(int contractId)
        {
            return await _context.ServiceRequests
                .Where(s => s.ContractId == contractId)
                .ToListAsync();
        }

        public async Task<ServiceRequest?> GetRequestByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);
        }

        public async Task<ServiceRequest> CreateRequestAsync(ServiceRequest request)
        {
            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> UpdateRequestStatusAsync(int id, string status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return false;

            request.Status = status;
            if (status == "Completed")
            {
                request.CompletionDate = DateTime.UtcNow;
            }
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
