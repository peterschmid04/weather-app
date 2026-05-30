param(
    [switch]$ValidateOnly,
    [switch]$WithNgrok,
    [switch]$NoPause
)

# Windows/PowerShell setup script.
# It starts directly when an existing .env is present, or creates a new .env
# from the minimum required user inputs and generated local passwords.
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path $RepoRoot

$EnvPath = Join-Path $RepoRoot ".env"
$EnvTempPath = Join-Path $RepoRoot ".env.tmp"

function Write-Info {
    param([string]$Message)
    Write-Host "[weather-app] $Message"
}

function Stop-Script {
    param([int]$Code = 0)

    if (-not $NoPause) {
        Write-Host ""
        Read-Host "Fertig. Enter druecken zum Schliessen"
    }

    exit $Code
}

trap {
    Write-Host ""
    Write-Host "Das Skript wurde mit Fehler beendet:"
    Write-Host $_.Exception.Message
    Stop-Script 1
}

function Read-PlainValue {
    param(
        [string]$Label,
        [string]$Default = "",
        [bool]$Required = $true
    )

    while ($true) {
        $prompt = if ($Default) { "$Label [$Default]" } else { $Label }
        $value = Read-Host $prompt
        if ([string]::IsNullOrWhiteSpace($value)) {
            $value = $Default
        }

        if (-not $Required -or -not [string]::IsNullOrWhiteSpace($value)) {
            return $value.Trim()
        }

        Write-Host "This value is required."
    }
}

function New-SafePassword {
    $bytes = New-Object byte[] 24
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [Convert]::ToBase64String($bytes).Replace("+", "A").Replace("/", "B").Replace("=", "C")
}

function Read-DotEnv {
    param([string]$Path)

    $values = @{}
    if (-not (Test-Path -LiteralPath $Path)) {
        return $values
    }

    foreach ($line in Get-Content -LiteralPath $Path) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith("#")) {
            continue
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -lt 1) {
            continue
        }

        $name = $line.Substring(0, $separatorIndex).Trim()
        $value = $line.Substring($separatorIndex + 1).Trim()
        $values[$name] = $value
    }

    return $values
}

function Test-EnvKey {
    param($Values, [string]$Name)

    if ($Values -is [hashtable]) {
        return $Values.ContainsKey($Name)
    }

    return $Values.Contains($Name)
}

function Get-EnvValue {
    param($Values, [string]$Name)

    if (Test-EnvKey $Values $Name) {
        return [string]$Values[$Name]
    }

    return ""
}

function Assert-EnvValue {
    param(
        [string]$Name,
        [string]$Value
    )

    if ($Value -match "[`r`n]") {
        throw "$Name must not contain line breaks."
    }
}

function Test-DockerReady {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "Docker is not installed or not available in PATH."
    }

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & docker compose version *> $null
        $composeVersionExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($composeVersionExitCode -ne 0) {
        throw "docker compose is not available. Please install Docker Desktop with Compose v2."
    }
}

function Invoke-DockerCompose {
    param([string[]]$Arguments)

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & docker compose @Arguments
        $composeExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($composeExitCode -ne 0) {
        throw "docker compose $($Arguments -join ' ') failed with exit code $composeExitCode."
    }
}

function Test-Ports {
    $ports = @(3000, 5122, 5432, 5050)
    foreach ($port in $ports) {
        if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
            $listener = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($listener) {
                Write-Info "Port $port is already in use. If this is the Weather App stack, docker compose up will reuse it."
            }
        }
    }
}

function Wait-Http {
    param(
        [string]$Name,
        [string]$Url,
        [int]$Seconds = 120
    )

    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri $Url -TimeoutSec 5
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                Write-Info "$Name is reachable."
                return
            }
        }
        catch {
            Start-Sleep -Seconds 3
        }
    }

    Write-Info "$Name was not reachable within $Seconds seconds. Check docker compose logs."
}

