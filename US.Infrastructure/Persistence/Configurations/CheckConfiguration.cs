using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monitor = US.Domain.Entities.Monitor;
using Check = US.Domain.Entities.Check;

namespace US.Infrastructure.Persistence.Configurations;

public sealed class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder.ToTable("checks");

        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.MonitorId, c.CheckedAt });

        builder.Property(c => c.CheckedAt).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().IsRequired();

        builder.Property(c => c.ErrorMessage).HasMaxLength(2000);

        builder.HasOne<Monitor>()
            .WithMany()
            .HasForeignKey(c => c.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}