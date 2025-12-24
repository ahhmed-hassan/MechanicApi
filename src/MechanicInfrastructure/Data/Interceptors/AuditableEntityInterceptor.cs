

using MechanicApplication.Common.Interfaces;
using MechanicDomain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System;

namespace MechanicInfrastructure.Data.Interceptors;


/// <summary>
/// Intercepts EF Core SaveChanges operations to apply audit metadata to <see cref="AuditableEntity"/> instances.
/// </summary>
/// <remarks>
/// Why this exists:
/// <list type="bullet">
/// <item>Centralizes audit behavior so domain entities remain focused on domain concerns and do not need to know persistence details.</item> 
/// <item>Runs in an EF Core interceptor so audit metadata is applied reliably for both synchronous and asynchronous saves,
///   and for any code path that uses the DbContext's change tracker (not just explicit repository code).</item> 
/// <item>Uses an injected <paramref name="user"/> and <paramref name="timeProvider"/> rather than direct static or system calls
///   to keep the audit behavior testable and to ensure consistent UTC timestamps across the application.</item>
/// <item>Explicitly updates owned entities as well because EF Core does not implicitly flow domain-level audit semantics into owned types;
///   treating owned instances consistently prevents missing or stale audit fields when complex object graphs change.</item>
///   </list>
/// </remarks>
/// <param name="user">Provides the current user's identifier. Nullability allowed to support anonymous/system operations; the interceptor records whatever identifier is available.</param>
/// <param name="timeProvider">Abstracts time access to produce UTC timestamps and allow deterministic unit tests.</param>

public class AuditableEntityInterceptor(
    IUser user,
    TimeProvider timeProvider
    ) : SaveChangesInterceptor
{

    private readonly IUser _user = user;
    private readonly TimeProvider _timeProvider = timeProvider;
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntites(eventData.Context);
        return base.SavingChanges(eventData, result);
    }


    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntites(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Walks the change tracker and applies audit metadata to added or modified <see cref="AuditableEntity"/> entries and their owned entities.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> whose change tracker will be inspected. If null, the method is a no-op.</param>
    /// <remarks>
    /// Why this logic:
    /// <list type="bullet">
    /// <item>Only updates entities that are being added or modified (or that include owned entities that changed). This avoids touching unchanged rows,
    ///   which prevents unnecessary updates and preserves original creation metadata.</item> 
    /// <item>Captures a single UTC timestamp per SaveChanges call so all audited entries share the same logical write time.</item>
    /// <item>Uses the provided user identifier as-is; allowing null/empty values preserves semantics for unauthenticated/system operations.</item> 
    /// <item> Iterates owned references and applies the same audit update to owned instances that are auditable and changed,
    ///   ensuring nested data remains consistent without requiring callers to remember to set audit fields manually.</item>
    /// <item>The method is intentionally conservative: it returns early if <paramref name="context"/> is null and only mutates entities tracked as <see cref="AuditableEntity"/>.</item> 
    /// </list>
    /// </remarks>

    private void UpdateEntites(DbContext? context)
    {
        if (context == null) return;

        var ApplyAuditInformation = (EntityEntry<AuditableEntity> entry, DateTimeOffset utcNow, string? userId)
            =>
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedBy = userId;
            entry.Entity.CreatedAtUtc = utcNow;
        }
        entry.Entity.LastModifiedBy = userId;
        entry.Entity.LastModifiedUtc = utcNow;
    };
        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is (EntityState.Added or EntityState.Modified) ||
                entry.HasChangedOwnedEntites())
            {
                var utcNow = _timeProvider.GetUtcNow();
                ApplyAuditInformation(entry, utcNow, _user.Id);

                foreach (var ownedEntity in entry.References
                    .Select(x => x.TargetEntry)
                    .Where(owned =>
                        owned is
                            EntityEntry<AuditableEntity>
                        {
                            State: EntityState.Added or EntityState.Modified
                        })
                    .Cast<EntityEntry<AuditableEntity>>())
                    ApplyAuditInformation(ownedEntity, utcNow, _user.Id);
            }
        }


    }


}
/// <summary>
/// Extension helpers for inspecting EF Core <see cref="EntityEntry"/> instances with ownership-aware semantics.
/// </summary>
/// <remarks>
/// Why this extension exists:
/// <list type="bullet">
/// <item> Provides a focused, reusable predicate to detect when an entity's owned graph contains changes that should trigger audit updates on the owner.</item>
/// <item> Encapsulating the owned-entity recursion here keeps the interceptor code clearer and signals the intention to treat owned changes as part of the parent's lifecycle.</item>
/// </list>
/// </remarks>
public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntites(this EntityEntry entry) =>
        entry.References.Any(r =>
        r.TargetEntry?.Metadata.IsOwned() == true &&
        (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified || r.TargetEntry.HasChangedOwnedEntites())
        );
}