function Write-EnvFile {
    param($Values)

    if (Test-Path -LiteralPath $EnvTempPath) {
        Remove-Item -LiteralPath $EnvTempPath -Force
    }

    $envLines = @(
        "# Local secrets for Weather App. Do not commit this file.",
        "POSTGRES_DB=$($Values.POSTGRES_DB)",
        "POSTGRES_USER=$($Values.POSTGRES_USER)",
        "POSTGRES_PASSWORD=$($Values.POSTGRES_PASSWORD)",
        "",
        "PGADMIN_DEFAULT_EMAIL=$($Values.PGADMIN_DEFAULT_EMAIL)",
        "PGADMIN_DEFAULT_PASSWORD=$($Values.PGADMIN_DEFAULT_PASSWORD)",
        "",
        "AUTH0_DOMAIN=$($Values.AUTH0_DOMAIN)",
        "AUTH0_AUDIENCE=$($Values.AUTH0_AUDIENCE)",
        "AUTH0_CLIENT_ID=$($Values.AUTH0_CLIENT_ID)",
        "AUTH0_SCOPE=$($Values.AUTH0_SCOPE)",
        "",
        "AUTH0_CONNECTION_DATABASE=$($Values.AUTH0_CONNECTION_DATABASE)",
        "AUTH0_CONNECTION_GOOGLE=$($Values.AUTH0_CONNECTION_GOOGLE)",
        "AUTH0_CONNECTION_APPLE=$($Values.AUTH0_CONNECTION_APPLE)",
        "AUTH0_CONNECTION_FACEBOOK=$($Values.AUTH0_CONNECTION_FACEBOOK)",
        "AUTH0_CONNECTION_GITHUB=$($Values.AUTH0_CONNECTION_GITHUB)",
        "",
        "OPENWEATHERMAP_API_KEY=$($Values.OPENWEATHERMAP_API_KEY)",
        "",
        "LOGS=$($Values.LOGS)",
        "LOG_DIRECTORY=$($Values.LOG_DIRECTORY)",
        "",
        "COMPOSE_PROFILES=$($Values.COMPOSE_PROFILES)",
        "NGROK_AUTHTOKEN=$($Values.NGROK_AUTHTOKEN)",
        "NGROK_URL=$($Values.NGROK_URL)"
    )

    Set-Content -LiteralPath $EnvTempPath -Value $envLines -Encoding UTF8

    if (Test-Path -LiteralPath $EnvPath) {
        $backupPath = Join-Path $RepoRoot (".env.backup." + (Get-Date -Format "yyyyMMdd-HHmmss"))
        Copy-Item -LiteralPath $EnvPath -Destination $backupPath
        Write-Info "Existing .env backed up to $backupPath."
    }

    Move-Item -LiteralPath $EnvTempPath -Destination $EnvPath -Force
}

function Start-Stack {
    Test-DockerReady
    Test-Ports

    $envValues = Read-DotEnv -Path $EnvPath
    $ngrokToken = Get-EnvValue $envValues "NGROK_AUTHTOKEN"
    $ngrokUrl = Get-EnvValue $envValues "NGROK_URL"

    Write-Info "Starting database and backend first."
    Invoke-DockerCompose -Arguments @("up", "-d", "db", "backend")
    Wait-Http -Name "Backend" -Url "http://localhost:5122/swagger/v1/swagger.json"

    Write-Info "Starting frontend and pgAdmin."
    Invoke-DockerCompose -Arguments @("up", "-d", "frontend", "pgadmin")
    Wait-Http -Name "Frontend" -Url "http://localhost:3000"

    if (-not [string]::IsNullOrWhiteSpace($ngrokToken) -and -not [string]::IsNullOrWhiteSpace($ngrokUrl)) {
        Write-Info "Starting optional ngrok tunnel."
        try {
            Invoke-DockerCompose -Arguments @("--profile", "ngrok", "up", "-d", "ngrok")
        }
        catch {
            Write-Info "ngrok could not be started. The local app still runs on http://localhost:3000."
            Write-Info $_.Exception.Message
        }
    }
}

if (Test-Path -LiteralPath $EnvPath) {
    if ($ValidateOnly) {
        Write-Info ".env found. No strict value validation is performed; it will not be rewritten."
        Stop-Script 0
    }

    Write-Info ".env found. Starting Docker Compose without asking for values."
    Start-Stack
    Stop-Script 0
}

