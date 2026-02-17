# Entity Boundaries, Relationships & Subdomain Analysis

## Overview

This document analyzes the domain model's entity boundaries, aggregate relationships, subdomain structure, and coupling hotspots. It proposes concrete solutions for boundary violations and identifies where domain events should replace direct coupling.

---

## Part 1: Subdomain Identification

The system operates within **one bounded context** (Mechanic Shop Management) but contains **four distinct subdomains** plus a generic one. The code currently treats everything as a flat domain, which causes the coupling problems detailed in Part 2.

### Subdomain Map

| Subdomain | Type | Owned Concepts | Lifecycle | Ubiquitous Language |
|---|---|---|---|---|
| **Shop Operations** | Core | WorkOrder, Spot, Scheduling, State Machine | Created → worked → completed | "schedule", "assign", "start", "complete", "cancel" |
| **Service Catalog** | Supporting | RepairTask, Part, pricing, durations | Long-lived, evolves independently | "service", "labor cost", "estimated duration", "parts list" |
| **Customer/Fleet** | Supporting | Customer, Vehicle | Long-lived, independent of work | "customer", "vehicle", "fleet", "registration" |
| **Billing** | Supporting | Invoice, InvoiceLineItem, Payment, Discount, Tax | Starts when work ends, own state machine | "invoice", "line item", "discount", "paid", "refunded" |
| **Identity** | Generic | AppUser, RefreshToken, Role | Already separated via ASP.NET Identity | "user", "token", "role" |

### How These Were Identified

The same real-world concept carries different semantics depending on which subdomain is speaking:

- **RepairTask** — In the Catalog it means "a service we offer at a price." In Shop Operations it means "the work assigned to this vehicle." The WorkOrder references catalog items and always reflects current pricing — the financial snapshot happens at billing time via Invoice.
- **Vehicle** — In Customer/Fleet it means "a car a customer owns." In Shop Operations it means "the thing sitting in spot B right now." The WorkOrder doesn't care about the customer's vehicle list — just which vehicle is being worked on.
- **Employee** — In Identity it means "a person with a role." In Shop Operations it means "the mechanic assigned to this job and their available time slots."
- **Invoice** — Speaks a completely different language than WorkOrder: discounts, tax, line items, payment status, refunds. None of those concepts exist in Shop Operations vocabulary.

### Why Not Separate Bounded Contexts?

Bounded contexts map to team boundaries, deployment boundaries, or fundamental linguistic incompatibilities. For this project:

- **Single developer** — no team boundary benefit
- **Monolith deployment** — no deployment boundary benefit
- **Linguistic differences exist** but are manageable within one codebase

Separate bounded contexts would require separate DbContexts, anti-corruption layers, and integration event infrastructure. The problems identified here are solvable by **respecting subdomain boundaries within the single bounded context** — entity design, ID references, and domain events.

**Exception:** Billing has the strongest case for eventual extraction (see Part 5).

---

## Part 2: Current Relationship Map & Boundary Violations

### Current Entity Relationships

```
  Customer ───owns───▶ Vehicle(s)
                          │
                   VehicleId (by ID)      but also Vehicle? nav prop
                          │
  Employee ◄──LaborId──── WorkOrder ──many:many──▶ RepairTask ───owns──▶ Part(s)
  (by ID)   + Labor nav      │
                              │
                           owns (1:1)
                              │
                              ▼
                          Invoice ───owns──▶ InvoiceLineItem(s)
```

### Current Aggregate Structure

| Aggregate | Root | Children | References Out |
|---|---|---|---|
| Customer | Customer | Vehicle | — |
| Employee | Employee | — | — |
| RepairTask | RepairTask | Part | — |
| WorkOrder | WorkOrder | Invoice, InvoiceLineItem | VehicleId, LaborId, RepairTasks (M:M) |
| RefreshToken | RefreshToken | — | UserId (string) |

### Navigation Property Audit

A full scan of every domain entity reveals the following navigation properties:

#### Legitimate Within-Aggregate References

These are correct — an aggregate root owning its children, or children referencing their parent:

