# API Testing Guide

This guide shows how to run the WebAPI, access Swagger UI, and test the main API endpoints (authentication, tasks, goals, metrics). It includes copyable examples for PowerShell, curl, Postman/VS Code REST client, and troubleshooting tips.

## Prerequisites

- .NET SDK 8+ installed (you used .NET 8 for this project)
- The repo checked out locally
- A running SQL Server instance if you want persistence (the sample uses local connection in `WebAPI/appsettings.json` by default)
- (Optional) OpenAI API key to test AI endpoints

## Files & URLs

- Server project: `WebAPI`
- Swagger UI (development): `http://localhost:5129` (launch settings)
- Swagger JSON: `http://localhost:5129/swagger/v1/swagger.json`
- Metrics: `http://localhost:5129/metrics`

> If your launch settings use different ports, replace `5129` with the port from `WebAPI/Properties/launchSettings.json` or your environment configuration.

---

## Quick step-by-step test (recommended)

If you just want to verify the app is working end-to-end, follow these exact steps (copy/paste):

1. Build the solution:

```powershell
dotnet build "Back-End AI Task Coach.sln"
```

2. Start the WebAPI (in a dedicated terminal):

```powershell
dotnet run --project .\WebAPI
```

3. Wait for the API to be ready (open a second terminal and run):

```powershell
for ($i=0; $i -lt 30; $i++) { try { if ((Invoke-WebRequest -UseBasicParsing http://localhost:5129/metrics).StatusCode -eq 200) { Write-Host "ready"; break } } catch { Start-Sleep -Seconds 1 } }
```

4. Open Swagger to inspect endpoints:

Open http://localhost:5129 in a browser.

5. Run the built-in smoke test (PowerShell, recommended):

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1 -Cleanup
```

Or run the cross-platform bash variant (WSL / Git Bash / Linux):

```bash
chmod +x ./scripts/smoke-test.sh
./scripts/smoke-test.sh CLEANUP=true
```

6. (Optional) Run the Postman collection via Newman:

```bash
cd postman
npm ci
npm run test:postman
```

7. Run the .NET tests locally (unit + integration):

```powershell
dotnet test
```

8. Confirm the created resources: use the `GET /api/tasks/user/{userId}` endpoint or check the in-memory/SQL DB depending on your configuration.

If any step fails, consult the Troubleshooting section below (search for "Troubleshooting").


## 1) Start the API (PowerShell)

Open PowerShell in the repository root and run:

```powershell
# Change to project folder (optional if you already ran dotnet commands from repo root)
Set-Location "c:\Users\madoc\Downloads\Back-End-AI-Task-Coach-master (1)\Back-End-AI-Task-Coach-master\WebAPI"

# Run the WebAPI project (uses launchSettings by default)
dotnet run --project .
```

Notes:
- If a port is in use, kill the previous process or choose another port (see Troubleshooting).
- The server logs show the "Now listening on: http://localhost:5129" message when started.

---

## 2) Open Swagger UI

Open a browser and navigate to:

```
http://localhost:5129
```

Swagger will load in Development environment (the project conditionally enables Swagger when `ASPNETCORE_ENVIRONMENT` is Development). In Swagger you can: 
- Browse endpoints, models and request/response examples
- Use the `Authorize` button to add a `Bearer` token for protected endpoints

---

## 3) Quick smoke tests (PowerShell)

These use `Invoke-RestMethod` (PowerShell) which returns parsed JSON objects.

### GET /api/test

```powershell
Invoke-RestMethod -Uri "http://localhost:5129/api/test" -Method Get
```

Expected: 200 OK JSON with `success: true` and a `version` field.

### GET /api/tasks (public index route)

```powershell
Invoke-RestMethod -Uri "http://localhost:5129/api/tasks" -Method Get
```

Expected: 200 OK with a small JSON payload or message like "Tasks API root." (controller may be `AllowAnonymous` for Index)

### GET /api/goals

```powershell
Invoke-RestMethod -Uri "http://localhost:5129/api/goals" -Method Get
```

Expected: 200 OK and a JSON message from the `GoalsController.Index` route.

### GET /metrics

```powershell
Invoke-RestMethod -Uri "http://localhost:5129/metrics" -Method Get
```

Expected: plaintext Prometheus-formatted metrics (Content-Type: text/plain).

---

## 4) Authentication flow (register / login / use token)

### Register a user (example)

```powershell
$body = @{
    email = "testuser@example.com"
    password = "Password123!"
    name = "Test User"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5129/api/auth/register" -Method Post -Body $body -ContentType 'application/json'
```

Expected: 201 or 200 with user data.

### Login and get JWT

```powershell
$login = @{
    email = "testuser@example.com"
    password = "Password123!"
} | ConvertTo-Json

$resp = Invoke-RestMethod -Uri "http://localhost:5129/api/auth/login" -Method Post -Body $login -ContentType 'application/json'

# Assuming response has a token property
$token = $resp.token
```

### Call a protected endpoint with token

```powershell
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5129/api/tasks" -Method Get -Headers $headers
```

Or call a POST endpoint that requires authentication (example creating a task):

```powershell
$task = @{
    title = "My sample task"
    description = "Test created from API guide"
    estimatedHours = 1.5
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5129/api/tasks" -Method Post -Headers $headers -Body $task -ContentType 'application/json'
```

> Replace request body fields with DTO fields used in your project. Check Swagger UI for exact contract.

---

## 5) curl examples (cross-platform)

```bash
# GET test
curl http://localhost:5129/api/test

# Register
curl -X POST http://localhost:5129/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"testuser@example.com","password":"Password123!","name":"Test User"}'

# Login
curl -X POST http://localhost:5129/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"testuser@example.com","password":"Password123!"}'

# Using token
curl -H "Authorization: Bearer <YOUR_TOKEN>" http://localhost:5129/api/tasks
```

---

## 6) VS Code REST Client (`.http` file) example

Create a file `requests.http` and paste:

```
### Get test
GET http://localhost:5129/api/test

### Register
POST http://localhost:5129/api/auth/register
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Password123!",
  "name": "Test User"
}

### Login
POST http://localhost:5129/api/auth/login
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Password123!"
}
```

Open the file in VS Code and use the `Send Request` links that appear above each request.

---

## 7) Postman / Importing Swagger

- Open Postman → Import → Paste Link
- Enter `http://localhost:5129/swagger/v1/swagger.json` to import all endpoints and models
- Use the `Authorization` tab to set `Bearer <token>` for protected requests

