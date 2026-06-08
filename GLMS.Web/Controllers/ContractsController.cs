using GLMS.Data.Entities;          // Your entity models (Contract, Client, etc.)
using GLMS.Services;
using GLMS.Web.Services;           // IFileValidationService, IApiService
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly IApiService _apiService;
    private readonly IFileValidationService _fileValidationService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IApiService apiService,                              // NEW: injected API service
        IFileValidationService fileValidationService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<ContractsController> logger)
    {
        _apiService = apiService;
        _fileValidationService = fileValidationService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    // GET: Contracts
    public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate)
    {
        // Call API to get contracts with filters
        var contracts = await _apiService.GetContractsAsync(status, startDate, endDate);

        // Get summary statistics
        ViewBag.TotalContracts = contracts.Count;
        ViewBag.ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active);
        ViewBag.ExpiringSoon = contracts.Count(c => c.Status == ContractStatus.Active && c.EndDate <= DateTime.Today.AddDays(30));

        // Preserve filter values for the view
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");

        return View(contracts);
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var contract = await _apiService.GetContractByIdAsync(id.Value);
        if (contract == null)
            return NotFound();

        // Also load service requests from API if needed (the API should include them)
        var serviceRequests = await _apiService.GetServiceRequestsByContractAsync(id.Value);
        ViewBag.ServiceRequests = serviceRequests;

        return View(contract);
    }

    // GET: Contracts/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = await _apiService.GetClientsAsync();   // NEW: get clients from API
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
            // Save file locally (same as before)
            string? savedFilePath = null;
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

                savedFilePath = Path.Combine("uploads", "contracts", uniqueFileName);
                _logger.LogInformation("Contract agreement file saved: {FilePath}", savedFilePath);
            }

            // Set file path on contract
            contract.FilePath = savedFilePath;
            contract.CreatedAt = DateTime.UtcNow;

            // Call API to create contract
            var createdContract = await _apiService.CreateContractAsync(contract);

            _logger.LogInformation("New contract created via API: {ContractNumber} for Client ID {ClientId}",
                createdContract.ContractNumber, createdContract.ClientId);
            TempData["Success"] = $"Contract '{createdContract.ContractNumber}' created successfully!";

            return RedirectToAction(nameof(Index));
        }

        ViewBag.Clients = await _apiService.GetClientsAsync();
        return View(contract);
    }

    // GET: Contracts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var contract = await _apiService.GetContractByIdAsync(id.Value);
        if (contract == null)
            return NotFound();

        ViewBag.Clients = await _apiService.GetClientsAsync();
        return View(contract);
    }

    // POST: Contracts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("ContractId,ContractNumber,ClientId,StartDate,EndDate,Status,ServiceLevel,TermsAndConditions,FilePath,CreatedAt")] Contract contract)
    {
        if (id != contract.ContractId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Call API to update contract
                var updated = await _apiService.UpdateContractAsync(contract);
                if (!updated)
                {
                    TempData["Error"] = "Failed to update contract. It may have been deleted.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Contract updated via API: {ContractNumber} (ID: {ContractId})", contract.ContractNumber, contract.ContractId);
                TempData["Success"] = $"Contract '{contract.ContractNumber}' updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", contract.ContractId);
                TempData["Error"] = "An error occurred while updating the contract.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Clients = await _apiService.GetClientsAsync();
        return View(contract);
    }

    // GET: Contracts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var contract = await _apiService.GetContractByIdAsync(id.Value);
        if (contract == null)
            return NotFound();

        // Get service requests count from API
        var serviceRequests = await _apiService.GetServiceRequestsByContractAsync(id.Value);
        if (serviceRequests.Any())
        {
            TempData["Error"] = $"Cannot delete contract '{contract.ContractNumber}' because it has {serviceRequests.Count} service request(s).";
            return RedirectToAction(nameof(Index));
        }

        return View(contract);
    }

    // POST: Contracts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _apiService.GetContractByIdAsync(id);
        if (contract != null)
        {
            // Delete associated file if exists (local file)
            if (!string.IsNullOrEmpty(contract.FilePath))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, contract.FilePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Deleted contract agreement file: {FilePath}", contract.FilePath);
                }
            }

            // Call API to delete contract
            var deleted = await _apiService.DeleteContractAsync(id);
            if (deleted)
            {
                _logger.LogInformation("Contract deleted via API: {ContractNumber} (ID: {ContractId})", contract.ContractNumber, contract.ContractId);
                TempData["Success"] = $"Contract '{contract.ContractNumber}' deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete contract from the system.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Contracts/DownloadFile/5
    public async Task<IActionResult> DownloadFile(int id)
    {
        var contract = await _apiService.GetContractByIdAsync(id);
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
        var displayFileName = fileName.Contains('_') ? fileName.Substring(fileName.IndexOf('_') + 1) : fileName;

        _logger.LogInformation("Contract agreement downloaded: {ContractNumber} - {FileName}", contract.ContractNumber, displayFileName);

        return File(memory, "application/pdf", displayFileName);
    }
}