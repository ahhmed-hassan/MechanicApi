
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net;

namespace MechanicInfrastructure.Data.Configrations;

public class WorkORderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(wo => wo.Id).IsClustered(false);

        builder.Property(wo => wo.LaborId)
            .IsRequired();

        builder.HasOne(wo=>wo.Labor)
            .WithMany()
            .HasForeignKey(wo=> wo.LaborId)
            .IsRequired();

        builder.HasOne(wo=> wo.Invoice)
            .WithOne()
            .HasForeignKey<Invoice>(i=> i.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wo => wo.RepairTasks)
            .WithMany()
            .UsingEntity(j=> j.ToTable("WorkOrderRepairTasks"));

        builder.HasOne(wo=> wo.Vehicle)
            .WithMany()
            .HasForeignKey(wo=> wo.VehicleId)
            .IsRequired();

        builder.HasIndex(w => w.LaborId);

        builder.HasIndex(w => w.VehicleId);

        builder.HasIndex(w => w.State);

        builder.HasIndex(wo => new { wo.StartAtUtc, wo.EndAtUtc });
       

        builder.Property(wo=> wo.State)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(wo => wo.Spot)
           .IsRequired()
           .HasConversion<string>();

        builder.Property(wo=> wo.StartAtUtc)
            .IsRequired();

        builder.Property(wo=> wo.EndAtUtc)
            .IsRequired();

        builder.Property(wo => wo.Tax)
            .HasPrecision(18, 2);

        builder.Property(wo => wo.Discount)
            .HasPrecision(18, 2);

        builder.Ignore(wo => wo.Total);
        builder.Ignore(wo => wo.TotalLaborCost);
        builder.Ignore(wo => wo.TotalPartsCost);
        builder.Ignore(w => w.EstimatedDuration);
    }
}
