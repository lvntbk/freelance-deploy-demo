using System.ComponentModel.DataAnnotations;

namespace FreelanceDeployDemo.API.Contracts.Clients;

public sealed class CreateClientRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string CompanyName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;
}

public sealed record ClientResponse(
    int Id,
    string FullName,
    string CompanyName,
    string Email,
    DateTime CreatedAt,
    int ProjectCount);
