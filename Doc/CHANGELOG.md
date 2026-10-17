# Changelog

This project has **no tagged releases yet**. It's a portfolio project that keeps changing, so
entries are grouped by date.

## October 2026

### Added

- Refresh token lifecycle: rotation on every refresh, reuse detection (reuse revokes all active
  tokens), explicit `POST /api/auth/revoke`; refresh tokens stored SHA-256 hashed
  (migration `20261017211242_RefreshTokensAndRemoveDeadEntities`)
- Energy & mood tracking: `EnergyMoodEntries` table and `/api/energymood/*` endpoints
  (migration `20261016150622_AddEnergyMoodEntries`)
- SignalR notifications: `/hubs/notifications` hub, per-user groups, `TaskCompleted` /
  `GoalCompleted` / `HabitCompleted` events with Prometheus metrics
- Habit progress endpoint (`POST /api/habits/{id}/progress`) that triggers completion events
- Health checks: `/health/live`, `/health/ready` (readiness includes a database check)
- `RefreshTokenCleanupService` background worker (5 min startup delay, runs every 6 h, 24 h
  revoked-token retention, batches of 500) with job metrics
- Grafana provisioning (datasource + dashboards) wired into the compose stack
- JWT startup guard: rejects missing, short (<32 chars), or placeholder signing keys

### Changed

- `AIService` now honors `OpenAI:Endpoint` and `OpenAI:Model` configuration. Previously a
  hardcoded endpoint and model ignored the settings.
- Prometheus metrics consolidated; `/metrics` reflects HTTP, background-job, and notification
  activity
- CI pipeline cleanup: placeholder deploy job removed, Trivy action pinned to `0.28.0`, SARIF
  upload moved to `codeql-action/upload-sarif@v3`, coverage artifact + Codecov added, GHCR image
  push retained for `main`/`master`
- Unused NuGet packages removed across projects; unused Redis registration and compose service
  removed
- Docs rewritten end to end to match the actual system

### Removed

- Hollow "API root" endpoints (`GET /api/tasks`, `GET /api/habits`, `GET /api/goals` previously
  returned only a static message)
- Dead entities and their tables (`TimeEntry` and related) via the refresh-tokens migration
- 11 orphaned DTO files, empty directories, and zero-byte junk files

### Fixed

- Test suite expanded to **131 passing tests** (from 117). Auth-enforcement integration tests now
  assert strict 401s.
- Build has zero warnings at `-v q`

## September 2025 - Initial build

- Clean Architecture solution: `Domain`, `Application`, `Infrastructure`, `WebAPI`, tests
- JWT auth (register/login/refresh), PBKDF2 password hashing (migration
  `20250905111333_InitialCreate`, plus `20250905124016_AddPhoneNumberColumn`)
- CRUD for tasks, goals, and habits with per-user ownership enforcement
- AI endpoints (task/goal prediction, suggestions, analysis, weekly plans, reflections) with
  local fallbacks when no API key is configured
- Analytics, gamification (derived on read), calendar ICS exports
- Dockerfile + docker-compose dev stack; GitHub Actions CI
