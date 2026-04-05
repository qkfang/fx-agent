using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TraderRecommendationsController : ControllerBase
{
    private readonly FxDbContext _db;

    public TraderRecommendationsController(FxDbContext db) => _db = db;

    [HttpGet("trader/{traderId}")]
    public async Task<ActionResult<List<TraderRecommendation>>> GetByTrader(int traderId)
        => await _db.TraderRecommendations.Where(r => r.TraderId == traderId).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TraderRecommendation>> Get(int id)
    {
        var rec = await _db.TraderRecommendations.FindAsync(id);
        return rec is null ? NotFound() : rec;
    }

    [HttpPost]
    public async Task<ActionResult<TraderRecommendation>> Create(TraderRecommendation recommendation)
    {
        _db.TraderRecommendations.Add(recommendation);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = recommendation.Id }, recommendation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TraderRecommendation recommendation)
    {
        if (id != recommendation.Id) return BadRequest();
        _db.Entry(recommendation).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
