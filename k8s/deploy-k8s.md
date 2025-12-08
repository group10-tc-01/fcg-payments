# Guia de Deploy no Kubernetes - FCG Users

Este guia explica passo a passo como fazer o deploy da aplicação FCG Users em um cluster Kubernetes, desde a configuração inicial até o acesso à aplicação.

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter:

1. **Docker** instalado e rodando
2. **kubectl** instalado e configurado
3. **Kind** instalado (para cluster local) ou acesso a um cluster Kubernetes (AKS, EKS, GKE)
4. **Git Bash** ou **WSL** (para Windows)
5. Credenciais de acesso ao **Azure Container Registry (ACR)**: `fiapcr.azurecr.io`
6. **SQL Server na Azure** já configurado com o database `fcg_user`

## 🚀 Passo 1: Instalação do Kind (Cluster Local)

Se você ainda não tem o Kind instalado:

### Linux/WSL/Git Bash:
```bash
# Download do Kind
curl -Lo ./kind https://kind.sigs.k8s.io/dl/v0.20.0/kind-linux-amd64

# Dar permissão de execução
chmod +x ./kind

# Mover para o PATH
sudo mv ./kind /usr/local/bin/kind

# Verificar instalação
kind version
```

### Windows (PowerShell):
```powershell
# Usando Chocolatey
choco install kind

# Ou baixar manualmente de: https://kind.sigs.k8s.io/docs/user/quick-start/#installation
```

## 🏗️ Passo 2: Criar o Cluster Kind

O arquivo `kind.yml` já está configurado com os port mappings necessários:
- Porta 8080: Aplicação FCG Users
- Porta 8081: Kafka UI

```bash
# Navegar até o diretório do projeto
cd /caminho/para/FCG.Users

# Criar o cluster com o nome fcg-users-cluster
kind create cluster --config=k8s/kind.yml --name=fcg-users-cluster

# Verificar se o cluster foi criado
kind get clusters

# Verificar o contexto do kubectl
kubectl cluster-info --context kind-fcg-users-cluster
```

**✅ Checkpoint:** Execute `kubectl get nodes` - você deve ver 2 nodes (control-plane e worker)

## 🔧 Passo 3: Configurar o Contexto do Kubectl

Configure o kubectl para usar o cluster recém-criado e o namespace correto:

```bash
# Definir o contexto para o cluster Kind
kubectl config use-context kind-fcg-users-cluster

# Definir o namespace padrão como fcg-users (opcional, mas recomendado)
kubectl config set-context --current --namespace=fcg-users
```

**✅ Checkpoint:** Execute `kubectl config current-context` - deve retornar `kind-fcg-users-cluster`

## 🔐 Passo 4: Criar Namespace e Secrets

### 4.1 Criar o Namespace

```bash
kubectl apply -f k8s/namespace.yml
```

### 4.2 Criar Secret do Azure Container Registry (ACR)

```bash
kubectl create secret docker-registry acr-secret \
  --docker-server=fiapcr.azurecr.io \
  --docker-username=<SEU_ACR_USERNAME> \
  --docker-password=<SEU_ACR_PASSWORD> \
  --docker-email=<SEU_EMAIL> \
  --namespace=fcg-users
```

**⚠️ Importante:** Substitua `<SEU_ACR_USERNAME>`, `<SEU_ACR_PASSWORD>` e `<SEU_EMAIL>` pelas suas credenciais reais.

### 4.3 Aplicar Secrets da Aplicação

Antes de aplicar, verifique se o arquivo `k8s/secrets.yml` contém:
- `JwtSettings__SecretKey`: Chave secreta do JWT (base64)
- `ConnectionStrings__DefaultConnection`: Connection string completa do SQL Server Azure (base64)

```bash
kubectl apply -f k8s/secrets.yml
```

**✅ Checkpoint:** Execute `kubectl get secrets -n fcg-users` - você deve ver `acr-secret` e `fcg-users-secret`

## ⚙️ Passo 5: Aplicar ConfigMaps

```bash
kubectl apply -f k8s/configmap.yml
```

**✅ Checkpoint:** Execute `kubectl get configmap -n fcg-users` - você deve ver `fcg-users-config`

