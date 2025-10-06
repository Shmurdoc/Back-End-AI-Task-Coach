# Deployment Guide

This document explains step-by-step how to deploy the AI Task Coach WebAPI to a production-like environment (Linux server or container). It focuses on a reliable, repeatable setup and highlights common pitfalls to watch for.

## Options covered
- Local production run (Linux/Windows service)
- Docker Compose (recommended for portability)
- Kubernetes (notes)

---

## Prerequisites
- .NET 8 runtime installed on the host (if running without Docker)
- SQL Server instance (managed or container) for persistent data
- Redis (optional but recommended for caching)
- SMTP / Twilio credentials for notifications (if used)
- Secrets stored in a secret manager (recommended)

---

## 1) Prepare configuration

Recommended structure: use environment variables for secrets and connection strings. Example env variables:

```bash
# Database
CONNECTIONSTRINGS__DEFAULTCONNECTION="Server=mydbhost;Database=AITaskCoach;User Id=sa;Password=YourStrong!Pass;TrustServerCertificate=True;"

# Redis (optional)
REDIS__CONFIG="redis:6379"

# JWT
JWT__KEY="<a-long-random-secret>"
JWT__ISSUER="AITaskCoach"
JWT__AUDIENCE="AITaskCoach_Users"

# OpenAI (if used)
OPENAI__APIKEY="sk-..."
```

In Docker Compose use `environment:` or an `.env` file. In Kubernetes use `Secrets` and `ConfigMaps`.

---

## 2) Docker Compose (recommended quick path)

Create or update `docker-compose.yml` with services: sql-server, redis (optional), webapi. A sample skeleton is already provided in the repo; adapt it.

Start services:

```bash
docker-compose up -d --build
```

Check logs:

```bash
docker-compose logs -f webapi
```

Common issues:
- SQL Server initialization takes time; ensure migrations run after SQL server is ready.
- Ports: adjust to avoid conflicts (80/443 in use).

---

## 3) Running without Docker (systemd service on Linux)

1. Publish the app:

```bash
dotnet publish WebAPI -c Release -o /var/www/aitaskcoach
```

2. Create systemd service file `/etc/systemd/system/aitaskcoach.service`:

```
[Unit]
Description=AI Task Coach WebAPI
After=network.target

[Service]
WorkingDirectory=/var/www/aitaskcoach
ExecStart=/usr/bin/dotnet /var/www/aitaskcoach/WebAPI.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ConnectionStrings__DefaultConnection="Server=..."
Environment=Jwt__Key="..."
# Add other env entries as needed

[Install]
WantedBy=multi-user.target
```

3. Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable aitaskcoach
sudo systemctl start aitaskcoach
sudo journalctl -u aitaskcoach -f
```

Watch the logs for Kestrel start and DB connectivity.

---

## 4) Database migrations

From the repo root or from a CI job run EF Core migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

If using Docker, run a migration container or ensure the webapi runs a migration step on startup (careful with multi-replica race conditions).

---

## 5) HTTPS / Certificates

- Terminate TLS at a reverse proxy (NGINX, Traefik) or use Kestrel with certificates.
- Prefer using a reverse proxy (NGINX) to handle TLS and static routing.

Sample NGINX snippet (proxy to Kestrel running on 5000):

```
server {
  listen 80;
  server_name api.example.com;
  return 301 https://$host$request_uri;
}

server {
  listen 443 ssl;
  server_name api.example.com;

  ssl_certificate /etc/letsencrypt/live/api.example.com/fullchain.pem;
  ssl_certificate_key /etc/letsencrypt/live/api.example.com/privkey.pem;

  location / {
    proxy_pass http://localhost:5000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection keep-alive;
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
  }
}
```

---

## 6) Prometheus metrics and observability

- Expose `/metrics` and configure Prometheus to scrape the endpoint.
- Ensure metrics endpoint is not publicly accessible if it reveals sensitive info; use internal network or auth/proxy rules.

---

## 7) Health checks

- Add Kubernetes `livenessProbe` and `readinessProbe` against `/health` or `/health/ready`.
- Ensure background services gracefully start and don't block readiness if they need time to initialize.

---

## 8) Running in Kubernetes (notes)

- Use an initContainer to run EF migrations before the app starts, or run migrations as a separate job.
- Use `Deployment` with `replicas: 2+` and a `Service` object for load balancing.
- Use `ConfigMaps` and `Secrets` for configuration and secrets.

---

## 9) CI/CD recommendations

- Build and publish the app in the CI pipeline and push artifacts to a container registry.
- Run `dotnet test` as part of CI.
- Run smoke-tests (the workflow added in `.github/workflows/smoke-test.yml`).

---

## 10) Post-deployment checks and what to look out for

1. Logs: look for DB connectivity errors, authentication failures, or unhandled exceptions.
2. Migrations: confirm database schema version and tables exist.
3. Background workers: verify background jobs start and do not fail repeatedly.
4. Performance: enable APM (Application Performance Monitoring) if available; watch for slow DB queries.
5. Metrics: ensure `/metrics` is scraped by Prometheus and dashboards populate.
6. Secrets: verify that secrets are not checked into source control and are loaded correctly in the environment.

---

## Troubleshooting common failures

- "UserId cannot be empty" when creating resources: ensure the client includes the token-derived user id or the request body includes a valid `userId` where required.
- Port conflicts: change ports or stop other processes.
- Locked files during deployment: ensure no old process is running and holding DLLs; stop services before replacing files.
- SSL certificate errors: verify certificates paths and permissions.

---

## Example: minimal Docker Compose snippet

```yaml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong!Pass"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
  webapi:
    build: .
    depends_on:
      - sqlserver
    environment:
      ConnectionStrings__DefaultConnection: "Server=sqlserver,1433;Database=AITaskCoach;User Id=sa;Password=YourStrong!Pass;TrustServerCertificate=True;"
      Jwt__Key: "<your-secret>"
    ports:
      - "5129:80"
```

---

If you want, I can:
- Create a ready-to-use `docker-compose.prod.yml` with migrations and health checks.
- Create a GitHub Actions CD workflow that builds a Docker image and pushes it to GHCR, Docker Hub, or your container registry.

Tell me which of those you'd like next and I'll implement it.
