# Domain Model Review

## Overall Assessment

The domain model is well above average for a .NET clean architecture project. Factory methods with `ErrorOr`, aggregate boundaries enforced by ID references, state machine in WorkOrder, sealed classes by default — the hard parts are done right. What follows are refinements, not fundamental problems.

---

## Part 1: Domain Model Improvements

### 1. Primitive Obsession — The Biggest Gap

The domain has significant primitive obsession despite good enum usage:

- `Customer.Name` is a `string` — a customer name has rules (not empty, not 500 chars)
- `Customer.Email` is a `string` — validated by `MailAddress` in the factory, but nothing prevents invalid state if bypassed internally
- `Customer.PhoneNumber` is a `string` — regex validation scattered in the entity
- `VehicleId`, `LaborId`, `CustomerId` are all `Guid` — nothing in the type system prevents passing a `VehicleId` where a `CustomerId` is expected

**The real cost:** Validation logic lives in entities instead of the types themselves. Every phone number validation repeats (or forgets) the regex. A `PhoneNumber` value object validates once at construction, and the type guarantees correctness from that point forward.

**Concrete bug vector:** `WorkOrder.Create` takes `Guid vehicleId, Guid laborId`. Swap the arguments and the compiler says nothing. With `VehicleId` and `LaborId` as strongly-typed IDs, that becomes a compile error.

**Trade-off:** Strongly-typed IDs in C# + EF Core are workable but not frictionless — value converters needed, some LINQ translations get awkward. Value objects for `Email`, `PhoneNumber`, `Name` are pure upside though — stored as their underlying type, no EF Core friction.

**Priority: High** — compounds in value over time, prevents entire categories of bugs.

---

### 2. Aggregate Boundary Question: Is RepairTask Its Own Aggregate?

`RepairTask` is structured as both a standalone aggregate (has its own repository, presumably) and a child collection inside `WorkOrder`.

**Key question:** Can a RepairTask exist without being part of a WorkOrder?

If RepairTasks are a **catalog/template** ("Oil Change costs $50, takes 30 mins") that get assigned to work orders, then a separate aggregate makes sense — they're reusable definitions. But the WorkOrder shouldn't own `RepairTask` entities directly. It should have something like `WorkOrderTask` (a join/assignment entity) that references a `RepairTaskId` and captures work-order-specific overrides (actual vs estimated duration/cost).

**Why this matters:** If you change a RepairTask's labor cost, does it retroactively change the cost of every WorkOrder that used it? That's probably wrong. A completed work order's total should be immutable — it reflects what was charged, not what's currently listed.

This is the same pattern as e-commerce `Product` vs `OrderLineItem` — the line item snapshots the price at time of order.

**Priority: High** — architectural decision that affects data integrity (price snapshotting). Think this through before more features are built on top.

---

### 3. Domain Events — Significantly Underused

The infrastructure for domain events exists (base class, dispatch mechanism) but only two events are defined:

- `WorkOrderCompleted` — good
- `WorkOrderCollectionModified` — smells like a UI/caching concern, not a domain event

**Missing domain events that should exist:**

| Event | Why It Matters |
|---|---|
| `WorkOrderCancelled` | Cancellation has business consequences: release the spot, notify customer, void pending invoice |
| `InvoicePaid` | Payment is a significant business event: accounting, receipts, possibly triggering follow-up scheduling |
| `WorkOrderScheduled` | Could trigger spot availability checks, employee schedule validation |
| `CustomerCreated` | Welcome emails, CRM integration, etc. |

**The deeper problem:** `Cancel()` transitions state but raises no event. Any cancellation side effects must be handled imperatively in the command handler. With a domain event:

1. The aggregate only worries about its own state transition
2. Side effects (releasing the spot, notifying) are handled by separate event handlers
3. Adding new side effects later doesn't require modifying the WorkOrder aggregate or its handler

**`WorkOrderCollectionModified`** serves cache invalidation or SignalR notification rather than a domain concept. If so, it belongs in the application/infrastructure layer. Domain events should represent business-meaningful things ("a work order was completed"), not technical concerns ("the collection changed").

**Priority: Medium-High** — sets up extensibility without modifying existing aggregates.

---

### 4. The Invoice Relationship to WorkOrder

Invoice is currently a child entity inside the WorkOrder aggregate. This means:

- To create an invoice, you load the entire WorkOrder aggregate
- To mark an invoice as paid, you load the entire WorkOrder aggregate
- Invoice lifecycle is coupled to WorkOrder lifecycle

