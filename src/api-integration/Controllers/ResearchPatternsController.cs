using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchPatternsController : ControllerBase
{
    private readonly FxDbContext _db;

    public ResearchPatternsController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ResearchPattern>>> GetAll()
        => await _db.ResearchPatterns.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ResearchPattern>> Get(int id)
    {
        var pattern = await _db.ResearchPatterns.FindAsync(id);
        return pattern is null ? NotFound() : pattern;
    }

    [HttpPost]
    public async Task<ActionResult<ResearchPattern>> Create(ResearchPattern pattern)
    {
        _db.ResearchPatterns.Add(pattern);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = pattern.Id }, pattern);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ResearchPattern pattern)
    {
        if (id != pattern.Id) return BadRequest();
        _db.Entry(pattern).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
