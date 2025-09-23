# AI Task Coach - Enterprise Productivity System 🚀

A comprehensive, enterprise-grade AI-powered productivity and task management system built with .NET 8.0 and Clean Architecture principles. Features advanced gamification, behavioral analytics, and real-time monitoring for maximum user engagement and productivity optimization.

## 🎮 What's New in Version 2.0 - Gamification Release

### 🏆 Enterprise Gamification System
- **Points & Progression**: Dynamic point system with streak bonuses and difficulty multipliers
- **Badge Collection**: 15+ unique badges with rarity levels (Common, Rare, Epic, Legendary)
- **Achievement System**: Progressive unlocks with real business value
- **Leaderboards**: Weekly and all-time competitive rankings
- **Challenge System**: Dynamic challenges with real-time progress tracking
- **Behavioral Analytics**: AI-powered relapse detection and intervention

### 🗄️ Production-Ready Database
- **Complete Schema**: 11 tables with proper relationships and constraints
- **EF Core Migrations**: Ready-to-deploy database structure
- **Data Integrity**: Comprehensive validation and foreign key relationships
- **Analytics Ready**: Built-in analytics tables for business intelligence

### 🔄 Real-Time Background Processing
- **Critical Mode Detection**: Automatic intervention for productivity issues
- **Procrastination Recovery**: Intelligent recovery mechanisms
- **Continuous Monitoring**: Background services track user behavior 24/7

## 🏗️ Architecture Overview

Built on **Clean Architecture** principles with:

- **Domain Layer**: Core business entities and rules
- **Application Layer**: Business logic, services, CQRS patterns, comprehensive DTOs
- **Infrastructure Layer**: Data access, external services, dependency injection
- **WebAPI Layer**: REST API, background services, middleware, SignalR hubs

## 🚀 Key Features

### Core Productivity Features
- **Smart Task Management**: AI-powered task suggestions and prioritization
- **Goal Tracking**: Long-term objective management with progress monitoring
- **Habit Formation**: Streak tracking and habit analytics
- **Time Management**: Detailed time tracking and productivity analysis
- **Calendar Integration**: Export schedules to external calendar systems

### Advanced Analytics
- **Behavioral Insights**: Pattern recognition and productivity optimization
- **Performance Metrics**: Comprehensive productivity analytics
- **Predictive Analytics**: AI-powered suggestions for improved workflows
- **Real-time Monitoring**: Live updates and instant feedback

### Enterprise Features
- **Background Processing**: Automated monitoring and intervention systems
- **Notification System**: Multi-channel notifications (Email, SMS)
- **Real-time Updates**: SignalR for live collaboration and updates
- **Comprehensive Logging**: Structured logging with Prometheus metrics
- **Health Monitoring**: Production-ready health checks and monitoring

## 🛠️ Technology Stack

### Backend
- **.NET 8.0** - Latest C# features and performance improvements
- **Entity Framework Core** - Advanced ORM with migrations
- **SQL Server** - Enterprise-grade database with full ACID compliance
- **Redis** - High-performance caching and session management
- **SignalR** - Real-time bidirectional communication

### Architecture & Patterns
- **Clean Architecture** - Proper separation of concerns
- **CQRS with MediatR** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling and testability
- **Background Services** - Long-running tasks and monitoring

### DevOps & Monitoring
- **Docker & Docker Compose** - Containerization and orchestration
- **Prometheus** - Metrics collection and monitoring
- **Grafana** - Advanced dashboards and visualization
- **Structured Logging** - Comprehensive application logging
- **Health Checks** - Production-ready monitoring endpoints

### Communication & Notifications
- **MailKit** - Enterprise email delivery
- **Twilio** - SMS and communication services
- **SignalR** - Real-time web communication

### Testing & Quality
- **xUnit** - Comprehensive unit testing framework
- **Moq** - Mocking framework for isolated testing
- **AutoFixture** - Test data generation
- **Integration Tests** - End-to-end API testing

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (or Docker)
- Redis (optional, for caching)
- Docker & Docker Compose (recommended)

### 1. Clone Repository
```bash
git clone https://github.com/your-username/ai-task-coach.git
cd ai-task-coach
```

### 2. Database Setup
```bash
# Using Entity Framework migrations
dotnet ef database update --project Infrastructure

# Or using Docker
docker-compose up sql-server
```

### 3. Build & Run
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API server
dotnet run --project WebAPI --urls http://localhost:5000

