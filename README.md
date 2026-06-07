# CQRS Order Service (.NET) with Spring Boot Comparison

This project is a .NET implementation of a CQRS-style order and inventory service. It uses ASP.NET Core, Entity Framework Core, PostgreSQL, Liquibase, JWT authentication, and a simple command/query bus split.

The README is written for two audiences:

- someone trying to run or extend the service
- someone coming from Java Spring Boot and wanting a direct comparison

## What This Service Does

The service exposes:

- order commands: create, confirm, cancel
- inventory commands: reserve, release, adjust
- order queries
- inventory queries
- JWT-based authentication
- REST endpoints
- a lightweight GraphQL-like endpoint implemented manually
- health/info/prometheus endpoints

Main code lives in [src/Cqrs.OrderService](/home/sangle/codex/netcore/src/Cqrs.OrderService).

## Tech Stack

- .NET `10.0`
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Liquibase for schema migrations
- PgBouncer for connection pooling
- Prometheus + Grafana for monitoring
- Docker Compose for local stack

## Project Structure

```text
src/Cqrs.OrderService
├── Application
│   ├── Command
│   ├── Handler
│   │   ├── Command
│   │   └── Query
│   └── Query
├── Bus
│   ├── Command
│   └── Query
├── Domain
│   ├── Exception
│   └── Model
├── Infrastructure
│   ├── DependencyInjection
│   └── Persistence
├── Presentation
│   ├── Auth
│   └── GraphQl
├── db
│   └── changelog
└── Program.cs
```

## Architectural Overview

The service is split into four main layers:

- `Presentation`: controllers, request/response DTOs, exception mapping
- `Application`: commands, queries, handlers
- `Domain`: business models and domain rules
- `Infrastructure`: EF Core persistence, authentication, DI wiring

The runtime flow for a write request is:

1. controller receives HTTP request
2. controller builds command object
3. `CommandBus` resolves the matching command handler
4. handler loads/modifies domain model through repository interfaces
5. EF Core persists changes to PostgreSQL
6. `CommandBus` commits or rolls back the transaction

The runtime flow for a read request is:

1. controller receives HTTP request
2. controller builds query object
3. `QueryBus` resolves the matching query handler
4. query handler reads from repository
5. handler returns DTO-oriented read model

## How To Run

### Prerequisites

- .NET SDK compatible with `net10.0`
- Docker and Docker Compose
- `make`

### Local Run

```bash
make run
```

What `make run` does:

1. applies Liquibase migrations
2. starts PostgreSQL and PgBouncer via Docker
3. runs the ASP.NET Core app on `http://localhost:8080`

### Full Stack

```bash
make docker-up
```

Useful URLs:

- App: `http://localhost:8080`
- OpenAPI JSON: `http://localhost:8080/openapi/v1.json`
- Prometheus: `http://localhost:9091`
- Grafana: `http://localhost:3000`

### Tests

```bash
make test
```

### Build

```bash
make build
```

## Configuration

Environment defaults come from `.env.example`.

Important variables:

- `DB_HOST`
- `DB_PORT`
- `DB_NAME`
- `DB_USERNAME`
- `DB_PASSWORD`
- `APP_JWT_SECRET`
- `APP_JWT_EXPIRATION_MS`

For local `make run`, the app uses PgBouncer on:

- `LOCAL_DB_HOST=localhost`
- `LOCAL_DB_PORT=5433`

## Main Endpoints

### Authentication

- `POST /api/v1/auth/login`

### Orders

- `POST /api/v1/orders`
- `GET /api/v1/orders/{orderId}`
- `GET /api/v1/orders?customerId=...`
- `POST /api/v1/orders/{orderId}/confirm`
- `DELETE /api/v1/orders/{orderId}`

### Inventory

- `GET /api/v1/inventory/report`
- `GET /api/v1/inventory/products/{productId}/stock`
- `GET /api/v1/inventory/low-stock`
- `POST /api/v1/inventory/reserve`
- `POST /api/v1/inventory/release`
- `POST /api/v1/inventory/adjust`

### Operational Endpoints

- `GET /actuator/health`
- `GET /actuator/health/liveness`
- `GET /actuator/info`
- `GET /actuator/prometheus`

