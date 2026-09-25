using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monitor = US.Domain.Entities.Monitor;
using Check = US.Domain.Entities.Check;
using MaintenanceOccurrence = US.Domain.Entities.MaintenanceOccurrence;

namespace US.Infrastructure.Persistence.Configurations;

public sealed class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    /// <summary>Surowe checki — do szczegółów incydentów i debugowania; dłużej żyją tylko agregaty.</summary>
    public static readonly TimeSpan RawRetention = TimeSpan.FromDays(30);

    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder.ToTable("checks");

        // snake_case: YC.EntityFrameworkCore.TigerData.TimescaleDB 1.1.0 wstawia nazwy kolumn do
        // timescaledb.segmentby/orderby bez cudzysłowów, więc kolumny z wielkimi literami nie działają
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.MonitorId).HasColumnName("monitor_id");
        builder.Property(c => c.CheckedAt).HasColumnName("checked_at");
        builder.Property(c => c.Status).HasColumnName("status");
        builder.Property(c => c.ResponseTimeMs).HasColumnName("response_time_ms");
        builder.Property(c => c.StatusCode).HasColumnName("status_code");
        builder.Property(c => c.ErrorMessage).HasColumnName("error_message");
        builder.Property(c => c.WasInMaintenance).HasColumnName("was_in_maintenance");
        builder.Property(c => c.MaintenanceOccurrenceId).HasColumnName("maintenance_occurrence_id");

        // hypertabela: każdy unikalny klucz musi zawierać kolumnę partycjonującą
        builder.HasKey(c => new { c.Id, c.CheckedAt });
        builder.HasIndex(c => new { c.MonitorId, c.CheckedAt });

        builder.Property(c => c.CheckedAt).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().IsRequired();

        builder.Property(c => c.ErrorMessage).HasMaxLength(2000);

        builder.Property(c => c.WasInMaintenance)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne<Monitor>()
            .WithMany()
            .HasForeignKey(c => c.MonitorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MaintenanceOccurrence>()
            .WithMany()
            .HasForeignKey(c => c.MaintenanceOccurrenceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.MaintenanceOccurrenceId)
            .HasFilter("maintenance_occurrence_id IS NOT NULL");

        builder.IsHypertable(c => c.CheckedAt, chunkInterval: TimeSpan.FromDays(1));

        // kompresja: zapytania prawie zawsze filtrują po monitorze i sortują od najnowszych
        builder.HasColumnstore(cs => cs
            .SegmentBy(c => c.MonitorId)
            .OrderByDescending(c => c.CheckedAt));
        builder.HasColumnstorePolicy(after: TimeSpan.FromDays(1));

        builder.HasRetentionPolicy(dropAfter: RawRetention);
    }
}
