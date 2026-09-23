using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;
using US.Infrastructure.Identity;

namespace US.Infrastructure.Persistence.Configurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.HasKey(m => new { m.OrganizationId, m.UserId });

        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(m => m.UserId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}