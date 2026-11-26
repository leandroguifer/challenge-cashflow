#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  CREATE ROLE entries_app LOGIN PASSWORD '$APP_PASSWORD';
  CREATE ROLE entries_publisher LOGIN PASSWORD '$APP_PASSWORD';
EOSQL