#!/usr/bin/env bash
set -euo pipefail

POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_USER="${POSTGRES_USER:-stl}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-stl_dev_password}"
BACKUP_DIRECTORY="${BACKUP_DIRECTORY:-}"
DRILL_SUFFIX="${DRILL_SUFFIX:-_dr_restore_drill}"
DOCKER_CONTAINER="${DOCKER_CONTAINER:-}"
KEEP_DRILL_DATABASES="${KEEP_DRILL_DATABASES:-0}"
DRY_RUN="${DRY_RUN:-0}"

DATABASES=(
  nexarr
  staffarr
  trainarr
  maintainarr
  routarr
  supplyarr
  compliancecore
)

usage() {
  cat <<'EOF'
Usage: dr-restore-drill.sh --backup-directory <path> [options]

Options:
  --backup-directory <path>   Directory containing per-database backups (*.custom, *.dump, or *.sql)
  --host <host>               Postgres host (default: localhost)
  --port <port>               Postgres port (default: 5432)
  --user <user>               Postgres user (default: stl)
  --password <password>       Postgres password (default: stl_dev_password)
  --drill-suffix <suffix>     Drill database suffix (default: _dr_restore_drill)
  --docker-container <name>   Run pg_* via docker exec in this container
  --database <name>           Limit to one database (repeatable)
  --keep-drill-databases      Do not drop drill databases after validation
  --dry-run                   Resolve backups only; do not restore
  -h, --help                  Show help
EOF
}

SELECTED_DATABASES=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --backup-directory)
      BACKUP_DIRECTORY="$2"
      shift 2
      ;;
    --host)
      POSTGRES_HOST="$2"
      shift 2
      ;;
    --port)
      POSTGRES_PORT="$2"
      shift 2
      ;;
    --user)
      POSTGRES_USER="$2"
      shift 2
      ;;
    --password)
      POSTGRES_PASSWORD="$2"
      shift 2
      ;;
    --drill-suffix)
      DRILL_SUFFIX="$2"
      shift 2
      ;;
    --docker-container)
      DOCKER_CONTAINER="$2"
      shift 2
      ;;
    --database)
      SELECTED_DATABASES+=("$2")
      shift 2
      ;;
    --keep-drill-databases)
      KEEP_DRILL_DATABASES=1
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
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "$BACKUP_DIRECTORY" ]]; then
  echo "BACKUP_DIRECTORY is required." >&2
  usage
  exit 1
fi

if [[ ! -d "$BACKUP_DIRECTORY" ]]; then
  echo "Backup directory not found: $BACKUP_DIRECTORY" >&2
  exit 1
fi

