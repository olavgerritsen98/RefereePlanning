namespace SRO.Domain.Entities;

public enum MatchStatus { Gepland, Afgelast, Gespeeld }
public enum AssignmentStatus { Draft, TeamAssigned, PendingNames, Finalized }
public enum SeasonPeriod { NajaarVeld, ZaalHelft1, ZaalHelft2, VoorjaarVeld }

public class Match : BaseEntity
{
    // --- Sportlink Data ---
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

    // --- SRO Plannings Logica ---
    public SeasonPeriod Period { get; set; } 
    public AssignmentStatus AssignmentStatus { get; set; } = AssignmentStatus.Draft;
    public bool IsKnkvMatch { get; set; } = false;
    public bool IsReserveAssignment { get; set; } = false;
    
    // Directe een-op-veel relatie met Team
    public Guid? AssignedTeamId { get; set; }
    public Team? AssignedTeam { get; set; }
    
    // Naam van de specifieke scheidsrechter zodra de teambegeleider deze invult
    public string? FinalRefereeName { get; set; } 
}
