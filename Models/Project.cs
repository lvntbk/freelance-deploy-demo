namespace FreelanceDeployDemo.API.Models;

public class Project
{
    public int Id { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Technology { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ClientId { get; set; }

    public Client? Client { get; set; }
}
