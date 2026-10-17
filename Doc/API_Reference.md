# API Reference

AI Task Coach exposes a JSON REST API built on ASP.NET Core 8, plus a SignalR hub for real-time
notifications and standard health/metrics endpoints. This document covers all **45 endpoints
across 13 controllers**, the response envelope conventions, authentication, and real-time events.

- **Base URL:** `http://localhost:5129` (see `WebAPI/Properties/launchSettings.json`)
- **Interactive docs:** Swagger UI at the root URL in Development (the JSON spec is at
  `/swagger/v1/swagger.json`)
- **Content type:** `application/json` everywhere except the calendar exports, which return `text/calendar`
- **IDs:** every entity and user identifier is a GUID
- **Dates:** all dates and timestamps are UTC

---

## Conventions

### Response envelope

Every controller action except the auth endpoints and calendar exports returns an envelope:

```json
{ "success": true, "data": { } }
```

Errors use the same shape with an `error` string instead of `data`:

```json
{ "success": false, "error": "Task not found" }
```

### Status codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | Validation failure or malformed input |
| 401 | Missing, expired, or invalid access token |
| 403 | Request references a different user than the caller |
| 404 | Resource missing or not owned by the caller |
| 409 | Conflict (e.g. email already in use) |
| 500 | Unhandled server error (returns `{ "success": false, "error": "Internal server error" }`) |

### Model validation

Model binding/DataAnnotation failures are rewritten by `InvalidModelStateResponseFactory` to:

```json
{ "success": false, "errors": [ "Email is required." ] }
```

### Ownership enforcement

Every `userId` route value and every `userId`-carrying request body must match the authenticated
caller's id. `EnsureSelfUserAttribute` short-circuits with **403** otherwise:

```json
{ "success": false, "error": "You may only access your own user data." }
```

Entity-id routes (`{id:guid}`) are enforced separately by ownership checks inside the application
services. Those surface as **404**, so foreign resources look the same as missing ones.

---

## Authentication - `/api/auth`

| Method | Route | Auth | Body | Notes |
|--------|-------|------|------|-------|
| POST | `/api/auth/register` | Anonymous | `{ email, password, name }` | Creates the user and returns tokens |
| POST | `/api/auth/login` | Anonymous | `{ email, password }` | Returns tokens |
| POST | `/api/auth/refresh` | Anonymous | `{ refreshToken }` | Rotates the refresh token |
| POST | `/api/auth/revoke` | Bearer | `{ refreshToken }` | Revokes the token; always returns 200 |

Register/login/refresh return an `AuthResponse` directly (not the standard envelope):

```json
{
  "success": true,
  "message": "...",
  "token": "<jwt access token>",
  "refreshToken": "<opaque refresh token>",
  "user": { "id": "...", "email": "...", "name": "...", "isActive": true, "createdAt": "..." }
}
```

Token behavior:

- **Access tokens** are JWTs valid for `Jwt:ExpiresInMinutes` (default 120 minutes).
- **Refresh tokens** are opaque, stored SHA-256 hashed, and valid for `Jwt:RefreshExpiresInDays`
  (default 7 days).
- **Rotation:** every successful refresh revokes the used token and issues a new one.
- **Reuse detection:** presenting an already-rotated or revoked refresh token revokes *all* of the
  caller's active refresh tokens.
- **Passwords** are hashed with PBKDF2 (SHA-256, 100,000 iterations, 16-byte salt).

---

## AI - `/api/ai`

All AI endpoints degrade gracefully. When `OpenAI:ApiKey` is not configured, they return locally
derived responses built from the caller's real data instead of calling an LLM.

| Method | Route | Body / params | Notes |
|--------|-------|---------------|-------|
| POST | `/api/ai/predict-task` | `{ task: TaskDto, userId }` | Suggestion for approaching a task |
| POST | `/api/ai/predict-goal` | `{ goal: GoalDto, userId }` | Suggestion for approaching a goal |
| POST | `/api/ai/suggest-task` | `{ taskDescription }` | Suggestion from a free-text description |
| GET | `/api/ai/analyze/{userId}` | — | Pattern analysis over the user's tasks, goals, habits, and energy entries |
| POST | `/api/ai/weekly-plan/{userId}` | `{ context }` (parsed as a week start date) | Weekly plan grounded in the user's open work; 400 on unparseable date |
| POST | `/api/ai/reflect/{userId}` | `{ text }` | Coaching response to reflection text |

`data` is a string for the suggestion endpoints and a structured object for `analyze` and
`weekly-plan`.

---

## Tasks - `/api/tasks`

| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/api/tasks/{id}` | — | Single task |
| GET | `/api/tasks/user/{userId}` | — | All tasks for the user |
| POST | `/api/tasks` | `CreateTaskDto` | Validated by `CreateTaskDtoValidator` |
| PUT | `/api/tasks/{id}` | `UpdateTaskDto` | Validated by `UpdateTaskDtoValidator`; 404 when missing |
| DELETE | `/api/tasks/{id}` | — | Returns `{ "success": true }` |

When an update transitions the task to `Completed`, the server broadcasts a `TaskCompleted`
SignalR event (see below). Task status values: `Todo`, `InProgress`, `Review`, `Completed`,
`Cancelled`, `OnHold`, `Blocked`.

## Goals - `/api/goals`

| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/api/goals/{id}` | — | Single goal |
| GET | `/api/goals/user/{userId}` | — | All goals for the user |
| POST | `/api/goals` | `GoalDto` | Validated by `CreateGoalDtoValidator` |
| PUT | `/api/goals/{id}` | `GoalDto` | 404 when missing |
| DELETE | `/api/goals/{id}` | — | Returns `{ "success": true }` |

