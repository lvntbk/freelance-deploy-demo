using FreelanceDeployDemo.API.Contracts.Projects;
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
    public async Task<ActionResult<List<ProjectResponse>>> GetProjects(
        CancellationToken cancellationToken)
    {
        var projects = await ProjectResponses(
                _context.Projects.AsNoTracking().OrderBy(p => p.Id))
            .ToListAsync(cancellationToken);

        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetProject(
        int id,
        CancellationToken cancellationToken)
    {
        var project = await ProjectResponses(
                _context.Projects.AsNoTracking().Where(p => p.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
            return NotFound();

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> CreateProject(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var clientName = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Id == request.ClientId)
            .Select(c => c.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        if (clientName is null)
        {
            ModelState.AddModelError(nameof(request.ClientId), "Client does not exist.");
            return ValidationProblem(ModelState);
        }

        var project = new Project
        {
            ProjectName = request.ProjectName.Trim(),
            Technology = request.Technology.Trim(),
            ClientId = request.ClientId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new ProjectResponse(
            project.Id,
            project.ProjectName,
            project.Technology,
            project.Status,
            project.CreatedAt,
            project.ClientId,
            clientName);

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, response);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<ProjectResponse>> UpdateProjectStatus(
        int id,
        UpdateProjectStatusRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project is null)
            return NotFound();

        project.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new ProjectResponse(
            project.Id,
            project.ProjectName,
            project.Technology,
            project.Status,
            project.CreatedAt,
            project.ClientId,
            project.Client!.FullName));
    }

    private static IQueryable<ProjectResponse> ProjectResponses(
        IQueryable<Project> projects) =>
        projects.Select(p => new ProjectResponse(
                p.Id,
                p.ProjectName,
                p.Technology,
                p.Status,
                p.CreatedAt,
                p.ClientId,
                p.Client!.FullName));
}