| Entity | Property | Type | Relationship |
|---|---|---|---|
| Customer | `Vehicles` | `IEnumerable<Vehicle>` | Root → children |
| Vehicle | `Customer` | `Customer?` | Child → parent |
| RepairTask | `Parts` | `List<Part>` | Root → children |
| Invoice | `LineItems` | `IReadOnlyList<InvoiceLineItem>` | Root → owned value objects |
| Invoice | `WorkOrder` | `WorkOrder?` | Child → parent (becomes violation when Invoice is promoted) |
| WorkOrder | `Invoice` | `Invoice?` | Root → child (becomes violation when Invoice is promoted) |

#### Cross-Aggregate Violations

All violations are concentrated on **WorkOrder** — it is the only entity that reaches outside its aggregate boundary:

| Entity | Property | Type | Reaches Into | Used For |
|---|---|---|---|---|
| WorkOrder | `Labor` | `Employee?` | Employee aggregate | Query handlers build DTOs with mechanic name |
| WorkOrder | `Vehicle` | `Vehicle?` | Customer aggregate | Query handlers build DTOs with vehicle info |
| WorkOrder | `RepairTasks` | `IEnumerable<RepairTask>` | RepairTask aggregate | Query handlers list assigned tasks; domain computes totals |

Each of these has a corresponding ID reference that is correct (`LaborId`, `VehicleId`, join table for RepairTasks). The navigation properties exist **solely** for EF Core `.Include()` convenience in query handlers.

#### Properties That Change Status If Invoice Is Promoted

If Invoice becomes its own aggregate (see Solution 1), these currently-legitimate references become cross-aggregate violations:

| Entity | Property | Becomes |
|---|---|---|
| WorkOrder | `Invoice?` | Cross-aggregate — should be removed, query by `WorkOrderId` from Invoice side |
| Invoice | `WorkOrder?` | Cross-aggregate — should be removed, already has `WorkOrderId` |

### Boundary Violations

#### Violation 1: Cross-Aggregate Navigation Properties on WorkOrder

WorkOrder has full navigation properties into three other subdomains:

```csharp
// WorkOrder.cs — these are write-model boundary violations
public Employee? Labor { get; set; }                   // reaches into Employee aggregate
public Vehicle? Vehicle { get; set; }                   // reaches into Customer aggregate
public IEnumerable<RepairTask> RepairTasks { get; }     // reaches into RepairTask aggregate
```

These exist because query handlers need them for DTO construction. The consequences:

- **EF Core change tracking:** Loading a WorkOrder for a write operation (e.g., state transition) also loads and tracks Employee, Vehicle, and RepairTask entities. Any accidental modification is silently persisted on `SaveChangesAsync()`.
- **Aggregate boundary erosion:** Write-side code can traverse into other aggregates. Nothing in the type system prevents `workOrder.Vehicle.Customer.Name = "oops"`.
- **Loading overhead:** Every WorkOrder write operation potentially loads entities it doesn't need.

**Evidence — the query in GetWorkOrderByIdQueryHandler loads 5 entity types across 4 aggregates:**

```csharp
_context.WorkOrders
    .Include(wo => wo.Vehicle)
        .ThenInclude(v => v.Customer)   // Customer/Fleet subdomain
    .Include(wo => wo.Labor)            // Employee subdomain
    .Include(wo => wo.Invoice)          // Billing subdomain
    .Include(wo => wo.RepairTasks)      // Service Catalog subdomain
        .ThenInclude(rt => rt.Parts)
```

#### Violation 2: Invoice Trapped Inside WorkOrder Aggregate

Invoice is configured as a child of WorkOrder (one-to-one, FK on Invoice side). But their lifecycles diverge:

- WorkOrder is **done** after completion — it's a historical record
- Invoice is **just starting** — may sit unpaid for weeks, have discounts applied, be paid, or refunded

To mark an invoice as paid or apply a discount, you must load the entire WorkOrder aggregate. `InvoiceStatus.Refunded` already exists — refunds are a billing lifecycle event that has nothing to do with shop operations.

#### Violation 3: Cross-Aggregate Queries in Handlers (Inconsistent)

Multiple handlers reach across aggregate boundaries for validation:

- `RemoveCustomerCommandHandler` queries WorkOrders to check vehicle associations
- `RemoveRepairTaskCommandHandler` queries WorkOrders to check RepairTask usage
- `CreateWorkOrderCommandHandler` does inline queries for vehicle scheduling conflicts instead of using `IWorkOrderPolicy`

These checks are inconsistent — some use the policy service, some are inline. Same business rule, different implementations.

#### Violation 4: Domain Events Raised in Handlers, Not Domain Methods

