# Developer Guide

How to run, test, and extend AI Task Coach.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB/Express is fine), or Docker for the full stack
- Optional: `dotnet-ef` tools (`dotnet tool install --global dotnet-ef`) for manual migrations

## Getting started

```powershell
# 1. Restore and build
dotnet restore
dotnet build

# 2. Configure the connection string
#    Edit WebAPI/appsettings.json → ConnectionStrings:DefaultConnection
#    (or use user secrets: dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..." --project WebAPI)

# 3. Create/update the database (4 migrations)
dotnet ef database update --project Infrastructure --startup-project WebAPI

# 4. Run
dotnet run --project WebAPI
```

The API listens on `http://localhost:5129`. In Development the Swagger UI is at the root URL
because `RoutePrefix` is empty. The JSON spec is at `/swagger/v1/swagger.json`. On startup the
app also applies pending migrations automatically (unless running under the non-relational test
provider), so step 3 is often unnecessary in local dev.

Create your first user with `POST /api/auth/register` from Swagger, then click **Authorize** and
paste the returned `token`.

## Configuration

| Key | Notes |
|-----|-------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Key` | Must be ≥ 32 chars and not a placeholder, or startup fails. Dev uses a fixed local key in `appsettings.Development.json` |
| `Jwt:Issuer` / `Jwt:Audience` | Token issuer/audience |
| `Jwt:ExpiresInMinutes` | Access token lifetime (default 120) |
| `Jwt:RefreshExpiresInDays` | Refresh token lifetime (default 7) |
| `OpenAI:ApiKey` | Optional. Empty means all AI endpoints use local fallbacks |
| `OpenAI:Endpoint` | Default `https://api.openai.com/v1/chat/completions` (any OpenAI-compatible endpoint works) |
| `OpenAI:Model` | Default `gpt-4o-mini` |

`appsettings.Production.json` intentionally ships `<set via environment variable or secret
manager>` placeholders. The startup guard rejects them, so production must supply real values
(environment variables, Key Vault, etc.).

## Testing

```powershell
dotnet test
```

- **131 tests**, all green, zero build warnings.
- Unit tests construct services/controllers directly with real in-memory repositories and mocked
  `IUserContext`.
- Integration tests use `WebApplicationFactory` (`AITaskCoach.Tests/Integration/`) and cover
  auth enforcement (anonymous → 401), health endpoints, `/metrics`, and endpoint behavior.

CI runs the same suite on every push/PR to `main`, `master`, or `develop`, with coverage uploaded
as an artifact and to Codecov.

## Conventions

**Controllers**

- Attributes: `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`,
  `[EnsureSelfUser]` (unless the controller has no user-scoped data), `[Produces("application/json")]`
- Thin actions: validate/bind → call one service method → wrap the result
- Success envelope: `Ok(new { success = true, data = ... })`; errors:
  `{ success = false, error = "..." }` with the appropriate status code
- Deletes return `{ success = true }` with no body data
- `Update` actions translate `InvalidOperationException` from services into 404

**Domain rules**

- All ids are GUIDs; all timestamps UTC
- Ownership checks live in application services (return `null`/throw → controller maps to 404)
- `EnsureSelfUserAttribute` only covers the `userId` route value and `UserId` properties on body
  arguments. Entity id routes are the service's responsibility.
- Async all the way, `CancellationToken` threaded through

**Validation**

- DataAnnotations on request records (`[Required]`, `[Range]`, …)
- FluentValidation validators for the richer DTOs (create/update task, create goal, login),
  invoked explicitly with `ValidateAndThrowAsync`

**Notifications**

- Completion flows call `INotificationBroadcaster` (`TaskCompletedAsync`, `GoalCompletedAsync`,
  `HabitCompletedAsync`). Controllers never use `IHubContext` directly.

## Adding a feature (typical path)

1. **Domain**: add/adjust the entity in `Domain/Entities` (GUID `Id`, UTC timestamps).
2. **Infrastructure**: add the `DbSet`, configure the model, create a migration:
   `dotnet ef migrations add <Name> --project Infrastructure --startup-project WebAPI`.
3. **Application**: repository interface + implementation, service interface + implementation,
   DTOs, optional validator.
4. **WebAPI**: register services in `Program.cs` if new, add the controller following the
   conventions above.
5. **Tests**: add unit tests next to the existing patterns; integration behavior (auth, status
   codes) in `AITaskCoach.Tests/Integration`.
6. Run `dotnet build` (must stay warning-free) and `dotnet test` (must stay green).

## Docker

```powershell
$env:SA_PASSWORD = "<strong-password>"
$env:OPENAI_API_KEY = "<optional>"
docker compose up --build
```

This starts the API (8080), SQL Server 2022 (1433), Prometheus (9090), and Grafana (3000) with
dashboards provisioned from `Doc/grafana/provisioning`.

## Branches and PRs

- Default branch: **`master`** (CI also accepts `main`/`develop` pushes)
- PRs target `master`; CI must pass (build, 131 tests, Trivy scan)
- Keep the solution building with zero warnings
