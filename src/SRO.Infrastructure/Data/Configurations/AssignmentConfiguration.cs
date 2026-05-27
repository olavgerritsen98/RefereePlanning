using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Data.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Match)
            .WithMany()
            .HasForeignKey(a => a.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Team)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => new { a.TenantId, a.MatchId }).IsUnique();
    }
}
