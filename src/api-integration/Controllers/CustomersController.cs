using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly FxDbContext _db;

    public CustomersController(FxDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAll()
        => await _db.Customers.Include(c => c.Portfolios).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> Get(int id)
    {
        var customer = await _db.Customers.Include(c => c.Portfolios).FirstOrDefaultAsync(c => c.Id == id);
        return customer is null ? NotFound() : customer;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Customer customer)
    {
        if (id != customer.Id) return BadRequest();
        _db.Entry(customer).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
