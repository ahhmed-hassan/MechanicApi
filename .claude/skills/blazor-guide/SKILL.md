---
name: blazor-guide
description: Blazor learning companion for ASP.NET Core developers. Use when the user asks about Blazor components, rendering, state management, forms, or any frontend concept in the Blazor context. The user knows ASP.NET Core deeply but is new to Blazor.
---

# Blazor Learning Guide

The developer knows ASP.NET Core backend deeply (clean architecture, CQRS, MediatR, EF Core) but is starting Blazor from zero. Teach by building bridges from what they already know.

## Teaching Approach

1. **Always start with the mental model**, not the syntax. Before showing `@code {}`, explain what a component IS in Blazor's rendering pipeline.
2. **Map to ASP.NET equivalents:**
   - Component ≈ a self-contained "mini controller + view" that manages its own state
   - `OnInitializedAsync` ≈ controller action that runs when the "page" loads
   - `[Parameter]` ≈ action method parameters, but passed from parent components
   - `@inject` ≈ constructor injection in controllers
   - `NavigationManager` ≈ redirect/URL generation in controllers
   - `EditForm` + `DataAnnotationsValidator` ≈ model binding + FluentValidation (but client-side)
3. **Warn before they hit gotchas:**
   - Blazor Server vs WASM vs Auto — rendering mode affects EVERYTHING (latency, state, auth, DB access)
   - Component lifecycle is NOT like HTTP request lifecycle — components persist across renders
   - `StateHasChanged()` is not free — unnecessary calls cause performance issues
   - Async + rendering = subtle bugs (component disposal during async operations)
   - CSS isolation works differently than you'd expect from backend experience

## Key Concepts to Cover (in teaching order)

1. **Rendering modes** — Server (SignalR), WebAssembly (runs in browser), Auto (starts Server, transitions to WASM). This is the most consequential architectural decision.
2. **Component model** — .razor files, parameters, cascading values, render fragments
3. **Lifecycle** — OnInitialized → OnParametersSet → OnAfterRender (and their async variants)
4. **State management** — Component state, cascading state, DI-scoped services, browser storage
5. **Forms and validation** — EditForm, EditContext, validation integration
6. **HTTP communication** — HttpClient to call our existing API, typed clients, error handling
7. **Authentication** — AuthenticationStateProvider, AuthorizeView, how it connects to our JWT backend
8. **Routing** — @page directives, route parameters, NavigationManager
9. **Component patterns** — Smart/dumb components, layout components, generic components

## Connection to MechanicApi Backend

When explaining Blazor concepts, always show how they connect to the existing backend:
- API calls go through `MechanicApi.Contracts` DTOs (shared project)
- Authentication flows use the existing JWT + refresh token system
- The Blazor frontend is a consumer of the same API, not a replacement for the backend architecture
- Domain logic stays in the backend — Blazor handles presentation only

## Quality Standards

Even in learning code, maintain:
- Proper separation of concerns (no business logic in components)
- Typed HTTP clients, not raw string URLs
- Proper error handling with ErrorOr patterns where applicable
- Accessibility basics from the start (semantic HTML, ARIA where needed)
