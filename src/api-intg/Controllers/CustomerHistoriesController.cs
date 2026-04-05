using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerHistoriesController : ControllerBase
{
    private readonly FxDbContext _db;

    public CustomerHistoriesController(FxDbContext db) => _db = db;

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<CustomerHistory>>> GetByCustomer(int customerId)
        => await _db.CustomerHistories.Where(h => h.CustomerId == customerId).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerHistory>> Get(int id)
    {
        var history = await _db.CustomerHistories.FindAsync(id);
        return history is null ? NotFound() : history;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerHistory>> Create(CustomerHistory history)
    {
        _db.CustomerHistories.Add(history);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = history.Id }, history);
    }
}