When an update transitions the goal to `Completed`, the server broadcasts a `GoalCompleted`
event.

## Habits - `/api/habits`

| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/api/habits/{id}` | — | Single habit |
| GET | `/api/habits/user/{userId}` | — | All habits for the user |
| POST | `/api/habits` | `HabitDto` | Create |
| POST | `/api/habits/{id}/progress` | `HabitDto` | Records progress; broadcasts `HabitCompleted` |
| PUT | `/api/habits/{id}` | `HabitDto` | 404 when missing |
| DELETE | `/api/habits/{id}` | — | Returns `{ "success": true }` |

## Goal Decomposition - `/api/goaldecomposition`

| Method | Route | Body | Notes |
|--------|-------|------|-------|
| POST | `/api/goaldecomposition/decompose` | `{ goalId }` | Breaks a stored goal into milestone steps; 404 when missing or owned by another user |

The goal title is read from the database, not from the request body. Decomposition is rule-based
(category and optional target date), not an LLM call.

---

## Analytics & Reporting

| Controller | Method | Route | Notes |
|------------|--------|-------|-------|
| Analytics | GET | `/api/analytics/productivity/{userId}` | Productivity summary across tasks, goals, habits |
| HabitAnalytics | GET | `/api/habitanalytics/habit-summary/{userId}` | Per-habit streaks/completion statistics |
| Report | GET | `/api/report/user-report/{userId}` | Combined user report |

## Energy & Mood - `/api/energymood`

| Method | Route | Body / params | Notes |
|--------|-------|---------------|-------|
| POST | `/api/energymood/record` | `{ userId, energyLevel (1–10), moodLevel (1–10), note?, timestamp? }` | Records one entry; `timestamp` defaults to now |
| GET | `/api/energymood/entries/{userId}` | `?from&to` (optional) | Entries in range |
| GET | `/api/energymood/averages/{userId}` | `?from&to` (optional) | `{ avgEnergy, avgMood }` |

## Gamification - `/api/gamification`

Points, levels, badges, streaks, and challenges are **derived on read** from the user's tasks,
goals, and habits. There is no points ledger table. Scoring: 10 points per completed task,
50 per completed goal, 25 per tracked habit; 1 level per 1,000 XP, where XP = points × 2.

| Method | Route | Notes |
|--------|-------|-------|
| GET | `/api/gamification/summary/{userId}` | Points, level, XP, badges, streaks |
| GET | `/api/gamification/points/{userId}` | `{ points, level, experience }` |
| GET | `/api/gamification/badges/{userId}` | Earned/locked badges |
| GET | `/api/gamification/achievements/{userId}` | Achievement checks |
| GET | `/api/gamification/challenges/{userId}` | Active challenges |
| GET | `/api/gamification/leaderboard` | Query: `type` (default `Points`), `limit` (default 10) |
| GET | `/api/gamification/motivation/{userId}` | Motivational message |

`type` values: `Points`, `TasksCompleted`, `CurrentStreak`, `BestStreak`, `HabitsCompleted`,
`GoalsAchieved`, `Weekly`, `Monthly`, `AllTime`.

## Calendar exports - `/api/calendar`

| Method | Route | Notes |
|--------|-------|-------|
| GET | `/api/calendar/daily/{userId}/{date}` | Downloads `daily-{userId}-{yyyyMMdd}.ics` |
| GET | `/api/calendar/weekly/{userId}/{startDate}` | Downloads `weekly-{userId}-{yyyyMMdd}.ics` |

Both return `text/calendar` file downloads, not JSON.

## Users - `/api/users`

| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/api/users/{id}` | — | 404 unless `id` is the caller |
| PUT | `/api/users/{id}` | `UserDto` | 404 unless caller; 409 when the new email is taken |
| POST | `/api/users/preferences/{userId}` | `UserPreferences` | Upserts preferences |

---

## Health & metrics

| Endpoint | Auth | Notes |
|----------|------|-------|
| `/health/live` | Anonymous | Liveness: process is up; returns JSON |
| `/health/ready` | Anonymous | Readiness: includes a database connectivity check |
| `/metrics` | Anonymous | Prometheus exposition (HTTP metrics, background-job metrics, notification metrics) |

Custom metrics include:

- `notifications_sent_total{type}` and `notification_delivery_time_seconds{type}` for SignalR
  broadcasts, labelled `task_completed`, `goal_completed`, `habit_completed`
- `background_jobs_executed_total{job}`, `background_jobs_failed_total{job}`,
  `background_job_duration_seconds{job}` for the refresh-token cleanup job

## Real-time notifications (SignalR)

- **Hub:** `/hubs/notifications`
- **Auth:** JWT bearer; browsers may pass the token via an `access_token` query string (standard
  SignalR pattern)
- **Groups:** on connect the client can call `JoinUserGroup` / `LeaveUserGroup` to join its own
  group (`user:{userId}` derived from the token). Clients cannot join another user's group.

| Event | Payload | Fired when |
|-------|---------|------------|
| `TaskCompleted` | `{ id, title, status, completedAt }` | A task update sets status to `Completed` |
| `GoalCompleted` | `{ id, title, status, completedAt }` | A goal update sets status to `Completed` |
| `HabitCompleted` | `{ id, name, currentStreak, completionRate, lastCompletedAt }` | Habit progress is recorded |
