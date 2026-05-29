using Microsoft.EntityFrameworkCore;
using SRO.Domain.Constraints;
using SRO.Domain.Engine;
using SRO.Domain.Entities;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;

namespace SRO.Infrastructure.Services;

public class PlanningService : IPlanningService
{
    private readonly SroDbContext _db;
    private readonly AssignmentEngine _engine;

    public PlanningService(SroDbContext db, AssignmentEngine engine)
    {
        _db = db;
        _engine = engine;
    }

    public async Task<AssignmentResult> GeneratePlanningAsync(SeasonPeriod period)
    {
        // 1. Get all supplier teams
        var supplierTeams = await _db.Teams
            .Where(t => t.IsSupplier)
            .ToListAsync();

        // 2. Get all matches for this period (full schedule for conflict detection)
        var allMatchesForPeriod = await _db.Matches
            .Where(m => m.Period == period && m.Status == MatchStatus.Gepland)
            .ToListAsync();

        // 3. Filter down to only Draft matches that need assignment
        var draftMatches = allMatchesForPeriod
            .Where(m => m.AssignmentStatus == AssignmentStatus.Draft)
            .ToList();

        // 4. Run the engine
        var result = _engine.AssignMatches(draftMatches, supplierTeams, allMatchesForPeriod);

        // 5. Persist the updated matches
        await _db.SaveChangesAsync();

        return result;
    }
}