## 👤 Passo 6: Criar RBAC (Service Account)

```bash
kubectl apply -f k8s/rbac.yml
```

**✅ Checkpoint:** Execute `kubectl get serviceaccount -n fcg-users` - você deve ver `fcg-users-sa`

## 📦 Passo 7: Deploy das Dependências

### 7.1 Zookeeper

```bash
kubectl apply -f k8s/zookeeper.yml
```

Aguarde o Zookeeper ficar pronto:
```bash
kubectl wait --for=condition=ready pod -l app=zookeeper -n fcg-users --timeout=300s
```

### 7.2 Kafka

```bash
kubectl apply -f k8s/kafka.yml
```

Aguarde o Kafka ficar pronto:
```bash
kubectl wait --for=condition=ready pod -l app=kafka -n fcg-users --timeout=300s
```

### 7.3 Kafka UI (Opcional - para monitoramento)

```bash
kubectl apply -f k8s/kafka-ui.yml
```

**✅ Checkpoint:** Execute `kubectl get pods -n fcg-users` - todos os pods devem estar com status `Running`

## 🚢 Passo 8: Deploy da Aplicação

### 8.1 Aplicar o Deployment

```bash
kubectl apply -f k8s/deployment.yml
```

### 8.2 Aplicar o Service

```bash
kubectl apply -f k8s/service.yml
```

### 8.3 Aplicar o HPA (Horizontal Pod Autoscaler)

Primeiro, aplique o Metrics Server (necessário para o HPA funcionar):

```bash
kubectl apply -f k8s/metrics-server.yml
```

Depois, aplique o HPA:

```bash
kubectl apply -f k8s/hpa.yml
```

**✅ Checkpoint:** Execute `kubectl get pods -n fcg-users` - você deve ver os pods `fcg-users-*` rodando

## 🔍 Passo 9: Verificar o Deploy

### Verificar todos os recursos

```bash
kubectl get all -n fcg-users
```

### Verificar status dos pods

```bash
kubectl get pods -n fcg-users
```

### Verificar logs da aplicação

```bash
# Ver logs em tempo real
kubectl logs -f deployment/fcg-users -n fcg-users

# Ver logs de um pod específico
kubectl logs <nome-do-pod> -n fcg-users
```

### Verificar se há erros

```bash
kubectl describe pod <nome-do-pod> -n fcg-users
```

## 🌐 Passo 10: Acessar a Aplicação

### Aplicação FCG Users

A aplicação estará disponível em:
```
http://localhost:8080
```

Para testar:
```bash
curl http://localhost:8080/health
```

### Kafka UI (Monitoramento)

O Kafka UI estará disponível em:
```
http://localhost:8081
```

Acesse no navegador para visualizar os tópicos, mensagens e status do Kafka.

## 📊 Monitoramento com K9s

### O que é K9s?

K9s é uma interface de terminal interativa para gerenciar clusters Kubernetes. Ele fornece uma maneira visual e eficiente de navegar, observar e gerenciar seus recursos do Kubernetes em tempo real.

### Instalação do K9s

#### Linux/WSL/Git Bash:
```bash
# Usando o instalador oficial
curl -sS https://webinstall.dev/k9s | bash

# Ou usando wget
wget -qO- https://webinstall.dev/k9s | bash

# Adicionar ao PATH (se necessário)
export PATH="$HOME/.local/bin:$PATH"

# Verificar instalação
k9s version
```

#### Windows (PowerShell):
```powershell
# Usando Chocolatey
choco install k9s

# Ou usando Scoop
scoop install k9s

# Verificar instalação
k9s version
```

#### macOS:
```bash
# Usando Homebrew
brew install derailed/k9s/k9s

# Verificar instalação
k9s version
```

### Como usar o K9s

#### Iniciar o K9s

```bash
# Iniciar K9s no namespace padrão
k9s

# Iniciar K9s diretamente no namespace fcg-users
k9s -n fcg-users

# Iniciar K9s com contexto específico
k9s --context kind-fcg-users-cluster
```

#### Navegação Básica no K9s

**Atalhos de Teclado Principais:**

