using MechanicDomain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicInfrastructure.Data.Configrations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ApplyAuditableConfiguration<Invoice>();
        

        builder.ToTable("Invoices");

        builder.Property(i => i.IssuedAtUtc)
            .IsRequired();

        builder.Property(i => i.DiscountAmount)
            .IsRequired()
            .HasPrecision(18,2);

        builder.Property(i=> i.TaxAmount)
            .IsRequired()
            .HasPrecision(18,2);

        builder.Property(i => i.PaidAt);

        builder.Navigation(i => i.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        //TODO : Complete the configuration of Invoice entity for InvoiceLineItems relationship

        builder.OwnsMany(i=> i.LineItems, items =>
        {
            items.ToTable("InvoiceLineItems");
            items.WithOwner().HasForeignKey(item => item.InvoiceId);

            items.HasKey(i=> new {  i.InvoiceId, i.LineNumber });

            items.Property(i=>i.LineNumber)
            .ValueGeneratedNever();

            items.Property(i => i.Description)
                .IsRequired()
                .HasMaxLength(200);

            items.Property(i => i.Quantity)
                .IsRequired();

            items.Property(i => i.UnitPrice)
                .IsRequired()
                .HasPrecision(18,2);

        });

    }
}
