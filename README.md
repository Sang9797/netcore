# CQRS Order Service (.NET)

REST-first order and inventory service built on ASP.NET Core, EF Core, MediatR, PostgreSQL, Redis, RabbitMQ, and Hangfire.

This README is written for a new developer joining the project. It explains what the system does, how the code is organized, how the runtime pieces fit together, and where to start when you need to continue development.

## 1. What This Project Is

This service manages two related business areas:

- Orders
  - place an order
  - confirm an order
  - cancel an order
  - query order history
- Inventory
  - reserve stock for an order
  - release stock
  - adjust stock
  - query stock and reports

The project uses a CQRS-style structure:

- commands change state
- queries read data
- controllers are thin HTTP adapters
- business rules live in handlers and domain models
- infrastructure handles database, cache, auth, messaging, and scheduled jobs

## 2. High-Level Architecture

Dependency direction:

`Domain <- Application <- Infrastructure <- Presentation/API`

Meaning:

- `Domain`
  - core business types like `Order`, `OrderStatus`, `Money`
  - no HTTP, database, or framework-specific code
- `Application`
  - use cases
  - commands, queries, validators, handlers
  - interfaces for repositories, cache, auth, transactions, messaging
- `Infrastructure`
  - EF Core persistence
  - Redis cache
  - RabbitMQ publisher/consumer
  - Hangfire jobs
  - JWT-style auth support
- `Presentation`
  - ASP.NET Core controllers
  - request/response DTOs
  - maps HTTP requests to MediatR commands and queries

## 3. Main Components

### 3.1 API Layer

Entry point:

- `src/Cqrs.OrderService/Program.cs`

What it does:

- loads configuration
- builds the PostgreSQL connection string
- registers Application and Infrastructure services
- configures authentication and authorization
- exposes controller routes
- exposes health/info/openapi endpoints
- enables Hangfire dashboard and recurring jobs when configured

HTTP controllers:

- `Presentation/Auth/AuthController.cs`
  - login endpoint
- `Presentation/OrderController.cs`
  - order write and read endpoints
- `Presentation/InventoryController.cs`
  - inventory write and read endpoints
- `Presentation/BackgroundJobsController.cs`
  - admin endpoint to enqueue an outbox-dispatch job

### 3.2 Application Layer

Main folders:

- `Application/Command`
- `Application/Query`
- `Application/Handler`
- `Application/Auth`
- `Application/Common`
- `Application/Abstractions`
- `Application/IntegrationEvents`
- `Application/IntegrationEventHandlers`

Important patterns:

- `MediatR`
  - all controllers call `ISender.Send(...)`
- `FluentValidation`
  - validators run before handlers
- pipeline behaviors
  - `ValidationBehavior`
  - `TransactionBehavior`
- `FluentResults`
  - handlers return structured success/failure results

What this means for a new developer:

- if you add a new write operation, create a command + validator + handler
- if you add a new read operation, create a query + validator + handler
- keep controllers thin and avoid putting business logic there

### 3.3 Domain Layer

Main folder:

- `Domain/Model`

Examples:

- `Order`
- `OrderItem`
- `Money`
- `Inventory`
- `OrderStatus`
- `TransactionType`

Use the domain layer for:

- business state transitions
- invariants
- business terminology

Example:

- `Order.Cancel(...)` only allows cancellation from valid states
- `Order.Confirm()` enforces transition rules

### 3.4 Persistence Layer

Main files:

- `Infrastructure/Persistence/OrdersDbContext.cs`
- `Infrastructure/Persistence/Repositories/*.cs`
- `Infrastructure/Persistence/Entities/*.cs`

Database:

- PostgreSQL
- schema managed by Liquibase scripts under `src/Cqrs.OrderService/db/changelog`

Important note:

- in-app migrations are intentionally disabled
- schema changes are expected to go through Liquibase, not EF migrations

### 3.5 Caching

Main folder:

- `Infrastructure/Caching`

Behavior:

- Redis is optional
- when `Redis:Enabled=false`, no-op cache implementations are used
- query cache versioning is used so writes can invalidate cached reads

### 3.6 Messaging / Integration

Main folder:

- `Infrastructure/Integration`

Current messaging pattern:

- command handlers may write integration events to an outbox table
- background dispatch publishes outbox messages to RabbitMQ
- consumer can read RabbitMQ events and write audit/inbox records