### GraphQL-like Endpoint

- `POST /graphql`
- `GET /graphiql`

This is not using Hot Chocolate or another full .NET GraphQL engine. The controller parses a limited set of query strings manually.

## Dependency Injection and Service Registration

The application uses ASP.NET Core DI.

In `Program.cs`, infrastructure services are registered directly, and application services are registered through:

```csharp
builder.Services.AddApplicationServices();
```

That registration scans the assembly and auto-registers:

- classes ending with `Service`
- classes ending with `Repository`
- implementations of `ICommandHandler<,>`
- implementations of `IQueryHandler<,>`

This means new handlers usually do not require manual registration in `Program.cs`.

## Transaction Model

This project now applies the transaction boundary in `CommandBus`.

That means one command dispatch maps to one EF Core database transaction.

Conceptually:

```text
Controller
  -> CommandBus
      -> BeginTransaction
      -> Handler
      -> Repository SaveChanges
      -> Commit or Rollback
```

This is the closest current equivalent to putting `@Transactional` on a Spring service method.

Important limitation:

- database rollback only affects database work
- external calls such as HTTP, Kafka, email, payments, or another service are not part of the database transaction

If you need reliable external side effects, use an outbox pattern rather than doing external calls inside the transactional command path.

## Detailed Comparison with Java Spring Boot

This section is the main mapping for a Java developer.

### 1. Entry Point and Bootstrapping

#### Spring Boot

Usually:

```java
@SpringBootApplication
public class OrderServiceApplication {
    public static void main(String[] args) {
        SpringApplication.run(OrderServiceApplication.class, args);
    }
}
```

#### This .NET Project

Bootstrapping is in `Program.cs` using the minimal hosting model.

