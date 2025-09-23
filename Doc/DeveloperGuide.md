# Developer Guide

Welcome! Here’s how I work on and maintain this project. If you’re new, this should help you get up to speed fast.

## How I Work (SDLC)
- I use an Agile approach: short iterations, regular reviews, and CI/CD.
- I move through requirements, design, implementation, testing, deployment, and maintenance in cycles.

## Getting Started
- Clone the repo and check out the `README.md` for a quick overview.
- Restore dependencies and build the solution.
- Take a look at `Architecture.md` to understand the structure.
- Run the tests and check the coverage.

## My Coding Standards
- I use C# 10+ and .NET 8 best practices.
- Clean architecture and SOLID principles are a must.
- MediatR is used for CQRS and handler separation.
- I write XML comments for public APIs.
- Everything is wired up with dependency injection.

## Testing
- I write unit and integration tests for every new feature.
- I use xUnit, Moq, and AutoFixture.
- Tests should pass locally and in CI before merging.
- I aim for high coverage and meaningful assertions.

## CI/CD & DevOps
- All code changes go through GitHub Actions.
- Build, test, security scan, and deploy are all automated.
- Docker images are built and pushed for every release.

## Best Practices I Follow
- Never commit secrets or sensitive data.
- Use environment variables for all config.
- Document endpoints and config options.
- Keep docs up to date as the project evolves.

## Where to Find More
- `README.md` for setup and usage
- `ProductionReadiness.md` for deployment
- `Architecture.md` for system design
