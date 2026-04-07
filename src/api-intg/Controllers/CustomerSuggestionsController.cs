using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerSuggestionsController : ControllerBase
{
    private readonly FxDbContext _db;

    public CustomerSuggestionsController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CustomerSuggestion>>> GetAll()
        => await _db.CustomerSuggestions.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerSuggestion>> Get(int id)
    {
        var suggestion = await _db.CustomerSuggestions.FindAsync(id);
        return suggestion is null ? NotFound() : suggestion;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerSuggestion>> Create(CustomerSuggestion suggestion)
    {
        suggestion.ReceivedAt = DateTime.UtcNow;
        _db.CustomerSuggestions.Add(suggestion);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = suggestion.Id }, suggestion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerSuggestion suggestion)
    {
        if (id != suggestion.Id) return BadRequest();
        _db.Entry(suggestion).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var suggestion = await _db.CustomerSuggestions.FindAsync(id);
        if (suggestion is null) return NotFound();
        _db.CustomerSuggestions.Remove(suggestion);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
