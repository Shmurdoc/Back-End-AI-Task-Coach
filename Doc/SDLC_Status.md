# SDLC Status

Snapshot of the software development lifecycle for AI Task Coach. Last updated: **2026-10-18**.

## Timeline

| Phase | Period | Status |
|-------|--------|--------|
| Concept & architecture | Aug 2025 | Done |
| Initial implementation (auth, core CRUD, AI, analytics, gamification) | Aug–Sep 2025 | Done |
| Dormancy | Sep 2025 – Oct 2026 | — |
| Energy/mood feature | Oct 2026 | Done |
| Cleanup and fixes (this effort) | Oct 2026 | Done |
| Production deployment | — | Not started (build/CI ready; no CD target) |

## Current state (2026-10-18)

- **Build:** warning-free
- **Tests:** 131/131 passing (unit + integration)
- **API surface:** 45 endpoints across 13 controllers, all documented in `API_Reference.md`
- **Database:** SQL Server, 9 tables, 4 EF Core migrations
- **Auth:** JWT access tokens, refreshing with rotation + reuse detection, revoke endpoint
- **Real-time:** SignalR hub with completion events for tasks, goals, habits
- **Observability:** health checks, Prometheus metrics, Grafana provisioning
- **CI:** automated build, tests with coverage, Trivy scan, GHCR image push
- **Docs:** rewritten to match the system. An earlier "v2.0" status overstated what existed; I corrected that content instead of keeping it.

## Working practices

- Clean Architecture boundaries are enforced by project references
- All user data is owner-scoped and enforced in two places (`EnsureSelfUser` filter and service
  checks)
- No secrets in the repository. Production config fails on purpose without real secrets.
- I checked these docs against the code: the build passes and the tests listed here pass.

## Known gaps

See `ProductionReadiness.md` (gap table + prioritized next steps) and `Issues.md` (known scope
cuts). The top three are rate limiting, pagination, and a real deployment pipeline.
