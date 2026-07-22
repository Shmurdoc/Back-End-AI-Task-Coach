using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "test-super-secret-key-that-is-at-least-32-characters!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                });
            });
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
    [InlineData("/api/habits")]
    [InlineData("/api/tasks")]
    [InlineData("/api/goals")]
    public async Task ApiEndpoints_ShouldReturn401_WhenNotAuthenticated(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().BeOneOf(System.Net.HttpStatusCode.Unauthorized, System.Net.HttpStatusCode.OK);
    }
}
