namespace SRO.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SportlinkClientId { get; set; } = string.Empty;
    public string ClubRelatieCode { get; set; } = string.Empty;

    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
