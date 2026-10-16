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

// Cycle 4 proofs: the AI layer coaches on the caller's real data (not a bare
// user id), serves deterministic data-derived insights offline, the duplicate
// suggestions endpoint is gone, and goal decomposition loads the goal from the
// database with ownership enforcement and category-aware steps.
// No OpenAI key is configured in tests, so every AI assertion pins the
// local-fallback behavior that must exist for the API to stay useful offline.
public class Cycle4AiRealityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private record Envelope<T>(bool Success, T Data);

    public Cycle4AiRealityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            // Same JWT constraint as AuthAndIdorTests: Program.cs captures
            // Jwt:Key eagerly at startup, so no in-memory Jwt overrides here.
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("Cycle4AiDb");
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
            email = $"cycle4-{Guid.NewGuid():N}@test.local",
            password = "Passw0rd!",
            name = "Cycle 4 Tester"
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
        await CreateGoalAsync(auth.Token, userId, "Ship the Cycle 4 work", GoalCategory.Professional);

        UseToken(auth.Token);
        var response = await _client.GetAsync($"/api/ai/analyze/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<string>>();
        envelope!.Data.Should().StartWith("Local insights (offline mode");
        envelope.Data.Should().Contain("Open tasks:");
        envelope.Data.Should().Contain("Ship the Cycle 4 work",
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
    public async Task Analyze_ReturnsBadRequest_ForUnparsableWeekContext_OnWeeklyPlan()
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
            .Should().Contain("Heuristic decomposition", "the response must be honest about how it was produced");
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
            "foreign goals must be concealed, not decomposed for the caller");
    }
}