Write-Host ""
Write-Host "No .env found. Enter the required Auth0 and OpenWeatherMap values."
Write-Host "PostgreSQL database/user and Auth0 connection names are filled automatically."
Write-Host "AUTH0_AUDIENCE is fixed to https://weather-api and is not asked here."
Write-Host "For local passwords, press Enter to accept the generated password or enter your own."
Write-Host "Auth0 social provider client secrets stay in the Auth0 Dashboard, not here."
Write-Host ""

$postgresDb = "weather_app"
$postgresUser = "weather_app"
$postgresPassword = Read-PlainValue "POSTGRES_PASSWORD, press Enter to accept generated password" (New-SafePassword)

$pgadminEmail = Read-PlainValue "PGADMIN_DEFAULT_EMAIL" "admin@example.com"
$pgadminPassword = Read-PlainValue "PGADMIN_DEFAULT_PASSWORD, press Enter to accept generated password" (New-SafePassword)

$auth0Domain = Read-PlainValue "AUTH0_DOMAIN, for example dev-abc.eu.auth0.com"
$auth0Audience = "https://weather-api"
$auth0ClientId = Read-PlainValue "AUTH0_CLIENT_ID"
$auth0Scope = "openid profile email read:weather"

$auth0Database = "Username-Password-Authentication"
$auth0Google = "google-oauth2"
$auth0Apple = "apple"
$auth0Facebook = "facebook"
$auth0GitHub = "github"

$openWeatherKey = Read-PlainValue "OPENWEATHERMAP_API_KEY"
$logs = "false"
$logDirectory = "/workspace/logs"
$composeProfiles = ""
$ngrokAuthtoken = ""
$ngrokUrl = ""

if ($WithNgrok) {
    $ngrokAuthtoken = Read-PlainValue "NGROK_AUTHTOKEN"
    $ngrokUrl = Read-PlainValue "NGROK_URL, for example https://your-ngrok-url.ngrok-free.app"
    $composeProfiles = "ngrok"
}
else {
    $ngrokAuthtoken = Read-PlainValue "NGROK_AUTHTOKEN optional, press Enter to skip" "" $false
    if (-not [string]::IsNullOrWhiteSpace($ngrokAuthtoken)) {
        $ngrokUrl = Read-PlainValue "NGROK_URL, for example https://your-ngrok-url.ngrok-free.app"
        $composeProfiles = "ngrok"
    }
}

$values = [ordered]@{
    POSTGRES_DB = $postgresDb
    POSTGRES_USER = $postgresUser
    POSTGRES_PASSWORD = $postgresPassword
    PGADMIN_DEFAULT_EMAIL = $pgadminEmail
    PGADMIN_DEFAULT_PASSWORD = $pgadminPassword
    AUTH0_DOMAIN = $auth0Domain
    AUTH0_AUDIENCE = $auth0Audience
    AUTH0_CLIENT_ID = $auth0ClientId
    AUTH0_SCOPE = $auth0Scope
    AUTH0_CONNECTION_DATABASE = $auth0Database
    AUTH0_CONNECTION_GOOGLE = $auth0Google
    AUTH0_CONNECTION_APPLE = $auth0Apple
    AUTH0_CONNECTION_FACEBOOK = $auth0Facebook
    AUTH0_CONNECTION_GITHUB = $auth0GitHub
    OPENWEATHERMAP_API_KEY = $openWeatherKey
    LOGS = $logs
    LOG_DIRECTORY = $logDirectory
    COMPOSE_PROFILES = $composeProfiles
    NGROK_AUTHTOKEN = $ngrokAuthtoken
    NGROK_URL = $ngrokUrl
}

foreach ($entry in $values.GetEnumerator()) {
    Assert-EnvValue $entry.Key $entry.Value
}

if ($ValidateOnly) {
    Write-Info "Values were entered. .env was not written because -ValidateOnly was used."
    Stop-Script 0
}

Write-EnvFile -Values $values

Write-Host ""
Write-Info ".env written. Starting Docker Compose..."
Start-Stack
Stop-Script 0
