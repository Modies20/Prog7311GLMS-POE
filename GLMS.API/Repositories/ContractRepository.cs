using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.API.Models;

namespace GLMS.API.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contract>> GetAllContractsAsync(string? status = null, string? clientName = null)
        {
            var query = _context.Contracts.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrEmpty(clientName))
            {
                query = query.Where(c => c.ClientName.Contains(clientName));
            }

            return await query.ToListAsync();
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.ContractId == id);
        }

        public async Task<Contract> CreateContractAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> UpdateContractStatusAsync(int id, string status)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return false;

            contract.Status = status;
            return await SaveChangesAsync();
        }

        public async Task<bool> ContractExistsAsync(int id)
        {
            return await _context.Contracts.AnyAsync(c => c.ContractId == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}