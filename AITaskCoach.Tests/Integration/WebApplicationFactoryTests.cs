using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Infrastructure.Persistence.Data;

namespace AITaskCoach.Tests.Integration;

public class WebApplicationFactoryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebApplicationFactoryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            // Jwt config intentionally comes from appsettings only: Program.cs captures
            // Jwt:Key eagerly at startup, so in-memory test overrides would reach
            // TokenService (per-request DI) but not the JWT validator.
            builder.ConfigureServices(services =>
            {
                // Remove real database
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetMetrics_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("# HELP");
    }

    [Fact]
    public async Task GetSwagger_ShouldReturnOk_InDevelopment()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/habits/user/11111111-1111-1111-1111-111111111111")]
    [InlineData("/api/tasks/user/11111111-1111-1111-1111-111111111111")]
    [InlineData("/api/goals/user/11111111-1111-1111-1111-111111111111")]
    public async Task ApiEndpoints_ShouldReturn401_WhenNotAuthenticated(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
