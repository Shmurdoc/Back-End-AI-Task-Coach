# Architecture

AI Task Coach is a .NET 8 Clean Architecture solution for personal productivity: tasks, goals,
habits, energy/mood tracking, AI-assisted planning, gamification, analytics, and real-time
notifications over SignalR.

## Project layout

| Project | Depends on | Contains |
|---------|-----------|----------|
| `Domain` | — | Entities, enums, base types |
| `Application` | Domain | Service/repository interfaces, business services, DTOs, validators |
| `Infrastructure` | Application, Domain | EF Core `ApplicationDbContext`, repositories, migrations |
| `WebAPI` | Application, Infrastructure | Controllers, filters, middleware, hub, background services, health checks |
| `AITaskCoach.Tests` | All | Unit and integration tests (131 tests) |

The solution is roughly 133 C# files / ~10,500 lines, excluding generated code.

The original plan had a MediatR/CQRS request pipeline in mind. I never implemented it. Services
are wired through constructor-injected interfaces (`IAIService`, `IGamificationService`,
`IAnalyticsService`, and so on). I kept it simple instead of adding indirection. Nothing in the
codebase references MediatR.

## Request lifecycle

```
Client
  → ASP.NET Core pipeline (HTTPS redirect optional in dev, routing)
  → JWT bearer authentication
  → EnsureSelfUser filter (route userId / body UserId must equal caller)
  → Controller (thin: validate → call service → wrap result)
  → Application service (business rules, ownership checks, notification triggers)
  → Repository → EF Core → SQL Server
```

Cross-cutting pieces:

- `GlobalExceptionMiddleware` converts unhandled exceptions to
  `{ "success": false, "error": "Internal server error" }` with a 500.
- `InvalidModelStateResponseFactory` rewrites validation failures to
  `{ "success": false, "errors": [...] }`.
- Completion events (task/goal set to `Completed`, habit progress recorded) fire the
  `INotificationBroadcaster`, which sends SignalR events to the user's group.

## Data model

9 tables:

| Table | Purpose |
|-------|---------|
| `Users` | Account, hashed password, profile |
| `UserPreferences` | Per-user preferences (1:1 with user) |
| `Goals` | User goals with category/status |
| `Tasks` | Task items with status/priority/due dates |
| `Habits` | Habit definitions and cadence |
| `HabitEntries` | Individual habit check-ins |
| `HabitAnalytics` | Per-habit aggregate statistics |
| `EnergyMoodEntries` | Energy/mood check-ins with notes |
| `RefreshTokens` | SHA-256-hashed refresh tokens with rotation/revocation state |

Migrations (4 total, applied automatically on startup via `MigrateDatabaseAsync` except under
the non-relational test provider):

1. `20250905111333_InitialCreate`
2. `20250905124016_AddPhoneNumberColumn`
3. `20261016150622_AddEnergyMoodEntries`
4. `20261017211242_RefreshTokensAndRemoveDeadEntities`

## Security

- **JWT access tokens** (default 120 min) and **opaque refresh tokens** (default 7 days, SHA-256
  hashed at rest). Refresh rotates on every use. Reusing a rotated or revoked token revokes all
  of the user's active refresh tokens.
- **Passwords:** PBKDF2 / SHA-256, 100,000 iterations, 16-byte salt.
- **Ownership:** `EnsureSelfUserAttribute` returns 403 for cross-user `userId` references. Entity
  ownership is checked in services and surfaces as 404 (foreign resources look missing).
- **Startup guard:** the app refuses to start when `Jwt:Key` is missing, shorter than 32
  characters, or contains placeholder markers. `appsettings.Production.json` ships placeholders
  on purpose, so production must inject real secrets via environment variables or a secret
  manager before it will boot.

## Real-time notifications

`NotificationHub` at `/hubs/notifications` (`[Authorize]`). Users join a group derived from their
token (`user:{userId}`). `INotificationBroadcaster` (singleton) emits `TaskCompleted`,
`GoalCompleted`, and `HabitCompleted` with compact payloads, and records Prometheus counters and
histograms per event type.

## Background processing

One background service: `RefreshTokenCleanupService`.

- Starts 5 minutes after boot, then runs every 6 hours
- Deletes revoked/expired refresh tokens older than 24 hours, in batches of 500
- Emits `background_jobs_*` Prometheus metrics labelled with job name `refresh_token_cleanup`

## AI integration

`IAIService` talks to any OpenAI-compatible Chat Completions endpoint
(`OpenAI:Endpoint`, `OpenAI:Model`, `OpenAI:ApiKey`). Every AI endpoint has a **local fallback**.
Without an API key the service derives useful responses (insights, outlines, pattern analysis)
from the user's real data instead of failing.

## Observability

- Structured logging via `ILogger` throughout
- Prometheus metrics at `/metrics` (HTTP metrics + custom counters/histograms)
- Health checks at `/health/live` and `/health/ready` (readiness includes a database check)
- Grafana dashboards provisioned from `Doc/grafana/provisioning` (dev stack)

## Deployment

- **Dockerfile:** multi-stage build; runtime image runs as the non-root `app` user;
  `dotnet WebAPI.dll` on ports 8080/8081.
- **docker-compose:** `webapi` + SQL Server 2022 + Prometheus + Grafana, one bridge network,
  named volumes for data.
- **CI (GitHub Actions):** build + 131 tests with coverage (artifact + Codecov), Trivy filesystem
  scan uploaded to code scanning, and a GHCR image build/push gated on pushes to `main`/`master`.

## A few decisions explained

- **Derived gamification, no ledger table.** Points/levels/badges are computed on read from real
  activity. No sync jobs, no double counting. It costs recomputation per request, which is fine
  at this scale.
- **Rejecting placeholders at startup.** I'd rather the app fail fast than boot with a shipping
  placeholder signing key.
- **404 instead of 403 for foreign entities.** This prevents resource enumeration through
  ownership errors. The self-user filter still returns 403 for obvious cross-user URLs.
- **No Redis.** It was set up early but nothing used it, so I removed the registration, config,
  and compose service instead of leaving it around.