if [[ ${#SELECTED_DATABASES[@]} -gt 0 ]]; then
  DATABASES=("${SELECTED_DATABASES[@]}")
fi

resolve_backup_path() {
  local database="$1"
  for extension in custom dump sql; do
    local candidate="$BACKUP_DIRECTORY/${database}.${extension}"
    if [[ -f "$candidate" ]]; then
      echo "$candidate"
      return 0
    fi
  done
  echo "No backup found for '$database' under '$BACKUP_DIRECTORY'." >&2
  return 1
}

run_psql() {
  local database="$1"
  shift
  if [[ -n "$DOCKER_CONTAINER" ]]; then
    docker exec -e "PGPASSWORD=$POSTGRES_PASSWORD" "$DOCKER_CONTAINER" \
      psql -v ON_ERROR_STOP=1 -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$database" "$@"
  else
    PGPASSWORD="$POSTGRES_PASSWORD" psql -v ON_ERROR_STOP=1 -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$database" "$@"
  fi
}

run_pg_restore() {
  local backup_path="$1"
  local target_database="$2"
  if [[ -n "$DOCKER_CONTAINER" ]]; then
    local container_backup="/tmp/dr-restore-$(basename "$backup_path")"
    docker cp "$backup_path" "${DOCKER_CONTAINER}:${container_backup}"
    docker exec -e "PGPASSWORD=$POSTGRES_PASSWORD" "$DOCKER_CONTAINER" \
      pg_restore --no-owner --no-privileges --dbname="$target_database" \
      -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" "$container_backup"
    docker exec "$DOCKER_CONTAINER" rm -f "$container_backup"
  else
    PGPASSWORD="$POSTGRES_PASSWORD" pg_restore --no-owner --no-privileges --dbname="$target_database" \
      -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" "$backup_path"
  fi
}

run_psql_file() {
  local backup_path="$1"
  local target_database="$2"
  if [[ -n "$DOCKER_CONTAINER" ]]; then
    local container_backup="/tmp/dr-restore-$(basename "$backup_path")"
    docker cp "$backup_path" "${DOCKER_CONTAINER}:${container_backup}"
    docker exec -e "PGPASSWORD=$POSTGRES_PASSWORD" "$DOCKER_CONTAINER" \
      psql -v ON_ERROR_STOP=1 -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" \
      -d "$target_database" -f "$container_backup"
    docker exec "$DOCKER_CONTAINER" rm -f "$container_backup"
  else
    PGPASSWORD="$POSTGRES_PASSWORD" psql -v ON_ERROR_STOP=1 -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" \
      -U "$POSTGRES_USER" -d "$target_database" -f "$backup_path"
  fi
}

validate_restored_database() {
  local target_database="$1"
  local output
  output="$(run_psql "$target_database" -tA -F "," -c \
    "SELECT (SELECT COUNT(*) FROM \"__EFMigrationsHistory\"), (SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'platform_metadata'));")"
  local migration_count="${output%,*}"
  local platform_metadata_exists="${output#*,}"
  if [[ "$migration_count" -le 0 ]]; then
    echo "Validation failed for '$target_database': no EF migration history rows." >&2
    exit 1
  fi
  if [[ "$platform_metadata_exists" != "t" ]]; then
    echo "Validation failed for '$target_database': platform_metadata table missing." >&2
    exit 1
  fi
  echo "Validated $target_database (migrations=$migration_count)"
}

echo "DR restore drill"
echo "  Host: ${POSTGRES_HOST}:${POSTGRES_PORT}"
echo "  Backup directory: $BACKUP_DIRECTORY"
echo "  Drill suffix: $DRILL_SUFFIX"
echo "  Databases: ${DATABASES[*]}"
if [[ -n "$DOCKER_CONTAINER" ]]; then
  echo "  Docker container: $DOCKER_CONTAINER"
fi
if [[ "$DRY_RUN" == "1" ]]; then
  echo "  Mode: DRY RUN"
fi

for database in "${DATABASES[@]}"; do
  backup_path="$(resolve_backup_path "$database")"
  drill_database="${database}${DRILL_SUFFIX}"
  echo
  echo "[$database] backup=$backup_path -> drill database=$drill_database"

  if [[ "$DRY_RUN" == "1" ]]; then
    continue
  fi

  run_psql postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '${drill_database}' AND pid <> pg_backend_pid();"
  run_psql postgres -c "DROP DATABASE IF EXISTS \"${drill_database}\";"
  run_psql postgres -c "CREATE DATABASE \"${drill_database}\";"

  extension="${backup_path##*.}"
  if [[ "$extension" == "sql" ]]; then
    run_psql_file "$backup_path" "$drill_database"
  else
    run_pg_restore "$backup_path" "$drill_database"
  fi

  validate_restored_database "$drill_database"

  if [[ "$KEEP_DRILL_DATABASES" != "1" ]]; then
    run_psql postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '${drill_database}' AND pid <> pg_backend_pid();"
    run_psql postgres -c "DROP DATABASE IF EXISTS \"${drill_database}\";"
    echo "[$database] cleaned up drill database '$drill_database'"
  fi
done

echo
if [[ "$DRY_RUN" == "1" ]]; then
  echo "DR restore drill dry run completed for ${#DATABASES[@]} databases."
else
  echo "DR restore drill passed for ${#DATABASES[@]} databases."
fi
