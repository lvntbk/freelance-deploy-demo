namespace FreelanceDeployDemo.API.Models;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Project> Projects { get; set; } = new();
}
