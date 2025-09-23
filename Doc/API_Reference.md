# AI Task Coach - API Reference

Complete API documentation for the AI Task Coach system. For interactive testing and full schema details, visit the Swagger UI at `/swagger` when running the application locally.

## Base URL
- **Local Development**: `http://localhost:5000`
- **Production**: `https://your-domain.com` (TBD)

## Authentication
- 🔄 **JWT/OAuth2**: Authentication system planned for production deployment
- 🔓 **Current State**: Endpoints accessible without authentication for development

## Core API Endpoints

### 📋 Tasks Management
- `GET /api/tasks` – Retrieve all tasks for the authenticated user
- `GET /api/tasks/{id}` – Get specific task details by ID
- `POST /api/tasks` – Create a new task with AI suggestions
- `PUT /api/tasks/{id}` – Update existing task
- `DELETE /api/tasks/{id}` – Delete a task
- `POST /api/tasks/{id}/complete` – Mark task as completed (triggers gamification)
- `GET /api/tasks/analytics` – Get task completion analytics

### 🎯 Goals Management
- `GET /api/goals` – Get all user goals with progress tracking
- `GET /api/goals/{id}` – Get specific goal details
- `POST /api/goals` – Create a new goal
- `PUT /api/goals/{id}` – Update goal progress and details
- `DELETE /api/goals/{id}` – Delete a goal
- `GET /api/goals/{id}/tasks` – Get all tasks associated with a goal

### 🔄 Habits Tracking
- `GET /api/habits` – Get all user habits with streak information
- `GET /api/habits/{id}` – Get specific habit details
- `POST /api/habits` – Create a new habit
- `PUT /api/habits/{id}` – Update habit configuration
- `DELETE /api/habits/{id}` – Delete a habit
- `POST /api/habits/{id}/check-in` – Record habit completion for today

### 👤 User Management
- `GET /api/users/{id}` – Get user profile and preferences
- `PUT /api/users/{id}` – Update user profile
- `GET /api/users/{id}/analytics` – Get comprehensive user analytics
- `GET /api/users/{id}/productivity-summary` – Get productivity insights

### 📧 Notifications
- `POST /api/notifications/email` – Send email notification
- `POST /api/notifications/sms` – Send SMS notification
- `GET /api/notifications/preferences/{userId}` – Get notification preferences
- `PUT /api/notifications/preferences/{userId}` – Update notification settings

## 🎮 Gamification Endpoints (New)

### Points & Scoring
- `GET /api/gamification/points/{userId}` – Get user's total points and recent activity
- `POST /api/gamification/award-points` – Award points for completed actions
- `GET /api/gamification/points/{userId}/history` – Get detailed points history

### Badges & Achievements
- `GET /api/gamification/badges/{userId}` – Get user's badge collection
- `GET /api/gamification/achievements/{userId}` – Get user's achievements with progress
- `POST /api/gamification/achievements/check/{userId}` – Trigger achievement check

### Leaderboards
- `GET /api/gamification/leaderboard` – Get global leaderboard (weekly)
- `GET /api/gamification/leaderboard/all-time` – Get all-time rankings
- `GET /api/gamification/leaderboard/{userId}/rank` – Get specific user's rank

### Challenges
- `GET /api/gamification/challenges/{userId}` – Get active challenges for user
- `GET /api/gamification/challenges/available` – Get available challenges to join
- `POST /api/gamification/challenges/{challengeId}/join` – Join a challenge
- `POST /api/gamification/challenges/{challengeId}/complete` – Complete a challenge

### Analytics & Insights
- `GET /api/gamification/insights/{userId}` – Get AI-powered productivity insights
- `GET /api/gamification/streak/{userId}` – Get current habit/task streaks
- `GET /api/gamification/motivation/{userId}` – Get personalized motivational message

## 📊 Analytics Endpoints

### Productivity Analytics
- `GET /api/analytics/productivity/{userId}` – Comprehensive productivity metrics
- `GET /api/analytics/time-tracking/{userId}` – Time usage analysis
- `GET /api/analytics/patterns/{userId}` – Behavioral pattern analysis
- `GET /api/analytics/weekly-report/{userId}` – Weekly productivity report

### Habit Analytics
- `GET /api/habit-analytics/streaks/{userId}` – Detailed streak analysis
- `GET /api/habit-analytics/completion-rates/{userId}` – Habit completion statistics
- `GET /api/habit-analytics/trends/{userId}` – Long-term habit trends

## 📅 Calendar Integration

### Calendar Export
- `GET /api/calendar/daily/{userId}` – Export daily schedule as ICS
- `GET /api/calendar/weekly/{userId}` – Export weekly schedule as ICS
- `POST /api/calendar/sync/{userId}` – Sync with external calendar services

## 🔧 System & Health Endpoints

### Health Checks
- `GET /health` – Basic health check
- `GET /health/ready` – Readiness probe for container orchestration
- `GET /health/live` – Liveness probe for monitoring

### Metrics
- `GET /metrics` – Prometheus metrics endpoint
- `GET /api/system/status` – System status and version information

## Request/Response Examples

### Create Task Example
```json
POST /api/tasks
{
  "title": "Complete project documentation",
  "description": "Update all markdown files with latest changes",
  "priority": 2,
  "estimatedHours": 3.0,
  "goalId": "123e4567-e89b-12d3-a456-426614174000",
  "tags": ["documentation", "project"],
  "energyLevel": 4
}
```

### Response
```json
{
  "id": "987e6543-e21b-12d3-a456-426614174111",
  "title": "Complete project documentation",
  "status": "Todo",
  "createdAt": "2025-09-05T10:30:00Z",
  "aiSuggestions": "Consider breaking this into smaller tasks for better focus",
  "points": 0
}
```

### Get User Badges Example
```json
GET /api/gamification/badges/{userId}

Response:
{
  "badges": [
    {
      "id": "task-master",
      "name": "Task Master",
      "description": "Complete 100 tasks",
      "rarity": "Epic",
      "awardedAt": "2025-09-01T14:20:00Z",
      "iconUrl": "/badges/task-master.png"
    }
  ],
  "totalBadges": 12,
  "rareCount": 3,
  "epicCount": 1,
  "legendaryCount": 0
}
```

## Error Responses

All endpoints return standardized error responses:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "title",
        "message": "Title is required"
      }
    ],
    "timestamp": "2025-09-05T10:30:00Z"
  }
}
```

## Rate Limiting
- **Development**: No rate limiting
- **Production**: 1000 requests per hour per user (planned)

## WebSocket/SignalR Endpoints

### Real-time Notifications
- `/hubs/notifications` – Real-time notifications for task updates, achievements, etc.
- `/hubs/gamification` – Live updates for points, badges, and leaderboard changes

## Development Notes

### Testing
- Use Swagger UI at `/swagger` for interactive API testing
- All endpoints support JSON request/response format
- CORS enabled for local development (localhost:3000, localhost:5000)

### Response Codes
- `200` - Success
- `201` - Created
- `400` - Bad Request (validation errors)
- `404` - Not Found
- `500` - Internal Server Error

---
*Last Updated: September 5, 2025*
*API Version: 2.0 (Gamification Release)*
