# AI Task Coach

A backend API for tracking tasks, goals, habits, and energy. It includes an AI coach layer that reads your recent activity before it responds.

**.NET 8 · Clean Architecture · EF Core + SQL Server · JWT auth · 45 endpoints · 131 tests**

## Features

### Auth and accounts

- Register, login, refresh, and revoke under `/api/auth`. Refresh tokens rotate on every use and are stored hashed. Reusing an old refresh token revokes every active token for that user.
- The app won't start without a usable `Jwt:Key`. Missing, too short, or placeholder keys fail at startup instead of causing errors later.
- User-scoped endpoints enforce ownership with an `EnsureSelfUser` filter. Naming another user in the URL or body returns 403. Touching another user's entity returns 404, so foreign rows look the same as missing ones. Integration tests cover this.

### Tasks, goals, habits, and mood

- Full CRUD for tasks, goals, and habits.
- Energy and mood entries, with averages over a time window.
- Goal decomposition that builds milestone steps from the goal's category (Professional, Health, Finance, and so on). It's rule-based, and the response says so.

### Calendar export

- `GET /api/calendar/daily/{userId}/{date}` and `/weekly/{userId}/{startDate}` return `.ics` files built from your tasks. You can import them into Google Calendar, Apple Calendar, or Outlook.

### Gamification

Every value is recalculated from your completion history each time you read it. There's no points ledger, badge table, or streak table, so the numbers can't drift from your actual activity. Anything that can't be derived from completion timestamps isn't reported.

- **Points:** 10 per completed task, 50 per completed goal, 25 per active habit.
- **Level:** 1,000 XP per level; XP is points × 2.
- **Badges:** Task Starter (10 tasks), Task Master (50 tasks, rare), Week Warrior (7-day streak), Month Master (30-day streak, epic). That's all four.
- **Streaks:** current and best, rebuilt from completion timestamps over a rolling 365-day window. The current day doesn't break your streak until it's over.
- **Leaderboards:** by tasks completed or current streak, plus motivation messages, achievements, and challenges.

### AI coach

Six endpoints under `/api/ai`: task and goal predictions, a task suggestion, pattern analysis, a weekly plan, and a reflection. The coach puts your recent activity into the prompt, so it isn't guessing from the endpoint name alone.

- It works with any OpenAI-compatible endpoint. `OpenAI:Endpoint` and `OpenAI:Model` (default `gpt-4o-mini`) are read from configuration.
- Without an API key the endpoints still work. They return insights computed directly from your data.

### Operations

- Prometheus metrics at `/metrics`: HTTP request metrics, notification counters (`notifications_sent_total`), and background-job telemetry.
- SignalR hub at `/hubs/notifications`. Completing a task or goal, or recording habit progress, broadcasts `TaskCompleted`, `GoalCompleted`, or `HabitCompleted` to that user's group.
- Health checks at `/health/live` and `/health/ready`. Readiness includes a database check.
- A refresh-token cleanup service runs every six hours. Revoked tokens older than 24 hours are deleted, with success, failure, and duration metrics.
- Validation failures return a consistent `{ success: false, errors: [...] }` shape.

## What's not here

The old version of this file listed 15+ badges, email and SMS notifications, and Redis caching. None of that existed in the code, so I removed it:

- **No email or SMS.** SignalR notifications exist; external channels don't.
- **No caching layer.** Redis was set up early, never used, and removed from registration, config, and the compose file.
- **No CQRS or MediatR.** Controllers call services, and services use repositories.
- **No time tracking.** The unused `TimeEntry` entity and table were removed.

If a feature isn't listed above, it isn't in the code.

## Architecture

Five projects. Dependencies point inward:

| Project | Contents |
|---|---|
| `Domain` | Entities and enums. Zero NuGet packages, not even EF Core. |
| `Application` | Services, DTOs, validators, repository interfaces. All business logic lives here. |
| `Infrastructure` | EF Core context, repositories, migrations, OpenAI client, token and calendar services. |
| `WebAPI` | Controllers, middleware, filters, the SignalR hub, DI wiring. |
| `AITaskCoach.Tests` | Unit, repository, and full-pipeline integration tests. |

Entity-to-DTO mapping is hand-written. There's no mediator between controllers and services, which leaves fewer moving parts when something breaks.

## Tech stack

- .NET 8 / ASP.NET Core, EF Core 8.0.5, SQL Server
- JWT bearer auth, FluentValidation, Swashbuckle (Swagger)
- SignalR, prometheus-net
- Tests: xUnit, Moq, FluentAssertions, AutoFixture, EF InMemory, WebApplicationFactory

## Running it locally

You need the .NET 8 SDK and a SQL Server (LocalDB or Express is fine, or use the Docker setup below).

```bash
git clone <this repo>
cd "Back-End AI Task Coach"
dotnet restore
dotnet ef database update --project Infrastructure --startup-project WebAPI   # applies the 4 migrations
dotnet run --project WebAPI
```

