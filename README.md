# Back-End AI Task Coach

Welcome to my AI-powered productivity and coaching backend! I built this system to help users (and myself) plan, adapt, and actually achieve goals—using smart scheduling, behavioral insights, and solid observability.

## Why I Built This
- **Adaptive Scheduling Engine**: Automatically restructures tasks and goals based on deadlines, workload, and user patterns.
- **Behavior-Aware Prioritization**: Learns from your history to flag at-risk tasks and trigger focus modes.
- **Smart Timetable Generator**: Builds personalized schedules and can export to your calendar.
- **Procrastination Recovery**: Detects inactivity and helps you get back on track.
- **AI Suggestions & Nudges**: Offers micro-suggestions and motivational feedback.

## Tech Stack
- .NET 8.0 (C#)
- CQRS, MediatR
- Entity Framework Core
- Redis, SQL Server
- Prometheus, Grafana
- Docker, Docker Compose
- SignalR
- MailKit, Twilio
- xUnit, Moq, AutoFixture

## What Makes It Special
- Modular clean architecture (Domain, Application, Infrastructure, WebAPI)
- Secure config and secrets management
- Real-time notifications and analytics
- Comprehensive test suite and CI/CD pipeline
- Production-ready observability and monitoring

## How to Run It
1. **Clone this repo**
2. **Restore dependencies**: `dotnet restore`
3. **Build the solution**: `dotnet build`
4. **Run the API**: `dotnet run --project WebAPI/WebAPI.csproj`
5. **Or use Docker**: `docker-compose up --build`
6. **API docs**: Go to `/swagger` once it’s running

## Want to Contribute?
- Fork the repo and make a feature branch
- Follow my coding standards in `DeveloperGuide.md`
- Write and run tests before submitting PRs
- See `Issues.md` for open tasks and known limitations

## License
MIT (see LICENSE file)
