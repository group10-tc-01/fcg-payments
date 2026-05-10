# FCG.Payments local Kubernetes

Esta pasta contem somente os manifests locais da aplicacao `fcg-payments`.

A infra compartilhada fica em:

```text
fcg-orchestration/fase-04/k8s
```

Suba a infra primeiro:

```bash
cd fcg-orchestration/fase-04/k8s
bash up.sh
```

Para recriar apenas o `fcg-payments`:

```bash
cd fcg-payments
bash k8s/local/up.sh
```

Para remover apenas o namespace da aplicacao:

```bash
bash k8s/local/down.sh
```

Comandos uteis:

```bash
kubectl get pods -n fcg-payments
kubectl logs -n fcg-payments deployment/fcg-payments -f
kubectl describe pod -n fcg-payments -l app.kubernetes.io/name=fcg-payments
curl http://localhost:5054/health
```
