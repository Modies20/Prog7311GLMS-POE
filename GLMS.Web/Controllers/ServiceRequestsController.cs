using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Data.DbContext;
using GLMS.Data.Entities;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyExchangeService _currencyService;
    private readonly IContractValidationService _contractValidationService;
    private readonly ILogger<ServiceRequestsController> _logger;

    public ServiceRequestsController(
        ApplicationDbContext context,
        ICurrencyExchangeService currencyService,
        IContractValidationService contractValidationService,
        ILogger<ServiceRequestsController> logger)
    {
        _context = context;
        _currencyService = currencyService;
        _contractValidationService = contractValidationService;
        _logger = logger;
    }

    // GET: ServiceRequests
    public async Task<IActionResult> Index()
    {
        var serviceRequests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .ThenInclude(c => c!.Client)
            .OrderByDescending(sr => sr.RequestDate)
            .ToListAsync();

        // Get summary statistics
        ViewBag.TotalRequests = serviceRequests.Count;
        ViewBag.PendingRequests = serviceRequests.Count(sr => sr.Status == RequestStatus.Pending);
        ViewBag.CompletedRequests = serviceRequests.Count(sr => sr.Status == RequestStatus.Completed);
        ViewBag.TotalValueZAR = serviceRequests.Sum(sr => sr.AmountZAR);

        return View(serviceRequests);
    }

    // GET: ServiceRequests/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id);

        if (serviceRequest == null)
            return NotFound();

        return View(serviceRequest);
    }

    // GET: ServiceRequests/Create
    public async Task<IActionResult> Create()
    {
        var activeContracts = await _context.Contracts
            .Include(c => c.Client)
            .Where(c => c.Status == ContractStatus.Active &&
                       c.StartDate <= DateTime.Today &&
                       c.EndDate >= DateTime.Today)
            .OrderBy(c => c.ContractNumber)
            .ToListAsync();

        if (!activeContracts.Any())
        {
            TempData["Warning"] = "No active contracts available. Please create an active contract before raising service requests.";
        }

        ViewBag.Contracts = activeContracts;
        ViewBag.CurrentRate = await _currencyService.GetUSDtoZARRateAsync();

        return View();
    }

    // POST: ServiceRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContractId,Description,AmountUSD,Notes")] ServiceRequest serviceRequest)
    {
        // Validate contract exists and is active
        var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("ContractId", "Selected contract does not exist.");
        }
        else if (!_contractValidationService.CanCreateServiceRequest(contract))
        {
            ModelState.AddModelError("ContractId", _contractValidationService.GetValidationErrorMessage(contract));
        }

        if (ModelState.IsValid)
        {
            // Get exchange rate and calculate ZAR amount
            var exchangeRate = await _currencyService.GetUSDtoZARRateAsync();
            serviceRequest.AmountZAR = serviceRequest.AmountUSD * exchangeRate;
            serviceRequest.ExchangeRateUsed = exchangeRate;
            serviceRequest.RequestNumber = GenerateRequestNumber();
            serviceRequest.RequestDate = DateTime.UtcNow;
            serviceRequest.Status = RequestStatus.Pending;

            _context.Add(serviceRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service request created: {RequestNumber} for Contract {ContractId}, Amount: {AmountZAR:C}",
                serviceRequest.RequestNumber, serviceRequest.ContractId, serviceRequest.AmountZAR);

            TempData["Success"] = $"Service Request '{serviceRequest.RequestNumber}' created successfully! " +
                                 $"Amount in ZAR: {serviceRequest.AmountZAR:C} (Rate: 1 USD = {exchangeRate:F2} ZAR)";

            return RedirectToAction(nameof(Index));
        }

        // Re-populate view data
        var activeContracts = await _context.Contracts
            .Include(c => c.Client)
            .Where(c => c.Status == ContractStatus.Active &&
                       c.StartDate <= DateTime.Today &&
                       c.EndDate >= DateTime.Today)
            .OrderBy(c => c.ContractNumber)
            .ToListAsync();

        ViewBag.Contracts = activeContracts;
        ViewBag.CurrentRate = await _currencyService.GetUSDtoZARRateAsync();

        return View(serviceRequest);
    }

    // POST: ServiceRequests/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, RequestStatus status)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);
        if (serviceRequest == null)
            return NotFound();

        var oldStatus = serviceRequest.Status;
        serviceRequest.Status = status;

        if (status == RequestStatus.Completed && !serviceRequest.CompletionDate.HasValue)
        {
            serviceRequest.CompletionDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Service request {RequestNumber} status changed from {OldStatus} to {NewStatus}",
            serviceRequest.RequestNumber, oldStatus, status);

        TempData["Success"] = $"Service Request '{serviceRequest.RequestNumber}' status updated to {GetStatusDisplayName(status)}";

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: ServiceRequests/GetExchangeRate (API endpoint for AJAX)
    [HttpGet]
    public async Task<IActionResult> GetExchangeRate()
    {
        try
        {
            var rate = await _currencyService.GetUSDtoZARRateAsync();
            return Json(new { success = true, rate = rate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate via API");
            return Json(new { success = false, message = ex.Message, rate = 18.50m });
        }
    }

    private string GenerateRequestNumber()
    {
        return $"SRQ-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private string GetStatusDisplayName(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Pending => "Pending",
            RequestStatus.Approved => "Approved",
            RequestStatus.InProgress => "In Progress",
            RequestStatus.Completed => "Completed",
            RequestStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }
}
