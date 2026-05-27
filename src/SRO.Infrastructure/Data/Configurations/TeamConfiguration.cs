using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TeamNaam).HasMaxLength(200).IsRequired();
        builder.Property(t => t.LeeftijdsCategorie).HasMaxLength(50);
        builder.Property(t => t.Klasse).HasMaxLength(100);
        builder.Property(t => t.SpelSoort).HasMaxLength(50);

        builder.HasIndex(t => new { t.TenantId, t.TeamCode }).IsUnique();
    }
}
