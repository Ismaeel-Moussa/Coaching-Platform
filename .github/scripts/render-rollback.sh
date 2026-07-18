#!/usr/bin/env bash
set -euo pipefail

target_deploy_id="${1:?A target Render deploy ID is required.}"

: "${RENDER_API_KEY:?RENDER_API_KEY is required.}"
: "${RENDER_SERVICE_ID:?RENDER_SERVICE_ID is required.}"
: "${HEALTH_URL:?HEALTH_URL is required.}"

response="$(curl \
  --fail-with-body \
  --silent \
  --show-error \
  --connect-timeout 10 \
  --max-time 30 \
  --request POST \
  --header "Accept: application/json" \
  --header "Content-Type: application/json" \
  --header "Authorization: Bearer ${RENDER_API_KEY}" \
  --data "$(jq --null-input --arg deployId "${target_deploy_id}" '{deployId: $deployId}')" \
  "https://api.render.com/v1/services/${RENDER_SERVICE_ID}/rollback")"
rollback_id="$(jq --raw-output '.id // .deploy.id // empty' <<<"${response}")"

if [[ -z "${rollback_id}" ]]; then
  echo "Render accepted no identifiable rollback deployment." >&2
  exit 1
fi

echo "Triggered rollback ${rollback_id} to deploy ${target_deploy_id}."

deadline=$((SECONDS + 600))
while (( SECONDS < deadline )); do
  deploy="$(curl \
    --fail \
    --silent \
    --show-error \
    --retry 3 \
    --retry-all-errors \
    --connect-timeout 10 \
    --max-time 30 \
    --header "Accept: application/json" \
    --header "Authorization: Bearer ${RENDER_API_KEY}" \
    "https://api.render.com/v1/services/${RENDER_SERVICE_ID}/deploys/${rollback_id}")"
  status="$(jq --raw-output '.status // .deploy.status // empty' <<<"${deploy}")"
  echo "Render rollback ${rollback_id}: ${status:-pending}"

  case "${status}" in
    live)
      break
      ;;
    build_failed|update_failed|pre_deploy_failed|canceled|deactivated)
      echo "Rollback failed with status '${status}'." >&2
      exit 1
      ;;
    *)
      sleep 15
      ;;
  esac
done

if [[ "${status:-}" != "live" ]]; then
  echo "Timed out waiting for rollback ${rollback_id}." >&2
  exit 1
fi

for attempt in {1..12}; do
  if health="$(curl \
    --fail \
    --silent \
    --show-error \
    --connect-timeout 10 \
    --max-time 30 \
    "${HEALTH_URL}")" \
    && jq --exit-status \
      '.status == "healthy" and .database == "connected"' \
      <<<"${health}" >/dev/null; then
    echo "Rollback completed and production is healthy."
    exit 0
  fi

  echo "Post-rollback health attempt ${attempt}/12 failed; retrying in 10 seconds."
  sleep 10
done

echo "Rollback became live, but production is still unhealthy. Manual intervention is required." >&2
exit 1
