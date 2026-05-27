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
    $overwrite = Read-PlainValue ".env already exists. Overwrite it? Type yes to overwrite" "no" $false
    if ($overwrite -ne "yes") {
        Write-Host "Keeping existing .env and starting Docker Compose."
        docker compose up -d
        exit $LASTEXITCODE
    }
}

Write-Host ""
Write-Host "Enter local configuration values. Real secrets are written only to .env."
Write-Host "Auth0 social provider client secrets stay in the Auth0 Dashboard, not here."
Write-Host ""

$postgresDb = Read-PlainValue "POSTGRES_DB" "weather_app"
$postgresUser = Read-PlainValue "POSTGRES_USER" "weather_app"
$postgresPassword = Read-SecretValue "POSTGRES_PASSWORD (blank = generate local password)" $false
if ([string]::IsNullOrWhiteSpace($postgresPassword)) {
    $postgresPassword = New-SafePassword
}

$pgadminEmail = Read-PlainValue "PGADMIN_DEFAULT_EMAIL" "admin@example.com"
$pgadminPassword = Read-SecretValue "PGADMIN_DEFAULT_PASSWORD (blank = use POSTGRES_PASSWORD)" $false
if ([string]::IsNullOrWhiteSpace($pgadminPassword)) {
    $pgadminPassword = $postgresPassword
}

$auth0Domain = Read-PlainValue "AUTH0_DOMAIN, for example dev-abc.eu.auth0.com"
$auth0Audience = Read-PlainValue "AUTH0_AUDIENCE" "https://weather-api"
$auth0ClientId = Read-PlainValue "AUTH0_CLIENT_ID"
$auth0Scope = Read-PlainValue "AUTH0_SCOPE" "openid profile email read:weather"

$auth0Database = Read-PlainValue "AUTH0_CONNECTION_DATABASE" "Username-Password-Authentication"
$auth0Google = Read-PlainValue "AUTH0_CONNECTION_GOOGLE" "google-oauth2"
$auth0Apple = Read-PlainValue "AUTH0_CONNECTION_APPLE" "apple"
$auth0Facebook = Read-PlainValue "AUTH0_CONNECTION_FACEBOOK" "facebook"
$auth0Instagram = Read-PlainValue "AUTH0_CONNECTION_INSTAGRAM" "instagram"

$openWeatherKey = Read-SecretValue "OPENWEATHERMAP_API_KEY"

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
    AUTH0_CONNECTION_INSTAGRAM = $auth0Instagram
    OPENWEATHERMAP_API_KEY = $openWeatherKey
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
    "AUTH0_CONNECTION_INSTAGRAM=$auth0Instagram",
    "",
    "OPENWEATHERMAP_API_KEY=$openWeatherKey"
)

Set-Content -Path ".env" -Value $envLines -Encoding UTF8

Write-Host ""
Write-Host ".env written. Starting Docker Compose..."
docker compose up -d
exit $LASTEXITCODE
