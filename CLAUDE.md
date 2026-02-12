# MechanicApi — Project Context

## What This Is
Full-stack application: ASP.NET Core backend (clean architecture, CQRS) + Blazor frontend.
Domain: automotive mechanic shop management — work orders, customers, vehicles, parts, invoicing.

## Tech Stack
- **Backend:** ASP.NET Core 8, Entity Framework Core 8, MediatR, ErrorOr, FluentValidation
- **Frontend:** Blazor (Server or WASM — TBD, learning phase)
- **Testing:** xUnit, Testcontainers (PostgreSQL), FluentAssertions
- **DB:** PostgreSQL

## Project Structure
```
src/
  MechanicApi.Domain/         # Entities, value objects, domain events, enums
  MechanicApi.Application/    # Commands, queries, handlers, validation, behaviors
  MechanicApi.Infrastructure/ # EF Core, repositories, external services
  MechanicApi.Api/            # Controllers (thin), DI composition root
  MechanicApi.Contracts/      # Request/response DTOs
  MechanicApi.Web/            # Blazor frontend (planned)
tests/
  MechanicApi.Domain.Tests/
  MechanicApi.Application.Tests/
  MechanicApi.Infrastructure.Tests/   # Integration tests with Testcontainers
  MechanicApi.Api.Tests/
```

## Commands
```bash
dotnet build src/MechanicApi.sln
dotnet test tests/MechanicApi.Domain.Tests
dotnet test tests/MechanicApi.Application.Tests
dotnet test tests/MechanicApi.Infrastructure.Tests  # requires Docker
dotnet ef migrations add <Name> --project src/MechanicApi.Infrastructure --startup-project src/MechanicApi.Api
dotnet ef database update --project src/MechanicApi.Infrastructure --startup-project src/MechanicApi.Api
```

## Architecture Rules — IMPORTANT

1. **Aggregates are boundaries.** No navigation properties between aggregates. Reference by ID only.
2. **Domain layer has zero dependencies.** No EF Core, no MediatR, no infrastructure types.
3. **Commands and queries go through MediatR.** Controllers are thin — they map HTTP to commands/queries, nothing else.
4. **ErrorOr for all operation results.** Never throw exceptions for business logic failures. Return `ErrorOr<T>`.
5. **Validation pipeline:** FluentValidation validators → MediatR behavior pipeline → handler. Handlers assume valid input.
6. **Domain events** for cross-aggregate side effects. Never call another aggregate's methods directly from a handler.
7. **Value objects** for domain concepts (Money, Address, VIN, PhoneNumber). No primitive obsession.
8. **Sealed classes by default.** Only unseal when inheritance is specifically needed.
9. **No AutoMapper.** Manual mapping in handlers or explicit extension methods.
10. **EF Core configurations** in separate `IEntityTypeConfiguration<T>` classes, never in `OnModelCreating`.

## Code Style
- Records for DTOs and value objects where immutability is the default
- `readonly struct` for small, frequently-allocated value types
- Nullable reference types enabled — no `null` where `Option`/`ErrorOr` fits better
- Use `OneOf` or discriminated union patterns where exhaustive matching matters
- Prefer `IReadOnlyList<T>` in public APIs, `List<T>` internally
- Private constructors + static factory methods for entities with invariants

## Engagement Modes

This project uses three distinct collaboration styles. Match the mode to the conversation:

### Mode 1: Thinking Partner (default for existing backend work)
When I ask about **concepts, patterns, trade-offs, or architecture decisions** — DO NOT write code immediately. Instead:
- Explain the concept deeply with the underlying "why"
- Present trade-offs between approaches (with concrete consequences, not hand-waving)
- Connect it to what we already have in this codebase
- Use small illustrative snippets only to clarify a point, not as implementation
- Wait for my explicit "OK, implement it" or "let's do it" before producing code
- If I say "think about X" or "let's discuss X" — this is always Mode 1

### Mode 2: Blazor Learning Guide
When I ask about **Blazor concepts or frontend implementation** — I am learning this from scratch. Assume I know ASP.NET Core well but know nothing about Blazor. Always:
- Explain the Blazor-specific mental model first (component lifecycle, render tree, etc.)
- Map Blazor concepts to ASP.NET equivalents I already know when possible
- Show how it connects to our existing backend (API calls, shared contracts)
- Warn about Blazor-specific gotchas before I hit them
- Explain rendering modes (Server vs WASM vs Auto) trade-offs when relevant

### Mode 3: Cross-Framework Translator
When I ask to **compare frameworks or see how something works in React/Angular/Spring** — explain by translation:
- Start from the C#/ASP.NET concept I already understand
- Show the exact equivalent in the target framework
- Highlight where the mental model differs (not just syntax differences)
- Note where the target framework is genuinely better or worse for that pattern
- This is for learning, not for shipping — prioritize understanding over production-readiness

## Quality Gates — ALWAYS enforce these

Before considering any code change "done":
1. `dotnet build` must pass with zero warnings
2. All existing tests must pass
3. New public behavior must have corresponding tests
4. No `// TODO` without a linked issue or explicit discussion
5. Architecture rules above must not be violated — if a change requires bending a rule, discuss it first (Mode 1)