**Consider making Invoice its own aggregate**, referencing `WorkOrderId`. The business lifecycles are different:

- A WorkOrder is completed and done
- An Invoice might be unpaid for weeks, have payment retries, partial payments, refunds

`InvoiceStatus.Refunded` already exists in the enum — refunds are a billing concern that shouldn't require loading a WorkOrder. If Invoice becomes its own aggregate, the `WorkOrderCompleted` event naturally triggers invoice creation through an event handler. Clean separation.

**Counter-argument:** If invoices are always 1:1 with work orders and billing is simple (pay in full, no partials, no payment plans), keeping it inside WorkOrder reduces complexity. This depends on where the domain is heading.

**Priority: Medium** — depends on billing complexity trajectory.

---

### 5. Vehicle Year Validation Bug

`Vehicle.cs` checks `year < 5` but defines `FirstYear = 1886`. The constant exists but isn't used in validation. Small fix, but it erodes trust in domain invariants.

**Priority: Low** — quick fix.

---

### 6. The Clone Pattern on Part

`Part.Clone()` with manual implementation is fragile. Add a property to Part and forget to update Clone — you silently lose data. Since Part is an entity (has identity), cloning is also semantically odd — creating a new entity with a new ID that copies values.

If the intent is "create a Part based on a template," a factory method like `Part.CreateFrom(Part template, Guid newId)` makes the intent clearer and keeps the factory pattern consistent.

**Priority: Low** — correctness concern, not architectural.

---

### 7. MediatR in the Domain Layer

The domain layer depends on MediatR for `DomainEvent : INotification`. The coupling is minimal (just the marker interface) and the pragmatic take is: it works, it's low-friction.

The purist alternative is defining `IDomainEvent` in the domain and mapping to MediatR in infrastructure — but that's ceremony for near-zero benefit in a team-of-one project.

**When it would matter:** If the domain layer needed to run without MediatR (different host, CLI tool, test harness without full DI). Unlikely, but worth knowing the escape hatch.

**Priority: Low** — acceptable pragmatic trade-off.

---

## Part 2: Domain Concerns Leaking into the Application Layer

### Critical Leaks

#### 1. Business Calculations Duplicated in WorkOrderMapper

**File:** `MechanicApplication/Features/WorkOrders/Mappers/WorkOrderMapper.cs`

The mapper recalculates `TotalPartCost`, `TotalLaborCost`, `TotalCost`, and `TotalDurationInMins` manually instead of using the domain's computed properties (`WorkOrder.TotalPartsCost`, `WorkOrder.TotalLaborCost`, `WorkOrder.Total`).

**Impact:** Two sources of truth. If calculation rules change, both places must be updated.

---

#### 2. Duration Calculation in CreateWorkOrderCommandHandler

**File:** `MechanicApplication/Features/WorkOrders/Commands/CreateWorkOrder/CreateWorkOrderCommandHandler.cs`

```csharp
var endAt = request.StartAt.AddMinutes(
    repaitrTasks.Sum(rt => (double)rt.EstimatedDurationInMins));
```

The handler calculates end time by summing repair task durations. The WorkOrder entity already has `EstimatedDuration` and computed `EndAtUtc`. The handler shouldn't know how to calculate durations.

---

#### 3. Timing Calculation in RelocateWorkOrderCommandHandler

**File:** `MechanicApplication/Features/WorkOrders/Commands/RelocateWorkOrder/RelocateWorkOrderCommandHandler.cs`

```csharp
var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();
var endAt = request.NewStartAt.Add(duration);
```

The handler manually calculates duration and new end time. This is domain knowledge about how WorkOrder timing works. The `UpdateTiming` method should handle this internally.

---

#### 4. Temporal State Transition Rule in UpdateWorkOrderStateCommandHandler

**File:** `MechanicApplication/Features/WorkOrders/Commands/UpdateWorkOrderState/UpdateWorkORderStateCommandHandler.cs`

```csharp
if(workOrder.StartAtUtc > _timeProvider.GetUtcNow())
{
    return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
}
```

"State transitions cannot happen before the work order starts" is a domain business rule. It belongs inside `WorkOrder.UpdateState()`, not in the handler.

---

#### 5. Domain Event Emission Outside the Domain (Multiple Handlers)

Handlers decide when to raise domain events:

```csharp
workOrder.AddDomainEvent(new WorkOrderCollectionModified());

if(workOrder.State == WorkOrderState.Completed)
{
    workOrder.AddDomainEvent(new WorkOrderCompleted(workOrder.Id));
}
```

