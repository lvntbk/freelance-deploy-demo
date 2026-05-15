using FreelanceDeployDemo.API.Data;
using FreelanceDeployDemo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelanceDeployDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _context.Projects
            .Include(p => p.Client)
            .ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return Ok(project);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateProjectStatus(int id, string status)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project is null)
            return NotFound();

        project.Status = status;
        await _context.SaveChangesAsync();

        return Ok(project);
    }
}
