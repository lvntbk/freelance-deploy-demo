using System.Net;
using System.Net.Http.Json;
using System.Text;
using FreelanceDeployDemo.API.Contracts.Clients;
using FreelanceDeployDemo.API.Contracts.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FreelanceDeployDemo.IntegrationTests;

public sealed class ApiErrorTests : IClassFixture<ApiFactory>, IDisposable
{
    private readonly HttpClient _http;

    public ApiErrorTests(ApiFactory factory)
    {
        _http = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData(
        "/api/clients",
        """{"fullName":"A","companyName":"","email":"invalid"}""",
        "FullName,CompanyName,Email")]
    [InlineData(
        "/api/projects",
        """{"projectName":"A","technology":"","clientId":0}""",
        "ProjectName,Technology,ClientId")]
    public async Task Invalid_create_request_returns_field_errors(
        string route, string json, string fields)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _http.PostAsync(route, content);

        await AssertValidationAsync(response, fields.Split(','));
    }

    [Theory]
    [InlineData("/api/clients/2147483647")]
    [InlineData("/api/projects/2147483647")]
    public async Task Get_missing_resource_returns_not_found(string route)
    {
        using var response = await _http.GetAsync(route);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_project_for_missing_client_returns_validation_error()
    {
        using var response = await _http.PostAsJsonAsync("/api/projects", new
        {
            projectName = "Orphan Project",
            technology = "ASP.NET Core",
            clientId = int.MaxValue
        });

        await AssertValidationAsync(response, "ClientId");
    }

    [Fact]
    public async Task Invalid_status_does_not_change_saved_project()
    {
        using var clientResult = await _http.PostAsJsonAsync("/api/clients", new
        {
            fullName = "Status Test Client",
            companyName = "Test Studio",
            email = $"{Guid.NewGuid():N}@example.test"
        });

        clientResult.EnsureSuccessStatusCode();
        var client = await clientResult.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(client);

        using var projectResult = await _http.PostAsJsonAsync("/api/projects", new
        {
            projectName = "Status Test Project",
            technology = "ASP.NET Core",
            clientId = client.Id
        });

        projectResult.EnsureSuccessStatusCode();
        var project = await projectResult.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);
        Assert.Equal("Pending", project.Status);

        using var response = await _http.PutAsJsonAsync(
            $"/api/projects/{project.Id}/status",
            new { status = "Unknown" });

        await AssertValidationAsync(response, "Status");

        var saved = await _http.GetFromJsonAsync<ProjectResponse>(
            $"/api/projects/{project.Id}");

        Assert.NotNull(saved);
        Assert.Equal("Pending", saved.Status);
    }

    [Fact]
    public async Task Update_missing_project_returns_not_found()
    {
        using var response = await _http.PutAsJsonAsync(
            "/api/projects/2147483647/status",
            new { status = "InProgress" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task AssertValidationAsync(
        HttpResponseMessage response, params string[] fields)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);

        foreach (var field in fields)
            Assert.Contains(field, problem.Errors.Keys);
    }

    public void Dispose() => _http.Dispose();
}