Important classes:

- `OutboxWriter`
- `RabbitMqOutboxDispatcher`
- `RabbitMqEventConsumer`
- `RabbitMqMessagePublisher`
- `EfIntegrationEventAuditRepository`

Why this matters:

- do not publish RabbitMQ messages directly from controller code
- prefer writing events through the outbox pattern so persistence and event publication stay reliable

### 3.7 Scheduled Jobs

Main folder:

- `Infrastructure/Jobs`

Hangfire is used for background operational jobs when `Hangfire:Enabled=true`.

Current recurring jobs:

- `OutboxBacklogMonitorJob`
  - monitors unpublished/failed outbox backlog
- `AutoCancelStaleOrdersJob`
  - cancels old pending orders through the normal application command flow
- `DailyOperationsReportJob`
  - logs a daily summary
- `RetryFailedOutboxMessagesJob`
  - retries failed unpublished outbox messages
- `CleanupOldAuditRecordsJob`
  - deletes old audit records by retention policy

Dashboard:

- `GET /hangfire`
- requires authenticated `ROLE_ADMIN`

## 4. Runtime Dependencies

The project can run with different feature combinations.

Core dependency:

- PostgreSQL

Optional integrations:

- Redis
  - query caching
- RabbitMQ
  - outbox dispatch and event consumption
- Hangfire
  - recurring jobs and background job dashboard

Local full stack services are defined in `docker-compose.yml`:

- `app`
- `postgres`
- `pgbouncer`
- `redis`
- `rabbitmq`
- `liquibase`
- `nginx`
- `prometheus`
- `grafana`

## 5. Project Structure

```text
src/Cqrs.OrderService
├── Application
│   ├── Abstractions
│   ├── Auth
│   ├── Command
│   ├── Common
│   ├── DependencyInjection
│   ├── Handler
│   ├── IntegrationEventHandlers
│   ├── IntegrationEvents
│   └── Query
├── Domain
│   ├── Exception
│   └── Model
├── Infrastructure
│   ├── Caching
│   ├── DependencyInjection
│   ├── Integration
│   ├── Jobs
│   └── Persistence
├── Presentation
│   └── Auth
└── Program.cs
```

Other important top-level folders:

- `tests/Cqrs.OrderService.Tests`
  - test project
- `k6`
  - load/stress scripts
- `docker`
  - Prometheus, Grafana, PostgreSQL support files
- `scripts`
  - helper scripts such as Postman generation and pentest checks

## 6. Request Flow

Typical write request:

1. HTTP request enters a controller.
2. Controller maps the payload to a command.
3. MediatR sends the command.
4. FluentValidation runs.
5. Transaction behavior wraps the handler.
6. Handler loads domain data through repository interfaces.
7. Domain model changes state.
8. Repository persists changes.
9. Cache version is updated if needed.
10. Outbox event may be created.
11. Controller maps `Result<T>` to HTTP response.

Typical read request:

1. HTTP request enters a controller.
2. Controller maps query parameters to a query object.
3. MediatR sends the query.
4. Validator runs.
5. Handler fetches data through a read repository.
6. Redis cache may serve or store the response.
7. Controller returns DTOs.

## 7. Features by Area

### 7.1 Authentication

Endpoint:

- `POST /api/v1/auth/login`

Behavior:

- validates username/password against data in PostgreSQL
- returns a bearer token
- all business endpoints require authentication unless explicitly marked anonymous

Seed users from Liquibase:

- `admin / admin123`
- `john / userpass`

Roles:

- `ROLE_ADMIN`
- `ROLE_USER`

### 7.2 Orders

Endpoints:

- `POST /api/v1/orders`
- `GET /api/v1/orders/{orderId}`
- `GET /api/v1/orders?customerId=...`
- `POST /api/v1/orders/{orderId}/confirm`
- `DELETE /api/v1/orders/{orderId}`

Business notes:

- order creation requires at least one item
- only pending orders can be confirmed
- only pending or confirmed orders can be cancelled

### 7.3 Inventory

Endpoints:

- `GET /api/v1/inventory/report`
- `GET /api/v1/inventory/products/{productId}/stock`
- `GET /api/v1/inventory/low-stock`
- `POST /api/v1/inventory/reserve`
- `POST /api/v1/inventory/release`
- `POST /api/v1/inventory/adjust`

Business notes:

