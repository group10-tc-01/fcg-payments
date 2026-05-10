#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
MANIFEST_DIR="$SCRIPT_DIR/manifests"

CLUSTER_NAME="${CLUSTER_NAME:-fcg-fase04-local}"
NAMESPACE="fcg-payments"
IMAGE_NAME="${IMAGE_NAME:-fcg-payments:local}"
SQL_PASSWORD="${FCG_LOCAL_SQL_PASSWORD:-FcgLocal123!}"
JWT_SECRET_KEY="${FCG_LOCAL_JWT_SECRET_KEY:-local-development-jwt-secret-key-for-fcg-fase04-1234567890}"

require_command() {
  local command_name="$1"

  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Missing required command: $command_name" >&2
    exit 1
  fi
}

require_command docker
require_command kind
require_command kubectl

if ! kind get clusters 2>/dev/null | grep -Fxq "$CLUSTER_NAME"; then
  echo "Kind cluster $CLUSTER_NAME was not found."
  echo "Create infra first with: bash ../fcg-orchestration/fase-04/k8s/up.sh"
  exit 1
fi

cd "$REPO_ROOT"

echo "Building Docker image: $IMAGE_NAME"
docker build -t "$IMAGE_NAME" -f src/FCG.Payments.WebApi/Dockerfile .

echo "Loading image into Kind: $IMAGE_NAME"
kind load docker-image "$IMAGE_NAME" --name "$CLUSTER_NAME"

echo "Applying namespace"
kubectl apply -f "$MANIFEST_DIR/00-namespace.yaml"

echo "Applying local secret"
kubectl -n "$NAMESPACE" create secret generic fcg-payments-local-secret \
  --from-literal=connection-string="Server=sqlserver-service.fcg-infra.svc.cluster.local;Database=fcg_payments;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=True;" \
  --from-literal=jwt-secret-key="$JWT_SECRET_KEY" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Applying FCG.Payments"
kubectl apply -f "$MANIFEST_DIR/01-configmap.yaml" -f "$MANIFEST_DIR/02-app.yaml"

echo "Waiting for FCG.Payments"
kubectl -n "$NAMESPACE" rollout status deployment/fcg-payments --timeout=300s

echo ""
echo "FCG.Payments is ready: http://localhost:5054"
