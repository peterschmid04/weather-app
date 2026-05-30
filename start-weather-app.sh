#!/usr/bin/env bash
set -euo pipefail

# macOS/Linux setup script.
# It mirrors the PowerShell script: start directly with an existing .env, otherwise create it
# atomically from required Auth0/OpenWeatherMap values and generated passwords.
cd "$(dirname "$0")"

VALIDATE_ONLY=false
WITH_NGROK=false
NO_PAUSE=false
SCRIPT_EXITING=false

pause_if_needed() {
  if [ "$NO_PAUSE" != "true" ] && [ -t 0 ]; then
    printf "\nFertig. Enter druecken zum Schliessen: " >&2
    IFS= read -r _ || true
  fi
}

exit_script() {
  local code="${1:-0}"
  SCRIPT_EXITING=true
  pause_if_needed
  exit "$code"
}

on_exit() {
  local code="$?"
  if [ "$SCRIPT_EXITING" != "true" ] && [ "$code" -ne 0 ]; then
    printf "\nDas Skript wurde mit Fehlercode %s beendet.\n" "$code" >&2
    pause_if_needed
  fi
}
trap on_exit EXIT

for arg in "$@"; do
  case "$arg" in
    --validate-only)
      VALIDATE_ONLY=true
      ;;
    --with-ngrok)
      WITH_NGROK=true
      ;;
    --no-pause)
      NO_PAUSE=true
      ;;
    --reset-db)
      echo "--reset-db is intentionally not automated because it deletes database volumes." >&2
      echo "Run docker compose down -v manually when you really want to reset data." >&2
      exit_script 1
      ;;
    *)
      echo "Unknown option: $arg" >&2
      echo "Supported: --validate-only, --with-ngrok, --no-pause" >&2
      exit_script 1
      ;;
  esac
done

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
      exit_script 1
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

detect_docker_compose() {
  if ! command -v docker >/dev/null 2>&1; then
    echo "Docker is not installed or not available in PATH." >&2
    exit_script 1
  fi

  if docker compose version >/dev/null 2>&1; then
    COMPOSE_CMD=(docker compose)
  elif command -v docker-compose >/dev/null 2>&1; then
    COMPOSE_CMD=(docker-compose)
  else
    echo "docker compose or docker-compose is not available." >&2
    exit_script 1
  fi
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
  local wait_seconds="${3:-120}"
  local deadline=$((SECONDS + wait_seconds))
  local next_notice=$((SECONDS + 10))

  info "Waiting for $name at $url for up to $wait_seconds seconds."
  while [ "$SECONDS" -lt "$deadline" ]; do
    if command -v curl >/dev/null 2>&1 && curl -fsS "$url" >/dev/null 2>&1; then
      info "$name is reachable."
      return 0
    fi

    if command -v wget >/dev/null 2>&1 && wget -qO- "$url" >/dev/null 2>&1; then
      info "$name is reachable."
      return 0
    fi

    if [ "$SECONDS" -ge "$next_notice" ]; then
      info "$name is still starting. Please wait..."
      next_notice=$((SECONDS + 10))
    fi

    sleep 3
  done

  info "$name was not reachable within $wait_seconds seconds. Check docker compose logs."
  return 1
}

postgres_password_mismatch() {
  local logs
  logs="$("${COMPOSE_CMD[@]}" logs --tail=200 db backend 2>&1 || true)"
  printf "%s" "$logs" | grep -qi "password authentication failed"
}

show_startup_diagnostics() {
  local service
  info "Container status:"
  "${COMPOSE_CMD[@]}" ps || true

  for service in "$@"; do
    info "Recent $service logs:"
    "${COMPOSE_CMD[@]}" logs --tail=80 "$service" || true
  done
}

reset_stack_volumes() {
  info "PostgreSQL password mismatch detected. Recreating Docker volumes with the current .env password."
  info "Running docker compose down -v. Local database data from this Compose stack will be reset."
  "${COMPOSE_CMD[@]}" down -v
  info "Starting database and backend again with fresh volumes."
  "${COMPOSE_CMD[@]}" up -d db backend
}