- inventory reporting is a read-model style use case
- stock mutations are explicit commands
- low-stock and report endpoints are good places to inspect cache behavior

### 7.4 Operational Endpoints

- `GET /actuator/health`
- `GET /actuator/health/liveness`
- `GET /actuator/info`
- `GET /actuator/prometheus`
- `GET /openapi/v1.json`
- `GET /hangfire` when enabled

## 8. How to Run the Project

Prerequisites:

- .NET SDK for `net10.0`
- Docker / Docker Compose
- `make`

Useful commands:

```bash
make help
make run
make migrate
make migrate-status
make test
make build
make docker-up
make docker-up-infra
make docker-down
```

Recommended first-time local flow:

1. Copy env defaults if needed:

```bash
cp -n .env.example .env
```

2. Run schema migrations:

```bash
make migrate
```

3. Start the app locally with supporting services:

```bash
make run
```

4. Login with the seeded admin account and call protected APIs.

Full stack with monitoring:

```bash
make docker-up
```

Useful local URLs:

- app: `http://localhost:8080`
- openapi document: `http://localhost:8080/openapi/v1.json`
- RabbitMQ management: `http://localhost:15672`
- Prometheus: `http://localhost:9091`
- Grafana: `http://localhost:3000`

## 9. Configuration Guide

Main config files:

- `src/Cqrs.OrderService/appsettings.json`
- `src/Cqrs.OrderService/appsettings.Development.json`
- `.env`
- environment variables from Docker Compose or shell

Important sections:

- `ConnectionStrings`
  - optional direct connection string override
- `Database`
  - `ApplyMigrations` exists, but schema changes are still expected through Liquibase
- `Jwt`
  - token secret and expiration
- `Redis`
  - enable/disable query cache
- `RabbitMq`
  - messaging config
- `Hangfire`
  - enable dashboard and recurring jobs

Development defaults:

- Redis enabled
- RabbitMQ enabled
- Hangfire enabled

Base appsettings defaults:

- Redis disabled
- RabbitMQ disabled
- Hangfire disabled

This split makes it easier to run the app with fewer dependencies in restricted environments while still keeping local development closer to production.

## 10. Testing and Quality Checks

Test command:

```bash
make test
```

Current automated test area visible in the repo:

- Redis cache integration behavior in `tests/Cqrs.OrderService.Tests/RedisQueryCacheIntegrationTests.cs`

Additional tooling:

- `make pentest`
- `make load-test`
- `make stress-test`

Load test scripts live in `k6/`.

## 11. Where to Make Changes

If you want to add a new feature, use this guide:

- add a new HTTP endpoint
  - update `Presentation`
  - create request/response DTOs
- add a new business action
  - create a command/query in `Application`
  - add validator
  - add handler
- change domain rules
  - update `Domain/Model`
  - keep invariants inside domain types when possible
- change persistence
  - update repository or EF mapping
  - add Liquibase scripts for schema changes
- add a background process
  - use `Infrastructure/Jobs` for recurring jobs
  - use `Infrastructure/Integration` for outbox/consumer flow
- add caching
  - implement through `Application.Abstractions.Caching`
  - update cache versioning on writes

## 12. Common Developer Rules for This Repo

- keep controllers thin
- prefer MediatR handlers for business logic
- use validators for request validation
- do not bypass the outbox pattern for integration events
- do not rely on EF migrations for schema rollout
- prefer configuration flags for optional infrastructure behavior
- when changing write behavior, check whether cache invalidation and integration events also need updates

## 13. Good Starting Points for a New Developer

If you are new to the codebase, read these files in this order:

1. `src/Cqrs.OrderService/Program.cs`
2. `src/Cqrs.OrderService/Application/DependencyInjection/ServiceCollectionExtensions.cs`
3. `src/Cqrs.OrderService/Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`
4. `src/Cqrs.OrderService/Presentation/OrderController.cs`
5. one command handler, for example `Application/Handler/Command/PlaceOrderCommandHandler.cs`
6. one query handler, for example `Application/Handler/Query/GetInventoryReportQueryHandler.cs`
7. `Infrastructure/Persistence/OrdersDbContext.cs`
8. `Infrastructure/Integration/*`
9. `Infrastructure/Jobs/*`

That path will help you understand startup, dependencies, request flow, persistence, integrations, and scheduled work without reading the whole repo at once.
