using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Application.DTOs.AuthDtos;
using Application.DTOs.TaskDtos;
using Infrastructure.Persistence.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace AITaskCoach.Tests.Integration;

public class SmokeEndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SmokeEndToEndTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace DB with in-memory
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("IntegrationSmokeDb"));
            });
        });
    }

    [Fact]
    public async Task Register_Login_CreateTask_EndToEnd()
    {
        var client = _factory.CreateClient();

    var register = new RegisterRequest { Email = $"smoke+{Guid.NewGuid()}@example.com", Password = "Sm0keTest!", Name = "Smoke" };
        var regResp = await client.PostAsJsonAsync("/api/auth/register", register);
        regResp.IsSuccessStatusCode.Should().BeTrue();
        var regBody = await regResp.Content.ReadFromJsonAsync<AuthResponse>();
        regBody.Should().NotBeNull();
        regBody!.Success.Should().BeTrue();

        var login = new LoginRequest { Email = register.Email, Password = register.Password };
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", login);
        loginResp.IsSuccessStatusCode.Should().BeTrue();
        var loginBody = await loginResp.Content.ReadFromJsonAsync<AuthResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.Success.Should().BeTrue();

    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Token);

    // loginBody.User.Id is already a Guid (UserDto.Id). Use it directly.
    var userId = loginBody.User!.Id;
    var create = new CreateTaskDto(userId, "E2E Task", "Created by test", Domain.Enums.TaskItemStatus.Todo, Domain.Enums.TaskPriority.Medium, 1.0, null, null, null, 3, 30, null);
    var createResp = await client.PostAsJsonAsync("/api/tasks", create);
    // Expect 201 Created
    createResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    var content = await createResp.Content.ReadAsStringAsync();
    content.Should().Contain("\"id\"");
    }
}
