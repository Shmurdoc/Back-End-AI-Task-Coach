using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.AuthDtos;
using Application.DTOs.GoalDtos;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AITaskCoach.Tests.Integration;

// These tests check that every endpoint uses the same response shape,
// { success, data }. Some endpoints used to return their own root keys
// such as result, summary, leaderboard, entries or avgEnergy.
public class ResponseEnvelopeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ResponseEnvelopeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            // Same JWT setup as AuthAndIdorTests: Program.cs reads Jwt:Key
            // at startup before test config is applied.
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ResponseEnvelopeDb");
                });
            });
        }).CreateClient();
    }

    private void UseToken(string? token)
    {
        _client.DefaultRequestHeaders.Authorization = token is null
            ? null
            : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<AuthResponse> RegisterAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"envelope-{Guid.NewGuid():N}@test.local",
            password = "Passw0rd!",
            name = "Envelope Tester"
        });

        response.IsSuccessStatusCode.Should().BeTrue();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        return auth!;
    }

    private static void AssertHouseEnvelope(JsonElement root)
    {
        root.ValueKind.Should().Be(JsonValueKind.Object);
        root.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(
            new[] { "success", "data" },
            "every controller returns { success, data }, not per-controller root keys");
        root.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    // ---------- GET endpoints that once used custom root keys ----------

    [Theory]
    [InlineData("/api/gamification/summary/{0}")]
    [InlineData("/api/gamification/points/{0}")]
    [InlineData("/api/gamification/badges/{0}")]
    [InlineData("/api/gamification/achievements/{0}")]
    [InlineData("/api/gamification/challenges/{0}")]
    [InlineData("/api/gamification/motivation/{0}")]
    [InlineData("/api/gamification/leaderboard")]
    [InlineData("/api/analytics/productivity/{0}")]
    [InlineData("/api/habitanalytics/habit-summary/{0}")]
    [InlineData("/api/report/user-report/{0}")]
    [InlineData("/api/energymood/entries/{0}")]
    [InlineData("/api/energymood/averages/{0}")]
    [InlineData("/api/ai/analyze/{0}")]
    public async Task GetEndpoints_ReturnHouseEnvelope_WithNoStrayRootKeys(string pathTemplate)
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var path = pathTemplate.Contains("{0}")
            ? string.Format(pathTemplate, auth.User!.Id)
            : pathTemplate;

        var response = await _client.GetAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"GET {path} should succeed for a registered user");

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertHouseEnvelope(doc.RootElement);
    }

    // ---------- POST endpoints ----------

    [Fact]
    public async Task EnergyMoodRecord_ReturnsHouseEnvelope_WithCreatedEntry()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var response = await _client.PostAsJsonAsync("/api/energymood/record", new
        {
            userId = auth.User!.Id,
            energyLevel = 6,
            moodLevel = 7,
            note = "morning check-in"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertHouseEnvelope(doc.RootElement);
        doc.RootElement.GetProperty("data").GetProperty("energyLevel").GetInt32().Should().Be(6,
            "the record endpoint must return the created entry, not a bare success flag");
    }

    [Fact]
    public async Task AiSuggestTask_ReturnsHouseEnvelope()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var response = await _client.PostAsJsonAsync("/api/ai/suggest-task", new
        {
            taskDescription = "Plan a day of documentation work"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertHouseEnvelope(doc.RootElement);
    }

    [Fact]
    public async Task GoalDecompose_ReturnsHouseEnvelope()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var create = await _client.PostAsJsonAsync("/api/goals", new CreateGoalDto(
            auth.User!.Id, "Plan the release", null,
            GoalCategory.Professional, 1, DateTime.UtcNow.AddDays(30)));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var goalEnvelope = await create.Content.ReadFromJsonAsync<Envelope<GoalDto>>();
        var goalId = goalEnvelope!.Data.Id;

        var response = await _client.PostAsJsonAsync("/api/goaldecomposition/decompose", new { goalId });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        AssertHouseEnvelope(doc.RootElement);
        doc.RootElement.GetProperty("data").GetProperty("subtasks").GetArrayLength().Should().BeGreaterThan(0);
    }

    private record Envelope<T>(bool Success, T Data);
}
