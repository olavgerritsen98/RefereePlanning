using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRO.Domain.Entities;
using SRO.Infrastructure.Data;

namespace SRO.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly SroDbContext _db;

    public MatchesController(SroDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMatches([FromQuery] SeasonPeriod? period)
    {
        var query = _db.Matches
            .Include(m => m.AssignedTeam)
            .Where(m => m.Status == MatchStatus.Gepland)
            .AsQueryable();

        if (period.HasValue)
            query = query.Where(m => m.Period == period.Value);

        var matches = await query
            .OrderBy(m => m.WedstrijdDatum)
            .ThenBy(m => m.AanvangsTijd)
            .Select(m => new
            {
                m.Id,
                m.WedstrijdCode,
                Datum = m.WedstrijdDatum.ToString("yyyy-MM-dd"),
                Tijd = m.AanvangsTijd,
                m.ThuisTeam,
                m.UitTeam,
                m.Klasse,
                AssignedTeam = m.AssignedTeam != null ? m.AssignedTeam.TeamNaam : null,
                AssignmentStatus = m.AssignmentStatus.ToString(),
                m.IsKnkvMatch,
                m.IsReserveAssignment
            })
            .ToListAsync();

        return Ok(matches);
    }
}