compose_up() {
  detect_docker_compose
  check_ports
  ngrok_token="$(get_env_value NGROK_AUTHTOKEN)"
  ngrok_url="$(get_env_value NGROK_URL)"

  info "Starting database and backend first."
  if ! "${COMPOSE_CMD[@]}" up -d db backend; then
    echo "Docker Compose core backend start failed. Check the error above." >&2
    exit_script 1
  fi

  if ! wait_http "Backend" "http://localhost:5122/swagger/v1/swagger.json" 300; then
    if postgres_password_mismatch; then
      reset_stack_volumes
      wait_http "Backend" "http://localhost:5122/swagger/v1/swagger.json" 300 || {
        show_startup_diagnostics backend db
        echo "Backend did not become reachable after volume reset. Fix the error above before frontend/ngrok are started." >&2
        exit_script 1
      }
    else
      show_startup_diagnostics backend db
      echo "Backend did not become reachable. Fix the error above before frontend/ngrok are started." >&2
      exit_script 1
    fi
  fi

  info "Starting frontend and pgAdmin."
  if ! "${COMPOSE_CMD[@]}" up -d frontend pgadmin; then
    echo "Docker Compose frontend/pgAdmin start failed. Check the error above." >&2
    exit_script 1
  fi

  if ! wait_http "Frontend" "http://localhost:3000" 900; then
    show_startup_diagnostics frontend backend
    echo "Frontend did not become reachable. Fix the error above before ngrok is started." >&2
    exit_script 1
  fi

  if [ -n "$ngrok_token" ] && [ -n "$ngrok_url" ]; then
    info "Starting optional ngrok tunnel."
    if ! "${COMPOSE_CMD[@]}" --profile ngrok up -d ngrok; then
      echo "ngrok could not be started. The local app still runs on http://localhost:3000." >&2
    fi
  fi
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
  if [ "$VALIDATE_ONLY" = "true" ]; then
    info ".env found. No strict value validation is performed; it will not be rewritten."
    exit_script 0
  fi

  info ".env found. Starting Docker Compose without asking for values."
  compose_up
  exit_script 0
fi

echo ""
echo "No .env found. Enter the required Auth0 and OpenWeatherMap values."
echo "PostgreSQL database/user and Auth0 connection names are filled automatically."
echo "AUTH0_AUDIENCE is fixed to https://weather-api and is not asked here."
echo "For local passwords, press Enter to accept the generated password or enter your own."
echo "Auth0 social provider client secrets stay in the Auth0 Dashboard, not here."
echo ""

POSTGRES_DB="weather_app"
POSTGRES_USER="weather_app"
POSTGRES_PASSWORD="$(prompt_value "POSTGRES_PASSWORD, press Enter to accept generated password" "$(generate_password)" "true")"

PGADMIN_DEFAULT_EMAIL="$(prompt_value "PGADMIN_DEFAULT_EMAIL" "admin@example.com" "true")"
PGADMIN_DEFAULT_PASSWORD="$(prompt_value "PGADMIN_DEFAULT_PASSWORD, press Enter to accept generated password" "$(generate_password)" "true")"

AUTH0_DOMAIN="$(prompt_value "AUTH0_DOMAIN, for example dev-abc.eu.auth0.com" "" "true")"
AUTH0_AUDIENCE="https://weather-api"
AUTH0_CLIENT_ID="$(prompt_value "AUTH0_CLIENT_ID" "" "true")"
AUTH0_SCOPE="openid profile email read:weather"

AUTH0_CONNECTION_DATABASE="Username-Password-Authentication"
AUTH0_CONNECTION_GOOGLE="google-oauth2"
AUTH0_CONNECTION_APPLE="apple"
AUTH0_CONNECTION_FACEBOOK="facebook"
AUTH0_CONNECTION_GITHUB="github"

OPENWEATHERMAP_API_KEY="$(prompt_value "OPENWEATHERMAP_API_KEY" "" "true")"
LOGS="false"
LOG_DIRECTORY="/workspace/logs"
COMPOSE_PROFILES=""
NGROK_AUTHTOKEN=""
NGROK_URL=""

if [ "$WITH_NGROK" = "true" ]; then
  NGROK_AUTHTOKEN="$(prompt_value "NGROK_AUTHTOKEN" "" "true")"
  NGROK_URL="$(prompt_value "NGROK_URL, for example https://your-ngrok-url.ngrok-free.app" "" "true")"
  COMPOSE_PROFILES="ngrok"
else
  NGROK_AUTHTOKEN="$(prompt_value "NGROK_AUTHTOKEN optional, press Enter to skip" "" "false")"
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

if [ "$VALIDATE_ONLY" = "true" ]; then
  info "Values were entered. .env was not written because --validate-only was used."
  exit_script 0
fi

write_env_file

info ".env written. Starting Docker Compose..."
compose_up
exit_script 0
