using System.Net;
using System.Net.Http.Json;
using FreelanceDeployDemo.API.Contracts.Clients;
using FreelanceDeployDemo.API.Contracts.Projects;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FreelanceDeployDemo.IntegrationTests;

public sealed class ProjectFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ProjectFlowTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_list_read_and_update_project_preserves_client_relationship()
    {
        using var http = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        using var clientResult = await http.PostAsJsonAsync("/api/clients", new
        {
            fullName = "Integration Client",
            companyName = "Test Studio",
            email = $"{Guid.NewGuid():N}@example.test"
        });

        Assert.Equal(HttpStatusCode.Created, clientResult.StatusCode);
        Assert.NotNull(clientResult.Headers.Location);

        var client = await clientResult.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(client);

        using var projectResult = await http.PostAsJsonAsync("/api/projects", new
        {
            projectName = "Integration Project",
            technology = "ASP.NET Core",
            clientId = client.Id
        });

        Assert.Equal(HttpStatusCode.Created, projectResult.StatusCode);
        Assert.NotNull(projectResult.Headers.Location);

        var project = await projectResult.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);
        Assert.Equal("Pending", project.Status);

        var fetched = await http.GetFromJsonAsync<ProjectResponse>(
            projectResult.Headers.Location);

        Assert.NotNull(fetched);
        Assert.Equal(project.Id, fetched.Id);
        Assert.Equal(client.Id, fetched.ClientId);
        Assert.Equal(client.FullName, fetched.ClientName);

        var projects = await http.GetFromJsonAsync<List<ProjectResponse>>("/api/projects");
        Assert.NotNull(projects);
        Assert.Contains(projects, p => p.Id == project.Id && p.ClientId == client.Id);

        using var updateResult = await http.PutAsJsonAsync(
            $"/api/projects/{project.Id}/status",
            new { status = "InProgress" });

        Assert.Equal(HttpStatusCode.OK, updateResult.StatusCode);

        var updated = await http.GetFromJsonAsync<ProjectResponse>(
            $"/api/projects/{project.Id}");

        Assert.NotNull(updated);
        Assert.Equal("InProgress", updated.Status);

        var fetchedClient = await http.GetFromJsonAsync<ClientResponse>(
            clientResult.Headers.Location);

        Assert.NotNull(fetchedClient);
        Assert.Equal(client.Id, fetchedClient.Id);
        Assert.Equal(1, fetchedClient.ProjectCount);
    }
}
