using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SRO.Domain.Entities;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;

namespace SRO.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly SroDbContext _db;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncService syncService, SroDbContext db, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Trigger a full sync for the given tenant (or the default dev tenant).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TriggerSync([FromQuery] Guid? tenantId = null)
    {
        var id = tenantId ?? await GetDefaultTenantIdAsync();
        if (id == Guid.Empty)
            return BadRequest("No tenant found. Seed the database first via POST /api/sync/seed.");

        await _syncService.SyncAllAsync(id);

        var matchCount = await _db.Matches.IgnoreQueryFilters().Where(m => m.TenantId == id).CountAsync();
        var teamCount = await _db.Teams.IgnoreQueryFilters().Where(t => t.TenantId == id).CountAsync();

        return Ok(new
        {
            message = "Sync completed",
            tenantId = id,
            matches = matchCount,
            teams = teamCount
        });
    }

    /// <summary>
    /// Seed the default tenant (KV Wageningen) for development.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        var existing = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.SportlinkClientId == "IcvlRrY2Wp");

        if (existing != null)
            return Ok(new { message = "Tenant already exists", tenantId = existing.Id });

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty, // Tenant entity doesn't filter itself
            Name = "KV Wageningen",
            SportlinkClientId = "IcvlRrY2Wp",
            ClubRelatieCode = "NCX38R6",
            CreatedAt = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Tenant seeded", tenantId = tenant.Id });
    }

    /// <summary>
    /// Get sync status: count of matches and teams per tenant.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromQuery] Guid? tenantId = null)
    {
        var id = tenantId ?? await GetDefaultTenantIdAsync();
        if (id == Guid.Empty)
            return Ok(new { message = "No tenant found" });

        var matchCount = await _db.Matches.IgnoreQueryFilters().Where(m => m.TenantId == id).CountAsync();
        var teamCount = await _db.Teams.IgnoreQueryFilters().Where(t => t.TenantId == id).CountAsync();
        var thuisMatches = await _db.Matches.IgnoreQueryFilters().Where(m => m.TenantId == id && m.IsThuis).CountAsync();
        var zonderScheids = await _db.Matches.IgnoreQueryFilters()
            .Where(m => m.TenantId == id && m.IsThuis && m.Scheidsrechters == "")
            .CountAsync();

        return Ok(new
        {
            tenantId = id,
            totalMatches = matchCount,
            thuisMatches,
            matchesZonderScheidsrechter = zonderScheids,
            teams = teamCount
        });
    }

    private async Task<Guid> GetDefaultTenantIdAsync()
    {
        var tenant = await _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync();
        return tenant?.Id ?? Guid.Empty;
    }
}
