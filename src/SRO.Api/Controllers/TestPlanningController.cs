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

    /// <summary>
    /// Seeds test data and runs the planning engine.
    /// POST /api/testplanning/run
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> SeedAndRun()
    {
        var tenantId = _tenantProvider.TenantId;

        // --- 1. Create supplier teams ---
        var suppliers = new List<Team>
        {
            new() { Id = Guid.NewGuid(), TenantId = tenantId, TeamCode = 90001, LokaleTeamCode = 1, TeamNaam = "KV Wageningen H1", LeeftijdsCategorie = "Senioren", Klasse = "Hoofdklasse", SpelSoort = "Veld", IsSupplier = true, SupplierCategorie = SupplierCategorie.Senioren },
            new() { Id = Guid.NewGuid(), TenantId = tenantId, TeamCode = 90002, LokaleTeamCode = 2, TeamNaam = "KV Wageningen H2", LeeftijdsCategorie = "Senioren", Klasse = "1e klasse", SpelSoort = "Veld", IsSupplier = true, SupplierCategorie = SupplierCategorie.Senioren },
            new() { Id = Guid.NewGuid(), TenantId = tenantId, TeamCode = 90003, LokaleTeamCode = 3, TeamNaam = "KV Wageningen J1", LeeftijdsCategorie = "Jeugd", Klasse = "Junioren A", SpelSoort = "Veld", IsSupplier = true, SupplierCategorie = SupplierCategorie.Jeugd },
            new() { Id = Guid.NewGuid(), TenantId = tenantId, TeamCode = 90004, LokaleTeamCode = 4, TeamNaam = "KV Wageningen J2", LeeftijdsCategorie = "Jeugd", Klasse = "Aspiranten B", SpelSoort = "Veld", IsSupplier = true, SupplierCategorie = SupplierCategorie.Jeugd },
        };

        // --- 2. Create test matches ---
        var baseDate = new DateTime(2026, 4, 11, 10, 0, 0); // Voorjaar period
        var matches = new List<Match>();

        // 6 senior matches on 2 Saturdays
        for (int i = 0; i < 6; i++)
        {
            var day = i < 3 ? baseDate : baseDate.AddDays(7);
            var time = day.AddHours(i % 3 * 2); // 10:00, 12:00, 14:00
            matches.Add(new Match
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WedstrijdCode = $"TEST-SEN-{i + 1:D3}",
                WedstrijdDatum = time,
                AanvangsTijd = time.ToString("HH:mm"),
                ThuisTeam = $"Tegenstander Senioren {i + 1}",
                UitTeam = "KV Wageningen",
                Klasse = "Senioren Hoofdklasse",
                Competitie = "Voorjaarscompetitie",
                Accommodatie = "Sportpark De Zoom",
                Veld = $"Veld {(i % 2) + 1}",
                Plaats = "Wageningen",
                Status = MatchStatus.Gepland,
                IsThuis = false,
                Period = SeasonPeriod.VoorjaarVeld,
                AssignmentStatus = AssignmentStatus.Draft
            });
        }

        // 4 youth matches
        for (int i = 0; i < 4; i++)
        {
            var day = i < 2 ? baseDate : baseDate.AddDays(7);
            var time = day.AddHours(9 + i % 2 * 2); // 09:00, 11:00
            matches.Add(new Match
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WedstrijdCode = $"TEST-JGD-{i + 1:D3}",
                WedstrijdDatum = time,
                AanvangsTijd = time.ToString("HH:mm"),
                ThuisTeam = $"Tegenstander Jeugd {i + 1}",
                UitTeam = "KV Wageningen JA1",
                Klasse = "Junioren A 1e klasse",
                Competitie = "Voorjaarscompetitie",
                Accommodatie = "Sportpark De Zoom",
                Veld = $"Veld {(i % 2) + 3}",
                Plaats = "Wageningen",
                Status = MatchStatus.Gepland,
                IsThuis = false,
                Period = SeasonPeriod.VoorjaarVeld,
                AssignmentStatus = AssignmentStatus.Draft
            });
        }

        // --- 3. Persist test data ---
        _db.Teams.AddRange(suppliers);
        _db.Matches.AddRange(matches);
        await _db.SaveChangesAsync();

        // --- 4. Run the planning engine ---
        var result = await _planningService.GeneratePlanningAsync(SeasonPeriod.VoorjaarVeld);

        // --- 5. Read back results for response ---
        var assignedMatches = await _db.Matches
            .Where(m => m.WedstrijdCode.StartsWith("TEST-") && m.AssignedTeamId != null)
            .Include(m => m.AssignedTeam)
            .OrderBy(m => m.WedstrijdDatum)
            .Select(m => new
            {
                m.WedstrijdCode,
                Datum = m.WedstrijdDatum.ToString("yyyy-MM-dd HH:mm"),
                m.ThuisTeam,
                m.Klasse,
                AssignedTeam = m.AssignedTeam!.TeamNaam,
                m.AssignmentStatus
            })
            .ToListAsync();

        return Ok(new
        {
            result.Assigned,
            result.Unassigned,
            Toewijzingen = assignedMatches
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
            .Where(t => t.TeamCode >= 90001 && t.TeamCode <= 90010)
            .ToListAsync();
        _db.Teams.RemoveRange(testTeams);

        await _db.SaveChangesAsync();
        return Ok(new { removed = new { matches = testMatches.Count, teams = testTeams.Count } });
    }
}
