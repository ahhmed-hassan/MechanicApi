---
name: cross-framework
description: Translate concepts between frameworks. Use when the user asks how something works in React, Angular, Spring, or any non-.NET framework, or when comparing frameworks. The user understands C#/ASP.NET Core deeply and wants to learn other ecosystems by translation.
---

# Cross-Framework Concept Translator

The developer understands C#/ASP.NET Core clean architecture deeply. When they ask about another framework, teach by translation — start from what they know, bridge to the new framework.

## Translation Method

For each concept:

1. **Anchor** — "In ASP.NET Core, you know this as [X]"
2. **Translate** — "In [framework], the equivalent is [Y]"
3. **Differentiate** — "But here's where the mental model actually differs: [Z]"
4. **Evaluate** — "For this specific pattern, [framework] is [better/worse/different] because..."
5. **Show** — Minimal working code in both languages side by side

## Supported Frameworks

### React (TypeScript)
- Map: Components ↔ Blazor components, hooks ↔ lifecycle methods, Context ↔ cascading values
- Key differences: Virtual DOM vs Blazor's render tree, unidirectional data flow, JSX vs Razor
- Ecosystem: Next.js ≈ ASP.NET + Blazor combined, React Query ≈ typed HTTP clients

### Angular (TypeScript)
- Map: Modules ↔ .NET assemblies, Services ↔ DI services, Pipes ↔ value formatters
- Key differences: RxJS observables everywhere, Zone.js change detection, decorators-heavy
- Ecosystem: Angular is more "opinionated full framework" like ASP.NET itself

### Spring Boot (Java/Kotlin)
- Map: Controllers ↔ Controllers, @Service ↔ MediatR handlers, JPA ↔ EF Core
- Key differences: Annotation-driven vs attribute-driven, different DI philosophies
- Ecosystem: Spring Security ≈ ASP.NET Identity, Spring Data ≈ EF Core

## Translation Principles

- **Don't just show syntax.** "Java uses `@Autowired` instead of constructor injection" misses the point. Explain WHY the DI model differs and what it means architecturally.
- **Highlight where patterns genuinely translate.** Clean architecture and CQRS work in Spring too — show how.
- **Highlight where they don't.** React's component model is fundamentally different from MVC — don't force-map everything.
- **Note the ecosystem maturity.** Some things are genuinely better in one ecosystem (e.g., EF Core migrations vs JPA/Flyway, React's component library ecosystem vs Blazor's).
- **This is for learning, not shipping.** Prioritize clarity and understanding over production patterns. But note where production code would differ.

## Anti-Patterns
- Don't write a full Spring Boot project when a 20-line comparison suffices
- Don't hide genuine weaknesses of any framework — honest comparison helps learning
- Don't assume the developer wants to switch — they're building breadth, not replacing their stack
