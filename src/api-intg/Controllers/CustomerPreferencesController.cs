using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerPreferencesController : ControllerBase
{
    private readonly FxDbContext _db;

    public CustomerPreferencesController(FxDbContext db) => _db = db;

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<CustomerPreference>> GetByCustomer(int customerId)
    {
        var pref = await _db.CustomerPreferences.FirstOrDefaultAsync(p => p.CustomerId == customerId);
        return pref is null ? NotFound() : pref;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerPreference>> Get(int id)
    {
        var pref = await _db.CustomerPreferences.FindAsync(id);
        return pref is null ? NotFound() : pref;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerPreference>> Create(CustomerPreference pref)
    {
        _db.CustomerPreferences.Add(pref);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = pref.Id }, pref);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerPreference pref)
    {
        if (id != pref.Id) return BadRequest();
        _db.Entry(pref).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
