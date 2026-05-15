using FreelanceDeployDemo.API.Data;
using FreelanceDeployDemo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceDeployDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _context.Clients
            .Include(c => c.Projects)
            .ToListAsync();

        return Ok(clients);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
    }
}
