## Smoke test script

This repository includes a PowerShell smoke-test script that performs a simple end-to-end flow against the running WebAPI:

- Register a new user
- Login and obtain a JWT
- Create a Task using the JWT
- (optional) Delete the created Task

File: `scripts/smoke-test.ps1`

Usage examples (PowerShell):

Run against the default localhost URL:

```powershell
.\scripts\smoke-test.ps1
```

Run against a custom base URL:

```powershell
.\scripts\smoke-test.ps1 -BaseUrl 'http://localhost:5129'
```

Run and delete the created task afterwards:

```powershell
.\scripts\smoke-test.ps1 -Cleanup
```

Exit codes:
- 0: success
- non-zero: failure (logged error message printed)

Notes:
- The script uses `Invoke-RestMethod` and requires PowerShell (Windows PowerShell or PowerShell Core).
- Ensure the WebAPI is running (default: http://localhost:5129) before running the script.
