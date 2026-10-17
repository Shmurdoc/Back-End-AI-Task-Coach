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

// These tests cover the AI endpoints. With no OpenAI key configured, they
// check that responses are built from the caller's real data, that the
// duplicate suggestions route is gone, and that goal decomposition loads
// the goal from the database and enforces ownership. Each AI assertion
// checks the local fallback that keeps the API useful offline.
public class AiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private record Envelope<T>(bool Success, T Data);

    public AiEndpointTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("AiEndpointDb");
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
            email = $"ai-{Guid.NewGuid():N}@test.local",
            password = "Passw0rd!",
            name = "AI Tester"
        });

        response.IsSuccessStatusCode.Should().BeTrue();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        return auth!;
    }

    private async Task<Guid> CreateGoalAsync(string? token, Guid userId, string title, GoalCategory category, DateTime? targetDate = null)
    {
        UseToken(token);
        var response = await _client.PostAsJsonAsync("/api/goals", new CreateGoalDto(
            userId, title, null, category,
            1, targetDate ?? DateTime.UtcNow.AddDays(21)));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<GoalDto>>();
        var goalId = envelope!.Data.Id;

        // New goals start NotStarted; AI context only surfaces InProgress goals.
        var update = await _client.PutAsJsonAsync($"/api/goals/{goalId}", new { status = GoalStatus.InProgress });
        update.IsSuccessStatusCode.Should().BeTrue();
        return goalId;
    }

    // ---------- AI coaches on real data, offline ----------

    [Fact]
    public async Task Analyze_WithoutApiKey_ReturnsLocalInsightsDerivedFromRealData()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;
        await CreateGoalAsync(auth.Token, userId, "Ship the beta", GoalCategory.Professional);

        UseToken(auth.Token);
        var response = await _client.GetAsync($"/api/ai/analyze/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<string>>();
        envelope!.Data.Should().StartWith("Local insights (offline mode");
        envelope.Data.Should().Contain("Open tasks:");
        envelope.Data.Should().Contain("Ship the beta",
            "the offline insight must be built from the user's actual goals, not a canned string");
    }

    [Fact]
    public async Task WeeklyPlan_WithoutApiKey_ReturnsLocalOutlineForRequestedWeek()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;
        var weekStart = new DateTime(2026, 10, 19); // a Monday
        // Target date inside the requested week so the outline has real content.
        await CreateGoalAsync(auth.Token, userId, "Prepare the weekly review", GoalCategory.Health,
            targetDate: weekStart.AddDays(2));

        UseToken(auth.Token);
        var response = await _client.PostAsJsonAsync($"/api/ai/weekly-plan/{userId}", new
        {
            context = weekStart.ToString("yyyy-MM-dd")
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<string>>();
        envelope!.Data.Should().StartWith("Local weekly outline");
        envelope.Data.Should().Contain("2026-10-19");
        envelope.Data.Should().Contain("Prepare the weekly review",
            "the weekly outline must be grounded in goals whose target dates fall in the requested week");
    }

    [Fact]
    public async Task WeeklyPlan_ReturnsBadRequest_ForUnparsableContext()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var response = await _client.PostAsJsonAsync($"/api/ai/weekly-plan/{auth.User!.Id}", new
        {
            context = "not-a-date"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeletedSuggestionsEndpoint_Returns404()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        // Removed as an exact duplicate of /api/ai/analyze/{userId}.
        var response = await _client.GetAsync($"/api/ai/suggestions/{auth.User!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------- goal decomposition is real and ownership-checked ----------

    [Fact]
    public async Task Decompose_ReturnsCategoryAwareStepsForOwnedGoal()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;
        var goalId = await CreateGoalAsync(auth.Token, userId, "Run a half marathon", GoalCategory.Health);

        UseToken(auth.Token);
        var response = await _client.PostAsJsonAsync("/api/goaldecomposition/decompose", new { goalId });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var result = doc.RootElement.GetProperty("data");
        result.GetProperty("goalTitle").GetString().Should().Be("Run a half marathon");

        var subtasks = result.GetProperty("subtasks");
        subtasks.GetArrayLength().Should().Be(4);
        var steps = subtasks.EnumerateArray().Select(s => s.GetString()!).ToList();
        steps.Should().Contain(s => s.Contains("measurable weekly target"),
            "Health goals get a health-specific milestone, not a generic template");
        steps.Should().Contain(s => s.Contains("mid-point review"),
            "a goal with a target date gets a dated mid-point review step");

        result.GetProperty("aiExplanation").GetString()
            .Should().Contain("Heuristic decomposition", "the response must say the steps came from a heuristic");
    }

    [Fact]
    public async Task Decompose_DifferentCategories_ProduceDifferentSteps()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;
        var healthGoal = await CreateGoalAsync(auth.Token, userId, "Improve cardio", GoalCategory.Health);
        var professionalGoal = await CreateGoalAsync(auth.Token, userId, "Launch the feature", GoalCategory.Professional);

        UseToken(auth.Token);
        var health = await _client.PostAsJsonAsync("/api/goaldecomposition/decompose", new { goalId = healthGoal });
        var professional = await _client.PostAsJsonAsync("/api/goaldecomposition/decompose", new { goalId = professionalGoal });

        using var healthDoc = JsonDocument.Parse(await health.Content.ReadAsStringAsync());
        using var profDoc = JsonDocument.Parse(await professional.Content.ReadAsStringAsync());

        var healthSteps = healthDoc.RootElement.GetProperty("data").GetProperty("subtasks")
            .EnumerateArray().Select(s => s.GetString()!).ToList();
        var profSteps = profDoc.RootElement.GetProperty("data").GetProperty("subtasks")
            .EnumerateArray().Select(s => s.GetString()!).ToList();

        profSteps.Should().Contain(s => s.Contains("deliverables and stakeholders"));
        healthSteps.Should().NotEqual(profSteps, "decomposition must depend on the goal's category");
    }

    [Fact]
    public async Task Decompose_MissingGoal_Returns404()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var response = await _client.PostAsJsonAsync("/api/goaldecomposition/decompose",
            new { goalId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Decompose_AnotherUsersGoal_Returns404()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync();
        var bobsGoal = await CreateGoalAsync(bob.Token, bob.User!.Id, "Bob's private goal", GoalCategory.Personal);

        UseToken(alice.Token);
        var response = await _client.PostAsJsonAsync("/api/goaldecomposition/decompose", new { goalId = bobsGoal });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "foreign goals must be hidden, not decomposed for the caller");
    }
}
