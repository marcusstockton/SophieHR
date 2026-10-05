#!/usr/bin/env bash
set -euo pipefail

# Wait-for-dependencies script
# Usage: the script reads the WAIT_FOR environment variable, a space-separated list
# of host:port entries to check. Example:
#   WAIT_FOR="sophiehr-db:5432 elasticsearch:9200 redis_cache:6379"

WAIT_FOR=${WAIT_FOR:-sophiehr-db:5432 elasticsearch:9200 redis_cache:6379}
TIMEOUT=${TIMEOUT:-60}
SLEEP_INTERVAL=${SLEEP_INTERVAL:-2}

echo "Waiting for dependencies: $WAIT_FOR"

deadline=$((SECONDS + TIMEOUT))

for hostport in $WAIT_FOR; do
  host=${hostport%%:*}
  port=${hostport##*:}

  echo "Checking $host:$port"
  while true; do
	# Try TCP connect using bash /dev/tcp
	if (echo > /dev/tcp/${host}/${port}) >/dev/null 2>&1; then
	  echo "Connected to $host:$port"
	  break
	fi

	if [ "$SECONDS" -gt "$deadline" ]; then
	  echo "Timeout waiting for $host:$port after ${TIMEOUT}s" >&2
	  exit 1
	fi

	sleep $SLEEP_INTERVAL
  done
done

echo "All dependencies available. Starting the application..."

# Exec the main process (dotnet)
exec "$@"
