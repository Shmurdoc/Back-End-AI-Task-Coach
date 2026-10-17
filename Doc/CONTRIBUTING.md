# Contributing

This is a personal portfolio project, but issues and pull requests are welcome. The bar is
simple: nothing merges unless the build has zero warnings and all tests pass.

## Setup

```powershell
dotnet restore
dotnet build          # must be warning-free
dotnet test           # 131 tests, all green
dotnet run --project WebAPI
```

See `DeveloperGuide.md` for database setup, configuration keys, and Docker usage.

## Workflow

1. Branch from **`master`** (the default branch): `git checkout -b feature/short-description`
2. Keep changes focused; one concern per PR.
3. Run `dotnet build` and `dotnet test` locally before pushing.
4. Open a PR against **`master`**. CI (build, tests with coverage, Trivy scan) must pass.

Branch notes: CI also runs on pushes to `main` and `develop`, but `master` is where development
happens.

## Code guidelines

- Follow the existing Clean Architecture boundaries: `Domain` ← `Application` ← `Infrastructure` /
  `WebAPI`. No backward references.
- Controllers stay thin: bind → call one service → wrap in the `{ success, data }` envelope.
- User-scoped endpoints must use `[Authorize]` + `[EnsureSelfUser]`; enforce entity ownership in
  the service layer.
- GUID ids, UTC timestamps, async methods with `CancellationToken`.
- No new warnings. No comments unless they explain *why* something non-obvious is done.
- Add tests for behavior changes: unit tests for services/controllers, integration tests for
  auth/status-code behavior.
