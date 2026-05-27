namespace SRO.Domain.Entities;

public enum SupplierCategorie
{
    Geen,
    Senioren,
    Jeugd,
    Wedstrijdsport,
    Midweek
}

public class Team : BaseEntity
{
    public int TeamCode { get; set; }
    public int LokaleTeamCode { get; set; } = -1;
    public string TeamNaam { get; set; } = string.Empty;
    public string LeeftijdsCategorie { get; set; } = string.Empty;
    public string Klasse { get; set; } = string.Empty;
    public string SpelSoort { get; set; } = string.Empty;
    public bool IsSupplier { get; set; } = false;
    public SupplierCategorie SupplierCategorie { get; set; } = SupplierCategorie.Geen;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Member> Members { get; set; } = new List<Member>();
}
