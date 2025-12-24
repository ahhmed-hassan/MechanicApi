

using MechanicApplication.Common.Interfaces;
using MechanicDomain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace MechanicInfrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(
    IUser user, 
    TimeProvider timeProvider
    ): SaveChangesInterceptor
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

   

    private void UpdateEntites(DbContext? context)
    {
        if (context == null) return;


        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is (EntityState.Added or EntityState.Modified) ||
                entry.HasChangedOwnedEntites())
            {
                var utcNow = _timeProvider.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.CreatedAtUtc = utcNow;
                }

                entry.Entity.LastModifiedBy = _user.Id;
                entry.Entity.LastModifiedUtc = utcNow;

                //TODO  : Refactor to recursive method or at least to avoid code duplication
                foreach (var ownedEntry in entry.References)
                {
                    if (ownedEntry.TargetEntry is { Entity: AuditableEntity ownedEntity ,
                                                    State : (EntityState.Added or EntityState.Modified) })
                    {
                        if (ownedEntry.TargetEntry.State == EntityState.Added)
                        {
                            ownedEntity.CreatedBy = _user.Id;
                            ownedEntity.CreatedAtUtc = utcNow;
                        }

                        ownedEntity.LastModifiedBy = _user.Id;
                        ownedEntity.LastModifiedUtc = utcNow;
                    }
                }
            }
        }


    }


}
public static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntites(this EntityEntry entry) => 
        entry.References.Any(r=> 
        r.TargetEntry?.Metadata.IsOwned() == true &&
        (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State ==EntityState.Modified || r.TargetEntry.HasChangedOwnedEntites())
        );
}

