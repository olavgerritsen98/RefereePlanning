namespace SRO.Domain.Entities;

public class Member : BaseEntity
{
    public string RelatieCode { get; set; } = string.Empty;
    public string Naam { get; set; } = string.Empty;
    public string Voornaam { get; set; } = string.Empty;
    public string Achternaam { get; set; } = string.Empty;
    public string? Tussenvoegsel { get; set; }
    public string? Geslacht { get; set; }
    public string Rol { get; set; } = string.Empty;

    // FK to Team
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
}
