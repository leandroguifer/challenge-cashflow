#!/bin/bash
set -e

cp /etc/rabbitmq/definitions.template.json /tmp/definitions.json

for name in RABBITMQ_ADMIN_USER RABBITMQ_ADMIN_PASSWORD ENTRIES_RABBITMQ_USER ENTRIES_RABBITMQ_PASSWORD BALANCE_RABBITMQ_USER BALANCE_RABBITMQ_PASSWORD; do
  sed -i "s|@${name}@|${!name}|g" /tmp/definitions.json
done

chmod 644 /tmp/definitions.json

exec docker-entrypoint.sh "$@"
