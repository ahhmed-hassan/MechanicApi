using MechanicDomain.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace MechanicInfrastructure.Data.Configrations;

public static class EntityTypeBuilderExtensions
{
    public static void ApplyAuditableConfiguration<T>(this EntityTypeBuilder<T> builder)
        where T : AuditableEntity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
    }
}
