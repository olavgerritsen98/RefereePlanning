namespace SRO.Domain.Entities;

public class Assignment : BaseEntity
{
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public bool IsManualOverride { get; set; } = false;
}
