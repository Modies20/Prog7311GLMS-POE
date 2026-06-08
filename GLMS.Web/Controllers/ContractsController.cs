using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Data.DbContext;
using GLMS.Data.Entities;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileValidationService _fileValidationService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        ApplicationDbContext context,
        IFileValidationService fileValidationService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<ContractsController> logger)
    {
        _context = context;
        _fileValidationService = fileValidationService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    // GET: Contracts
    public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, out var contractStatus))
        {
            query = query.Where(c => c.Status == contractStatus);
            ViewBag.CurrentStatus = status;
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.StartDate >= startDate.Value);
            ViewBag.CurrentStartDate = startDate.Value.ToString("yyyy-MM-dd");
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.EndDate <= endDate.Value);
            ViewBag.CurrentEndDate = endDate.Value.ToString("yyyy-MM-dd");
        }

        var contracts = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        // Get summary statistics
        ViewBag.TotalContracts = contracts.Count;
        ViewBag.ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active);
        ViewBag.ExpiringSoon = contracts.Count(c => c.Status == ContractStatus.Active && c.EndDate <= DateTime.Today.AddDays(30));

        return View(contracts);
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.ContractId == id);

        if (contract == null)
            return NotFound();

        return View(contract);
    }

    // GET: Contracts/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
        return View();
    }

    // POST: Contracts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ContractNumber,ClientId,StartDate,EndDate,Status,ServiceLevel,TermsAndConditions")] Contract contract,
        IFormFile? agreementFile)
    {
        // Validate file if uploaded
        if (agreementFile != null && agreementFile.Length > 0)
        {
            if (!_fileValidationService.IsValidFile(agreementFile))
            {
                ModelState.AddModelError("agreementFile", _fileValidationService.GetFileValidationError(agreementFile));
            }
        }

        if (ModelState.IsValid)
        {
            // Save file if uploaded
            if (agreementFile != null && agreementFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "contracts");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(agreementFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await agreementFile.CopyToAsync(fileStream);
                }

                contract.FilePath = Path.Combine("uploads", "contracts", uniqueFileName);
                _logger.LogInformation("Contract agreement file saved: {FilePath}", contract.FilePath);
            }

            contract.CreatedAt = DateTime.UtcNow;
            _context.Add(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New contract created: {ContractNumber} for Client ID {ClientId}",
                contract.ContractNumber, contract.ClientId);
            TempData["Success"] = $"Contract '{contract.ContractNumber}' created successfully!";

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
        return View(contract);
    }

    // GET: Contracts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
            return NotFound();

        ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
        return View(contract);
    }

    // POST: Contracts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("ContractId,ContractNumber,ClientId,StartDate,EndDate,Status,ServiceLevel,TermsAndConditions,FilePath")] Contract contract)
    {
        if (id != contract.ContractId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingContract = await _context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.ContractId == id);
                if (existingContract != null)
                {
                    contract.CreatedAt = existingContract.CreatedAt;
                    contract.FilePath = existingContract.FilePath;
                }

                _context.Update(contract);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contract updated: {ContractNumber} (ID: {ContractId})", contract.ContractNumber, contract.ContractId);
                TempData["Success"] = $"Contract '{contract.ContractNumber}' updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(contract.ContractId))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
        return View(contract);
    }

    // GET: Contracts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.ContractId == id);

        if (contract == null)
            return NotFound();

        // Check if contract has service requests
        if (contract.ServiceRequests.Any())
        {
            TempData["Error"] = $"Cannot delete contract '{contract.ContractNumber}' because it has {contract.ServiceRequests.Count} service request(s).";
            return RedirectToAction(nameof(Index));
        }

        return View(contract);
    }

    // POST: Contracts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract != null)
        {
            // Delete associated file if exists
            if (!string.IsNullOrEmpty(contract.FilePath))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, contract.FilePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Deleted contract agreement file: {FilePath}", contract.FilePath);
                }
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract deleted: {ContractNumber} (ID: {ContractId})", contract.ContractNumber, contract.ContractId);
            TempData["Success"] = $"Contract '{contract.ContractNumber}' deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Contracts/DownloadFile/5
    public async Task<IActionResult> DownloadFile(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null || string.IsNullOrEmpty(contract.FilePath))
            return NotFound("Contract or file not found.");

        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.FilePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound("The contract agreement file no longer exists on the server.");

        var memory = new MemoryStream();
        await using (var stream = new FileStream(filePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        var fileName = Path.GetFileName(contract.FilePath);
        // Remove GUID prefix for downloaded filename
        var displayFileName = fileName.Contains('_') ? fileName.Substring(fileName.IndexOf('_') + 1) : fileName;

        _logger.LogInformation("Contract agreement downloaded: {ContractNumber} - {FileName}", contract.ContractNumber, displayFileName);

        return File(memory, "application/pdf", displayFileName);
    }

    private bool ContractExists(int id)
    {
        return _context.Contracts.Any(e => e.ContractId == id);
    }
}