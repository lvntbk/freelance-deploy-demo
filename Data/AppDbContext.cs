using FreelanceDeployDemo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelanceDeployDemo.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
}