When `UpdateWorkOrderStateCommandHandler` transitions a WorkOrder to `Completed`, the handler decides to raise events:

```csharp
workOrder.UpdateState(request.NewState);
workOrder.AddDomainEvent(new WorkOrderCollectionModified());
if (workOrder.State == WorkOrderState.Completed)
    workOrder.AddDomainEvent(new WorkOrderCompleted(workOrder.Id));
```

The domain method `UpdateState()` knows it transitioned to Completed — it should raise the event itself. If a second handler or background job also completes WorkOrders, it must duplicate this logic or silently miss raising events.

---

### Note: RepairTask Many-to-Many — Considered and Accepted

The many-to-many between WorkOrder and RepairTask was evaluated for a "snapshot entity" pattern (similar to e-commerce `Product` vs `OrderLineItem`). After analysis, the current design is **acceptable for this domain**:

- **Invoice already snapshots prices.** `InvoiceLineItem` captures `UnitPrice` and `Description` at invoice creation time. The financial source of truth is the Invoice, not the WorkOrder.
- **WorkOrder is operational, not financial.** A work order represents "what work needs doing" — showing current catalog prices is correct behavior for scheduled work.
- **The e-commerce pattern doesn't apply.** An online order IS a binding price agreement. A mechanic work order is not — pricing is finalized at billing time.

The one minor consequence: a completed WorkOrder's computed `Total` may drift from its Invoice's `Total` as catalog prices change. This is cosmetic (the Invoice is the source of truth) and not worth a structural change.

---

## Part 3: Proposed Boundary Restructuring

### Target Relationship Map

```
Shop Operations                Customer/Fleet         Service Catalog         Billing
┌─────────────────┐           ┌──────────────┐       ┌──────────────┐       ┌──────────────────┐
│   WorkOrder     │           │   Customer   │       │  RepairTask  │       │     Invoice      │
│                 │           │     .Vehicles│       │    .Parts    │       │  .CustomerName   │
│  .VehicleId ────┼─ Guid ──▶│   Vehicle    │       │              │       │  .VehicleInfo    │
│  .LaborId  ────┼─ Guid ──▶│              │       │              │       │  .LineItems      │
│  .RepairTasks ──┼─ M:M ───▶└──────────────┘       └──────────────┘       │  .Status         │
│                 │                                                         └───────▲──────────┘
│  (Completed) ──┼─ domain event ──────────────────────────────────────────────────┘
└─────────────────┘
```

Key changes from current state:

- Navigation properties (`Labor`, `Vehicle`) removed — ID references only
- Invoice promoted to its own aggregate — connected via domain event
- Invoice carries snapshotted customer/vehicle info — self-contained document
- RepairTask M:M retained (see note above)

### Target Aggregate Structure

| Aggregate | Root | Children | References Out |
|---|---|---|---|
| Customer | Customer | Vehicle | — |
| Employee | Employee | — | — |
| RepairTask | RepairTask | Part | — |
| WorkOrder | WorkOrder | — | VehicleId (Guid), LaborId (Guid), RepairTaskIds (M:M join table) |
| Invoice | Invoice | InvoiceLineItem | WorkOrderId (Guid) |
| RefreshToken | RefreshToken | — | UserId (string) |

---

## Part 4: Solutions with Reasoning and Alternatives

### Solution 1: Promote Invoice to Its Own Aggregate

**Problem:** Invoice is a child of WorkOrder. To modify billing (apply discount, mark paid, refund), you must load the entire WorkOrder aggregate. Their lifecycles are fundamentally different.

**Current state of the codebase:** The Billing feature folder (`Features/Billing`) already treats Invoice as independent in practice — `SettleInvoiceCommandHandler` loads Invoice directly from `_context.Invoices`, `GetInvoicePdfQureyHandler` loads Invoice with LineItems only, and the PDF generator never touches WorkOrder. But the domain model still says Invoice is a child of WorkOrder, and `GetInvoiceByIdQueryHandler` traverses `Invoice → WorkOrder → Vehicle → Customer` via navigation properties for the DTO, loading 4 entities across 3 aggregates.

**Proposed Solution:** Make Invoice a separate aggregate root referencing `WorkOrderId` by ID. Invoice becomes a **self-contained document** by snapshotting the customer and vehicle info it needs at creation time — the same way `InvoiceLineItem` already snapshots `UnitPrice` and `Description`.

