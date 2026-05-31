using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRO.Domain.Entities;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;

namespace SRO.Api.Controllers;

/// <summary>
/// Temporary controller for testing the planning engine with synthetic data.
/// Remove before going to production.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestPlanningController : ControllerBase
{
    private readonly SroDbContext _db;
    private readonly IPlanningService _planningService;
    private readonly ITenantProvider _tenantProvider;

    public TestPlanningController(SroDbContext db, IPlanningService planningService, ITenantProvider tenantProvider)
    {
        _db = db;
        _planningService = planningService;
        _tenantProvider = tenantProvider;
    }

    private async Task CleanTestData()
    {
        var oldMatches = await _db.Matches.Where(m => m.WedstrijdCode.StartsWith("TEST-")).ToListAsync();
        _db.Matches.RemoveRange(oldMatches);
        var oldTeams = await _db.Teams.Where(t => t.TeamCode >= 90001 && t.TeamCode <= 90030).ToListAsync();
        _db.Teams.RemoveRange(oldTeams);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds 20 supplier teams + edge-case matches WITHOUT running the planning engine.
    /// POST /api/testplanning/seed
    /// 
    /// RESERVE-TOEWIJZING: Wanneer een wedstrijd een thuiswedstrijd is van KV Wageningen 1 t/m 5,
    /// dan levert de KNKB normaal de officiële scheidsrechter. Het team dat hier wordt toegewezen
    /// is de "reserve" — zij staan paraat als backup mocht de KNKB-scheids niet komen.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedOnly()
    {
        var tenantId = _tenantProvider.TenantId;
        await CleanTestData();

        // ═══════════════════════════════════════════════════════════════════
        // 20 SUPPLIER TEAMS — duidelijke namen zodat je meteen ziet wie het is
        // ═══════════════════════════════════════════════════════════════════
        var teams = new List<Team>
        {
            // --- SENIOREN (10 teams) ---
            T(tenantId, 90001, "Heren 1", "Senioren", "Hoofdklasse", SupplierCategorie.Senioren),
            T(tenantId, 90002, "Heren 2", "Senioren", "1e klasse", SupplierCategorie.Senioren),
            T(tenantId, 90003, "Heren 3", "Senioren", "2e klasse", SupplierCategorie.Senioren),
            T(tenantId, 90004, "Heren 4", "Senioren", "3e klasse", SupplierCategorie.Senioren),
            T(tenantId, 90005, "Heren 5", "Senioren", "4e klasse", SupplierCategorie.Senioren),
            T(tenantId, 90006, "Dames 1", "Senioren", "Hoofdklasse", SupplierCategorie.Senioren),
            T(tenantId, 90007, "Dames 2", "Senioren", "1e klasse", SupplierCategorie.Senioren),
            T(tenantId, 90008, "Dames 3", "Senioren", "2e klasse", SupplierCategorie.Senioren),
            T(tenantId, 90009, "Veteranen 1", "Senioren", "Overgangsklasse", SupplierCategorie.Senioren),
            T(tenantId, 90010, "Veteranen 2", "Senioren", "1e klasse", SupplierCategorie.Senioren),

            // --- JEUGD (8 teams) ---
            T(tenantId, 90011, "Jongens A1", "Jeugd", "Junioren A", SupplierCategorie.Jeugd),
            T(tenantId, 90012, "Jongens A2", "Jeugd", "Junioren A", SupplierCategorie.Jeugd),
            T(tenantId, 90013, "Jongens B1", "Jeugd", "Junioren B", SupplierCategorie.Jeugd),
            T(tenantId, 90014, "Meisjes A1", "Jeugd", "Aspiranten A", SupplierCategorie.Jeugd),
            T(tenantId, 90015, "Meisjes A2", "Jeugd", "Aspiranten A", SupplierCategorie.Jeugd),
            T(tenantId, 90016, "Meisjes B1", "Jeugd", "Aspiranten B", SupplierCategorie.Jeugd),
            T(tenantId, 90017, "Jongens C1", "Jeugd", "Pupillen", SupplierCategorie.Jeugd),
            T(tenantId, 90018, "Meisjes C1", "Jeugd", "Pupillen", SupplierCategorie.Jeugd),

            // --- WEDSTRIJDSPORT (2 teams — mogen alle wedstrijden fluiten) ---
            T(tenantId, 90019, "Gemengd 1", "Senioren", "Wedstrijdsport", SupplierCategorie.Wedstrijdsport),
            T(tenantId, 90020, "Gemengd 2", "Senioren", "Wedstrijdsport", SupplierCategorie.Wedstrijdsport),
        };

        var sat = new DateTime(2026, 4, 11); // Zaterdag 1
        var sun = new DateTime(2026, 4, 12); // Zondag
        var sat2 = new DateTime(2026, 4, 18); // Zaterdag 2
        var matches = new List<Match>();

        // ═══════════════════════════════════════════════════════════════════
        // EIGEN WEDSTRIJDEN — teams die zelf spelen (en dus NIET kunnen fluiten)
        // ═══════════════════════════════════════════════════════════════════

        // Heren 1 speelt thuis om 14:30 → blocked 14:00-16:30 (home buffer)
        matches.Add(Own("TEST-OWN-H1", sat, "14:30", "Heren 1", "HV Oranje Nassau",
            "Senioren Hoofdklasse", "Sportpark De Zoom", isThuis: true, tenantId));

        // Heren 2 speelt UIT om 12:00 → blocked 10:30-14:15 (away 90+90+45)
        matches.Add(Own("TEST-OWN-H2", sat, "12:00", "Hellas Den Haag", "Heren 2",
            "Senioren 1e klasse", "Sportpark Hellas", isThuis: false, tenantId));

        // Heren 3 speelt thuis om 10:00 → blocked 09:30-12:00
        matches.Add(Own("TEST-OWN-H3", sat, "10:00", "Heren 3", "Push Breda",
            "Senioren 2e klasse", "Sportpark De Zoom", isThuis: true, tenantId));

        // Dames 1 speelt UIT om 15:00 → blocked 13:30-16:45
        matches.Add(Own("TEST-OWN-D1", sat, "15:00", "HGC Wassenaar", "Dames 1",
            "Senioren Hoofdklasse", "Sportpark HGC", isThuis: false, tenantId));

        // Jongens A1 speelt UIT om 10:00 → blocked 08:30-12:15
        matches.Add(Own("TEST-OWN-JA1", sat, "10:00", "Ares Delft JA1", "Jongens A1",
            "Junioren A 1e klasse", "Sportpark Ares", isThuis: false, tenantId));

        // Jongens A2 speelt thuis om 12:00 → blocked 11:30-14:00
        matches.Add(Own("TEST-OWN-JA2", sat, "12:00", "Jongens A2", "SCHC JA1",
            "Junioren A 2e klasse", "Sportpark De Zoom", isThuis: true, tenantId));

        // Meisjes A1 speelt UIT om 14:00 → blocked 12:30-16:15
        matches.Add(Own("TEST-OWN-MA1", sat, "14:00", "Ede MA1", "Meisjes A1",
            "Aspiranten A 1e klasse", "Sportpark Ede", isThuis: false, tenantId));

        // Gemengd 1 speelt thuis om 09:00 → blocked 08:30-11:00
        matches.Add(Own("TEST-OWN-G1", sat, "09:00", "Gemengd 1", "WS Arnhem",
            "Wedstrijdsport Overgangsklasse", "Sportpark De Zoom", isThuis: true, tenantId));

        // ═══════════════════════════════════════════════════════════════════
        // TE FLUITEN WEDSTRIJDEN — edge cases met toelichting
        // ═══════════════════════════════════════════════════════════════════

        // --- EDGE CASE 1: Tijdconflicten senioren ---
        // 10:00 sen: Heren 2 blocked (uit 10:30-14:15), Heren 3 blocked (thuis 09:30-12:00)
        //            → Heren 1 of Heren 4/5 + Dames
        matches.Add(Fluit("TEST-FLUIT-001", sat, "10:00",
            "Rapide Vught H1", "HOC Rotterdam H2",
            "Senioren Hoofdklasse", "Sportpark De Zoom", "Veld 2", tenantId));

        // 10:30 sen: Nog steeds Heren 2+3 blocked, verbruikt nog een senior team
        matches.Add(Fluit("TEST-FLUIT-002", sat, "10:30",
            "Upward Arnhem H1", "USHC Utrecht H2",
            "Senioren 1e klasse", "Sportpark De Zoom", "Veld 3", tenantId));

        // 11:00 sen: Heren 2 blocked, Heren 3 nu net vrij (12:00 - OK na eigen 10:00+90min+30=12:00? exact!)
        matches.Add(Fluit("TEST-FLUIT-003", sat, "11:00",
            "Schaerweijde H1", "HDS Leiden H1",
            "Senioren Hoofdklasse", "Sportpark Noord", "Veld 1", tenantId));

        // --- EDGE CASE 2: Team speelt ZELF op hetzelfde moment ---
        // 14:30 sen: Heren 1 speelt zelf! Heren 2 net vrij na 14:15 (12:00 uit + 90min + 45min = 14:15)
        matches.Add(Fluit("TEST-FLUIT-004", sat, "14:30",
            "Alliance H1", "Be Quick Hilversum H1",
            "Senioren Hoofdklasse", "Sportpark West", "Veld 1", tenantId));

        // 15:00 sen: Heren 1 blocked (speelt 14:30 thuis → 14:00-16:30), Dames 1 blocked (uit 13:30-16:45)
        matches.Add(Fluit("TEST-FLUIT-005", sat, "15:00",
            "Spandersbosch H1", "WHC Weesp H1",
            "Senioren Hoofdklasse", "Sportpark Oost", "Veld 1", tenantId));

        // --- EDGE CASE 3: Jeugdwedstrijden met beperkte beschikbaarheid ---
        // 10:00 jgd: Jongens A1 blocked (uit 08:30-12:15), Jongens A2 vrij → krijgt deze
        matches.Add(Fluit("TEST-FLUIT-006", sat, "10:00",
            "Kampong JA2", "HDM JA1",
            "Junioren A 2e klasse", "Sportpark De Zoom", "Veld 4", tenantId));

        // 10:00 jgd: Jongens A1 blocked, Jongens A2 vrij maar al bezet door 006!
        //            Jongens B1 is Junioren B → mag ook Junior-wedstrijd fluiten
        //            → Als B1 vrij is krijgt die hem, anders UNASSIGNED
        matches.Add(Fluit("TEST-FLUIT-007", sat, "10:00",
            "Hurley JA1", "Laren JA2",
            "Junioren A 1e klasse", "Sportpark De Zoom", "Veld 5", tenantId));

        // 10:00 jgd: DERDE jeugdwedstrijd tegelijk! Test capaciteitstekort
        //            Meisjes A1 blocked (uit 12:30-16:15? nee die is pas 14:00, 10:00 kan nog!)
        //            Maar check Meisjes A2 en Jongens C1/Meisjes C1 (Pupillen = "pupil" in klasse)
        matches.Add(Fluit("TEST-FLUIT-008", sat, "10:00",
            "Wageningen MC1", "Arnhem MC1",
            "Pupillen C 1e klasse", "Sportpark De Zoom", "Veld 6", tenantId));

        // --- EDGE CASE 4: Reserve-toewijzing (Wageningen 4+5 thuiswedstrijden) ---
        // Wageningen 1,2,3 + U19-1 → KNKV levert altijd scheids → IsKnkvMatch=true (niet in planning)
        // Wageningen 4,5 → KNKV stuurt MISSCHIEN een scheids (pas donderdag duidelijk)
        //                  → WEL inplannen maar IsReserveAssignment=true (team staat paraat als backup)

        // Wageningen 4 thuis → reserve-toewijzing
        matches.Add(Fluit("TEST-FLUIT-009", sat, "17:00",
            "KV Wageningen 4", "SCHC 3",
            "Senioren 2e klasse", "Sportpark De Zoom", "Veld 1", tenantId));

        // Wageningen 5 tegelijk met Wageningen 4 → 2 reserve-teams nodig op hetzelfde moment!
        matches.Add(Fluit("TEST-FLUIT-010", sat, "17:00",
            "KV Wageningen 5", "Hurley 2",
            "Senioren 3e klasse", "Sportpark De Zoom", "Veld 2", tenantId));

        // Wageningen 1 thuis → IsKnkvMatch=true → deze verschijnt NIET in de planning
        // (toegevoegd als controle: de engine moet deze overslaan)
        matches.Add(new Match
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            WedstrijdCode = "TEST-KNKV-W1",
            WedstrijdDatum = sun.AddHours(14).AddMinutes(30), AanvangsTijd = "14:30",
            ThuisTeam = "KV Wageningen 1", UitTeam = "Bloemendaal 1",
            Klasse = "Senioren Ereklasse", Competitie = "Voorjaarscompetitie",
            Accommodatie = "Sportpark De Zoom", Veld = "Veld 1", Plaats = "Wageningen",
            Status = MatchStatus.Gepland, IsThuis = true,
            Period = SeasonPeriod.VoorjaarVeld, AssignmentStatus = AssignmentStatus.Draft,
            IsKnkvMatch = true // ← KNKV levert scheids, niet plannen!
        });

        // Wageningen 2 thuis → ook IsKnkvMatch=true
        matches.Add(new Match
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            WedstrijdCode = "TEST-KNKV-W2",
            WedstrijdDatum = sat.AddHours(17), AanvangsTijd = "17:00",
            ThuisTeam = "KV Wageningen 2", UitTeam = "Kampong 2",
            Klasse = "Senioren Overgangsklasse", Competitie = "Voorjaarscompetitie",
            Accommodatie = "Sportpark De Zoom", Veld = "Veld 3", Plaats = "Wageningen",
            Status = MatchStatus.Gepland, IsThuis = true,
            Period = SeasonPeriod.VoorjaarVeld, AssignmentStatus = AssignmentStatus.Draft,
            IsKnkvMatch = true
        });

        // --- EDGE CASE 5: Overbelasting — te veel wedstrijden voor beschikbare teams ---
        // Zaterdag 2: veel wedstrijden tegelijk, test round-robin eerlijke verdeling
        matches.Add(Fluit("TEST-FLUIT-012", sat2, "10:00",
            "Hurley H2", "Kampong H3",
            "Senioren 1e klasse", "Sportpark De Zoom", "Veld 1", tenantId));

        matches.Add(Fluit("TEST-FLUIT-013", sat2, "10:00",
            "Alliance H3", "HGC H4",
            "Senioren 2e klasse", "Sportpark De Zoom", "Veld 2", tenantId));

        matches.Add(Fluit("TEST-FLUIT-014", sat2, "10:00",
            "Laren H2", "Pinoke H3",
            "Senioren 2e klasse", "Sportpark De Zoom", "Veld 3", tenantId));

        matches.Add(Fluit("TEST-FLUIT-015", sat2, "10:00",
            "SCHC H5", "Rapide H3",
            "Senioren 3e klasse", "Sportpark De Zoom", "Veld 4", tenantId));

        matches.Add(Fluit("TEST-FLUIT-016", sat2, "10:00",
            "Arnhem JA1", "Ede JA1",
            "Junioren A 1e klasse", "Sportpark De Zoom", "Veld 5", tenantId));

        matches.Add(Fluit("TEST-FLUIT-017", sat2, "10:00",
            "Upward JB1", "Push JB1",
            "Junioren B 1e klasse", "Sportpark De Zoom", "Veld 6", tenantId));

        // --- EDGE CASE 6: Wedstrijdsport-team als vangnet ---
        // Alleen de 2 Gemengd-teams (Wedstrijdsport) mogen alle wedstrijden fluiten
        // Als alle senioren/jeugd bezet zijn, moeten zij inspringen
        matches.Add(Fluit("TEST-FLUIT-018", sat2, "10:00",
            "HOC H5", "Vught H4",
            "Senioren 4e klasse", "Sportpark De Zoom", "Veld 7", tenantId));

        // --- EDGE CASE 7: Spanning over 2e dag — Gemengd 1 blocked door eigen wedstrijd ---
        // Gemengd 1 speelt om 09:00 op zat 1, maar zat 2 zijn ze vrij
        // Test dat conflict alleen geldt op dezelfde dag
        matches.Add(Fluit("TEST-FLUIT-019", sat, "09:30",
            "WHC MA1", "Pinoke MA1",
            "Aspiranten A 2e klasse", "Sportpark De Zoom", "Veld 7", tenantId));

        // --- EDGE CASE 8: Wageningen 5 thuis op zat 2 — test reserve op andere dag ---
        matches.Add(Fluit("TEST-FLUIT-020", sat2, "14:30",
            "KV Wageningen 5", "Schaerweijde 2",
            "Senioren 3e klasse", "Sportpark De Zoom", "Veld 1", tenantId));

        _db.Teams.AddRange(teams);
        _db.Matches.AddRange(matches);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Testdata geladen! 20 supplier teams, 8 eigen wedstrijden, 20 te fluiten wedstrijden.",
            supplierTeams = teams.Select(t => new { t.TeamNaam, Categorie = t.SupplierCategorie.ToString() }),
            eigenWedstrijden = matches.Count(m => m.WedstrijdCode.StartsWith("TEST-OWN")),
            teFluitenWedstrijden = matches.Count(m => m.WedstrijdCode.StartsWith("TEST-FLUIT")),
            edgeCases = new[]
            {
                "1. TIJDCONFLICT: Heren 2+3 blocked om 10:00, beperkte keuze",
                "2. SPEELT ZELF: Heren 1 speelt 14:30, kan niet fluiten tot 16:30",
                "3. JEUGD TEKORT: 3 jeugdwedstrijden tegelijk om 10:00, beperkt aanbod",
                "4. RESERVE: Wageningen 4+5 thuis → team is BACKUP (KNKV stuurt misschien nog scheids, donderdag duidelijk)",
                "5. KNKV EXCLUSIE: Wageningen 1+2 thuis met IsKnkvMatch=true → worden NIET gepland",
                "6. OVERBELASTING: 7 wedstrijden tegelijk op zat 2 om 10:00 — genoeg teams?",
                "7. WEDSTRIJDSPORT VANGNET: Gemengd 1+2 mogen alles fluiten als rest bezet is",
                "8. RESERVE ANDERE DAG: Wageningen 5 thuis op zat 2 → reserve-vinkje"
            }
        });
    }

    /// <summary>
    /// Seeds test data and runs the planning engine.
    /// POST /api/testplanning/run
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> SeedAndRun()
    {
        // First seed
        await SeedOnly();

        // Then run planning
        var result = await _planningService.GeneratePlanningAsync(SeasonPeriod.VoorjaarVeld);

        // Read back results
        var assignedMatches = await _db.Matches
            .Where(m => m.WedstrijdCode.StartsWith("TEST-FLUIT") && m.AssignedTeamId != null)
            .Include(m => m.AssignedTeam)
            .OrderBy(m => m.WedstrijdDatum).ThenBy(m => m.AanvangsTijd)
            .Select(m => new
            {
                m.WedstrijdCode,
                Datum = m.WedstrijdDatum.ToString("yyyy-MM-dd HH:mm"),
                m.ThuisTeam,
                m.Klasse,
                AssignedTeam = m.AssignedTeam!.TeamNaam,
                m.AssignmentStatus,
                m.IsReserveAssignment
            })
            .ToListAsync();

        var unassignedMatches = await _db.Matches
            .Where(m => m.WedstrijdCode.StartsWith("TEST-FLUIT") && m.AssignedTeamId == null)
            .OrderBy(m => m.WedstrijdDatum).ThenBy(m => m.AanvangsTijd)
            .Select(m => new { m.WedstrijdCode, Datum = m.WedstrijdDatum.ToString("yyyy-MM-dd HH:mm"), m.ThuisTeam })
            .ToListAsync();

        return Ok(new
        {
            result.Assigned,
            result.Unassigned,
            toewijzingen = assignedMatches,
            nietToegewezen = unassignedMatches
        });
    }

    /// <summary>
    /// Cleans up test data.
    /// DELETE /api/testplanning/cleanup
    /// </summary>
    [HttpDelete("cleanup")]
    public async Task<IActionResult> Cleanup()
    {
        var testMatches = await _db.Matches
            .Where(m => m.WedstrijdCode.StartsWith("TEST-"))
            .ToListAsync();
        _db.Matches.RemoveRange(testMatches);

        var testTeams = await _db.Teams
            .Where(t => t.TeamCode >= 90001 && t.TeamCode <= 90030)
            .ToListAsync();
        _db.Teams.RemoveRange(testTeams);

        await _db.SaveChangesAsync();
        return Ok(new { removed = new { matches = testMatches.Count, teams = testTeams.Count } });
    }

    // ═══════════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════════

    private static Team T(Guid tenantId, int code, string naam, string leeftijd, string klasse, SupplierCategorie cat) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        TeamCode = code,
        LokaleTeamCode = code - 90000,
        TeamNaam = naam,
        LeeftijdsCategorie = leeftijd,
        Klasse = klasse,
        SpelSoort = "Veld",
        IsSupplier = true,
        SupplierCategorie = cat
    };

    private static Match Own(string code, DateTime day, string time, string thuis, string uit,
        string klasse, string accommodatie, bool isThuis, Guid tenantId)
    {
        var parts = time.Split(':');
        var datum = day.AddHours(int.Parse(parts[0])).AddMinutes(int.Parse(parts[1]));
        return new Match
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            WedstrijdCode = code,
            WedstrijdDatum = datum, AanvangsTijd = time,
            ThuisTeam = thuis, UitTeam = uit,
            Klasse = klasse, Competitie = "Voorjaarscompetitie",
            Accommodatie = accommodatie, Veld = "Veld 1",
            Plaats = isThuis ? "Wageningen" : accommodatie.Replace("Sportpark ", ""),
            Status = MatchStatus.Gepland, IsThuis = isThuis,
            Period = SeasonPeriod.VoorjaarVeld, AssignmentStatus = AssignmentStatus.Draft
        };
    }

    private static Match Fluit(string code, DateTime day, string time, string thuis, string uit,
        string klasse, string accommodatie, string veld, Guid tenantId)
    {
        var parts = time.Split(':');
        var datum = day.AddHours(int.Parse(parts[0])).AddMinutes(int.Parse(parts[1]));
        return new Match
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            WedstrijdCode = code,
            WedstrijdDatum = datum, AanvangsTijd = time,
            ThuisTeam = thuis, UitTeam = uit,
            Klasse = klasse, Competitie = "Voorjaarscompetitie",
            Accommodatie = accommodatie, Veld = veld, Plaats = "Wageningen",
            Status = MatchStatus.Gepland, IsThuis = true,
            Period = SeasonPeriod.VoorjaarVeld, AssignmentStatus = AssignmentStatus.Draft
        };
    }
}
