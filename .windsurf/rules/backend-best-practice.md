---
trigger: glob
description: When working with any of our backend tech stack, when working inside /apps/api folder, or when manually use.
globs: apps/api/**/*.cs
---

# TaskHub Backend: Best Practices & Style Guide

This document outlines the architectural patterns, coding conventions, and best practices for the ASP.NET Core backend of TaskHub. Adherence to these guidelines is mandatory to ensure code quality, maintainability, and consistency.

---

## 1. Core Principles

- **Single Responsibility Principle (SRP)**: Each class and method should have one, and only one, reason to change.
- **Dependency Injection (DI)**: Use constructor injection for all dependencies (services, repositories, loggers). Avoid service locator patterns.
- **Asynchronous by Default**: Use `async`/`await` for all I/O-bound operations (database, network calls) to ensure the application is scalable and responsive. All controller actions and service methods should return a `Task<T>`.
- **Immutability**: Prefer immutable DTOs and models where possible to prevent unintended side effects.

---

## 2. Architecture & Project Structure

We follow a clean, feature-oriented architecture within the `apps/api` project.

```
api/
├── src/
│   ├── Controllers/      # Minimal API endpoints, grouped by feature
│   ├── Core/             # Core business logic and domain entities
│   │   ├── Entities/     # EF Core data models (PascalCase)
│   │   └── Interfaces/   # Abstractions for services and repositories
│   ├── Infrastructure/   # Implementations of interfaces
│   │   ├── Data/         # EF Core DbContext and migrations
│   │   └── Repositories/ # Data access logic implementation
│   ├── Services/         # Business logic implementation
│   ├── Hubs/             # SignalR hubs for real-time features
│   └── Program.cs        # Service registration, middleware pipeline
└── tests/                # xUnit tests mirroring the src structure
```

---

## 3. API Design (Minimal APIs)

- **Grouping**: Group related endpoints using `MapGroup("/api/feature")`.
- **Verbs**: Use correct HTTP verbs: `GET` (retrieve), `POST` (create), `PUT` (update/replace), `DELETE` (remove).
- **Naming**: Endpoints should be plural nouns (e.g., `/api/boards`, `/api/cards`).
- **Responses**: Return `TypedResults` (e.g., `Results.Ok<T>()`, `Results.NotFound()`) for compile-time type safety and OpenAPI generation.
- **DTOs**: Use Data Transfer Objects (DTOs) for all API inputs and outputs to decouple the API contract from the internal data model.

---

## 4. Data Access (Entity Framework Core 9)

- **Repository Pattern**: Abstract data access logic behind repository interfaces (e.g., `ICardRepository`). This isolates EF Core from business logic and simplifies testing.
- **Querying**: Use LINQ for queries. Leverage `AsNoTracking()` for read-only operations to improve performance.
- **Migrations**: Database schema changes must be managed via EF Core migrations. Column names in the database must be `snake_case`.
- **Model Configuration**: Use `IEntityTypeConfiguration<T>` to configure entities, including the mapping from `PascalCase` properties to `snake_case` column names.

---

## 5. Testing (xUnit)

- **Test-Driven Development (TDD)**: Write a failing test before writing implementation code. Follow the Red-Green-Refactor cycle.
- **Arrange-Act-Assert**: Structure tests clearly with these three sections.
- **Unit Tests**: Focus on a single class or method. Mock all external dependencies (e.g., using `Moq` or `NSubstitute`).
- **Integration Tests**: Test interactions between components (e.g., API endpoint to database). Use `WebApplicationFactory` for in-memory testing of the API and an in-memory database (like SQLite) or a test container for the database to ensure isolation.
- **Naming**: `[MethodName]_[Scenario]_[ExpectedBehavior]` (e.g., `CreateBoard_WithValidData_ReturnsCreatedResult`).

---

## 6. Error Handling & Logging

- **Structured Logging**: Use **Serilog** for all logging. Enrich logs with context (e.g., `CorrelationId`, `UserId`).
- **Exception Middleware**: Implement a global exception handling middleware to catch unhandled exceptions. It should log the full exception and return a standardized, user-friendly error response.
- **Validation**: Use a library like **FluentValidation** to validate incoming DTOs. Validation failures should return a `400 Bad Request` with a structured error object.

---

## 7. Naming & Coding Style

- **Classes, Methods, Properties**: `PascalCase` (e.g., `BoardService`, `GetUserAsync`, `CreatedAt`).
- **Local Variables & Parameters**: `camelCase` (e.g., `var boardId = ...`).
- **Interfaces**: Prefix with `I` (e.g., `IBoardService`).
- **Private Fields**: Prefix with an underscore `_` (e.g., `private readonly ILogger<BoardService> _logger;`).
- **File Naming**: Match the primary class name (e.g., `BoardService.cs`).

- **Explicit Namespace Qualification**: To enhance clarity and prevent type collisions, all generated C# code **must** use fully qualified type names for any class, struct, interface, or enum that is not a primitive type (`string`, `int`, etc.).
    -   **Correct**: `public Microsoft.AspNetCore.Mvc.IActionResult GetTask(System.Guid id) { ... }`
    -   **Incorrect**: `public IActionResult GetTask(Guid id) { ... }`
    -   **Exception**: This is not required for primitive types, types within the same namespace, or when generating a complete file from scratch where `using` statements are present at the top.
