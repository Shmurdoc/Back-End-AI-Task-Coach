# AI Task Coach – Architecture Overview & Design Rationale

## 1. Solution Architecture

**Layered Clean Architecture:**
- **Domain Layer:**  
  - Core business entities (Habit, Goal, TaskItem, User, etc.)
  - Domain logic and value objects (enums, analytics, progress, etc.)
- **Application Layer:**  
  - Use cases, CQRS handlers, service interfaces, validators
  - Notification abstraction, business rules, MediatR integration
- **Infrastructure Layer:**  
  - EF Core DbContext, repositories, persistence, external service integrations (MailKit, Twilio)
  - Caching (Redis), logging, configuration
- **WebAPI Layer:**  
  - REST API controllers, DI setup, middleware (metrics, error handling)
  - Swagger/OpenAPI, SignalR hubs for real-time notifications

**Supporting Infrastructure:**
- **Observability:** Prometheus metrics, Grafana dashboards
- **Testing:** Unit/integration tests, coverage, CI/CD with security scanning
- **Deployment:** Docker, docker-compose, GitHub Actions, cloud-ready

---

## 2. Main Functionalities

- **Habit Tracking:**  
  Create, update, delete, and retrieve habits. Track entries, streaks, analytics, and completion rates.
- **Goal & Task Management:**  
  Entities and relationships for goals and tasks, with progress tracking and analytics.
- **User Context:**  
  User entity, context abstraction, but no authentication yet.
- **Notifications:**  
  Email (MailKit) and SMS (Twilio) with retry, logging, and testability.
- **Analytics:**  
  Habit analytics, completion trends, streaks, and progress history.
- **Observability:**  
  Metrics for API, background jobs, nudges, and system health.
- **Real-time Updates:**  
  SignalR hub for client notifications.
- **Caching:**  
  Redis for scalable, fast access to heavy AI suggestions and schedules.
- **DevOps:**  
  Automated build, test, security scan, and deployment pipeline.

---

## 3. What Is Missing for a True Cognitive System?

- **No Cognitive/AI Reasoning:**  
  - No integration with LLMs (e.g., OpenAI, Azure Cognitive Services) for natural language understanding, adaptive coaching, or generative feedback.
  - No ML models for habit/task/goal recommendation, anomaly detection, or personalized nudges.
  - No context-aware or self-improving logic (no reinforcement learning, no user modeling).
- **No Semantic Understanding:**  
  - System does not interpret user intent, context, or emotional state.
  - No conversational interface or adaptive dialog.
- **No Automated Knowledge Graph:**  
  - No entity linking, relationship inference, or knowledge base construction.
- **No Cognitive Feedback Loop:**  
  - No learning from user feedback, no dynamic adjustment of strategies.

---

## 4. Why This Approach Was Chosen

- **Practicality & Maintainability:**  
  - Clean architecture ensures separation of concerns, testability, and maintainability.
  - Focused on robust, scalable, observable backend as a foundation.
- **Enterprise Readiness:**  
  - Prioritized security, observability, and deployment automation for real-world use.
- **Extensibility:**  
  - The current structure allows easy integration of AI/ML modules in the future (e.g., plug in OpenAI, add ML.NET, or connect to external cognitive APIs).
- **Resource Constraints:**  
  - Building a true cognitive system requires significant data, compute, and ML expertise.
  - This approach delivers immediate business value and a solid platform for future cognitive upgrades.

---

## 5. Summary Table

| Area                  | Current State         | Cognitive System Gap                |
|-----------------------|----------------------|-------------------------------------|
| Habit/Goal Tracking   | CRUD, analytics      | No adaptive/AI-driven suggestions   |
| Notifications         | Email/SMS, retry     | No context-aware nudges             |
| User Management       | Entity only          | No auth, no personalization         |
| Analytics             | Trends, streaks      | No predictive/semantic analytics    |
| Observability         | Full (Prometheus)    | -                                   |
| Real-time Updates     | SignalR              | No conversational AI                |
| AI/Cognitive          | None                 | No LLM, no ML, no NLU               |

---

## 6. Conclusion

This project is a robust, scalable, and observable backend for an AI-powered coaching platform.  
It is **not yet a cognitive system**—but is architected to allow future integration of AI/ML and cognitive services as business needs and resources allow.

---

## 7. Where Should Grafana Dashboard Files Live?

**Best Practice:**  
- Place operational/infrastructure files (like Grafana dashboards, Prometheus configs, docker-compose, etc.) in a dedicated folder at the root of your repository, such as `/ops`, `/infrastructure`, or `/Monitoring`.
- This keeps documentation (`Doc/`) focused on design, guides, and API docs, while operational assets are grouped for deployment and automation.

**Recommendation:**  
- Move Grafana dashboard JSON and related monitoring configs from `Doc/` to a new folder, e.g., `monitoring/` or `infrastructure/`.
- Update documentation and deployment scripts to reference the new location.

---
