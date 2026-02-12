---
description: Review a file or directory for architecture rule violations. Checks aggregate boundaries, layer dependencies, missing value objects, and other rules defined in CLAUDE.md.
---

Review the following for architecture violations against our CLAUDE.md rules: $ARGUMENTS

Check specifically for:
1. Navigation properties crossing aggregate boundaries (should be ID references only)
2. Domain layer depending on infrastructure types (EF Core, MediatR in domain entities)
3. Business logic in controllers (should be in handlers)
4. Thrown exceptions for business logic (should use ErrorOr)
5. Primitive obsession (strings/ints where value objects should exist)
6. Missing sealed modifier on classes that don't need inheritance
7. AutoMapper or reflection-based mapping usage
8. EF configurations in OnModelCreating instead of IEntityTypeConfiguration

For each violation found:
- State the rule being violated
- Show the specific line(s)
- Suggest the fix
- Rate severity: CRITICAL (breaks architecture) / WARNING (technical debt) / SUGGESTION (improvement)
