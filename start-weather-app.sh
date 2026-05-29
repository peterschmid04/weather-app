#!/usr/bin/env bash
set -euo pipefail

# macOS/Linux setup script.
# It mirrors the PowerShell script: validate existing .env, otherwise create it
# atomically from required Auth0/OpenWeatherMap values and generated passwords.
cd "$(dirname "$0")"

VALIDATE_ONLY=false
WITH_NGROK=false

for arg in "$@"; do
  case "$arg" in
    --validate-only)
      VALIDATE_ONLY=true
      ;;
    --with-ngrok)
      WITH_NGROK=true
      ;;
    --reset-db)
      echo "--reset-db is intentionally not automated because it deletes database volumes." >&2
      echo "Run docker compose down -v manually when you really want to reset data." >&2
      exit 1
      ;;
    *)
      echo "Unknown option: $arg" >&2
      echo "Supported: --validate-only, --with-ngrok" >&2
      exit 1
      ;;
  esac
done

restore_tty() {
  stty echo 2>/dev/null || true
}
trap restore_tty EXIT INT TERM

info() {
  printf '[weather-app] %s\n' "$1"
}

prompt_value() {
  local label="$1"
  local default_value="${2:-}"
  local required="${3:-true}"
  local value

  while :; do
    if [ -n "$default_value" ]; then
      printf "%s [%s]: " "$label" "$default_value" >&2
    else
      printf "%s: " "$label" >&2
    fi

    IFS= read -r value
    if [ -z "$value" ]; then
      value="$default_value"
    fi

    if [ "$required" != "true" ] || [ -n "$value" ]; then
      printf "%s" "$value"
      return
    fi

    echo "This value is required." >&2
  done
}

prompt_secret() {
  local label="$1"
  local required="${2:-true}"
  local value

  while :; do
    printf "%s: " "$label" >&2
    if [ -t 0 ]; then
      stty -echo 2>/dev/null || true
    fi
    IFS= read -r value
    restore_tty
    printf "\n" >&2

    if [ "$required" != "true" ] || [ -n "$value" ]; then
      printf "%s" "$value"
      return
    fi

    echo "This value is required." >&2
  done
}

generate_password() {
  if command -v openssl >/dev/null 2>&1; then
    set +o pipefail
    openssl rand -base64 32 | tr -dc 'A-Za-z0-9' | cut -c 1-28
    set -o pipefail
    return
  fi

  if command -v python3 >/dev/null 2>&1; then
    python3 -c 'import secrets,string; alphabet=string.ascii_letters+string.digits; print("".join(secrets.choice(alphabet) for _ in range(28)))'
    return
  fi

  set +o pipefail
  dd if=/dev/urandom bs=32 count=1 2>/dev/null | base64 | tr -dc 'A-Za-z0-9' | cut -c 1-28
  set -o pipefail
}

assert_env_value() {
  local name="$1"
  local value="$2"
  case "$value" in
    *"
"*)
      echo "$name must not contain line breaks." >&2
      exit 1
      ;;
  esac
}

get_env_value() {
  local name="$1"
  if [ ! -f ".env" ]; then
    printf ""
    return
  fi

  awk -v key="$name" '
    index($0, key "=") == 1 {
      sub("^[^=]*=", "")
      value=$0
      sub("\r$", "", value)
    }
    END { if (value != "") print value }
  ' .env
}

is_placeholder() {
  local value="${1:-}"
  local normalized
  normalized="$(printf "%s" "$value" | tr '[:upper:]' '[:lower:]')"

  [ -z "$normalized" ] ||
    case "$normalized" in
      change-me*|your-*|replace-*) true ;;
      *) false ;;
    esac
}

