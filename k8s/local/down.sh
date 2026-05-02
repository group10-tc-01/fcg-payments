#!/usr/bin/env bash
set -euo pipefail

echo "Deleting namespace fcg-payments"
kubectl delete namespace fcg-payments --ignore-not-found=true
