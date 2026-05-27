namespace SRO.Domain.Entities;

public enum MatchStatus
{
    Gepland,
    Afgelast,
    Gespeeld
}

public class Match : BaseEntity
{
    public string WedstrijdCode { get; set; } = string.Empty;
    public DateTime WedstrijdDatum { get; set; }
    public string AanvangsTijd { get; set; } = string.Empty;
    public string ThuisTeam { get; set; } = string.Empty;
    public string UitTeam { get; set; } = string.Empty;
    public string Klasse { get; set; } = string.Empty;
    public string Competitie { get; set; } = string.Empty;
    public string Accommodatie { get; set; } = string.Empty;
    public string Veld { get; set; } = string.Empty;
    public string Plaats { get; set; } = string.Empty;
    public string Scheidsrechters { get; set; } = string.Empty;
    public MatchStatus Status { get; set; } = MatchStatus.Gepland;
    public bool IsThuis { get; set; } = true;

    // Assignment (nullable)
    public Guid? AssignedTeamId { get; set; }
    public Team? AssignedTeam { get; set; }
}