```
Invoice (aggregate root, self-contained document)
  ├── WorkOrderId (Guid) — reference for traceability
  ├── CustomerName (string) — snapshotted at creation
  ├── VehicleInfo (string) — snapshotted at creation
  ├── IssuedAtUtc
  ├── DiscountAmount
  ├── TaxAmount
  ├── Status (Unpaid → Paid → Refunded)
  ├── PaidAt
  └── InvoiceLineItem (owned, already implemented as value object)
        ├── Description — already snapshotted
        ├── UnitPrice — already snapshotted
        └── Quantity
```

**Why snapshot customer/vehicle info instead of joining at query time?**

An invoice is a point-in-time financial document — like a paper invoice that has the customer name and vehicle info printed on it. If the customer changes their name or sells the vehicle after the invoice is issued, the invoice should still reflect the original details. This is intentional denormalization for the same reason `InvoiceLineItem.UnitPrice` is denormalized: financial records are immutable historical documents.

The WorkOrder is loaded **once** at invoice creation time (which `IssueInvoiceCommandHandler` already does). After that, every billing operation — mark as paid, apply discount, refund, generate PDF, list unpaid invoices — is fully self-contained:

| Operation | Today (child of WorkOrder) | After promotion |
|---|---|---|
| Mark as paid | Load WorkOrder + all children | Load Invoice only |
| Apply discount | Load WorkOrder + all children | Load Invoice only |
| List unpaid invoices | Query WorkOrders, filter by Invoice status | Query Invoices directly |
| Refund | Load WorkOrder + all children | Load Invoice only |
| Generate PDF | Already loads Invoice only (correct) | Same — no change needed |
| Get invoice details | `Invoice → WorkOrder → Vehicle → Customer` (4 entities, 3 aggregates) | Load Invoice only (self-contained) |

**Event flow for creation:**
```
WorkOrder.UpdateState(Completed)
  → raises WorkOrderCompleted(workOrderId)
    → CreateInvoiceOnWorkOrderCompletedHandler
      → loads WorkOrder with RepairTasks, Vehicle, Customer (ONE TIME)
      → builds line items, snapshots customer/vehicle info
      → Invoice.Create(workOrderId, customerName, vehicleInfo, lineItems, discount, tax)
      → _context.Invoices.Add(invoice)
```

Alternatively, `IssueInvoiceCommand` can remain an explicit command (not event-triggered) if you want manual control over when invoicing happens. The aggregate separation is the important part — the trigger mechanism is a refinement.

**Alternative A: Keep Invoice inside WorkOrder, add a dedicated billing endpoint.**
Create an `InvoiceService` that loads only the Invoice (bypassing the WorkOrder aggregate) using a direct `DbSet<Invoice>` query.

- Pros: No structural change, minimal migration
- Cons: Architecturally dishonest — the domain says Invoice is part of WorkOrder, but the application layer circumvents that. Two access paths to the same entity create confusion about who owns it. EF Core change tracking still links them.

**Alternative B: Keep Invoice inside WorkOrder, accept the coupling.**
If billing will always be simple (pay in full, no partials, no payment plans), the overhead of a separate aggregate may not be justified.

- Pros: Simpler model, fewer moving parts
- Cons: Every billing operation loads the full WorkOrder. Adding billing complexity later requires the structural change anyway, but with more data to migrate. The `Refunded` status already hints at billing complexity beyond simple pay-in-full.

**Recommendation:** Promote Invoice to its own aggregate. The codebase already treats it as independent in practice — this formalizes that reality and eliminates the fragile `invoice.WorkOrder!.Vehicle!.Customer!` navigation chain in the mapper.

---

### Solution 2: Remove Cross-Aggregate Navigation Properties

**Problem:** WorkOrder has navigation properties (`Vehicle`, `Labor`, `RepairTasks`) to entities in other subdomains. These exist for query convenience but violate aggregate boundaries on the write side. EF Core's change tracker watches entities from multiple aggregates simultaneously.

**Proposed Solution:** Remove navigation properties from the write model. Keep only ID references (`VehicleId`, `LaborId`). Serve query needs through the appropriate pattern depending on the data's nature.

#### What the Write Model Becomes

```csharp
public sealed class WorkOrder : AuditableEntity
{
    public Guid VehicleId { get; }            // ID reference only
    public Guid LaborId { get; private set; } // ID reference only
    // RepairTasks accessed via join table, not navigation property
    // No Employee? Labor
    // No Vehicle? Vehicle
    // No Invoice? Invoice (after Solution 1)
}
```

