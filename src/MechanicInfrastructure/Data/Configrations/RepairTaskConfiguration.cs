using MechanicDomain.RepairTasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicInfrastructure.Data.Configrations;

public class RepairTaskConfiguration : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .ValueGeneratedNever();

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.EstimatedDurationInMins)
            .IsRequired()
            .HasConversion<string>(); // Default to string representation : https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations#built-in-converters

        builder.Property(rt => rt.LaborCost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasMany(rt => rt.Parts)
            .WithOne()
            .HasForeignKey("RepairtTasksId") //Shadow property for foreign key https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many#required-one-to-many-with-shadow-foreign-key
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(rt => rt.Parts)
            .UsePropertyAccessMode(PropertyAccessMode.Field); //Access parts collection via its backing field not property's getter 

    }
}
