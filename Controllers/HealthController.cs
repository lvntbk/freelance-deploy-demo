using FreelanceDeployDemo.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceDeployDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Check()
    {
        var canConnect = await _context.Database.CanConnectAsync();

        return Ok(new
        {
            status = "Healthy",
            database = canConnect ? "Connected" : "Disconnected",
            timestamp = DateTime.UtcNow
        });
    }
}