Rough equivalent:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
```

#### Mapping

- `@SpringBootApplication` -> `WebApplication.CreateBuilder(args)`
- Spring auto configuration -> ASP.NET Core setup in `Program.cs`
- `SpringApplication.run(...)` -> `app.Run()`

Difference:

Spring hides more startup configuration behind annotations and auto-configuration. ASP.NET Core usually keeps more of it explicit in `Program.cs`.

### 2. Controllers

#### Spring Boot

Typical style:

```java
@RestController
@RequestMapping("/api/v1/orders")
public class OrderController { }
```

#### This .NET Project

Typical style:

```csharp
[ApiController]
[Authorize]
[Route("api/v1/orders")]
public sealed class OrderController : ControllerBase
{
}
```

#### Mapping

- `@RestController` -> `[ApiController]` + `ControllerBase`
- `@RequestMapping` -> `[Route]`
- `@GetMapping`, `@PostMapping`, `@DeleteMapping` -> `[HttpGet]`, `[HttpPost]`, `[HttpDelete]`
- `@RequestBody` -> `[FromBody]`
- `@RequestParam` -> `[FromQuery]`
- `@PathVariable` -> action method parameter bound from route

Difference:

The programming model is very similar. The biggest difference is syntax and attribute naming.

### 3. Dependency Injection

#### Spring Boot

Typical service bean:

```java
@Service
public class OrderService { }
```

Typical repository bean:

```java
@Repository
public class OrderRepository { }
```

#### This .NET Project

There are no Java-style stereotype annotations like `@Service` or `@Repository`.

Instead, registration is done through the ASP.NET Core DI container.

This project now uses assembly scanning in `AddApplicationServices()` to register:

- `*Service`
- `*Repository`
- command handlers
- query handlers

#### Mapping

- `@Component`, `@Service`, `@Repository` -> `builder.Services.AddScoped(...)` or assembly scan registration
- constructor injection in Spring -> primary constructor injection in C#

Difference:

Spring commonly discovers beans by annotation scanning. In .NET, explicit registration is more common, though assembly scanning is possible and now used here for app-level types.

### 4. Service Layer vs Command Handler Layer

#### Spring Boot

A common layered design is:

- controller
- service
- repository

#### This .NET Project

The write path is:

- controller
- `CommandBus`
- command handler
- repository

The read path is:

- controller
- `QueryBus`
- query handler
- repository

#### Mapping

- Spring service method -> command handler or query handler
- service dispatcher patterns in Java -> `CommandBus` / `QueryBus`

Difference:

This project is explicitly CQRS-oriented. Spring applications can do CQRS too, but many CRUD-oriented Spring services do not separate read and write flows this clearly.

### 5. Transactions

#### Spring Boot

Typical:

```java
@Transactional
public void reserveInventory(...) {
    repository.save(...);
    auditRepository.save(...);
}
```

Spring creates a proxy and opens a transaction around the method.

#### This .NET Project

The command transaction is currently managed in `CommandBus`.

Conceptually:

```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
var result = await handler.Handle(command, cancellationToken);
await transaction.CommitAsync(cancellationToken);
```

#### Mapping

- `@Transactional` on service method -> transaction started around command dispatch
- `PlatformTransactionManager` / JPA transaction handling -> EF Core transaction API

Important difference:

Spring developers often assume the transaction is attached to the service method via annotation. In this project, the transaction is attached to the command execution pipeline. The effect is similar, but the place where the rule lives is different.

### 6. JPA/Hibernate vs EF Core

#### Spring Boot

Common stack:

- Spring Data JPA
- Hibernate
- `JpaRepository`
- `@Entity`

#### This .NET Project

Common stack here:

- EF Core
- `DbContext`
- `DbSet<T>`
- fluent mapping in `OnModelCreating`

#### Mapping

- `EntityManager` -> `DbContext`
- `JpaRepository` -> custom repository backed by `DbContext`
- `@Entity` / `@Table` / `@Column` -> EF entity classes plus fluent config
- `save()` -> `SaveChangesAsync()`
- lazy or eager fetch strategies -> explicit `Include(...)` / `AsNoTracking()`

Difference:

This project does not use a generated repository abstraction like Spring Data JPA. Repositories are handwritten. That gives more control, but less out-of-the-box convenience.

### 7. Exception Handling

#### Spring Boot

Typical:

```java
@ControllerAdvice
public class GlobalExceptionHandler { }
```

#### This .NET Project

Global exception handling is implemented with ASP.NET Core middleware in `GlobalExceptionHandler.cs`.

#### Mapping

- `@ControllerAdvice` -> exception-handling middleware
- `@ExceptionHandler(...)` -> exception type pattern matching and response mapping

Difference:

Spring often centralizes this through annotated advice classes. ASP.NET Core commonly does this in middleware.

### 8. Validation

#### Spring Boot

Typical:

```java
public ResponseEntity<?> create(@Valid @RequestBody CreateOrderRequest request)
```

with annotations like `@NotNull`, `@Min`, `@Size`.

#### This .NET Project

Request DTOs use data annotations such as:

- `[Required]`
- `[Range]`
- `[MinLength]`

#### Mapping

- `@Valid` + Jakarta Bean Validation -> `[ApiController]` + data annotation validation

Difference:

The idea is the same. The annotations and runtime integration are different.

### 9. Security

#### Spring Boot

Typical stack:

- Spring Security
- `SecurityFilterChain`
- JWT filter or OAuth2 resource server

#### This .NET Project

The app uses ASP.NET Core authentication/authorization with a custom bearer token handler:

- `builder.Services.AddAuthentication("Bearer")`
- custom `HmacJwtAuthenticationHandler`
- custom `JwtTokenService`

#### Mapping

- Spring Security filter chain -> ASP.NET Core authentication middleware
- custom JWT filter -> custom `AuthenticationHandler`
- `@PreAuthorize` or role checks -> `[Authorize]` plus claim/role checks

Difference:

This implementation is intentionally lightweight and custom. It is not using a full external identity provider or the richer policy model you may expect in larger Spring Security deployments.

### 10. Configuration

#### Spring Boot

Typical:

- `application.yml`
- `application-dev.yml`
- `@ConfigurationProperties`

#### This .NET Project

Typical:

- `appsettings.json`
- `appsettings.Development.json`
- environment variables
- `IConfiguration`

#### Mapping

- `application.yml` -> `appsettings.json`
- Spring profiles -> ASP.NET Core environments
- `@ConfigurationProperties` -> options/config binding

Difference:

This project still reads some values directly from `IConfiguration`, especially JWT settings, rather than using strongly-typed options end-to-end.

### 11. Database Migrations

#### Spring Boot

Common choices:

- Flyway
- Liquibase

#### This .NET Project

The project uses Liquibase through Docker Compose and `make migrate`.

#### Mapping

- Spring + Liquibase auto-run on startup -> explicit Liquibase container execution

Difference:

This repo keeps migrations outside EF Core migrations and runs them as an operational concern, which is often a cleaner choice for teams that want SQL-first schema management.

### 12. Observability

#### Spring Boot

Typical:

- Actuator
- Micrometer
- Prometheus endpoint

#### This .NET Project

The app exposes Spring-style operational paths:

- `/actuator/health`
- `/actuator/health/liveness`
- `/actuator/info`
- `/actuator/prometheus`

#### Mapping

- Spring Actuator endpoints -> ASP.NET Core mapped endpoints

Difference:

The endpoint names intentionally resemble Spring Boot, but the implementation is custom and lighter.

### 13. Streams, Lambdas, and Functional Interfaces

This is another place where the concepts are close, but the syntax and standard library are different.

#### Java / Spring Boot

Typical Java code often uses:

- `Stream<T>`
- lambda expressions like `x -> x.getName()`
- functional interfaces such as `Function<T, R>`, `Consumer<T>`, `Predicate<T>`, `Supplier<T>`

Example:

```java
List<OrderItemDto> items = order.getItems().stream()
    .filter(i -> i.getQuantity() > 0)
    .map(i -> new OrderItemDto(i.getProductId(), i.getQuantity()))
    .toList();
