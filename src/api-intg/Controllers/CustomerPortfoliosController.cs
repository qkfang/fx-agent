using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerPortfoliosController : ControllerBase
{
    private readonly FxDbContext _db;

    public CustomerPortfoliosController(FxDbContext db) => _db = db;

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<CustomerPortfolio>>> GetByCustomer(int customerId)
        => await _db.CustomerPortfolios.Where(p => p.CustomerId == customerId).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerPortfolio>> Get(int id)
    {
        var portfolio = await _db.CustomerPortfolios.FindAsync(id);
        return portfolio is null ? NotFound() : portfolio;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerPortfolio>> Create(CustomerPortfolio portfolio)
    {
        _db.CustomerPortfolios.Add(portfolio);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = portfolio.Id }, portfolio);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerPortfolio portfolio)
    {
        if (id != portfolio.Id) return BadRequest();
        _db.Entry(portfolio).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var portfolio = await _db.CustomerPortfolios.FindAsync(id);
        if (portfolio is null) return NotFound();
        _db.CustomerPortfolios.Remove(portfolio);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
