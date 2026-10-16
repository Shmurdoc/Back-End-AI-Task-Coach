using System.Net;
using System.Net.Http.Json;
using Application.DTOs.AuthDtos;
using Application.DTOs.TaskDtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence.Data;
using Domain.Enums;

namespace AITaskCoach.Tests.Integration;

// Cycle 2 proofs: cross-user requests are rejected at the boundary (403/404,
// never 200/500) and refresh tokens rotate through the new endpoint.
// Uses its own in-memory database name so it never shares state with
// WebApplicationFactoryTests ("TestDb").
public class AuthAndIdorTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private record Envelope<T>(bool Success, T Data);

    public AuthAndIdorTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            // No Jwt:* in-memory overrides here: Program.cs reads Jwt:Key eagerly at
            // startup (before test config callbacks apply), while TokenService reads DI
            // config per request - an override would split signer and validator onto
            // different keys. The Development appsettings key serves both sides.
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IdorTestDb");
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

    private async Task<AuthResponse> RegisterAsync(string? email = null)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = email ?? $"idor-{Guid.NewGuid():N}@test.local",
            password = "Passw0rd!",
            name = "Idor Tester"
        });

        response.IsSuccessStatusCode.Should().BeTrue();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        return auth;
    }

    private static CreateTaskDto NewTaskDto(Guid userId, string title = "Owned task")
        => new(userId, title, null, TaskItemStatus.Todo, TaskPriority.Medium, 1.0,
            null, null, null, 3, 25, null);

    private async Task<Guid> CreateTaskAsync(string? token, Guid ownerId)
    {
        UseToken(token);
        var response = await _client.PostAsJsonAsync("/api/tasks", NewTaskDto(ownerId));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<TaskDto>>();
        envelope.Should().NotBeNull();
        return envelope!.Data.Id;
    }

    // ---------- registration issues a refresh token ----------

    [Fact]
    public async Task Register_ReturnsAccessTokenAndRefreshToken()
    {
        var auth = await RegisterAsync();

        auth.Token.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBe(auth.Token);
    }

    // ---------- route-level userId guard: 403, not 404/200 ----------

    [Fact]
    public async Task GetTasksForUser_WhenRouteUserIdIsForeign_Returns403()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.GetAsync($"/api/tasks/user/{bob.User!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTasksForUser_WhenRouteUserIdIsSelf_Returns200()
    {
        var alice = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.GetAsync($"/api/tasks/user/{alice.User!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ---------- body-level UserId guard: 403 on spoofed owner ----------

    [Fact]
    public async Task CreateTask_WhenBodyClaimsForeignUserId_Returns403()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.PostAsJsonAsync("/api/tasks", NewTaskDto(bob.User!.Id, "Spoofed"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTask_WhenBodyClaimsOwnUserId_Returns201_AndStoresCaller()
    {
        var alice = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.PostAsJsonAsync("/api/tasks", NewTaskDto(alice.User!.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<TaskDto>>();
        envelope!.Data.UserId.Should().Be(alice.User.Id);
    }

    // ---------- entity-id access: 404 for foreign rows, no side effects ----------

    [Fact]
    public async Task TaskOwnedByOtherUser_Returns404_OnGetUpdateDelete_AndSurvives()
    {
        var alice = await RegisterAsync();
        var taskId = await CreateTaskAsync(alice.Token!, alice.User!.Id);

        var bob = await RegisterAsync();
        UseToken(bob.Token);

        var get = await _client.GetAsync($"/api/tasks/{taskId}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var put = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", new UpdateTaskDto(Title: "Renamed by attacker"));
        put.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var delete = await _client.DeleteAsync($"/api/tasks/{taskId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Alice still sees the untouched row after all three attacks.
        UseToken(alice.Token);
        var ownerGet = await _client.GetAsync($"/api/tasks/{taskId}");
        ownerGet.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ownerGet.Content.ReadFromJsonAsync<Envelope<TaskDto>>();
        envelope!.Data.Title.Should().Be("Owned task");
    }

    // ---------- refresh flow ----------

    [Fact]
    public async Task Login_ThenRefresh_WithValidRefreshToken_ReturnsRotatedPair()
    {
        var email = $"idor-{Guid.NewGuid():N}@test.local";
        var alice = await RegisterAsync(email);

        UseToken(null);
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Passw0rd!"
        });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginAuth = await login.Content.ReadFromJsonAsync<AuthResponse>();
        loginAuth!.Token.Should().NotBeNullOrWhiteSpace();
        loginAuth.RefreshToken.Should().NotBeNullOrWhiteSpace();

        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = loginAuth.RefreshToken
        });

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await refresh.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Success.Should().BeTrue();
        auth.Token.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBe(loginAuth.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithAccessToken_Returns401()
    {
        var alice = await RegisterAsync();

        UseToken(null);
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = alice.Token
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithGarbageToken_Returns401()
    {
        UseToken(null);
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = "definitely-not-a-jwt"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---------- RequireCoach removed: own-user analytics is reachable ----------

    [Fact]
    public async Task ProductivitySummary_WithOwnUserId_Returns200()
    {
        var alice = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.GetAsync($"/api/analytics/productivity/{alice.User!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProductivitySummary_WithForeignUserId_Returns403()
    {
        var alice = await RegisterAsync();
        var bob = await RegisterAsync();

        UseToken(alice.Token);
        var response = await _client.GetAsync($"/api/analytics/productivity/{bob.User!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
