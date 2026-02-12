---
name: implement
description: Implementation mode — write production code following the architecture rules and quality gates. Use when the user explicitly says "implement it", "let's do it", "code it", "build it", or gives a clear implementation instruction after a design discussion.
disable-model-invocation: true
---

# Implementation Mode

The user has finished discussing and is ready for code. Produce production-quality implementation following the project's architecture and quality gates.

## Pre-Implementation Checklist

Before writing code, verify:
1. Which layer(s) does this change touch? (Domain → Application → Infrastructure → API)
2. Does it require a new migration?
3. Does it affect existing tests?
4. Are there aggregate boundary implications?

## Implementation Order

Always implement bottom-up:
1. **Domain layer first** — entities, value objects, domain events
2. **Application layer** — commands/queries, handlers, validators
3. **Infrastructure** — EF Core configurations, repository updates, migrations
4. **API layer** — controller endpoints, request/response mapping
5. **Tests** — unit tests for domain logic, integration tests for handlers

## Code Standards (from CLAUDE.md)

- ErrorOr<T> for all results, never throw for business logic
- FluentValidation in MediatR pipeline behavior
- Manual mapping (no AutoMapper)
- Sealed classes by default
- Value objects for domain concepts
- IEntityTypeConfiguration<T> for EF configs
- Private constructors + static factory methods for entities

## After Implementation

1. Run `dotnet build` — must pass with zero warnings
2. Run relevant tests — must all pass
3. Write new tests for new behavior
4. Summarize what was changed and why (for the developer's learning)
