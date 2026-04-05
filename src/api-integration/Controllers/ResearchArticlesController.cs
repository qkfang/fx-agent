using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResearchArticlesController : ControllerBase
{
    private readonly FxDbContext _db;

    public ResearchArticlesController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ResearchArticle>>> GetAll()
        => await _db.ResearchArticles.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ResearchArticle>> Get(int id)
    {
        var article = await _db.ResearchArticles.FindAsync(id);
        return article is null ? NotFound() : article;
    }

    [HttpPost]
    public async Task<ActionResult<ResearchArticle>> Create(ResearchArticle article)
    {
        _db.ResearchArticles.Add(article);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = article.Id }, article);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ResearchArticle article)
    {
        if (id != article.Id) return BadRequest();
        _db.Entry(article).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _db.ResearchArticles.FindAsync(id);
        if (article is null) return NotFound();
        _db.ResearchArticles.Remove(article);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
