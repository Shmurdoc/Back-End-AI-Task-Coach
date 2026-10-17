# Production Readiness

An assessment checked against the current codebase. "Ready" means implemented **and tested**.
Everything else is listed as a gap.

## Ready

| Area | Status |
|------|--------|
| Authentication | JWT access tokens + opaque refresh tokens; rotation on refresh; reuse detection revokes all user tokens; explicit revoke endpoint |
| Password storage | PBKDF2 / SHA-256, 100,000 iterations, per-password salt |
| Authorization | `EnsureSelfUser` filter (403 on cross-user `userId`) + service-level ownership checks (404 on foreign entities) |
| Configuration safety | Startup fails on missing/short/placeholder `Jwt:Key`; production config ships placeholders so booting requires real secrets |
| Error handling | Global exception middleware; consistent `{ success, data/error }` envelope; validation failures normalized |
| Data layer | EF Core + SQL Server; 4 migrations; automatic migration on startup |
| Observability | `/health/live` + `/health/ready` (DB check); Prometheus `/metrics` with HTTP, notification, and background-job metrics; structured `ILogger` logging |
| Background jobs | Refresh-token cleanup (5 min delay, every 6 h, 24 h retention, batches of 500) with failure/execution metrics |
| Real-time | SignalR hub with JWT auth; per-user groups; completion events for tasks/goals/habits |
| Containers | Multi-stage Dockerfile; non-root runtime user |
| CI | Build + 131 tests + coverage artifact & Codecov + Trivy filesystem scan to GitHub code scanning + GHCR image build/push on default branch |
| AI resilience | All AI endpoints return local, data-grounded fallbacks when no API key is configured |
| Tests | 131 tests, zero build warnings |

## Gaps (known, deliberate)

| Area | Gap |
|------|-----|
| Authorization | No roles/RBAC. Every authenticated user is a regular user, and there is no admin surface. |
| Rate limiting | None (login/refresh especially) |
| API surface | No versioning, so breaking changes would be breaking |
| Secret management | Environment-variable injection only; no built-in Key Vault/AWS Secrets Manager integration |
| Transport | Local dev is HTTP; TLS termination/proxy configuration is not provided |
| Deployment | CI pushes a container image; there is no CD to an environment, no IaC, no environment promotion |
| Scaling | Migrate-on-startup assumes a single instance; no distributed lock; no distributed cache; analytics/gamification compute on read |
| Query efficiency | No pagination on list endpoints; per-user analytics scans full histories |
| Database ops | No backup/restore automation or PITR strategy; dev DB is a local SQL Server instance |
| Testing depth | Integration tests use the EF InMemory provider, not a real SQL Server (compose provides one for manual testing) |
| Observability ops | Prometheus/Grafana stack is dev-local; no alerting rules, no log aggregation, no tracing |
| Account lifecycle | No email verification, password reset, or MFA |

## Recommended next steps (in order)

1. Rate limiting on `/api/auth/*` and a global per-user limit.
2. Pagination for `user/{userId}` list endpoints.
3. Role model + admin-only endpoints (or explicitly document the single-tenant, no-admin design).
4. Secret manager integration for `Jwt:Key`, `DefaultConnection`, `OpenAI:ApiKey`.
5. CD pipeline: push image → staging environment → smoke tests → manual approval → production.
6. Backup/restore runbook for the production database; test a restore.
7. Replace migrate-on-startup with an explicit migration step in the deploy pipeline.
8. Load test the analytics/leaderboard paths and add caching if needed.
