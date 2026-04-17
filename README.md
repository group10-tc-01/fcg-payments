# 💳 FCG.Payments - API de Pagamentos e Carteira Digital

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Responsabilidade](#-responsabilidade)
- [Arquitetura](#-arquitetura)
- [Tecnologias e Bibliotecas](#-tecnologias-e-bibliotecas)
- [Modelo de Dados](#-modelo-de-dados)
- [Regras de Negócio](#-regras-de-negócio)
- [Endpoints da API](#-endpoints-da-api)
- [Eventos](#-eventos)
- [Configuração e Execução](#-configuração-e-execução)

---

## 🎯 Sobre o Projeto

**FCG.Payments** é uma API RESTful desenvolvida em .NET 8 para gerenciamento completo de carteiras digitais e processamento de pagamentos. A aplicação implementa processamento assíncrono de transações via **Event-Driven Architecture** com **Apache Kafka**, garantindo consistência, rastreabilidade e escalabilidade nas operações financeiras.

### 🚀 Responsabilidade

A API é responsável por:

- 💰 **Gerenciamento de carteiras digitais de usuários**
- 💳 **Processamento de pagamentos com validação de saldo**
- 📊 **Histórico completo de transações**
- 🔄 **Consumo e publicação de eventos de domínio**
- 🔒 **Transações atômicas (débito + registro)**
- ⚡ **Processamento assíncrono via Kafka**
- 🛡️ **Validação de saldo e prevenção de saldo negativo**

---

## 🏛️ Arquitetura

A aplicação segue os princípios da **Clean Architecture**, garantindo separação de responsabilidades, testabilidade e manutenibilidade do código.

### Estrutura de Camadas

```
┌─────────────────────────────────────────┐
│       FCG.Payments.WebApi               │  ← Camada de Apresentação (API REST)
│   Controllers, Middlewares, Filters    │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│     FCG.Payments.Application            │  ← Camada de Aplicação (Use Cases)
│   UseCases, Validations, DTOs          │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│       FCG.Payments.Domain               │  ← Camada de Domínio (Regras de Negócio)
│   Entities, Exceptions, Events         │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│    FCG.Payments.Infrastructure.*        │  ← Camada de Infraestrutura
│  SqlServer, Kafka, Auth (JWT/BCrypt)   │
└─────────────────────────────────────────┘
```

### Camadas do Projeto

#### 1️⃣ **Domain** (`FCG.Payments.Domain`)
- Entidades de negócio: `Wallet`, `Payment`, `PaymentStatus`
- Exceções de domínio: `DomainException`, `NotFoundException`, `ConflictException`, `UnauthorizedException`
- Eventos de domínio: `IDomainEvent`
- Value Objects: `Money`, `PaymentDetails`
- Abstrações: `BaseEntity`, `IUnitOfWork`

#### 2️⃣ **Application** (`FCG.Payments.Application`)
- **Use Cases** (CQRS): Commands e Queries
  - Wallets: Criar carteira, consultar saldo, depositar
  - Payments: Processar pagamento, listar histórico
- **Validações** com FluentValidation
- **Abstrações**: Repositories, Messaging, Pagination
- **Behaviors**: Validação, Logging, Transaction

#### 3️⃣ **Infrastructure**
- **SqlServer** (`FCG.Payments.Infrastructure.SqlServer`): Persistência com Entity Framework Core
- **Auth** (`FCG.Payments.Infrastructure.Auth`): Implementação JWT
- **Kafka** (`FCG.Payments.Infrastructure.Kafka`): Produção e consumo de eventos
  - Consumers: `UserCreatedEventConsumer`, `OrderPlacedEventConsumer`
  - Producers: `PaymentProcessedEventProducer`
- **MongoDb** (`FCG.Payments.Infrastructure.MongoDb`): Armazenamento de eventos (opcional)

#### 4️⃣ **Presentation** (`FCG.Payments.WebApi`)
- Controllers versionados (`/v1/...`)
- Middlewares customizados (Exception Handler, Correlation ID)
- Health Checks
- Swagger/OpenAPI

---

## 🛠️ Tecnologias e Bibliotecas

### Core Framework
- **.NET 8** - Framework principal
- **C# 12** - Linguagem de programação

### Comunicação Assíncrona
- **Apache Kafka** (`Confluent.Kafka 2.6.1`) - Mensageria para Event-Driven Architecture
- **MediatR** (`13.1.0`) - Mediator pattern para CQRS

### Persistência
- **Entity Framework Core 9.0** - ORM
- **SQL Server 2022** - Banco de dados relacional
- **Migrations** - Controle de versionamento do schema

### Segurança
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer 8.0.22`)
- **Authorization Policies** - Controle de acesso baseado em roles

### Validação e Qualidade
- **FluentValidation** (`12.1.0`) - Validação de objetos
- **Serilog** (`4.3.0`) - Logging estruturado
- **Seq** - Centralização de logs

### API e Documentação
- **Swagger/OpenAPI** (`Swashbuckle.AspNetCore 6.6.2`)
- **API Versioning** (`Asp.Versioning.Http 8.1.0`)

### Observabilidade
- **Health Checks** - Monitoramento de saúde da aplicação
- **Correlation ID** - Rastreamento de requisições

### Testes
- **xUnit** - Framework de testes
- **FluentAssertions** - Assertions fluentes
- **Testcontainers** - Testes de integração

---

## 💾 Modelo de Dados

### Tabela `Wallets`

```sql
CREATE TABLE Wallets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 1000.00,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT CK_Wallet_Balance CHECK (Balance >= 0),
    INDEX IX_Wallets_UserId (UserId)
);
```

**Campos:**
| Campo | Tipo | Descrição |
|-------|------|-----------|
| `Id` | UNIQUEIDENTIFIER | Identificador único da carteira (GUID) |
| `UserId` | UNIQUEIDENTIFIER | Identificador do usuário (cross-database, sem FK) |
| `Balance` | DECIMAL(18,2) | Saldo atual da carteira (não pode ser negativo) |
| `CreatedAt` | DATETIME2 | Data/hora de criação |
| `UpdatedAt` | DATETIME2 | Data/hora da última atualização |

⚠️ **Importante:** `UserId` não possui Foreign Key pois `Users` está em outro banco de dados (microserviço separado).

### Tabela `Payments`

```sql
CREATE TABLE Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    GameId UNIQUEIDENTIFIER NOT NULL,
    WalletId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    FailureReason NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2 NULL,

    CONSTRAINT FK_Payments_Wallets FOREIGN KEY (WalletId) 
        REFERENCES Wallets(Id),
    CONSTRAINT CK_Payment_Status CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    CONSTRAINT CK_Payment_Amount CHECK (Amount > 0),
    INDEX IX_Payments_UserId (UserId),
    INDEX IX_Payments_Status (Status),
    INDEX IX_Payments_CreatedAt (CreatedAt DESC)
);
```

**Campos:**
| Campo | Tipo | Descrição |
|-------|------|-----------|
| `Id` | UNIQUEIDENTIFIER | Identificador único do pagamento |
| `UserId` | UNIQUEIDENTIFIER | Identificador do usuário (sem FK - cross-database) |
| `GameId` | UNIQUEIDENTIFIER | Identificador do jogo (sem FK - cross-database) |
| `WalletId` | UNIQUEIDENTIFIER | Referência à carteira (com FK local) |
| `Amount` | DECIMAL(18,2) | Valor da transação |
| `Status` | NVARCHAR(20) | Status do pagamento: `Pending`, `Approved`, `Rejected` |
| `FailureReason` | NVARCHAR(500) | Motivo da rejeição (se aplicável) |
| `CreatedAt` | DATETIME2 | Data/hora de criação |
| `ProcessedAt` | DATETIME2 | Data/hora do processamento |

---

## 📐 Regras de Negócio

### RN-PAY-001: Criação de Wallet
- ✅ Wallet é criada **automaticamente** ao consumir `UserCreatedEvent`
- ✅ Saldo inicial: **R$ 1.000,00**
- ✅ Apenas **uma Wallet por UserId**
- ✅ **Idempotência**: verificar se Wallet já existe antes de criar
- ✅ Wallet não pode ser criada manualmente via API

### RN-PAY-002: Consulta de Saldo
- ✅ Endpoint: `GET /v1/wallets/{userId}/balance`
- ✅ Retornar saldo atual da Wallet
- ✅ Se Wallet não existir, retornar erro **404 Not Found**
- ✅ Usuário comum pode consultar apenas seu próprio saldo
- ✅ Admin pode consultar qualquer saldo

### RN-PAY-003: Processamento de Pagamento
**Fluxo de Processamento:**

1. ✅ Consumir `OrderPlacedEvent` (UserId, GameId, Amount)
2. ✅ Criar registro `Payment` com `Status = 'Pending'`
3. ✅ Validar se Wallet do usuário existe
4. ✅ Verificar se saldo é suficiente (`Balance >= Amount`)

**Se saldo suficiente:**
- ✅ Debitar valor da Wallet (`Balance = Balance - Amount`)
- ✅ Atualizar Payment: `Status = 'Approved'`, `ProcessedAt = GETUTCDATE()`
- ✅ Publicar `PaymentProcessedEvent` com `Status = 'Approved'`

**Se saldo insuficiente:**
- ✅ Atualizar Payment: `Status = 'Rejected'`, `FailureReason = "Saldo insuficiente"`
- ✅ Publicar `PaymentProcessedEvent` com `Status = 'Rejected'`

### RN-PAY-004: Transação Atômica
- ✅ **Débito da Wallet + Atualização do Payment** devem ser **atômicos** (mesma transação)
- ✅ Em caso de erro, fazer **rollback completo**
- ✅ Utilizar `IUnitOfWork` para gerenciar transação

### RN-PAY-005: Histórico de Pagamentos
- ✅ Endpoint: `GET /v1/payments/history`
- ✅ **Usuário comum**: listar apenas seus próprios pagamentos
- ✅ **Admin**: pode listar pagamentos de qualquer usuário
- ✅ Filtros disponíveis: `Status`, `DateFrom`, `DateTo`, `UserId` (admin)
- ✅ **Paginação obrigatória** (`pageNumber`, `pageSize`)
- ✅ Ordenação por `CreatedAt DESC` (mais recentes primeiro)

### RN-PAY-006: Idempotência de Eventos
- ✅ Se evento já foi processado, **ignorar** (evitar duplicação)
- ✅ Utilizar `CorrelationId` para rastreamento
- ✅ Verificar existência de registro antes de processar

### RN-PAY-007: Recarga de Saldo (Sprint 3 - Opcional)
- ✅ Endpoint: `POST /v1/wallets/{id}/deposit`
- ✅ **Apenas Admin** pode adicionar saldo na Wallet dos usuários
- ✅ Registrar transação de recarga no histórico
- ✅ Valor mínimo de recarga: **R$ 10,00**
- ✅ Valor máximo de recarga: **R$ 10.000,00**

---

## 🔌 Endpoints da API

### Carteira Digital (Wallets)

| Método | Endpoint | Autenticação | Autorização | Descrição |
|--------|----------|--------------|-------------|-----------|
| `GET` | `/v1/wallets/{userId}/balance` | ✅ Sim | User (próprio) ou Admin | Consultar saldo da carteira |
| `POST` | `/v1/wallets/{id}/deposit` | ✅ Sim | Admin | Recarregar saldo (opcional) |

**GET /v1/wallets/{userId}/balance**
```json
Response: 200 OK
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "walletId": "7b9e2c1a-8f4d-4e5b-9c3d-1a2b3c4d5e6f",
  "balance": 1250.50,
  "lastUpdated": "2026-01-18T14:30:00Z"
}
```

**POST /v1/wallets/{id}/deposit** _(Admin apenas)_
```json
Request:
{
  "amount": 500.00,
  "description": "Recarga manual - Promoção"
}

Response: 200 OK
{
  "walletId": "7b9e2c1a-8f4d-4e5b-9c3d-1a2b3c4d5e6f",
  "previousBalance": 1250.50,
  "newBalance": 1750.50,
  "depositedAmount": 500.00,
  "depositedAt": "2026-01-18T15:00:00Z"
}
```

### Pagamentos (Payments)

| Método | Endpoint | Autenticação | Autorização | Descrição |
|--------|----------|--------------|-------------|-----------|
| `GET` | `/v1/payments/history` | ✅ Sim | User (próprio) ou Admin | Histórico de pagamentos |

**GET /v1/payments/history?pageNumber=1&pageSize=10&status=Approved**
```json
Response: 200 OK
{
  "data": [
    {
      "id": "9c8b7a6d-5e4f-3d2c-1b0a-9f8e7d6c5b4a",
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "gameId": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",
      "amount": 49.90,
      "status": "Approved",
      "failureReason": null,
      "createdAt": "2026-01-18T10:30:00Z",
      "processedAt": "2026-01-18T10:30:05Z"
    },
    {
      "id": "8b7a6c5d-4e3f-2d1c-0b9a-8f7e6d5c4b3a",
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "gameId": "2b3c4d5e-6f7g-8h9i-0j1k-2l3m4n5o6p7q",
      "amount": 150.00,
      "status": "Rejected",
      "failureReason": "Saldo insuficiente",
      "createdAt": "2026-01-17T18:20:00Z",
      "processedAt": "2026-01-17T18:20:03Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 3,
  "totalRecords": 25
}
```

**Filtros disponíveis:**
- `status` (opcional): `Pending`, `Approved`, `Rejected`
- `dateFrom` (opcional): Data inicial (formato: `yyyy-MM-dd`)
- `dateTo` (opcional): Data final (formato: `yyyy-MM-dd`)
- `userId` (opcional - **Admin apenas**): Filtrar por usuário específico

---

## 📨 Eventos

A aplicação utiliza **Apache Kafka** para comunicação assíncrona baseada em eventos (Event-Driven Architecture).

### 📥 Eventos Consumidos

#### UserCreatedEvent

**Tópico Kafka:** `user-created`

```json
{
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "7b9e2c1a-8f4d-4e5b-9c3d-1a2b3c4d5e6f",
  "name": "João Silva",
  "email": "joao@example.com",
  "createdAt": "2026-01-18T10:30:00Z"
}
```

**Ação:**
- ✅ Criar `Wallet` com saldo inicial de **R$ 1.000,00**
- ✅ Verificar idempotência (não duplicar carteiras)

#### OrderPlacedEvent

**Tópico Kafka:** `order-placed`

```json
{
  "correlationId": "f1e2d3c4-b5a6-9870-1234-567890abcdef",
  "orderId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "userId": "7b9e2c1a-8f4d-4e5b-9c3d-1a2b3c4d5e6f",
  "gameId": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",
  "amount": 49.90,
  "createdAt": "2026-01-18T10:30:00Z"
}
```

**Ação:**
- ✅ Processar pagamento (verificar saldo e debitar)
- ✅ Criar registro `Payment` com status apropriado
- ✅ Publicar `PaymentProcessedEvent`

### 📤 Eventos Publicados

#### PaymentProcessedEvent

**Tópico Kafka:** `payment-processed`

```json
{
  "correlationId": "f1e2d3c4-b5a6-9870-1234-567890abcdef",
  "paymentId": "9c8b7a6d-5e4f-3d2c-1b0a-9f8e7d6c5b4a",
  "orderId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "userId": "7b9e2c1a-8f4d-4e5b-9c3d-1a2b3c4d5e6f",
  "gameId": "1a2b3c4d-5e6f-7g8h-9i0j-1k2l3m4n5o6p",
  "amount": 49.90,
  "status": "Approved",
  "processedAt": "2026-01-18T10:30:05Z"
}
```

**Status possíveis:**
- `Approved` - Pagamento aprovado (saldo suficiente)
- `Rejected` - Pagamento rejeitado (saldo insuficiente ou erro)

**Quando é disparado:**
- ✅ Ao processar com sucesso um `OrderPlacedEvent`
- ✅ Após debitar saldo da carteira (se aprovado)
- ✅ Após validar insuficiência de saldo (se rejeitado)

---

## ⚙️ Configuração de Ambiente

### Variáveis e Secrets Necessários

| Variável | Descrição | Obrigatório | Exemplo |
|----------|-----------|:-----------:|---------|
| `ConnectionStrings:DefaultConnection` | Connection string do SQL Server | ✅ Sim | `Server=localhost;Database=fcg_payments;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;` |
| `JwtSettings:SecretKey` | Chave secreta para assinatura JWT | ✅ Sim | `chave-base64-com-minimo-32-caracteres` |
| `KafkaSettings:SaslUsername` | Usuário SASL do Kafka (produção) | ⚠️ Produção | `$ConnectionString` |
| `KafkaSettings:SaslPassword` | Senha SASL do Kafka (produção) | ⚠️ Produção | `Endpoint=sb:...` |

### Pré-requisitos

- .NET 8 SDK
- Docker e Docker Compose
- SQL Server 2022
- Apache Kafka (via Docker)

### Configuração Local (user-secrets)

```bash
cd src/FCG.Payments.WebApi

# Inicializar user-secrets (já feito se o .csproj contém UserSecretsId)
dotnet user-secrets init

# Configurar os secrets obrigatórios
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Database=fcg_payments;User Id=sa;Password=SuaSenhaForte123;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:SecretKey" "sua-chave-secreta-jwt-com-minimo-32-caracteres"

# Secrets opcionais (Kafka SASL - apenas para ambiente com Event Hubs)
dotnet user-secrets set "KafkaSettings:SaslUsername" "$ConnectionString"
dotnet user-secrets set "KafkaSettings:SaslPassword" "Endpoint=sb:..."

# Verificar secrets configurados
dotnet user-secrets list
```

### Execução via Docker

1. Copie o arquivo `.env.example` para `.env`:
   ```bash
   cp .env.example .env
   ```

2. Preencha as variáveis no `.env`:
   ```env
   SA_PASSWORD=SuaSenhaForte123
   JWT_SECRET_KEY=sua-chave-secreta-jwt-com-minimo-32-caracteres
   ```

3. Suba os serviços:
   ```bash
   docker-compose up -d
   ```

### Arquivos que NUNCA devem ser commitados

| Arquivo | Motivo |
|---------|--------|
| `appsettings.Development.json` | Pode conter secrets locais |
| `appsettings.Production.json` | Contém configurações de produção |
| `appsettings.Docker.json` | Contém configurações de infraestrutura |
| `.env` | Contém senhas e chaves reais |
| `secrets.json` | Arquivo de secrets do .NET |

Esses arquivos já estão no `.gitignore` do repositório.