Then open **http://localhost:5129**. In Development, Swagger UI is served at the root URL (`RoutePrefix` is empty). The JSON spec is still at `/swagger/v1/swagger.json`.

Settings that matter:

- `ConnectionStrings:DefaultConnection` points at a local SQL Express instance by default. Change it to your server.
- `Jwt:Key` has a dev value in `appsettings.Development.json`. For anything beyond local use, set `Jwt__Key` through an environment variable or user-secrets. The `Production` appsettings ships placeholder values on purpose. The startup guard rejects them, so production won't boot until real secrets are injected.
- `OpenAI:ApiKey` is optional. Leave it empty and the AI endpoints use local fallbacks.

### Docker

```bash
docker compose up --build
```

| Service | Port | Notes |
|---|---|---|
| webapi | 8080 | the API |
| sqlserver | 1433 | SQL Server 2022 (Express edition) |
| prometheus | 9090 | scrapes the API's `/metrics` every 5 seconds |
| grafana | 3000 | admin / admin, Prometheus datasource and dashboard auto-provisioned from `Doc/grafana/` |

## API map

45 endpoints across 13 controllers. The `{userId}` routes are ownership-checked.

| Area | Routes |
|---|---|
| Auth | `POST /api/auth/register` · `/login` · `/refresh` · `/revoke` |
| Tasks | `GET /api/tasks/{id}` · `GET /api/tasks/user/{userId}` · `POST /api/tasks` · `PUT /{id}` · `DELETE /{id}` |
| Goals | same shape as Tasks |
| Habits | same shape as Tasks, plus `POST /{id}/progress` to record check-ins |
| Users | `GET /api/users/{id}` · `PUT /api/users/{id}` · `POST /api/users/preferences/{userId}` |
| Energy & mood | `POST /api/energymood/record` · `GET /entries/{userId}` · `GET /averages/{userId}` |
| Calendar | `GET /api/calendar/daily/{userId}/{date}` · `/weekly/{userId}/{startDate}` |
| Gamification | `summary` · `points` · `badges` · `achievements` · `challenges` · `motivation` (per user) · `GET /leaderboard` |
| Analytics | `GET /api/analytics/productivity/{userId}` |
| Habit analytics | `GET /api/habitanalytics/habit-summary/{userId}` |
| Reports | `GET /api/report/user-report/{userId}` |
| Goal decomposition | `POST /api/goaldecomposition/decompose` |
| AI | `predict-task` · `predict-goal` · `suggest-task` · `analyze/{userId}` · `weekly-plan/{userId}` · `reflect/{userId}` |
| Health & ops | `GET /health/live` · `/health/ready` · `/metrics` · SignalR `/hubs/notifications` (health and metrics are anonymous) |

## Tests

```bash
dotnet test AITaskCoach.Tests
```

131 tests, all passing on the last run. The suite covers service logic (token issuing and refresh rotation, gamification math), repositories against EF InMemory, SignalR broadcasting, and integration tests that boot the whole API through `WebApplicationFactory`. Those tests cover auth enforcement (anonymous requests get 401s), the AI offline fallback, health and metrics endpoints, and general endpoint behavior. For example, removed endpoints stay 404 and data comes from the database instead of hardcoded fixtures.

CI (`.github/workflows/ci.yml`) runs build and tests with coverage (artifact + Codecov) on every push. It scans for vulnerabilities with Trivy, uploads the results to GitHub code scanning, and pushes a container image to GHCR on `main`/`master`.

## Numbers

- 10,549 lines of C# across 133 files (EF migrations included)
- 45 endpoints, 13 controllers, 9 tables, 4 migrations
- 131 tests passing; build clean at 0 warnings, 0 errors
- 1 background service (refresh-token cleanup)

## Roadmap

Things that are missing:

- [ ] Rate limiting on `/api/auth/*` and per-user API limits
- [ ] Pagination for the `user/{userId}` list endpoints
- [ ] Roles/RBAC (everything is a regular user right now)
- [ ] Secret manager integration for `Jwt:Key`, `DefaultConnection`, `OpenAI:ApiKey`
- [ ] A real CD pipeline: image → staging → smoke tests → production
- [ ] OAuth2 providers alongside the existing JWT flow

## Docs

The `Doc/` folder was rewritten in October 2026 to match the code: `API_Reference.md` (all 45 endpoints), `Architecture.md`, `DeveloperGuide.md`, `CHANGELOG.md`, `SECURITY.md`, `CONTRIBUTING.md`, `ProductionReadiness.md` (gap list), `Issues.md` (known limitations), and `SDLC_Status.md`. The stale and empty files from the old v2.0 era are gone.

If something here doesn't match the code, that's a bug in the docs. Open an issue and I'll fix the file or the claim.

## License

MIT. See [LICENSE](LICENSE).

---

Last updated: October 2026.
