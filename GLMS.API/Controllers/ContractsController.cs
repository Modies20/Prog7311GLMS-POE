using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GLMS.API.Models;
using GLMS.API.Repositories;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository _contractRepository;

        public ContractsController(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        // GET: api/contracts?status=Active&clientName=Tech
        [HttpGet]
        public async Task<IActionResult> GetAllContracts([FromQuery] string? status, [FromQuery] string? clientName)
        {
            var contracts = await _contractRepository.GetAllContractsAsync(status, clientName);
            return Ok(contracts);
        }

        // GET: api/contracts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById(int id)
        {
            var contract = await _contractRepository.GetContractByIdAsync(id);
            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            return Ok(contract);
        }

        // POST: api/contracts
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateContract([FromBody] Contract contract)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdContract = await _contractRepository.CreateContractAsync(contract);
            return CreatedAtAction(nameof(GetContractById), new { id = createdContract.ContractId }, createdContract);
        }

        // PATCH: api/contracts/5/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateContractStatus(int id, [FromBody] ContractStatusUpdateDto statusUpdate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _contractRepository.ContractExistsAsync(id);
            if (!exists)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            var result = await _contractRepository.UpdateContractStatusAsync(id, statusUpdate.Status);
            if (result)
                return Ok(new { message = "Contract status updated successfully", status = statusUpdate.Status });

            return StatusCode(500, new { message = "Failed to update contract status" });
        }
    }
}