#### How to Query Without .Include()

The right query strategy depends on what the data represents:

**Decision rule:** Is this data a **record of what happened** or a **view of the current state**?

| Purpose | Pattern | Example |
|---|---|---|
| Record of what happened | Snapshot at creation (denormalize) | Invoice with CustomerName, VehicleInfo, LineItem prices |
| View of current state | Join at query time | Dashboard, schedule board, active work orders list |

**For historical/snapshot data:** capture it on the entity at creation time. Invoice already does this with `InvoiceLineItem.UnitPrice`. After Solution 1, it also captures `CustomerName` and `VehicleInfo`. No joins needed at read time.

**For live/current data:** use query-time joins. Three approaches, from simplest to most separated:

#### Approach A: Query-Time Joins via LINQ (Recommended)

Use explicit joins in query handlers. No navigation properties needed — EF Core can join on any FK column:

```csharp
var dto = await _context.WorkOrders
    .Where(wo => wo.Id == id)
    .Select(wo => new WorkOrderDetailDto
    {
        WorkOrderId = wo.Id,
        StartAtUtc = wo.StartAtUtc,
        EndAtUtc = wo.EndAtUtc,
        State = wo.State,
        Spot = wo.Spot,

        // Join to Employee table via LaborId
        Labor = _context.Employees
            .Where(e => e.Id == wo.LaborId)
            .Select(e => new LaborDto
            {
                LaborId = e.Id,
                Name = e.FirstName + " " + e.LastName
            })
            .FirstOrDefault(),

        // Join to Vehicle + Customer tables via VehicleId
        Vehicle = _context.Vehicles
            .Where(v => v.Id == wo.VehicleId)
            .Select(v => new VehicleDto
            {
                VehicleId = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate,
                CustomerName = v.Customer != null ? v.Customer.Name : null
            })
            .FirstOrDefault(),

        // Invoice is now its own aggregate — query from Invoice side
        InvoiceId = _context.Invoices
            .Where(i => i.WorkOrderId == wo.Id)
            .Select(i => i.Id)
            .FirstOrDefault()
    })
    .FirstOrDefaultAsync();
```

- Pros: No new infrastructure. EF Core translates these to SQL JOINs or subqueries. The domain model stays clean. Always returns current data.
- Cons: Query handlers become more verbose. The RepairTask M:M join is awkward without navigation properties (see note below).

> **Note on RepairTasks M:M without navigation properties:** EF Core's many-to-many relies on navigation properties for the implicit join table. Without them, you'd need to make the join table an explicit entity (`WorkOrderRepairTask` with `WorkOrderId` + `RepairTaskId`). This gives you full control and makes the join queryable without navigation properties.

#### Approach B: Dedicated Query Service

Extract query logic into a service class that encapsulates the cross-aggregate joins:

```csharp
// Interface in Application layer
public interface IWorkOrderReadService
{
    Task<WorkOrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PaginatedList<WorkOrderListItemDto>> GetListAsync(WorkOrderFilter filter, CancellationToken ct);
}

// Implementation in Infrastructure layer — free to use raw SQL, joins, views, etc.
public class WorkOrderReadService(IAppDbContext context) : IWorkOrderReadService
{
    public async Task<WorkOrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // Uses joins, Dapper, raw SQL, or database views — whatever is optimal
    }
}
```

- Pros: Clean separation. Query handlers delegate to the service. Query logic is centralized and testable.
- Cons: Another abstraction layer. Only justified if you have multiple query handlers that share similar join patterns.

#### Approach C: Database Views (Full CQRS)

Create SQL views that pre-join the data query handlers need:

```sql
CREATE VIEW vw_WorkOrderDetail AS
SELECT
    wo.Id, wo.StartAtUtc, wo.EndAtUtc, wo.State, wo.Spot,
    e.Id AS LaborId, e.FirstName + ' ' + e.LastName AS LaborName,
    v.Id AS VehicleId, v.Make, v.Model, v.Year, v.LicensePlate,
    c.Name AS CustomerName,
    i.Id AS InvoiceId
FROM WorkOrders wo
LEFT JOIN Employees e ON e.Id = wo.LaborId
LEFT JOIN Vehicles v ON v.Id = wo.VehicleId
LEFT JOIN Customers c ON c.Id = v.CustomerId
LEFT JOIN Invoices i ON i.WorkOrderId = wo.Id;
```

