using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchDraftsController : ControllerBase
{
    private readonly FxDbContext _db;

    public ResearchDraftsController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ResearchDraft>>> GetAll()
        => await _db.ResearchDrafts.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ResearchDraft>> Get(int id)
    {
        var draft = await _db.ResearchDrafts.FindAsync(id);
        return draft is null ? NotFound() : draft;
    }

    [HttpPost]
    public async Task<ActionResult<ResearchDraft>> Create(ResearchDraft draft)
    {
        _db.ResearchDrafts.Add(draft);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = draft.Id }, draft);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ResearchDraft draft)
    {
        if (id != draft.Id) return BadRequest();
        _db.Entry(draft).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
