#!/usr/bin/env sh
set -eu

cd "$(dirname "$0")"

prompt_value() {
  label="$1"
  default_value="${2:-}"
  required="${3:-true}"

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
  label="$1"
  required="${2:-true}"

  while :; do
    printf "%s: " "$label" >&2
    stty -echo 2>/dev/null || true
    IFS= read -r value
    stty echo 2>/dev/null || true
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
    openssl rand -base64 24 | tr -dc 'A-Za-z0-9' | cut -c 1-24
  else
    LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom | head -c 24
  fi
}

assert_env_value() {
  name="$1"
  value="$2"
  case "$value" in
    *"
"*)
      echo "$name must not contain line breaks." >&2
      exit 1
      ;;
  esac
}

if [ -f ".env" ]; then
  overwrite="$(prompt_value ".env already exists. Overwrite it? Type yes to overwrite" "no" "false")"
  if [ "$overwrite" != "yes" ]; then
    echo "Keeping existing .env and starting Docker Compose."
    docker compose up -d
    exit $?
  fi
fi

echo ""
echo "Enter local configuration values. Real secrets are written only to .env."
echo "Auth0 social provider client secrets stay in the Auth0 Dashboard, not here."
echo ""

POSTGRES_DB="$(prompt_value "POSTGRES_DB" "weather_app")"
POSTGRES_USER="$(prompt_value "POSTGRES_USER" "weather_app")"
POSTGRES_PASSWORD="$(prompt_secret "POSTGRES_PASSWORD (blank = generate local password)" "false")"
if [ -z "$POSTGRES_PASSWORD" ]; then
  POSTGRES_PASSWORD="$(generate_password)"
fi

PGADMIN_DEFAULT_EMAIL="$(prompt_value "PGADMIN_DEFAULT_EMAIL" "admin@example.com")"
PGADMIN_DEFAULT_PASSWORD="$(prompt_secret "PGADMIN_DEFAULT_PASSWORD (blank = use POSTGRES_PASSWORD)" "false")"
if [ -z "$PGADMIN_DEFAULT_PASSWORD" ]; then
  PGADMIN_DEFAULT_PASSWORD="$POSTGRES_PASSWORD"
fi

AUTH0_DOMAIN="$(prompt_value "AUTH0_DOMAIN, for example dev-abc.eu.auth0.com" "" "true")"
AUTH0_AUDIENCE="$(prompt_value "AUTH0_AUDIENCE" "https://weather-api")"
AUTH0_CLIENT_ID="$(prompt_value "AUTH0_CLIENT_ID" "" "true")"
AUTH0_SCOPE="$(prompt_value "AUTH0_SCOPE" "openid profile email read:weather")"

AUTH0_CONNECTION_DATABASE="$(prompt_value "AUTH0_CONNECTION_DATABASE" "Username-Password-Authentication")"
AUTH0_CONNECTION_GOOGLE="$(prompt_value "AUTH0_CONNECTION_GOOGLE" "google-oauth2")"
AUTH0_CONNECTION_APPLE="$(prompt_value "AUTH0_CONNECTION_APPLE" "apple")"
AUTH0_CONNECTION_FACEBOOK="$(prompt_value "AUTH0_CONNECTION_FACEBOOK" "facebook")"
AUTH0_CONNECTION_GITHUB="$(prompt_value "AUTH0_CONNECTION_GITHUB" "github")"

OPENWEATHERMAP_API_KEY="$(prompt_secret "OPENWEATHERMAP_API_KEY" "true")"

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
  "OPENWEATHERMAP_API_KEY=$OPENWEATHERMAP_API_KEY"
do
  assert_env_value "${pair%%=*}" "${pair#*=}"
done

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
} > .env

echo ""
echo ".env written. Starting Docker Compose..."
docker compose up -d
