using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.WedstrijdCode).IsRequired().HasMaxLength(50);
        builder.Property(m => m.ThuisTeam).IsRequired().HasMaxLength(100);
        builder.Property(m => m.UitTeam).IsRequired().HasMaxLength(100);
        
        // Enums opslaan als strings voor betere leesbaarheid in de database
        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.AssignmentStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Period)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.FinalRefereeName).HasMaxLength(100);

        // Relatie met Team inrichten
        builder.HasOne(m => m.AssignedTeam)
            .WithMany()
            .HasForeignKey(m => m.AssignedTeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
