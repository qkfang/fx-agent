using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradersController : ControllerBase
{
    private readonly FxDbContext _db;

    public TradersController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Trader>>> GetAll()
        => await _db.Traders.Include(t => t.Recommendations).Include(t => t.NewsFeeds).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Trader>> Get(int id)
    {
        var trader = await _db.Traders.Include(t => t.Recommendations).Include(t => t.NewsFeeds)
            .FirstOrDefaultAsync(t => t.Id == id);
        return trader is null ? NotFound() : trader;
    }

    [HttpPost]
    public async Task<ActionResult<Trader>> Create(Trader trader)
    {
        _db.Traders.Add(trader);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = trader.Id }, trader);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Trader trader)
    {
        if (id != trader.Id) return BadRequest();
        _db.Entry(trader).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var trader = await _db.Traders.FindAsync(id);
        if (trader is null) return NotFound();
        _db.Traders.Remove(trader);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