Map this as a keyless entity in EF Core:

```csharp
public DbSet<WorkOrderDetailView> WorkOrderDetailViews { get; }

// In configuration:
builder.Entity<WorkOrderDetailView>().HasNoKey().ToView("vw_WorkOrderDetail");
```

- Pros: Best query performance. Complete read/write separation. Query handlers are trivial.
- Cons: Views must be maintained alongside schema changes. Requires a migration for the view. Overkill unless query performance is a measured bottleneck.

**Recommendation:** Start with Approach A (query-time joins). It requires no new infrastructure and enforces the boundary immediately. If query handlers become too verbose or repetitive, extract into a query service (Approach B). Database views (Approach C) are a future optimization, not a starting point.

**Alternative: Keep navigation properties, make them query-only.**
Configure EF to only populate navigation properties on explicit `.Include()`. Mark them with internal setters.

- Pros: Least disruption
- Cons: The properties still exist on the entity. Nothing prevents write-side code from accessing them. The change tracker still loads and watches them. This is a half-measure that doesn't enforce the boundary.

---

### Solution 3: Domain Events Raised Inside Domain Methods

**Problem:** Domain events are raised in command handlers, not inside domain methods. Every handler must remember to emit the right events. The domain doesn't own its own event semantics.

**Current (handler raises events):**
```csharp
// UpdateWorkOrderStateCommandHandler.cs
workOrder.UpdateState(request.NewState);
workOrder.AddDomainEvent(new WorkOrderCollectionModified());
if (workOrder.State == WorkOrderState.Completed)
    workOrder.AddDomainEvent(new WorkOrderCompleted(workOrder.Id));
```

**Proposed (domain method raises events):**

```csharp
// WorkOrder.cs
public ErrorOr<Updated> UpdateState(WorkOrderState newState)
{
    if (!CanTransitionTo(newState))
        return WorkOrderErrors.StateTransitionNotAllowed(...);

    State = newState;

    if (newState == WorkOrderState.Completed)
        AddDomainEvent(new WorkOrderCompleted(Id));

    return Result.Updated;
}

public ErrorOr<Updated> Cancel()
{
    if (State == WorkOrderState.Completed)
        return WorkOrderErrors.CannotCancelCompleted;

    State = WorkOrderState.Cancelled;
    AddDomainEvent(new WorkOrderCancelled(Id));

    return Result.Updated;
}
```

**Reasoning:**

- The aggregate is the single authority on what constitutes a meaningful state change. It knows when it has been completed, cancelled, or created — the handler shouldn't be making that determination.
- Adding a new handler that modifies WorkOrder state won't silently forget to raise events.
- Event handlers (side-effect processors) are decoupled from the command that triggered the state change. You can add new reactions without modifying existing handlers.

**Events to add:**

| Event | Raised By | Enables |
|---|---|---|
| `WorkOrderCompleted` | `UpdateState(Completed)` | Invoice creation, completion notification |
| `WorkOrderCancelled` | `Cancel()` | Spot release, customer notification, void pending invoice |
| `WorkOrderScheduled` | `Create()` | Confirmation notification, calendar sync |
| `WorkOrderStarted` | `UpdateState(InProgress)` | Dashboard update, timer start |
| `InvoicePaid` | `MarkAsPaid()` | Receipt generation, accounting sync |

**What to do with `WorkOrderCollectionModified`:**

This event serves cache invalidation, not a domain concept. It should be removed from the domain layer. Cache invalidation can be handled by:

- An application-layer event/notification after `SaveChangesAsync()`
- A decorator or interceptor on the repository/DbContext
- The existing `HybridCache` tag invalidation in command handlers (which already does this)

**Alternative: Keep events in handlers, enforce via code review.**

