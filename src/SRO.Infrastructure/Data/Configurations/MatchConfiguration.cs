using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.WedstrijdCode).HasMaxLength(50).IsRequired();
        builder.Property(m => m.AanvangsTijd).HasMaxLength(10);
        builder.Property(m => m.ThuisTeam).HasMaxLength(200);
        builder.Property(m => m.UitTeam).HasMaxLength(200);
        builder.Property(m => m.Klasse).HasMaxLength(100);
        builder.Property(m => m.Competitie).HasMaxLength(100);
        builder.Property(m => m.Accommodatie).HasMaxLength(200);
        builder.Property(m => m.Veld).HasMaxLength(100);
        builder.Property(m => m.Plaats).HasMaxLength(100);
        builder.Property(m => m.Scheidsrechters).HasMaxLength(500);

        builder.HasIndex(m => new { m.TenantId, m.WedstrijdCode }).IsUnique();

        builder.HasOne(m => m.AssignedTeam)
            .WithMany()
            .HasForeignKey(m => m.AssignedTeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