validate_env_values() {
  local errors=0
  local required_values=(
    POSTGRES_PASSWORD
    PGADMIN_DEFAULT_PASSWORD
    AUTH0_DOMAIN
    AUTH0_AUDIENCE
    AUTH0_CLIENT_ID
    OPENWEATHERMAP_API_KEY
  )

  for name in "${required_values[@]}"; do
    value="$(get_env_value "$name")"
    if is_placeholder "$value"; then
      echo "- $name is missing or still a placeholder." >&2
      errors=$((errors + 1))
    fi
  done

  auth0_domain="$(get_env_value AUTH0_DOMAIN)"
  if ! is_placeholder "$auth0_domain" && ! printf "%s" "$auth0_domain" | grep -Eq '^[A-Za-z0-9][A-Za-z0-9-]*(\.[A-Za-z0-9][A-Za-z0-9-]*)*\.auth0\.com$'; then
    echo "- AUTH0_DOMAIN must look like your-tenant.region.auth0.com." >&2
    errors=$((errors + 1))
  fi

  auth0_audience="$(get_env_value AUTH0_AUDIENCE)"
  if ! is_placeholder "$auth0_audience" && ! printf "%s" "$auth0_audience" | grep -Eq '^https?://'; then
    echo "- AUTH0_AUDIENCE should be an API identifier URL, for example https://weather-api." >&2
    errors=$((errors + 1))
  fi

  auth0_client_id="$(get_env_value AUTH0_CLIENT_ID)"
  if ! is_placeholder "$auth0_client_id" && ! printf "%s" "$auth0_client_id" | grep -Eq '^[A-Za-z0-9_-]{16,128}$'; then
    echo "- AUTH0_CLIENT_ID must be the public SPA client id from Auth0." >&2
    errors=$((errors + 1))
  fi

  logs_value="$(get_env_value LOGS)"
  if [ -n "$logs_value" ] && [ "$logs_value" != "true" ] && [ "$logs_value" != "false" ]; then
    echo "- LOGS must be true or false." >&2
    errors=$((errors + 1))
  fi

  ngrok_token="$(get_env_value NGROK_AUTHTOKEN)"
  ngrok_url="$(get_env_value NGROK_URL)"
  compose_profiles="$(get_env_value COMPOSE_PROFILES)"
  if [ -n "$ngrok_token" ] || [ -n "$ngrok_url" ]; then
    if [ -z "$ngrok_token" ] || [ -z "$ngrok_url" ]; then
      echo "- NGROK_AUTHTOKEN and NGROK_URL must both be set, or both stay empty." >&2
      errors=$((errors + 1))
    fi

    if [ -n "$ngrok_url" ] && ! printf "%s" "$ngrok_url" | grep -Eq '^https://'; then
      echo "- NGROK_URL must start with https://." >&2
      errors=$((errors + 1))
    fi

    case ",$compose_profiles," in
      *,ngrok,*) ;;
      *) info "NGROK_AUTHTOKEN and NGROK_URL are set. This script will start Compose with --profile ngrok." ;;
    esac
  fi

  return "$errors"
}

detect_docker_compose() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "Docker is not installed or not available in PATH." >&2
    exit 1
  fi

  if ! docker info >/dev/null 2>&1; then
    echo "Docker is installed, but the Docker daemon is not running." >&2
    exit 1
  fi

  if docker compose version >/dev/null 2>&1; then
    COMPOSE_CMD=(docker compose)
    return
  fi

  if command -v docker-compose >/dev/null 2>&1; then
    COMPOSE_CMD=(docker-compose)
    return
  fi

  echo "docker compose or docker-compose is not available." >&2
  exit 1
}

check_ports() {
  local port
  for port in 3000 5122 5432 5050; do
    if command -v lsof >/dev/null 2>&1 && lsof -iTCP:"$port" -sTCP:LISTEN >/dev/null 2>&1; then
      info "Port $port is already in use. If this is the Weather App stack, docker compose up will reuse it."
    elif command -v ss >/dev/null 2>&1 && ss -ltn | grep -Eq "[:.]$port[[:space:]]"; then
      info "Port $port is already in use. If this is the Weather App stack, docker compose up will reuse it."
    elif command -v netstat >/dev/null 2>&1 && netstat -an | grep -Eq "[:.]$port[[:space:]].*LISTEN"; then
      info "Port $port is already in use. If this is the Weather App stack, docker compose up will reuse it."
    fi
  done
}

