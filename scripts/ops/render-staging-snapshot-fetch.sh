#!/usr/bin/env bash
set -euo pipefail

OUTPUT_DIRECTORY="${OUTPUT_DIRECTORY:-${RENDER_STAGING_SNAPSHOT_DIRECTORY:-}}"
DRY_RUN="${DRY_RUN:-0}"
SELECTED_DATABASES=()

usage() {
  cat <<'EOF'
Usage: render-staging-snapshot-fetch.sh [options]

Fetches Render staging Postgres snapshots (pg_dump -Fc) into a backup directory.

Environment (one URL per product database):
  RENDER_STAGING_NEXARR_DATABASE_URL
  RENDER_STAGING_STAFFARR_DATABASE_URL
  RENDER_STAGING_TRAINARR_DATABASE_URL
  RENDER_STAGING_MAINTAINARR_DATABASE_URL
  RENDER_STAGING_ROUTARR_DATABASE_URL
  RENDER_STAGING_SUPPLYARR_DATABASE_URL
  RENDER_STAGING_COMPLIANCECORE_DATABASE_URL

Options:
  --output-directory <path>   Backup output directory
  --database <name>           Limit to one database (repeatable)
  --dry-run                   Resolve targets only; do not pg_dump
  -h, --help                  Show help
EOF
}

declare -A ENV_BY_DB=(
  [nexarr]=RENDER_STAGING_NEXARR_DATABASE_URL
  [staffarr]=RENDER_STAGING_STAFFARR_DATABASE_URL
  [trainarr]=RENDER_STAGING_TRAINARR_DATABASE_URL
  [maintainarr]=RENDER_STAGING_MAINTAINARR_DATABASE_URL
  [routarr]=RENDER_STAGING_ROUTARR_DATABASE_URL
  [supplyarr]=RENDER_STAGING_SUPPLYARR_DATABASE_URL
  [compliancecore]=RENDER_STAGING_COMPLIANCECORE_DATABASE_URL
)

ALL_DATABASES=(nexarr staffarr trainarr maintainarr routarr supplyarr compliancecore)

while [[ $# -gt 0 ]]; do
  case "$1" in
    --output-directory)
      OUTPUT_DIRECTORY="$2"
      shift 2
      ;;
    --database)
      SELECTED_DATABASES+=("$2")
      shift 2
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

if [[ -z "$OUTPUT_DIRECTORY" ]]; then
  OUTPUT_DIRECTORY="${TMPDIR:-/tmp}/stl-render-staging-$(date -u +%Y%m%d-%H%M%S)"
fi

mkdir -p "$OUTPUT_DIRECTORY"

if [[ ${#SELECTED_DATABASES[@]} -eq 0 ]]; then
  SELECTED_DATABASES=("${ALL_DATABASES[@]}")
fi

command -v pg_dump >/dev/null 2>&1 || {
  echo "pg_dump is not on PATH." >&2
  exit 1
}

echo "Render staging snapshot fetch"
echo "  Output directory: $OUTPUT_DIRECTORY"
echo "  Databases: ${SELECTED_DATABASES[*]}"

for database in "${SELECTED_DATABASES[@]}"; do
  env_name="${ENV_BY_DB[$database]:-}"
  if [[ -z "$env_name" ]]; then
    echo "Unknown database: $database" >&2
    exit 1
  fi

  database_url="${!env_name:-}"
  if [[ -z "$database_url" ]]; then
    echo "Missing environment variable: $env_name" >&2
    exit 1
  fi

  backup_path="$OUTPUT_DIRECTORY/${database}.custom"
  echo ""
  echo "[$database] pg_dump -> $backup_path"

  if [[ "$DRY_RUN" == "1" ]]; then
    continue
  fi

  pg_dump "$database_url" -Fc -f "$backup_path"
done

echo ""
if [[ "$DRY_RUN" == "1" ]]; then
  echo "Render staging snapshot fetch dry run completed."
else
  echo "Render staging snapshot fetch completed."
fi
