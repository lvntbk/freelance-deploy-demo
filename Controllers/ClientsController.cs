using FreelanceDeployDemo.API.Contracts.Clients;
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
    public async Task<ActionResult<List<ClientResponse>>> GetClients(
        CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new ClientResponse(
                c.Id,
                c.FullName,
                c.CompanyName,
                c.Email,
                c.CreatedAt,
                c.Projects.Count))
            .ToListAsync(cancellationToken);

        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientResponse>> GetClient(
        int id,
        CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClientResponse(
                c.Id,
                c.FullName,
                c.CompanyName,
                c.Email,
                c.CreatedAt,
                c.Projects.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (client is null)
            return NotFound();

        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponse>> CreateClient(
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        var client = new Client
        {
            FullName = request.FullName.Trim(),
            CompanyName = request.CompanyName.Trim(),
            Email = request.Email.Trim()
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ClientResponse(
            client.Id,
            client.FullName,
            client.CompanyName,
            client.Email,
            client.CreatedAt,
            0);

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, response);
    }
}
