using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TraderSuggestionsController : ControllerBase
{
    private readonly FxDbContext _db;

    public TraderSuggestionsController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<TraderSuggestion>>> GetAll()
        => await _db.TraderSuggestions
            .Include(s => s.Trader)
            .Include(s => s.Customer)
            .Include(s => s.ResearchArticle)
            .ToListAsync();

    [HttpGet("trader/{traderId}")]
    public async Task<ActionResult<List<TraderSuggestion>>> GetByTrader(int traderId)
        => await _db.TraderSuggestions
            .Include(s => s.Customer)
            .Include(s => s.ResearchArticle)
            .Where(s => s.TraderId == traderId)
            .ToListAsync();

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<TraderSuggestion>>> GetByCustomer(int customerId)
        => await _db.TraderSuggestions
            .Include(s => s.Trader)
            .Include(s => s.ResearchArticle)
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TraderSuggestion>> Get(int id)
    {
        var suggestion = await _db.TraderSuggestions
            .Include(s => s.Trader)
            .Include(s => s.Customer)
            .Include(s => s.ResearchArticle)
            .FirstOrDefaultAsync(s => s.Id == id);
        return suggestion is null ? NotFound() : suggestion;
    }

    [HttpPost]
    public async Task<ActionResult<TraderSuggestion>> Create(TraderSuggestion suggestion)
    {
        suggestion.CreatedAt = DateTime.UtcNow;
        _db.TraderSuggestions.Add(suggestion);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = suggestion.Id }, suggestion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TraderSuggestion suggestion)
    {
        if (id != suggestion.Id) return BadRequest();
        _db.Entry(suggestion).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var suggestion = await _db.TraderSuggestions.FindAsync(id);
        if (suggestion is null) return NotFound();
        _db.TraderSuggestions.Remove(suggestion);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
