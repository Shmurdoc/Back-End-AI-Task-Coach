# How I Designed the Architecture

## 1. My Approach

I went with a layered clean architecture for this project because I wanted it to be easy to maintain, test, and extend. Here’s how I broke it down:

- **Domain Layer**: This is where all the core business logic lives—entities like Habit, Goal, TaskItem, User, plus enums and analytics.
- **Application Layer**: Handles CQRS (using MediatR), service interfaces, validators, and business rules.
- **Infrastructure Layer**: Deals with persistence (EF Core), repositories, external services (MailKit, Twilio), Redis caching, and logging.
- **WebAPI Layer**: Exposes everything via REST controllers, sets up DI, middleware (metrics, error handling), Swagger docs, and SignalR for real-time stuff.

## 2. Supporting Tech
- **Observability**: Prometheus for metrics, Grafana for dashboards
- **Testing**: Unit/integration tests, coverage, CI/CD with security scanning
- **Deployment**: Docker, docker-compose, GitHub Actions

## 3. Why I Made These Choices
- **CQRS & MediatR**: Keeps commands and queries clean and scalable
- **Modular structure**: Makes it easy to add new features or swap out parts
- **Secure config**: All secrets are handled via environment variables or secret managers
- **Production observability**: Metrics, logs, and dashboards are first-class

## 4. Project Structure (at a glance)
```
Back-End AI Task Coach
├── Domain
│   ├── Entities
│   ├── Enums
│   └── Common
├── Application
│   ├── CQRS
│   ├── DTOs
│   ├── Behaviors
│   ├── IRepositories
│   ├── IService
│   ├── Extensions
│   ├── Mappers
│   └── Validators
├── Infrastructure
│   ├── Persistence
│   ├── DependencyInjection
│   └── Extensions
├── WebAPI
│   ├── Controllers
│   ├── Middleware
│   ├── Hubs
│   ├── Background
│   ├── Extensions
│   ├── appsettings.json
│   └── Program.cs
├── AITaskCoach.Tests
│   ├── Infrastructure
│   ├── Repositories
│   ├── Integration
│   └── Properties
├── Doc
│   ├── Architecture_Overview.md
│   ├── ProductionReadinessChecklist.md
│   └── grafana_dashboard_seed.json
├── Dockerfile
├── docker-compose.yml
└── prometheus.yml
```

## 5. How You Can Extend It
- Want to add AI/ML? Plug modules into Application/Infrastructure.
- Need more notifications or analytics? Just add new providers.
- Want calendar or focus tool integration? The structure makes it easy.

## 6. Related Docs
- See `ProductionReadiness.md` for deployment/security
- See `DeveloperGuide.md` for onboarding and SDLC
