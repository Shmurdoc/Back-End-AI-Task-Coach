# Known Issues & Limitations

This page lists the scope cuts and technical debt I know about, so reviewers and future
maintainers can see where the edges are.

## Architecture

- **No MediatR/CQRS.** The original plan mentioned a request pipeline; I never added it. Services
  are injected directly. That's a simplicity choice, not an oversight.
- **Derived gamification.** Points, levels, badges, streaks, and leaderboards are computed on
  read from real activity. There is no points ledger. Leaderboard computation scans users, so it
  is fine for hundreds of users, not millions.
- **Analytics compute on read.** Productivity summaries and reports aggregate full histories per
  request. Caching or materialization would be the next step at scale.

## API behavior

- **No pagination.** `GET .../{userId}` list endpoints return every row for that user.
- **No rate limiting.** Nothing throttles login attempts or API traffic.
- **No API versioning.** Routes are unversioned.
- **401 vs 403 vs 404 semantics.** Unauthenticated requests get 401. A different user in the
  route or body gets 403 (via `EnsureSelfUser`). Foreign entity ids get 404 (services hide
  existence). I did this on purpose, but it means client code has to treat 403 and 404
  differently.
- **`EnsureSelfUser` is attribute-based.** New user-scoped endpoints must remember to add it.
  Entity-level ownership is still enforced in services.
- **`/metrics` is anonymous.** Fine for a local/dev stack. On the public internet it should sit
  behind network policy or auth.

## AI features

- **Quality depends on the configured LLM.** Without `OpenAI:ApiKey`, endpoints return local
  fallbacks derived from the user's data. Useful, but not the same as a model response.
- **No prompt/response caching or eval suite.** Costs and quality are not tracked.

## Operations

- **Single-instance assumptions.** Migrations run on startup. The refresh-token cleanup service
  is safe to run concurrently only because it's idempotent, not because of a distributed lock.
- **No distributed cache/Redis.** Redis was set up early and removed because nothing used it.
- **Backups are manual.** No automated backup/restore process for SQL Server is provided.
- **Observability stack is dev-local.** Prometheus and Grafana run in compose. There is no
  alerting or centralized logging.

## Testing

- Integration tests use the EF InMemory provider. SQL Server-specific behavior (types, indexes,
  constraints) is exercised only by running the compose stack by hand.
- No frontend/UX tests. This repository is backend-only.

## Historical note

Earlier versions of the docs in this repository described features that never existed (MediatR
pipelines, notification workers, Redis usage, notification/calendar-sync endpoints, and a
`TimeEntry` table). I rewrote those docs on 2026-10-18 to match the actual system. If you find a
claim that contradicts the code, please open an issue and I'll fix it.
