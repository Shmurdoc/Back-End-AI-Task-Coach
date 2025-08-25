# My API Reference

Here’s a quick overview of the main API endpoints I’ve built for this project. For the full, always-up-to-date details, check out the Swagger/OpenAPI docs at `/swagger` when you run the WebAPI locally.

## Authentication
- (Coming soon) Most endpoints will require JWT/OAuth2 authentication.

## Endpoints

### Habits
- `GET /api/habits` – Returns all habits
- `GET /api/habits/{id}` – Returns a specific habit by ID
- `POST /api/habits` – Lets you create a new habit
- `PUT /api/habits/{id}` – Update an existing habit
- `DELETE /api/habits/{id}` – Delete a habit

### Goals
- `GET /api/goals` – Returns all goals
- `GET /api/goals/{id}` – Returns a specific goal by ID
- `POST /api/goals` – Create a new goal
- `PUT /api/goals/{id}` – Update a goal
- `DELETE /api/goals/{id}` – Delete a goal

### Users
- `GET /api/users/{id}` – Get user details by ID
- (More user endpoints will be added as I build out user management)

### Notifications
- `POST /api/notifications/email` – Send an email notification
- `POST /api/notifications/sms` – Send an SMS notification

### Analytics
- `GET /api/analytics/summary` – Get a productivity summary
- `GET /api/analytics/weekly-report` – Get a weekly report

### Metrics
- `GET /metrics` – Prometheus metrics endpoint for monitoring

## Error Handling
- I use standard HTTP status codes
- Error responses include a message and error code for clarity

## Notes
- Endpoints and features will evolve as I keep building
- For known issues or limitations, see `Issues.md`
