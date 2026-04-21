using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Data.DbContext;
using GLMS.Data.Entities;

namespace GLMS.Web.Controllers;

public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Clients
    public async Task<IActionResult> Index()
    {
        var clients = await _context.Clients
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(clients);
    }

    // GET: Clients/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var client = await _context.Clients
            .Include(c => c.Contracts)
            .ThenInclude(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.ClientId == id);

        if (client == null)
            return NotFound();

        return View(client);
    }

    // GET: Clients/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Email,Phone,Address,Region,TaxId")] Client client)
    {
        if (ModelState.IsValid)
        {
            client.CreatedAt = DateTime.UtcNow;
            _context.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New client created: {ClientName} (ID: {ClientId})", client.Name, client.ClientId);
            TempData["Success"] = $"Client '{client.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // GET: Clients/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var client = await _context.Clients.FindAsync(id);
        if (client == null)
            return NotFound();

        return View(client);
    }

    // POST: Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,Email,Phone,Address,Region,TaxId,CreatedAt")] Client client)
    {
        if (id != client.ClientId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Client updated: {ClientName} (ID: {ClientId})", client.Name, client.ClientId);
                TempData["Success"] = $"Client '{client.Name}' updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.ClientId))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // GET: Clients/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.ClientId == id);

        if (client == null)
            return NotFound();

        // Check if client has contracts
        if (client.Contracts.Any())
        {
            TempData["Error"] = $"Cannot delete client '{client.Name}' because they have {client.Contracts.Count} existing contract(s).";
            return RedirectToAction(nameof(Index));
        }

        return View(client);
    }

    // POST: Clients/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client != null)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Client deleted: {ClientName} (ID: {ClientId})", client.Name, client.ClientId);
            TempData["Success"] = $"Client '{client.Name}' deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ClientExists(int id)
    {
        return _context.Clients.Any(e => e.ClientId == id);
    }
}