- Pros: No domain changes needed
- Cons: Relies on developer discipline. The current codebase already has inconsistencies (some handlers raise events, some don't). Scales poorly with team size.

**Recommendation:** Move event emission into domain methods. The infrastructure already exists (`AddDomainEvent`, `SaveChangesAsync` dispatching). This is a small change with high leverage.

---

### Solution 4: Consistent Cross-Aggregate Validation via Policy Services

**Problem:** Cross-aggregate business rules are enforced inconsistently. Some go through `IWorkOrderPolicy` (scheduling checks in `RelocateWorkOrderCommandHandler`), others are inline queries (vehicle overlap in `CreateWorkOrderCommandHandler`, deletion eligibility in `RemoveCustomerCommandHandler`).

**Proposed Solution:** Centralize all cross-aggregate validation in policy/specification services. Each subdomain boundary crossing should go through a named, testable policy.

```text
IWorkOrderPolicy (already exists, expand it)
  ├── IsVehicleAlreadyScheduled()     — already exists
  ├── IsLaborOccupied()               — already exists
  ├── CheckSpotAvailabilityAsync()    — already exists
  ├── IsOutsideOperatingHours()       — already exists
  └── ValidateMinimumRequirement()    — already exists

ICustomerPolicy (new)
  └── CanDeleteCustomer()             — checks for associated work orders

IRepairTaskPolicy (new)
  └── CanDeleteRepairTask()           — checks for work order usage
```

**Reasoning:**

- Named policies make business rules discoverable and testable in isolation.
- A single rule has a single implementation — no risk of two handlers implementing the same check differently.
- Policy interfaces live in the Application layer (they need to query across aggregates). Implementations live in Infrastructure.
- Domain rules that don't require cross-aggregate queries (operating hours, minimum duration) should move into the domain as value objects or a domain service.

**Alternative: Use domain events for deletion eligibility checks.**
When a `CustomerDeletionRequested` event is raised, a handler checks for work order associations and either approves or rejects.

- Pros: Fully event-driven, no direct cross-aggregate query
- Cons: Over-engineered for a synchronous validation check. The user needs an immediate yes/no answer, not an eventual consistency model. Events are better suited for reactions than for synchronous pre-condition checks.

**Recommendation:** Policy services for synchronous cross-aggregate validation. Domain events for asynchronous reactions to state changes. Don't conflate the two.

---

## Part 5: Future Consideration — Billing as a Separate Bounded Context

Billing has the strongest case for eventual extraction into its own bounded context:

| Signal | Evidence |
|---|---|
| **Different ubiquitous language** | "invoice", "line item", "discount", "tax", "refund" — none of these terms appear in shop operations |
| **Independent lifecycle** | Invoice lives long after WorkOrder is completed |
| **Replaceable** | Could conceivably swap in a third-party billing system (Stripe Billing, QuickBooks) |
| **Own state machine** | `Unpaid → Paid → Refunded` is independent of `Scheduled → InProgress → Completed → Cancelled` |
| **Expanding complexity** | `InvoiceStatus.Refunded` already exists, hinting at financial operations beyond simple pay-in-full |

**When to consider this:** If the billing requirements grow to include partial payments, payment plans, recurring billing, tax jurisdiction rules, or integration with external accounting systems.

**How it would work:** A separate `BillingDbContext` with its own Invoice aggregate. Communication with Shop Operations via integration events (`WorkOrderCompleted` → `CreateInvoiceCommand`). Anti-corruption layer maps shop concepts to billing concepts.

**Not needed now.** The intermediate step (Solution 1: Invoice as separate aggregate with snapshotted customer/vehicle data) provides most of the benefits with none of the infrastructure overhead. The Invoice is already a self-contained document at that point — extracting it later is straightforward because it has no dependencies on other aggregates.

---

## Summary: Priority and Sequencing

Changes are ordered by dependency — later items build on earlier ones.

| #   | Change                                                       | Effort     | Impact      | Depends On |
| --- | ------------------------------------------------------------ | ---------- | ----------- | ---------- |
| 1 | Move domain event emission into domain methods | Low | High | — |
| 2 | Add missing domain events (Cancelled, Scheduled, Started) | Low | Medium | #1 |
| 3 | Remove `WorkOrderCollectionModified` from domain layer | Low | Low | #1 |
| 4 | Promote Invoice to separate aggregate with snapshotted fields | Medium | High | #1, #2 |
| 5 | Remove cross-aggregate navigation properties from WorkOrder | Medium | Medium | #4 |
| 6 | Add query-time projections to replace `.Include()` chains | Medium | Medium | #5 |
| 7 | Centralize cross-aggregate validation in policy services | Low-Medium | Medium | — |

Items 1-3 are low-risk, high-value changes that can be done immediately. Item 4 is the most impactful structural change. Items 5-6 build on the foundation laid by earlier changes. Item 7 is independent and can be done at any time.
