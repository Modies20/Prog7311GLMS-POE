using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GLMS.API.Models;
using GLMS.API.Repositories;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestRepository _requestRepository;
        private readonly IContractRepository _contractRepository;

        public ServiceRequestsController(IServiceRequestRepository requestRepository, IContractRepository contractRepository)
        {
            _requestRepository = requestRepository;
            _contractRepository = contractRepository;
        }

        // GET: api/servicerequests/contract/5
        [HttpGet("contract/{contractId}")]
        public async Task<IActionResult> GetRequestsByContract(int contractId)
        {
            var requests = await _requestRepository.GetRequestsByContractIdAsync(contractId);
            return Ok(requests);
        }

        // GET: api/servicerequests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var request = await _requestRepository.GetRequestByIdAsync(id);
            if (request == null)
                return NotFound(new { message = $"Service request with ID {id} not found" });

            return Ok(request);
        }

        // POST: api/servicerequests
        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] ServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify contract exists
            var contractExists = await _contractRepository.ContractExistsAsync(request.ContractId);
            if (!contractExists)
                return BadRequest(new { message = "Contract does not exist" });

            var createdRequest = await _requestRepository.CreateRequestAsync(request);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.ServiceRequestId }, createdRequest);
        }

        // PATCH: api/servicerequests/5/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] string status)
        {
            var result = await _requestRepository.UpdateRequestStatusAsync(id, status);
            if (result)
                return Ok(new { message = "Request status updated successfully", status = status });

            return NotFound(new { message = $"Service request with ID {id} not found" });
        }
    }
}