wait_http() {
  local name="$1"
  local url="$2"
  local deadline=$((SECONDS + 120))

  while [ "$SECONDS" -lt "$deadline" ]; do
    if command -v curl >/dev/null 2>&1 && curl -fsS "$url" >/dev/null 2>&1; then
      info "$name is reachable."
      return
    fi

    if command -v wget >/dev/null 2>&1 && wget -qO- "$url" >/dev/null 2>&1; then
      info "$name is reachable."
      return
    fi

    sleep 3
  done

  info "$name was not reachable within 120 seconds. Check docker compose logs."
}

compose_up() {
  detect_docker_compose
  check_ports
  ngrok_token="$(get_env_value NGROK_AUTHTOKEN)"
  ngrok_url="$(get_env_value NGROK_URL)"
  if [ -n "$ngrok_token" ] && [ -n "$ngrok_url" ]; then
    compose_args=(--profile ngrok up -d)
  else
    compose_args=(up -d)
  fi

  if ! "${COMPOSE_CMD[@]}" "${compose_args[@]}"; then
    echo "Docker Compose start failed. Check the error above." >&2
    exit 1
  fi

  wait_http "Frontend" "http://localhost:3000"
  wait_http "Backend" "http://localhost:5122/swagger/v1/swagger.json"
}

write_env_file() {
  local tmp=".env.tmp"
  rm -f "$tmp"

  {
    echo "# Local secrets for Weather App. Do not commit this file."
    echo "POSTGRES_DB=$POSTGRES_DB"
    echo "POSTGRES_USER=$POSTGRES_USER"
    echo "POSTGRES_PASSWORD=$POSTGRES_PASSWORD"
    echo ""
    echo "PGADMIN_DEFAULT_EMAIL=$PGADMIN_DEFAULT_EMAIL"
    echo "PGADMIN_DEFAULT_PASSWORD=$PGADMIN_DEFAULT_PASSWORD"
    echo ""
    echo "AUTH0_DOMAIN=$AUTH0_DOMAIN"
    echo "AUTH0_AUDIENCE=$AUTH0_AUDIENCE"
    echo "AUTH0_CLIENT_ID=$AUTH0_CLIENT_ID"
    echo "AUTH0_SCOPE=$AUTH0_SCOPE"
    echo ""
    echo "AUTH0_CONNECTION_DATABASE=$AUTH0_CONNECTION_DATABASE"
    echo "AUTH0_CONNECTION_GOOGLE=$AUTH0_CONNECTION_GOOGLE"
    echo "AUTH0_CONNECTION_APPLE=$AUTH0_CONNECTION_APPLE"
    echo "AUTH0_CONNECTION_FACEBOOK=$AUTH0_CONNECTION_FACEBOOK"
    echo "AUTH0_CONNECTION_GITHUB=$AUTH0_CONNECTION_GITHUB"
    echo ""
    echo "OPENWEATHERMAP_API_KEY=$OPENWEATHERMAP_API_KEY"
    echo ""
    echo "LOGS=$LOGS"
    echo "LOG_DIRECTORY=$LOG_DIRECTORY"
    echo ""
    echo "COMPOSE_PROFILES=$COMPOSE_PROFILES"
    echo "NGROK_AUTHTOKEN=$NGROK_AUTHTOKEN"
    echo "NGROK_URL=$NGROK_URL"
  } > "$tmp"

  chmod 600 "$tmp" 2>/dev/null || true

  if [ -f ".env" ]; then
    backup=".env.backup.$(date +%Y%m%d-%H%M%S)"
    cp ".env" "$backup"
    info "Existing .env backed up to $backup."
  fi

  mv "$tmp" ".env"
}

if [ -f ".env" ]; then
  info ".env found. Validating existing file; it will not be rewritten."
  if ! validate_env_values; then
    echo ".env is not valid. Fix it or remove it and run this script again." >&2
    exit 1
  fi

  if [ "$VALIDATE_ONLY" = "true" ]; then
    info ".env validation passed."
    exit 0
  fi

  compose_up
  exit 0
fi

echo ""
echo "No .env found. Enter the required Auth0 and OpenWeatherMap values."
echo "PostgreSQL, pgAdmin and Auth0 connection names are filled automatically."
echo "Auth0 social provider client secrets stay in the Auth0 Dashboard, not here."
echo ""

POSTGRES_DB="weather_app"
POSTGRES_USER="weather_app"
POSTGRES_PASSWORD="$(generate_password)"

