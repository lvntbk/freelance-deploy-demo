using System.ComponentModel.DataAnnotations;

namespace FreelanceDeployDemo.API.Contracts.Projects;

public sealed class CreateProjectRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string ProjectName { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Technology { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ClientId { get; init; }
}

public sealed class UpdateProjectStatusRequest
{
    [Required]
    [RegularExpression(
        "^(Pending|InProgress|Completed|Cancelled)$",
        ErrorMessage = "Status must be Pending, InProgress, Completed or Cancelled.")]
    public string Status { get; init; } = string.Empty;
}

public sealed record ProjectResponse(
    int Id,
    string ProjectName,
    string Technology,
    string Status,
    DateTime CreatedAt,
    int ClientId,
    string ClientName);
