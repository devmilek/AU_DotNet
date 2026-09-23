using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using US.Domain.Entities;

namespace US.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();
        builder.Property(i => i.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(i => i.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(i => i.TokenHash).IsUnique();

        // maks. jedno aktywne zaproszenie na (organizacja, email)
        builder.HasIndex(i => new { i.OrganizationId, i.Email })
            .IsUnique()
            .HasFilter("\"AcceptedAt\" IS NULL AND \"RevokedAt\" IS NULL");

        builder.HasOne(i => i.Organization)
            .WithMany()
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}