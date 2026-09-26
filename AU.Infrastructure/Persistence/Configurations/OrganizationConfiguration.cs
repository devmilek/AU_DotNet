using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AU.Domain.Entities;

namespace AU.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).HasMaxLength(100).IsRequired();
        builder.Property(o => o.Slug).HasMaxLength(60).IsRequired();
        builder.HasIndex(o => o.Slug).IsUnique();

        builder.HasMany(o => o.Members)
            .WithOne(m => m.Organization)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Members).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}