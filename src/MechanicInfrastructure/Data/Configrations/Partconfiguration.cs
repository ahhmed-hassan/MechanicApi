
using MechanicDomain.RepairTasks.Parts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicInfrastructure.Data.Configrations;

public class Partconfiguration : IEntityTypeConfiguration<Part>
{
    

    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p=> p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Cost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Quantity)
            .IsRequired();
      
    }
}
