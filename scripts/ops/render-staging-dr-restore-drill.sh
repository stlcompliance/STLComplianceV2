#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIRECTORY="${BACKUP_DIRECTORY:-${RENDER_STAGING_SNAPSHOT_DIRECTORY:-}}"
SKIP_SNAPSHOT_FETCH="${SKIP_SNAPSHOT_FETCH:-0}"
DRY_RUN="${DRY_RUN:-0}"
SELECTED_DATABASES=()

usage() {
  cat <<'EOF'
Usage: render-staging-dr-restore-drill.sh [options]

Fetches Render staging snapshots (optional) and runs dr-restore-drill per database.

Options:
  --backup-directory <path>   Directory containing or receiving backups
  --database <name>           Limit to one database (repeatable)
  --skip-snapshot-fetch       Use existing backups only
  --dry-run                   Resolve targets only
  -h, --help                  Show help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --backup-directory)
      BACKUP_DIRECTORY="$2"
      shift 2
      ;;
    --database)
      SELECTED_DATABASES+=("$2")
      shift 2
      ;;
    --skip-snapshot-fetch)
      SKIP_SNAPSHOT_FETCH=1
      shift
      ;;
    --dry-run)
      DRY_RUN=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "$BACKUP_DIRECTORY" ]]; then
  BACKUP_DIRECTORY="${TMPDIR:-/tmp}/stl-render-staging-$(date -u +%Y%m%d-%H%M%S)"
fi

fetch_args=(--output-directory "$BACKUP_DIRECTORY")
drill_args=(--backup-directory "$BACKUP_DIRECTORY")

for database in "${SELECTED_DATABASES[@]}"; do
  fetch_args+=(--database "$database")
  drill_args+=(--database "$database")
done

if [[ "$DRY_RUN" == "1" ]]; then
  fetch_args+=(--dry-run)
fi

echo "Render staging DR restore drill"
echo "  Backup directory: $BACKUP_DIRECTORY"

if [[ "$SKIP_SNAPSHOT_FETCH" != "1" ]]; then
  "$SCRIPT_DIR/render-staging-snapshot-fetch.sh" "${fetch_args[@]}"
fi

if [[ "$DRY_RUN" == "1" ]]; then
  echo "Render staging DR restore drill dry run completed."
  exit 0
fi

declare -A ENV_BY_DB=(
  [nexarr]=RENDER_STAGING_NEXARR_DATABASE_URL
  [staffarr]=RENDER_STAGING_STAFFARR_DATABASE_URL
  [trainarr]=RENDER_STAGING_TRAINARR_DATABASE_URL
  [maintainarr]=RENDER_STAGING_MAINTAINARR_DATABASE_URL
  [routarr]=RENDER_STAGING_ROUTARR_DATABASE_URL
  [supplyarr]=RENDER_STAGING_SUPPLYARR_DATABASE_URL
  [compliancecore]=RENDER_STAGING_COMPLIANCECORE_DATABASE_URL
)

if [[ ${#SELECTED_DATABASES[@]} -eq 0 ]]; then
  SELECTED_DATABASES=(nexarr staffarr trainarr maintainarr routarr supplyarr compliancecore)
fi

urldecode() {
  local data="${1//+/ }"
  printf '%b' "${data//%/\\x}"
}

parse_postgres_uri() {
  local uri="$1"
  if [[ "$uri" =~ ^postgres(ql)?://([^:]+):([^@]+)@([^:/]+)(:([0-9]+))?/([^?]+) ]]; then
    POSTGRES_USER="${BASH_REMATCH[2]}"
    POSTGRES_PASSWORD="$(urldecode "${BASH_REMATCH[3]}")"
    POSTGRES_HOST="${BASH_REMATCH[4]}"
    POSTGRES_PORT="${BASH_REMATCH[6]:-5432}"
    return 0
  fi
  echo "Unable to parse PostgreSQL URI." >&2
  return 1
}

for database in "${SELECTED_DATABASES[@]}"; do
  env_name="${ENV_BY_DB[$database]}"
  database_url="${!env_name:-}"
  if [[ -z "$database_url" ]]; then
    echo "Missing environment variable: $env_name" >&2
    exit 1
  fi

  backup_path="$BACKUP_DIRECTORY/${database}.custom"
  if [[ ! -f "$backup_path" ]]; then
    echo "Missing backup for '$database': $backup_path" >&2
    exit 1
  fi

  parse_postgres_uri "$database_url"

  echo ""
  echo "[$database] restore drill on ${POSTGRES_HOST}:${POSTGRES_PORT}"

  POSTGRES_HOST="$POSTGRES_HOST" POSTGRES_PORT="$POSTGRES_PORT" POSTGRES_USER="$POSTGRES_USER" POSTGRES_PASSWORD="$POSTGRES_PASSWORD" \
    "$SCRIPT_DIR/dr-restore-drill.sh" --backup-directory "$BACKUP_DIRECTORY" --host "$POSTGRES_HOST" --port "$POSTGRES_PORT" --user "$POSTGRES_USER" --password "$POSTGRES_PASSWORD" --database "$database"
done

echo ""
echo "Render staging DR restore drill passed for ${#SELECTED_DATABASES[@]} databases."