```

#### This .NET Project

The equivalent style in C# is usually:

- LINQ over `IEnumerable<T>` or `IQueryable<T>`
- lambda expressions like `i => i.ProductId`
- delegates such as `Func<T, TResult>`, `Action<T>`, `Predicate<T>`

Example from the same style used in this project:

```csharp
var items = command.Items
    .Select(i => new OrderItem(
        i.ProductId,
        i.ProductName,
        i.Quantity,
        new Money(i.UnitPrice, i.Currency)))
    .ToList();
```

#### Mapping

- Java `Stream<T>` -> C# LINQ over `IEnumerable<T>` / `IQueryable<T>`
- Java `Function<T, R>` -> C# `Func<T, TResult>`
- Java `Consumer<T>` -> C# `Action<T>`
- Java `Predicate<T>` -> C# `Predicate<T>` or `Func<T, bool>`
- Java `Supplier<T>` -> C# `Func<T>`
- Java method reference `User::getName` -> C# method group or lambda like `u => u.Name`

#### How It Appears in This Codebase

This project uses LINQ heavily in places where a Java developer would expect streams:

- DTO mapping with `.Select(...)`
- filtering with `.Where(...)`
- sorting with `.OrderByDescending(...)`
- materialization with `.ToList()`
- set creation with `.ToHashSet(...)`

Examples in the current code:

- controller request mapping:

```csharp
request.Items
    .Select(i => new OrderItemCommand(
        i.ProductId,
        i.ProductName,
        i.Quantity,
        i.UnitPrice,
        i.Currency))
    .ToList();
```

- repository query composition:

```csharp
dbContext.Orders
    .AsNoTracking()
    .Include(o => o.Items)
    .Where(o => o.CustomerId == customerId)
    .OrderByDescending(o => o.CreatedAt)
    .Select(o => ToDomain(o))
    .ToListAsync(cancellationToken);
