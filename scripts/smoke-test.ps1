param(
    [string]$BaseUrl = 'http://localhost:5129',
    [switch]$Cleanup
)

function Fail([string]$msg, [int]$code = 1) {
    Write-Error $msg
    exit $code
}

Write-Host "Starting smoke test against $BaseUrl"

$suffix = [DateTime]::UtcNow.Ticks
$email = "smoketest+$suffix@example.com"
$password = 'Sm0keTest!'

Write-Host "Registering user: $email"
try {
    $regBody = @{ email = $email; password = $password; name = 'Smoke Tester' } | ConvertTo-Json
    $reg = Invoke-RestMethod -Uri "$BaseUrl/api/auth/register" -Method Post -Body $regBody -ContentType 'application/json'
} catch {
    Fail "Registration failed: $($_.Exception.Message)"
}

if (-not $reg.success) { Fail "Registration returned success=false: $($reg | ConvertTo-Json -Compress)" }

$token = $reg.token
$userId = $reg.user.id
Write-Host "Registered user id: $userId"

Write-Host "Logging in to verify token"
try {
    $loginBody = @{ email = $email; password = $password } | ConvertTo-Json
    $login = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType 'application/json'
} catch {
    Fail "Login failed: $($_.Exception.Message)"
}

if (-not $login.success) { Fail "Login returned success=false: $($login | ConvertTo-Json -Compress)" }

$token = $login.token
Write-Host "Login succeeded, token length: $($token.Length)"

Write-Host "Creating a Task for user $userId"
try {
    $taskBody = @{
        userId = [guid]$userId
        title = "Smoke Test Task"
        description = "Created by smoke-test.ps1"
        status = 0
        priority = 1
        estimatedHours = 1.0
        dueDate = $null
        tags = @('smoke')
        dependencies = $null
        energyLevel = 3
        focusTimeMinutes = 30
        goalId = $null
    } | ConvertTo-Json -Depth 4

    $headers = @{ Authorization = "Bearer $token" }
    $created = Invoke-RestMethod -Uri "$BaseUrl/api/tasks" -Method Post -Body $taskBody -ContentType 'application/json' -Headers $headers
} catch {
    Fail "Create task failed: $($_.Exception.Message)"
}

if (-not $created.success) { Fail "Create task returned success=false: $($created | ConvertTo-Json -Compress)" }

$taskId = $created.data.id
Write-Host "Task created: $taskId"

if ($Cleanup) {
    Write-Host "Cleaning up: deleting created task $taskId"
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/tasks/$taskId" -Method Delete -Headers @{ Authorization = "Bearer $token" }
        Write-Host "Deleted task $taskId"
    } catch {
        Write-Warning ([string]::Format('Failed to delete task {0}: {1}', $taskId, $_.Exception.Message))
    }
}

$summary = @{ success = $true; user = @{ id = $userId; email = $email }; task = @{ id = $taskId; title = $created.data.title } }
Write-Host "Smoke test completed successfully"
Write-Output ($summary | ConvertTo-Json -Depth 4)
exit 0