| Tecla | Ação |
|-------|------|
| `?` | Mostrar ajuda com todos os atalhos |
| `:pods` ou `:po` | Ver pods |
| `:deployments` ou `:deploy` | Ver deployments |
| `:services` ou `:svc` | Ver services |
| `:configmaps` ou `:cm` | Ver configmaps |
| `:secrets` | Ver secrets |
| `:namespaces` ou `:ns` | Ver namespaces |
| `:nodes` ou `:no` | Ver nodes |
| `:hpa` | Ver Horizontal Pod Autoscalers |
| `Ctrl+A` | Ver todos os recursos disponíveis |
| `Esc` | Voltar/Sair do menu atual |
| `Ctrl+C` | Sair do K9s |

**Navegação e Ações:**

| Tecla | Ação |
|-------|------|
| `↑` `↓` | Navegar entre recursos |
| `Enter` | Ver detalhes do recurso selecionado |
| `d` | Descrever o recurso (kubectl describe) |
| `l` | Ver logs do pod selecionado |
| `Ctrl+D` | Deletar recurso selecionado |
| `e` | Editar recurso |
| `y` | Ver YAML do recurso |
| `s` | Abrir shell no container |
| `Ctrl+K` | Matar pod |
| `/` | Filtrar recursos (busca) |
| `Ctrl+R` | Atualizar/Refresh |
| `0` | Mostrar todos os namespaces |

#### Workflow Recomendado para Monitorar a Aplicação

**1. Visão Geral dos Pods:**
```bash
# Iniciar K9s no namespace fcg-users
k9s -n fcg-users

# Pressione :pods
```
Aqui você verá:
- Status de todos os pods
- Número de restarts
- Uso de CPU e memória (se metrics-server estiver instalado)
- Idade dos pods

**2. Ver Logs em Tempo Real:**
- Navegue até o pod desejado usando `↑` `↓`
- Pressione `l` para ver logs
- Pressione `0` para ver logs desde o início
- Pressione `1` para ver logs da última hora
- Pressione `/` para buscar nos logs
- Pressione `s` para pausar/continuar scroll automático
- Pressione `Esc` para voltar

**3. Monitorar Deployments:**
```bash
# No K9s, digite
:deployments
```
Você verá:
- Número de réplicas desejadas vs disponíveis
- Status de rollout
- Estratégia de atualização

**4. Verificar Services e Endpoints:**
```bash
# No K9s, digite
:services
```
- Pressione `Enter` no service para ver os endpoints
- Verifique se os pods estão conectados corretamente

**5. Verificar ConfigMaps e Secrets:**
```bash
# Ver ConfigMaps
:configmaps

# Ver Secrets
:secrets
```
- Pressione `y` para ver o YAML completo
- **⚠️ Cuidado:** Secrets são exibidos em base64, mas podem ser decodificados

**6. Monitorar HPA (Autoscaling):**
```bash
# No K9s, digite
:hpa
```
Você verá:
- Métricas atuais (CPU/Memória)
- Limites configurados
- Número atual de réplicas
- Decisões de scaling

**7. Verificar Eventos:**
```bash
# No K9s, digite
:events
```
Eventos mostram:
- Erros de scheduling
- Problemas de pull de imagens
- Falhas de health checks
- Ações de scaling

**8. Acessar Shell no Container:**
- Navegue até o pod
- Pressione `s` para abrir shell
- Execute comandos dentro do container
- Digite `exit` para sair

### Dicas Avançadas de K9s

#### Filtrar Recursos
```bash
# Dentro de qualquer view, pressione /
# Digite um termo para filtrar
# Exemplo: /fcg-users para filtrar pods com esse nome
```

#### Ver Todos os Namespaces
```bash
# Pressione 0 (zero) para ver recursos de todos os namespaces
# Pressione o número correspondente para voltar ao namespace específico
```

#### Pulsar (Pulse) - Monitoramento Visual
```bash
# No K9s, digite
:pulse
```
Mostra uma visão geral do cluster com métricas em tempo real.

#### PortForward Direto do K9s
- Navegue até o pod
- Pressione `Shift+F`
- Digite a porta local:porta do container
- Exemplo: `8080:8080`

#### Copiar Nome de Recurso
- Navegue até o recurso
- Pressione `c` para copiar o nome para clipboard