```

#### Important Difference: `IQueryable` vs `Stream`

This matters more than the syntax.

In Java, a stream usually runs in memory over objects already loaded into the JVM.

In EF Core, LINQ on `IQueryable<T>` is not just an in-memory pipeline. It is an expression tree that EF Core translates into SQL. That means:

- `.Where(...)`, `.Select(...)`, `.OrderBy(...)` can become SQL
- execution is deferred until `ToListAsync()`, `SingleAsync()`, and similar terminal calls
- not every C# method can be translated to SQL

So the closer comparison is:

- Java Stream API -> C# LINQ syntax
- Spring Data JPA query derivation / JPQL execution -> EF Core LINQ-to-SQL translation

That is why repository code in this project uses EF-friendly expressions inside queries and keeps custom object mapping simple.

#### Functional Interfaces vs Handler Interfaces

Java developers often use "functional interface" in two different senses:

1. standard function-like interfaces such as `Function`, `Predicate`, `Consumer`
2. application-specific single-method interfaces

This project has the second kind in several places:

- `ICommandHandler<TCommand, TResult>`
- `IQueryHandler<TQuery, TResult>`

Those are not usually treated as inline lambda targets here. They are DI-resolved application contracts.

Java equivalent:

```java
public interface CommandHandler<C, R> {
    R handle(C command);
}
```

C# version in this project:

```csharp
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}
```

#### Practical Translation Rule for a Spring Developer

When you read this .NET codebase:

- think "stream pipeline" when you see LINQ chains like `.Where(...).Select(...).ToList()`
- think "functional interface" when you see `Func<>`, `Action<>`, or `Predicate<>`
- think "application service contract" when you see `ICommandHandler<,>` or `IQueryHandler<,>`

Those are related ideas, but they are not interchangeable.

## Mental Model for a Spring Boot Developer

If you normally think in Spring terms, use this translation table:

| Spring Boot / Java | This Project / .NET |
|---|---|
| `@SpringBootApplication` | `Program.cs` + `WebApplication` |
| `@RestController` | `[ApiController]` controller |
| `@Service` | handler or service class registered in DI |
| `@Repository` | repository class registered in DI |
| `@Transactional` | transaction around `CommandBus.Dispatch(...)` |
| Spring Data JPA repository | handwritten EF Core repository |
| `EntityManager` | `DbContext` |
| `@ControllerAdvice` | exception middleware |
| Spring Security | ASP.NET Core auth middleware |
| `application.yml` | `appsettings.json` + env vars |
| Actuator | manually mapped `/actuator/*` endpoints |
| Java Stream API | LINQ over `IEnumerable<T>` / `IQueryable<T>` |
| `Function` / `Consumer` / `Predicate` | `Func<>` / `Action<>` / `Predicate<>` |

## Important Differences You Should Not Ignore

These are the places where Java instincts can mislead you:

1. There is no annotation-driven bean model like Spring stereotypes by default.
2. There is no annotation-based `@Transactional` in this codebase.
3. Transactions are currently centered in the command pipeline, not at arbitrary service methods.
4. Repositories are manual EF Core repositories, not Spring Data generated repositories.
5. The GraphQL endpoint is not a full GraphQL server framework.
6. Liquibase is externalized through Docker tooling, not embedded startup behavior.

## External Calls and Transaction Safety

If you need to call another service, do not place the HTTP call inside the same database transaction and assume it is "all or nothing".

Recommended approach:

1. handle command
2. write domain state
3. write outbox message in the same DB transaction
4. commit
5. process outbox asynchronously and call external system

That is the right equivalent of a production-safe integration flow in both Spring Boot and ASP.NET Core.

## Current State and Extension Guidance

If you extend this service, keep these rules:

- add new write use cases as commands + command handlers
- add new read use cases as queries + query handlers
- keep controller code thin
- keep domain rules in domain models or application handlers
- keep EF details in repositories
- keep external integrations outside the transactional command path

If you add new handlers or `*Service` / `*Repository` classes, they should be auto-registered by the assembly scanning extension.

## Suggested Next Improvements

The codebase is workable, but these would improve it materially:

- add real integration tests for transaction rollback behavior
- introduce an outbox for reliable external calls
- replace manual GraphQL parsing with a proper GraphQL library if that surface grows
- expand test coverage beyond the current placeholder test project
- move JWT config to fully typed options binding

## Quick Summary

This service is best understood as:

- ASP.NET Core instead of Spring MVC
- EF Core instead of JPA/Hibernate
- explicit CQRS handlers instead of a broad service layer
- command-pipeline transactions instead of `@Transactional`
- Docker/Liquibase-driven operations instead of everything hidden in framework startup

If you come from Spring Boot, the concepts are familiar. The main difference is that .NET makes more of the wiring visible, and this repo expresses transaction and CQRS behavior in code structure rather than Java annotations.
