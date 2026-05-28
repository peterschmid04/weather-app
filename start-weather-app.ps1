$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path $RepoRoot

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

function ConvertTo-PlainText {
    param([System.Security.SecureString]$SecureValue)

    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureValue)
    try {
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

function Read-SecretValue {
    param(
        [string]$Label,
        [bool]$Required = $true
    )

    while ($true) {
        $secure = Read-Host $Label -AsSecureString
        $value = ConvertTo-PlainText $secure

        if (-not $Required -or -not [string]::IsNullOrWhiteSpace($value)) {
            return $value.Trim()
        }

        Write-Host "This value is required."
    }
}

function New-SafePassword {
    $bytes = New-Object byte[] 18
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [Convert]::ToBase64String($bytes).Replace("+", "A").Replace("/", "B").Replace("=", "C")
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

if (Test-Path -LiteralPath ".env") {
    Write-Host ".env found. Starting Docker Compose without asking for values."
    docker compose up -d
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "No .env found. Enter the required Auth0 and OpenWeatherMap values."
Write-Host "PostgreSQL, pgAdmin and Auth0 connection names are filled automatically."
Write-Host "Auth0 social provider client secrets stay in the Auth0 Dashboard, not here."
Write-Host ""

$postgresDb = "weather_app"
$postgresUser = "weather_app"
$postgresPassword = New-SafePassword

$pgadminEmail = "admin@example.com"
$pgadminPassword = $postgresPassword

$auth0Domain = Read-PlainValue "AUTH0_DOMAIN, for example dev-abc.eu.auth0.com"
$auth0Audience = Read-PlainValue "AUTH0_AUDIENCE" "https://weather-api"
$auth0ClientId = Read-PlainValue "AUTH0_CLIENT_ID"
$auth0Scope = "openid profile email read:weather"

$auth0Database = "Username-Password-Authentication"
$auth0Google = "google-oauth2"
$auth0Apple = "apple"
$auth0Facebook = "facebook"
$auth0GitHub = "github"

$openWeatherKey = Read-SecretValue "OPENWEATHERMAP_API_KEY"
$logs = "false"
$logDirectory = "/workspace/logs"
$ngrokAuthtoken = ""
$ngrokUrl = "https://relaxed-yak-pleasantly.ngrok-free.app"

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
    NGROK_AUTHTOKEN = $ngrokAuthtoken
    NGROK_URL = $ngrokUrl
}

foreach ($entry in $values.GetEnumerator()) {
    Assert-EnvValue $entry.Key $entry.Value
}

$envLines = @(
    "# Local secrets for Weather App. Do not commit this file.",
    "POSTGRES_DB=$postgresDb",
    "POSTGRES_USER=$postgresUser",
    "POSTGRES_PASSWORD=$postgresPassword",
    "",
    "PGADMIN_DEFAULT_EMAIL=$pgadminEmail",
    "PGADMIN_DEFAULT_PASSWORD=$pgadminPassword",
    "",
    "AUTH0_DOMAIN=$auth0Domain",
    "AUTH0_AUDIENCE=$auth0Audience",
    "AUTH0_CLIENT_ID=$auth0ClientId",
    "AUTH0_SCOPE=$auth0Scope",
    "",
    "AUTH0_CONNECTION_DATABASE=$auth0Database",
    "AUTH0_CONNECTION_GOOGLE=$auth0Google",
    "AUTH0_CONNECTION_APPLE=$auth0Apple",
    "AUTH0_CONNECTION_FACEBOOK=$auth0Facebook",
    "AUTH0_CONNECTION_GITHUB=$auth0GitHub",
    "",
    "OPENWEATHERMAP_API_KEY=$openWeatherKey",
    "",
    "LOGS=$logs",
    "LOG_DIRECTORY=$logDirectory",
    "",
    "NGROK_AUTHTOKEN=$ngrokAuthtoken",
    "NGROK_URL=$ngrokUrl"
)

Set-Content -Path ".env" -Value $envLines -Encoding UTF8

Write-Host ""
Write-Host ".env written. Starting Docker Compose..."
docker compose up -d
exit $LASTEXITCODE