PGADMIN_DEFAULT_EMAIL="admin@example.com"
PGADMIN_DEFAULT_PASSWORD="$(generate_password)"

AUTH0_DOMAIN="$(prompt_value "AUTH0_DOMAIN, for example dev-abc.eu.auth0.com" "" "true")"
AUTH0_AUDIENCE="$(prompt_value "AUTH0_AUDIENCE" "https://weather-api")"
AUTH0_CLIENT_ID="$(prompt_value "AUTH0_CLIENT_ID" "" "true")"
AUTH0_SCOPE="openid profile email read:weather"

AUTH0_CONNECTION_DATABASE="Username-Password-Authentication"
AUTH0_CONNECTION_GOOGLE="google-oauth2"
AUTH0_CONNECTION_APPLE="apple"
AUTH0_CONNECTION_FACEBOOK="facebook"
AUTH0_CONNECTION_GITHUB="github"

OPENWEATHERMAP_API_KEY="$(prompt_secret "OPENWEATHERMAP_API_KEY" "true")"
LOGS="false"
LOG_DIRECTORY="/workspace/logs"
COMPOSE_PROFILES=""
NGROK_AUTHTOKEN=""
NGROK_URL=""

if [ "$WITH_NGROK" = "true" ]; then
  NGROK_AUTHTOKEN="$(prompt_secret "NGROK_AUTHTOKEN" "true")"
  NGROK_URL="$(prompt_value "NGROK_URL, for example https://your-ngrok-url.ngrok-free.app" "" "true")"
  COMPOSE_PROFILES="ngrok"
else
  NGROK_AUTHTOKEN="$(prompt_secret "NGROK_AUTHTOKEN optional, press Enter to skip" "false")"
  if [ -n "$NGROK_AUTHTOKEN" ]; then
    NGROK_URL="$(prompt_value "NGROK_URL, for example https://your-ngrok-url.ngrok-free.app" "" "true")"
    COMPOSE_PROFILES="ngrok"
  fi
fi

for pair in \
  "POSTGRES_DB=$POSTGRES_DB" \
  "POSTGRES_USER=$POSTGRES_USER" \
  "POSTGRES_PASSWORD=$POSTGRES_PASSWORD" \
  "PGADMIN_DEFAULT_EMAIL=$PGADMIN_DEFAULT_EMAIL" \
  "PGADMIN_DEFAULT_PASSWORD=$PGADMIN_DEFAULT_PASSWORD" \
  "AUTH0_DOMAIN=$AUTH0_DOMAIN" \
  "AUTH0_AUDIENCE=$AUTH0_AUDIENCE" \
  "AUTH0_CLIENT_ID=$AUTH0_CLIENT_ID" \
  "AUTH0_SCOPE=$AUTH0_SCOPE" \
  "AUTH0_CONNECTION_DATABASE=$AUTH0_CONNECTION_DATABASE" \
  "AUTH0_CONNECTION_GOOGLE=$AUTH0_CONNECTION_GOOGLE" \
  "AUTH0_CONNECTION_APPLE=$AUTH0_CONNECTION_APPLE" \
  "AUTH0_CONNECTION_FACEBOOK=$AUTH0_CONNECTION_FACEBOOK" \
  "AUTH0_CONNECTION_GITHUB=$AUTH0_CONNECTION_GITHUB" \
  "OPENWEATHERMAP_API_KEY=$OPENWEATHERMAP_API_KEY" \
  "LOGS=$LOGS" \
  "LOG_DIRECTORY=$LOG_DIRECTORY" \
  "COMPOSE_PROFILES=$COMPOSE_PROFILES" \
  "NGROK_AUTHTOKEN=$NGROK_AUTHTOKEN" \
  "NGROK_URL=$NGROK_URL"
do
  assert_env_value "${pair%%=*}" "${pair#*=}"
done

if ! validate_env_values; then
  echo "Entered values are not valid. .env was not written." >&2
  exit 1
fi

if [ "$VALIDATE_ONLY" = "true" ]; then
  info "Entered values are valid. .env was not written because --validate-only was used."
  exit 0
fi

write_env_file

info ".env written. Starting Docker Compose..."
compose_up
