using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TraderNewsFeedsController : ControllerBase
{
    private readonly FxDbContext _db;

    public TraderNewsFeedsController(FxDbContext db) => _db = db;

    [HttpGet("trader/{traderId}")]
    public async Task<ActionResult<List<TraderNewsFeed>>> GetByTrader(int traderId)
        => await _db.TraderNewsFeeds.Where(n => n.TraderId == traderId).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TraderNewsFeed>> Get(int id)
    {
        var feed = await _db.TraderNewsFeeds.FindAsync(id);
        return feed is null ? NotFound() : feed;
    }

    [HttpPost]
    public async Task<ActionResult<TraderNewsFeed>> Create(TraderNewsFeed feed)
    {
        _db.TraderNewsFeeds.Add(feed);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = feed.Id }, feed);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TraderNewsFeed feed)
    {
        if (id != feed.Id) return BadRequest();
        _db.Entry(feed).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