### Monitoramento Contínuo com K9s

**Para monitorar a aplicação FCG Users continuamente:**

1. **Abra múltiplos terminais com K9s:**
   - Terminal 1: `k9s -n fcg-users` → `:pods` (monitorar pods)
   - Terminal 2: `k9s -n fcg-users` → `:events` (monitorar eventos)
   - Terminal 3: `k9s -n fcg-users` → `:hpa` (monitorar autoscaling)

2. **Use o modo de logs contínuo:**
   - Selecione o pod `fcg-users-*`
   - Pressione `l` para logs
   - Os logs serão atualizados em tempo real

3. **Configure alertas visuais:**
   - K9s automaticamente destaca em vermelho pods com problemas
   - Pods com muitos restarts ficam em amarelo

### Monitoramento com Comandos Kubectl (Alternativa)

Se preferir usar kubectl diretamente:

```bash
# Ver logs em tempo real
kubectl logs -f deployment/fcg-users -n fcg-users

# Monitorar pods
kubectl get pods -n fcg-users -w

# Ver métricas do HPA
kubectl get hpa -n fcg-users --watch

# Ver eventos do namespace
kubectl get events -n fcg-users --sort-by='.lastTimestamp'
```

## 🔄 Atualizações e Manutenção

### Atualizar a aplicação após mudanças

```bash
# Aplicar mudanças no ConfigMap
kubectl apply -f k8s/configmap.yml

# Aplicar mudanças no Secret
kubectl apply -f k8s/secrets.yml

# Aplicar mudanças no Deployment
kubectl apply -f k8s/deployment.yml

# Reiniciar os pods para aplicar as mudanças
kubectl rollout restart deployment/fcg-users -n fcg-users

# Verificar status do rollout
kubectl rollout status deployment/fcg-users -n fcg-users
```

### Escalar manualmente a aplicação

```bash
# Aumentar para 3 réplicas
kubectl scale deployment/fcg-users --replicas=3 -n fcg-users

# Verificar
kubectl get pods -n fcg-users
```

## 🧹 Limpeza

### Remover apenas a aplicação

```bash
kubectl delete deployment fcg-users -n fcg-users
kubectl delete service fcg-users-service -n fcg-users
```

### Remover tudo do namespace

```bash
kubectl delete namespace fcg-users
```

### Deletar o cluster Kind

```bash
kind delete cluster --name=fcg-users-cluster
```

## ⚠️ Troubleshooting

### Pods não sobem (ImagePullBackOff)

Verifique se o secret do ACR foi criado corretamente:
```bash
kubectl get secret acr-secret -n fcg-users
kubectl describe secret acr-secret -n fcg-users
```

### Erro de conexão com SQL Server

1. Verifique a connection string no secret:
```bash
kubectl get secret fcg-users-secret -n fcg-users -o yaml
```

2. Verifique os logs da aplicação:
```bash
kubectl logs deployment/fcg-users -n fcg-users --tail=50
```

3. Certifique-se de que o SQL Server na Azure permite conexões do IP do cluster

### Pods com status CrashLoopBackOff

```bash
# Ver logs do pod
kubectl logs <nome-do-pod> -n fcg-users

# Ver eventos do pod
kubectl describe pod <nome-do-pod> -n fcg-users
```

### HPA não funciona

Verifique se o Metrics Server está rodando:
```bash
kubectl get deployment metrics-server -n kube-system
kubectl top nodes
kubectl top pods -n fcg-users
```

## 📝 Notas Importantes

1. **Segurança**: Nunca commite o arquivo `secrets.yml` com valores reais no Git
2. **SQL Server**: A aplicação usa SQL Server na Azure (externo ao cluster)
3. **Kafka e Zookeeper**: Rodando dentro do cluster para desenvolvimento
4. **Portas**:
   - 8080: Aplicação FCG Users
   - 8081: Kafka UI
   - 30000: NodePort da aplicação (interno do Kind)
   - 30081: NodePort do Kafka UI (interno do Kind)
5. **Persistência**: Kafka usa PersistentVolumeClaim para armazenar dados
6. **Recursos**: Ajuste os limites de CPU/Memória conforme necessário em `deployment.yml`