# Or use Docker Compose (recommended)
docker-compose up --build
```

### 4. Explore the API
- **Swagger UI**: http://localhost:5000/swagger
- **Health Checks**: http://localhost:5000/health
- **Metrics**: http://localhost:5000/metrics

## 📊 API Endpoints Overview

### 🎮 Gamification (New in v2.0)
- `GET /api/gamification/points/{userId}` - User points and history
- `GET /api/gamification/badges/{userId}` - Badge collection
- `GET /api/gamification/achievements/{userId}` - Achievement progress
- `GET /api/gamification/leaderboard` - Competitive rankings
- `GET /api/gamification/challenges/{userId}` - Active challenges
- `GET /api/gamification/insights/{userId}` - AI-powered insights

### 📋 Core Features
- `GET/POST/PUT/DELETE /api/tasks` - Complete task management
- `GET/POST/PUT/DELETE /api/goals` - Goal tracking and progress
- `GET/POST/PUT/DELETE /api/habits` - Habit formation and analytics
- `GET/PUT /api/users/{id}` - User management and preferences

### 📊 Analytics
- `GET /api/analytics/productivity/{userId}` - Productivity metrics
- `GET /api/analytics/weekly-report/{userId}` - Weekly summaries
- `GET /api/habit-analytics/streaks/{userId}` - Streak analysis

## 🎮 Gamification Features Deep Dive

### Point System
- **Task Completion**: Base points + difficulty multipliers
- **Streak Bonuses**: Additional points for consistency
- **Time Management**: Bonus points for on-time completion
- **Quality Metrics**: Points for detailed task descriptions and planning

### Badge System
Available badges include:
- **Task Master** (Epic) - Complete 100 tasks
- **Perfectionist** (Rare) - 95%+ completion rate over 30 days
- **Early Bird** (Common) - Complete 10 tasks before 9 AM
- **Night Owl** (Common) - Complete 10 tasks after 9 PM
- **Speedster** (Rare) - Complete tasks 20% faster than estimated
- **Planner** (Epic) - Plan next day's tasks for 30 consecutive days

### Achievement Categories
1. **Task Completion**: Milestones for completed tasks
2. **Habit Formation**: Streak-based achievements
3. **Time Management**: Efficiency and punctuality achievements
4. **Goal Achievement**: Long-term objective completions
5. **Social**: Community engagement and helping others

## 🔧 Configuration

### Environment Variables
```bash
# Database
CONNECTION_STRING=Server=localhost;Database=AITaskCoach;Trusted_Connection=true;

# Redis (optional)
REDIS_CONNECTION=localhost:6379

# Notifications
SMTP_SERVER=smtp.gmail.com
EMAIL_API_KEY=your_email_api_key
TWILIO_SID=your_twilio_sid
TWILIO_TOKEN=your_twilio_token

# JWT (for production)
JWT_SECRET_KEY=your_jwt_secret_key
JWT_ISSUER=AITaskCoach
JWT_AUDIENCE=AITaskCoach_Users
```

### Docker Compose
```bash
# Start all services
docker-compose up -d

# Scale API instances
docker-compose up --scale api=3

# View logs
docker-compose logs -f api
```

## 📈 Monitoring & Observability

### Metrics (Prometheus)
- API request duration and count
- Database query performance
- Background service health
- User engagement metrics
- Gamification effectiveness metrics

### Health Checks
- `/health` - Basic application health
- `/health/ready` - Readiness probe for K8s
- `/health/live` - Liveness probe for K8s

### Logging
Structured JSON logging with:
- Request/response correlation IDs
- User context and security events
- Performance metrics and slow queries
- Business events (task completions, achievements unlocked)

## 🧪 Testing

### Run Tests
```bash
# Unit tests
dotnet test AITaskCoach.Tests/

# Integration tests
dotnet test --filter Category=Integration

# Coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories
- **Unit Tests**: Service layer and business logic
- **Integration Tests**: API endpoints and database operations
- **Repository Tests**: Data access layer verification
- **Background Service Tests**: Worker service functionality

## 🚀 Deployment

### Production Checklist
See `Doc/ProductionReadiness.md` for comprehensive deployment guidelines.

Key requirements:
- [ ] JWT authentication configured
- [ ] HTTPS certificates installed
- [ ] Database backups automated
- [ ] Monitoring dashboards deployed
- [ ] Error alerting configured

### Docker Production
```dockerfile
# Build production image
docker build -f Dockerfile.prod -t aitaskcoach:latest .

# Run with production configuration
docker run -d \
  --name aitaskcoach \
  -p 80:80 \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e CONNECTION_STRING="${PROD_DB_CONNECTION}" \
  aitaskcoach:latest
```

## 📚 Documentation

- **[Architecture Guide](Doc/Architecture.md)** - Detailed architectural decisions and patterns
- **[API Reference](Doc/API_Reference.md)** - Complete endpoint documentation
- **[Production Guide](Doc/ProductionReadiness.md)** - Deployment and production guidelines
- **[Changelog](Doc/CHANGELOG.md)** - Version history and breaking changes
- **[Developer Guide](Doc/DeveloperGuide.md)** - Development setup and guidelines

## 🤝 Contributing

### Getting Started
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Follow coding standards in `Doc/DeveloperGuide.md`
4. Write tests for new functionality
5. Submit a pull request

### Development Workflow
- Follow Clean Architecture principles
- Write comprehensive tests
- Use conventional commit messages
- Update documentation for API changes
- Ensure all CI/CD checks pass

## 📊 Project Statistics

- **Lines of Code**: 15,000+ (including comprehensive gamification system)
- **API Endpoints**: 40+ (8 new gamification endpoints)
- **Database Tables**: 11 (complete relational schema)
- **Background Services**: 3 (monitoring, recovery, analytics)
- **Test Coverage**: Comprehensive unit and integration tests
- **Documentation**: 10+ markdown files with complete guides

## 🔮 Roadmap

### Version 2.1 (Q4 2025)
- [ ] JWT/OAuth2 authentication system
- [ ] Advanced AI integration for task suggestions
- [ ] Mobile API optimizations
- [ ] Enhanced analytics dashboard

### Version 2.2 (Q1 2026)
- [ ] Machine learning for productivity predictions
- [ ] Social features and team collaboration
- [ ] Third-party integrations (Google Calendar, Slack)
- [ ] Advanced reporting and business intelligence

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ Support

- **Documentation**: Comprehensive guides in the `Doc/` folder
- **Issues**: Report bugs and request features via GitHub Issues
- **API Testing**: Use the built-in Swagger UI at `/swagger`
- **Health Monitoring**: Check application status at `/health`

---

**Built with ❤️ using .NET 8.0 and Clean Architecture**

*Last Updated: September 5, 2025 - Version 2.0 (Gamification Release)*
