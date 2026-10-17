# Security Policy

## Supported versions

This project is actively maintained on the **`master`** branch. Fixes land there; there are no
maintained release branches.

## Reporting a vulnerability

Email **Madoc Mhlongo, madocmhlongo05@gmail.com** with:

- A description of the issue and its impact
- Steps to reproduce (request/response, or a short PoC)
- The affected endpoint/component

Please do not open a public issue for security reports. You can expect an acknowledgement within
a few days. This is a personal portfolio project, so fixes are best-effort but taken seriously.

## What is implemented

| Control | Detail |
|---------|--------|
| Password storage | PBKDF2 / SHA-256, 100,000 iterations, 16-byte random salt |
| Access tokens | JWT, default 120-minute lifetime, standard issuer/audience validation |
| Refresh tokens | Opaque, stored SHA-256 hashed; rotated on every use; reuse revokes all active tokens for the user |
| Account access | `EnsureSelfUser` filter rejects cross-user `userId` references with 403; entity ownership enforced in services (404) |
| Secrets | No secrets committed. Dev key lives in `appsettings.Development.json`; production config uses placeholders that **fail startup** until real secrets are provided via environment variables or a secret manager |
| Transport (dev) | HTTP on localhost; TLS is expected to be terminated by a proxy/ingress in any real deployment |
| Containers | Multi-stage Dockerfile; runtime runs as non-root `user app` |
| Scanning | CI runs Trivy filesystem scanning with results uploaded to GitHub code scanning |
| Error hygiene | Global exception middleware returns a generic 500 message; no stack traces or internal details in responses |

## Out of scope / known gaps

- No rate limiting (see `ProductionReadiness.md` for the prioritized gap list)
- No MFA, email verification, or password reset flows
- `/metrics` and health endpoints are anonymous by design for the local stack
