using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.RelatieCode).HasMaxLength(20).IsRequired();
        builder.Property(m => m.Naam).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Voornaam).HasMaxLength(100);
        builder.Property(m => m.Achternaam).HasMaxLength(100);
        builder.Property(m => m.Tussenvoegsel).HasMaxLength(50);
        builder.Property(m => m.Geslacht).HasMaxLength(20);
        builder.Property(m => m.Rol).HasMaxLength(50);

        builder.HasIndex(m => new { m.TenantId, m.RelatieCode, m.TeamId }).IsUnique();

        builder.HasOne(m => m.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