---

## 8) Running unit and integration tests

From repository root run:

```powershell
# Run all tests in solution
dotnet test
```

You should see a test summary (in this repo there are 43 tests that should all pass).

---

## 9) Troubleshooting

- "Unable to render this definition" in Swagger UI:
  - Hard-refresh the browser (Ctrl+F5) or open in an incognito window to bypass cache
  - Confirm `swagger.json` is valid by visiting `http://localhost:5129/swagger/v1/swagger.json`

- Port already in use / AddressInUseException:
  - Find the process using the port: `netstat -ano | findstr :5129`
  - Kill it: `taskkill /PID <pid> /F`

- File locked during build (Infrastructure.dll locked): stop running dotnet processes that are using the dll, then rebuild. If locked repeatedly, reboot or use Process Explorer to track handle owners.

- Https redirection warnings: if `ASPNETCORE_URLS` or launch settings don't expose an https port, Kestrel logs a warning—this is safe in local dev unless you specifically need HTTPS.

- Authentication/Authorization failures: ensure you used the token returned from `/api/auth/login` and passed it as `Authorization: Bearer <token>`.

---

## 10) Environment variables & production notes

Set production secrets via environment variables (PowerShell examples):

```powershell
# Set for current PowerShell session only
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=MY\SQLEXPRESS;Database=AITaskFlowCoach;Trusted_Connection=True;"
$env:Jwt__Key = "<your_secret_key_here>"
$env:OpenAI__ApiKey = "<your_openai_key>"

# Then run
dotnet run --project WebAPI
```

> In production, use a secret manager (Azure Key Vault, AWS Secrets Manager, etc.) instead of plain environment variables.

---

## 11) Quick checklist before running feature tests

- Start the API: `dotnet run --project WebAPI`
- Open Swagger: `http://localhost:5129`
- Register user → login → copy token
- Use token for protected endpoints
- Check metrics at `/metrics`

---

## Sample DTO request bodies (concrete examples)

Below are copyable examples matching the project's DTOs (`CreateTaskDto`, `CreateGoalDto`, `CreateHabitDto`). Use them in Swagger, Postman, or curl.

#### CreateTaskDto example
```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "title": "Implement API testing guide",
  "description": "Write tests and documentation for the API",
  "status": 0,
  "priority": 1,
  "estimatedHours": 2.0,
  "dueDate": null,
  "tags": ["docs","testing"],
  "dependencies": null,
  "energyLevel": 3,
  "focusTimeMinutes": 30,
  "goalId": null
}
```

#### CreateGoalDto example
```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "title": "Ship MVP",
  "description": "Deliver the minimum viable product",
  "category": 0,
  "status": 0,
  "priority": 2,
  "targetDate": null,
  "createdAt": "2025-10-06T00:00:00Z",
  "completedAt": null,
  "progress": 0.0
}
```

#### CreateHabitDto example
```json
{
  "name": "Daily Pushups",
  "description": "Do 30 pushups every morning",
  "frequency": 2,
  "preferredTime": null,
  "targetCount": 30,
  "unit": "reps",
  "color": "#FF5733",
  "icon": "fitness",
  "motivation": "Stay healthy",
  "category": 0,
  "triggers": ["morning"],
  "rewards": ["feeling energetic"],
  "difficultyLevel": 2,
  "environmentFactors": null
}
```

### Import Postman collection

- Open Postman → Import → File → choose `Doc/Postman_Collection.json` in this repository.
- After import, update the collection variable `jwt` with a token obtained from `/api/auth/login`.
- Run requests in sequence or use the Postman Runner.

### Use the VS Code Task to start the server and open Swagger

- Open Command Palette (Ctrl+Shift+P) → `Tasks: Run Task` → `Run WebAPI and Open Swagger`.
- The first task starts the server in a dedicated terminal; the second launches your default browser at `http://localhost:5129`.

---

## Where this file lives

`Doc/API_Testing_Guide.md` — open this in your editor for quick copy/paste examples.

---

If you'd like, I can:
- Generate a Postman collection file for direct import
- Create a VS Code task that starts the server and opens the browser automatically

Which of these would you like next?