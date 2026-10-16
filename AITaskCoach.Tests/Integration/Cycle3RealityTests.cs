using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs;
using Application.DTOs.AuthDtos;
using Application.DTOs.GoalDtos;
using Application.DTOs.HabitDtos;
using Application.DTOs.TaskDtos;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AITaskCoach.Tests.Integration;

// Cycle 3 proofs: the features that previously returned hardcoded zeros or
// process-local dictionaries now serve real database-backed data, removed
// endpoints are truly gone, and AI degrades gracefully with no API key.
// Own in-memory database so it never shares state with the other
// integration suites ("TestDb", "IdorTestDb").
public class Cycle3RealityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private record Envelope<T>(bool Success, T Data);

    public Cycle3RealityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            // Same JWT constraint as AuthAndIdorTests: Program.cs captures
            // Jwt:Key eagerly at startup, so no in-memory Jwt overrides here —
            // the Development appsettings key serves signing and validation.
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("Cycle3RealityDb");
                });
            });
        });
        _client = _factory.CreateClient();
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
            email = $"cycle3-{Guid.NewGuid():N}@test.local",
            password = "Passw0rd!",
            name = "Cycle 3 Tester"
        });

        response.IsSuccessStatusCode.Should().BeTrue();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        return auth;
    }

    private async Task<Guid> CreateGoalAsync(string? token, Guid userId)
    {
        UseToken(token);
        var response = await _client.PostAsJsonAsync("/api/goals", new CreateGoalDto(
            userId, "Ship the report feature", null, GoalCategory.Professional,
            1, DateTime.UtcNow.AddDays(14)));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<Application.DTOs.GoalDtos.GoalDto>>();
        envelope.Should().NotBeNull();
        return envelope!.Data.Id;
    }

    private async Task<Guid> CreateTaskAsync(string? token, Guid userId, Guid? goalId = null)
    {
        UseToken(token);
        var response = await _client.PostAsJsonAsync("/api/tasks", new CreateTaskDto(
            userId, "Write integration tests", null, TaskItemStatus.Todo, TaskPriority.High,
            2.0, null, null, null, 3, 30, goalId));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<TaskDto>>();
        envelope.Should().NotBeNull();
        return envelope!.Data.Id;
    }

    private async Task<Guid> CreateHabitAsync(string? token, Guid userId)
    {
        UseToken(token);
        var response = await _client.PostAsJsonAsync("/api/habits", new
        {
            id = Guid.NewGuid(),
            name = "Morning stretch",
            description = "Ten minutes after waking up",
            frequency = HabitFrequency.Daily,
            preferredTime = (TimeSpan?)null,
            targetCount = 1,
            unit = "time",
            color = "#3498db",
            icon = "fitness",
            motivation = "Feel awake",
            currentStreak = 0,
            bestStreak = 0,
            completionRate = 0.0,
            isActive = true,
            category = HabitCategory.Health,
            triggers = Array.Empty<string>(),
            rewards = Array.Empty<string>(),
            aiInsights = (string?)null,
            difficultyLevel = 1,
            environmentFactors = (string?)null,
            lastCompletedAt = (DateTime?)null,
            userId,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<HabitDto>>();
        envelope.Should().NotBeNull();
        return envelope!.Data.Id;
    }

    // ---------- reports and analytics serve real data ----------

    [Fact]
    public async Task UserReport_ReflectsSeededGoalsTasksAndHabits()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;

        await CreateGoalAsync(auth.Token, userId);
        await CreateTaskAsync(auth.Token, userId);
        await CreateHabitAsync(auth.Token, userId);

        UseToken(auth.Token);
        var response = await _client.GetAsync($"/api/report/user-report/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<UserReportDto>>();
        envelope.Should().NotBeNull();
        envelope!.Data.UserId.Should().Be(userId);
        envelope.Data.TotalGoals.Should().Be(1);
        envelope.Data.TotalTasks.Should().Be(1);
        envelope.Data.TotalHabits.Should().Be(1);
        envelope.Data.ActiveHabits.Should().Be(1);
        envelope.Data.TasksByPriority.Should().ContainKey(nameof(TaskPriority.High));
    }

    [Fact]
    public async Task HabitSummary_ReflectsSeededHabits()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;
        await CreateHabitAsync(auth.Token, userId);

        UseToken(auth.Token);
        var response = await _client.GetAsync($"/api/habitanalytics/habit-summary/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<HabitSummaryDto>>();
        envelope.Should().NotBeNull();
        envelope!.Data.UserId.Should().Be(userId);
        envelope.Data.TotalHabits.Should().Be(1);
        envelope.Data.ActiveHabits.Should().Be(1);
        envelope.Data.Habits.Should().ContainSingle(h => h.Name == "Morning stretch" && h.IsActive);
    }

    [Fact]
    public async Task Leaderboard_IncludesRegisteredUserWithRank()
    {
        var auth = await RegisterAsync();

        UseToken(auth.Token);
        var response = await _client.GetAsync("/api/gamification/leaderboard");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("data").TryGetProperty("leaderboard", out var leaderboard).Should().BeTrue();
        leaderboard.ValueKind.Should().Be(JsonValueKind.Array);

        var entry = leaderboard.EnumerateArray().FirstOrDefault(e =>
            e.EnumerateObject().Any(p =>
                string.Equals(p.Name, "userId", StringComparison.OrdinalIgnoreCase) &&
                p.Value.GetGuid() == auth.User!.Id));
        entry.ValueKind.Should().NotBe(JsonValueKind.Undefined, "the registered user must appear in the real leaderboard");
        entry.EnumerateObject().Should().Contain(p =>
            string.Equals(p.Name, "rank", StringComparison.OrdinalIgnoreCase) && p.Value.GetInt32() >= 1);
    }

    // ---------- users: real GET/PUT roundtrip ----------

    [Fact]
    public async Task Users_Get_ReturnsRegisteredUser()
    {
        var auth = await RegisterAsync();

        UseToken(auth.Token);
        var response = await _client.GetAsync($"/api/users/{auth.User!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<UserDto>>();
        envelope.Should().NotBeNull();
        envelope!.Data.Id.Should().Be(auth.User.Id);
        envelope.Data.Email.Should().Be(auth.User.Email);
    }

    [Fact]
    public async Task Users_Update_PersistsNameAndPhoneNumber()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;

        UseToken(auth.Token);
        var get = await _client.GetFromJsonAsync<Envelope<UserDto>>($"/api/users/{userId}");
        var dto = get!.Data;

        var update = await _client.PutAsJsonAsync($"/api/users/{userId}", new UserDto
        {
            Id = userId,
            Email = dto.Email,
            Name = "Renamed Tester",
            PhoneNumber = "+15551234567",
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await _client.GetFromJsonAsync<Envelope<UserDto>>($"/api/users/{userId}");
        after!.Data.Name.Should().Be("Renamed Tester");
        after.Data.PhoneNumber.Should().Be("+15551234567");
        after.Data.Email.Should().Be(dto.Email, "email is unchanged by this update");
    }

    [Fact]
    public async Task Users_Update_WhenEmailTakenByAnotherUser_Returns409()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync();

        UseToken(alice.Token);
        var current = await _client.GetFromJsonAsync<Envelope<UserDto>>($"/api/users/{alice.User!.Id}");

        var response = await _client.PutAsJsonAsync($"/api/users/{alice.User.Id}", new UserDto
        {
            Id = alice.User.Id,
            Email = bob.User!.Email,
            Name = current!.Data.Name,
            PhoneNumber = current.Data.PhoneNumber,
            IsActive = current.Data.IsActive,
            CreatedAt = current.Data.CreatedAt
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // EnsureSelfUserAttribute only fires for a route param named "userId" or a body
    // property named "UserId". UsersController uses "{id}" + UserDto.Id, so the filter
    // correctly skips it — the action's own guard (UsersController.cs:40) is the real
    // protection and it deliberately conceals other users as NotFound instead of leaking
    // their existence via a 403. Assert that posture directly.
    [Fact]
    public async Task Users_Update_WhenTargetingAnotherUser_Returns404()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.PutAsJsonAsync($"/api/users/{bob.User!.Id}", new UserDto
        {
            Id = bob.User.Id,
            Email = bob.User.Email,
            Name = "Hijacked",
            PhoneNumber = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------- removed endpoints are really gone ----------

    [Fact]
    public async Task RemovedGamificationWriteEndpoints_Return404()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var completeObjective = await _client.PostAsJsonAsync("/api/gamification/complete-objective", new { });
        completeObjective.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var awardPoints = await _client.PostAsJsonAsync("/api/gamification/award-points", new { });
        awardPoints.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovedControllers_RoutesReturn404()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        // These controllers were deleted in the Cycle 3 dead-code pass.
        (await _client.GetAsync("/api/focus")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _client.GetAsync("/api/location")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _client.GetAsync("/api/ocr")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------- energy/mood persists in the database ----------

    [Fact]
    public async Task EnergyMood_RecordThenRead_PersistsInDatabase()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;

        UseToken(auth.Token);
        var record = await _client.PostAsJsonAsync("/api/energymood/record", new
        {
            userId,
            energyLevel = 7,
            moodLevel = 8,
            note = "post-workout focus"
        });
        record.StatusCode.Should().Be(HttpStatusCode.OK);

        var entriesResponse = await _client.GetAsync($"/api/energymood/entries/{userId}");
        entriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var doc = JsonDocument.Parse(await entriesResponse.Content.ReadAsStringAsync()))
        {
            var entries = doc.RootElement.GetProperty("data");
            entries.GetArrayLength().Should().Be(1);
            entries[0].GetProperty("energyLevel").GetInt32().Should().Be(7);
            entries[0].GetProperty("moodLevel").GetInt32().Should().Be(8);
            entries[0].GetProperty("note").GetString().Should().Be("post-workout focus");
        }

        var averagesResponse = await _client.GetAsync($"/api/energymood/averages/{userId}");
        using (var doc = JsonDocument.Parse(await averagesResponse.Content.ReadAsStringAsync()))
        {
            var averages = doc.RootElement.GetProperty("data");
            averages.GetProperty("avgEnergy").GetDouble().Should().Be(7);
            averages.GetProperty("avgMood").GetDouble().Should().Be(8);
        }

        // The real proof of persistence: the row exists in the EF store, not
        // only in a process-local dictionary the service once kept.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.EnergyMoodEntries.Count(e => e.UserId == userId).Should().Be(1);
    }

    // ---------- AI degrades gracefully without an API key ----------

    [Fact]
    public async Task AiSuggestTask_WithNoApiKey_ReturnsGracefulSuccessNot500()
    {
        var auth = await RegisterAsync();
        UseToken(auth.Token);

        var response = await _client.PostAsJsonAsync("/api/ai/suggest-task", new
        {
            taskDescription = "Plan a weekly meal prep routine"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "AIService catches the failed OpenAI call and returns a fallback instead of throwing");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    // ---------- calendar export contains real tasks ----------

    [Fact]
    public async Task CalendarDaily_ExportContainsStartedTask()
    {
        var auth = await RegisterAsync();
        var userId = auth.User!.Id;
        var taskId = await CreateTaskAsync(auth.Token, userId);

        UseToken(auth.Token);
        var startedAt = DateTime.UtcNow;
        var update = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", new
        {
            startedAt
        });
        update.IsSuccessStatusCode.Should().BeTrue();

        var ics = await _client.GetAsync($"/api/calendar/daily/{userId}/{startedAt:yyyy-MM-dd}");
        ics.StatusCode.Should().Be(HttpStatusCode.OK);
        ics.Content.Headers.ContentType!.MediaType.Should().Be("text/calendar");

        var content = await ics.Content.ReadAsStringAsync();
        content.Should().Contain("BEGIN:VCALENDAR");
        content.Should().Contain("BEGIN:VEVENT");
        content.Should().Contain("Write integration tests");
    }
}