This appears in `CreateWorkOrderCommandHandler`, `UpdateWorkOrderStateCommandHandler`, `DeleteWorkOrderCommandHandler`, and `RelocateWorkOrderCommandHandler`.

Domain events should be raised by domain operations internally. `WorkOrder.UpdateState()` should raise `WorkOrderCompleted` when transitioning to Completed. The handler shouldn't know which events to emit.

---

#### 6. Vehicle Overlap Check Inconsistency in CreateWorkOrderCommandHandler

**File:** `MechanicApplication/Features/WorkOrders/Commands/CreateWorkOrder/CreateWorkOrderCommandHandler.cs`

The handler queries directly for vehicle scheduling conflicts instead of using `IWorkOrderPolicy.IsVehicleAlreadyScheduled` (which is used in `RelocateWorkOrderCommandHandler`). Two different implementations of the same business rule.

---

#### 7. Customer Deletion Rule in RemoveCustomerCommandHandler

**File:** `MechanicApplication/Features/Customers/Commands/RemoveCustomer/RemoveCustomerCommandHandler.cs`

The handler checks if a customer has associated work orders. This is a domain business rule about deletion eligibility. Additionally, the code logs a warning but doesn't return an error — this is likely a bug (deletion proceeds even with associated work orders).

---

#### 8. Overlap Logic in WorkOrderExtensions

**File:** `MechanicApplication/Common/Extensions/WorkOrderExtensions.cs`

```csharp
public static bool RangeOverlapps(this WorkOrder workOrder,
    DateTimeOffset start, DateTimeOffset end)
    => workOrder.StartAtUtc < end && workOrder.EndAtUtc > start;
```

This encodes business logic about what "overlapping" means for work orders. Should be a method on WorkOrder itself.

---

### Architectural Concerns

#### 9. IAppDbContext Exposes DbSets Directly — No Repository Abstraction

**File:** `MechanicApplication/Common/Interfaces/IAppDbContext.cs`

All persistence goes through direct `DbSet<T>` access. Handlers build EF queries directly, violating aggregate boundaries. Query handlers use `.Include()` chains that cross aggregate boundaries:

```csharp
_context.WorkOrders.AsNoTracking()
    .Include(wo => wo.Vehicle)
        .ThenInclude(v => v.Customer)
    .Include(wo => wo.Labor)
    .Include(wo => wo.Invoice)
    .Include(wo => wo.RepairTasks)
        .ThenInclude(rt => rt.Parts);
```

This violates the architecture rule "No navigation properties between aggregates. Reference by ID only." WorkOrder, Customer, Employee, and Invoice are separate aggregates, but queries load them all together.

**Better approach:** Repository interfaces in Application layer, or CQRS read models that bypass domain entities for queries.

---

#### 10. IWorkOrderPolicy Mixes Domain Rules with Application Services

**File:** `MechanicInfrastructure/Services/AvailabilityChecker.cs`

This service contains both:
- **Domain rules** that belong in the domain layer: operating hours, minimum appointment duration
- **Application services** that legitimately require cross-aggregate queries: labor availability, spot availability, vehicle scheduling conflicts

Operating hours and minimum duration should be domain value objects or a `SchedulingPolicy` domain service. Cross-aggregate availability checks can stay in the application/infrastructure layer.

---

## Summary: Priority Matrix

| Priority | Item | Category |
|---|---|---|
| **High** | RepairTask aggregate boundary (price snapshotting) | Domain Design |
| **High** | Value objects for Email, PhoneNumber, strongly-typed IDs | Domain Design |
| **High** | Domain events raised inside domain methods, not handlers | Layer Leak |
| **High** | Temporal state transition rule belongs in WorkOrder | Layer Leak |
| **Medium-High** | More domain events (Cancelled, InvoicePaid, Scheduled) | Domain Design |
| **Medium** | Invoice as its own aggregate | Domain Design |
| **Medium** | Remove/relocate WorkOrderCollectionModified | Domain Design |
| **Medium** | Duration/timing calculations belong in domain | Layer Leak |
| **Medium** | Repository abstraction instead of IAppDbContext | Architecture |
| **Medium** | Consistent use of IWorkOrderPolicy vs inline queries | Layer Leak |
| **Low** | Fix Vehicle year validation bug | Bug |
| **Low** | Replace Part.Clone with factory method | Domain Design |
| **Low** | MediatR dependency in domain layer | Architecture |
| **Low** | Customer deletion rule bug (warns but doesn't prevent) | Bug |
