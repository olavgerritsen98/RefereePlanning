using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.SportlinkClientId).HasMaxLength(50).IsRequired();
        builder.Property(t => t.ClubRelatieCode).HasMaxLength(50).IsRequired();

        builder.HasMany(t => t.Matches)
            .WithOne()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(t => t.Teams)
            .WithOne()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // Tenant has no TenantId filter on itself
        builder.HasQueryFilter(t => true);
    }
}
