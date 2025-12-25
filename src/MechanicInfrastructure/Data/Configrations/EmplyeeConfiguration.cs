using MechanicDomain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicInfrastructure.Data.Configrations;

internal class EmplyeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<string>(); //Default to string representation : https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations#built-in-converters


    }
}
