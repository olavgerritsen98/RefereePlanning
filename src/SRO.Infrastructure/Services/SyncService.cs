using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRO.Domain.Entities;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;

namespace SRO.Infrastructure.Services;

public class SyncService : ISyncService
{
    private readonly SroDbContext _db;
    private readonly ISportlinkClient _sportlinkClient;
    private readonly ILogger<SyncService> _logger;

    public SyncService(SroDbContext db, ISportlinkClient sportlinkClient, ILogger<SyncService> logger)
    {
        _db = db;
        _sportlinkClient = sportlinkClient;
        _logger = logger;
    }

    public async Task SyncAllAsync(Guid tenantId)
    {
        _logger.LogInformation("═══════════════════════════════════════════");
        _logger.LogInformation("Starting full sync for tenant {TenantId}", tenantId);
        _logger.LogInformation("═══════════════════════════════════════════");

        await SyncTeamsAsync(tenantId);
        await SyncMembersAsync(tenantId);
        await SyncMatchesAsync(tenantId);

        _logger.LogInformation("═══════════════════════════════════════════");
        _logger.LogInformation("Full sync completed for tenant {TenantId}", tenantId);
        _logger.LogInformation("═══════════════════════════════════════════");
    }

    public async Task SyncTeamsAsync(Guid tenantId)
    {
        _logger.LogInformation("───── STEP: Sync Teams ─────");

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId)
            ?? throw new InvalidOperationException($"Tenant {tenantId} not found");

        _logger.LogInformation("Tenant: {Name} (ClientId: {ClientId})", tenant.Name, tenant.SportlinkClientId);
        _logger.LogInformation("Fetching teams from Sportlink API...");

        var sportlinkTeams = await _sportlinkClient.GetTeamsAsync(tenant.SportlinkClientId);
        _logger.LogInformation("Received {Count} unique teams from Sportlink", sportlinkTeams.Count);

        var existingTeams = await _db.Teams
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();
        _logger.LogInformation("Existing teams in database: {Count}", existingTeams.Count);

        var existingByCode = existingTeams.ToDictionary(t => t.TeamCode);

        int added = 0, updated = 0;

        foreach (var incoming in sportlinkTeams)
        {
            if (existingByCode.TryGetValue(incoming.TeamCode, out var existing))
            {
                existing.TeamNaam = incoming.TeamNaam;
                existing.LeeftijdsCategorie = incoming.LeeftijdsCategorie;
                existing.Klasse = incoming.Klasse;
                existing.SpelSoort = incoming.SpelSoort;
                existing.LokaleTeamCode = incoming.LokaleTeamCode;
                updated++;
            }
            else
            {
                var team = new Team
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TeamCode = incoming.TeamCode,
                    LokaleTeamCode = incoming.LokaleTeamCode,
                    TeamNaam = incoming.TeamNaam,
                    LeeftijdsCategorie = incoming.LeeftijdsCategorie,
                    Klasse = incoming.Klasse,
                    SpelSoort = incoming.SpelSoort,
                    IsSupplier = false,
                    SupplierCategorie = SupplierCategorie.Geen
                };
                _db.Teams.Add(team);
                _logger.LogInformation("  + NEW team: {TeamNaam} (code: {Code}, categorie: {Cat}, spel: {Spel})",
                    incoming.TeamNaam, incoming.TeamCode, incoming.LeeftijdsCategorie, incoming.SpelSoort);
                added++;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("✓ Teams sync complete: {Added} added, {Updated} updated", added, updated);
    }

    public async Task SyncMatchesAsync(Guid tenantId)
    {
        _logger.LogInformation("───── STEP: Sync Matches ─────");

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId)
            ?? throw new InvalidOperationException($"Tenant {tenantId} not found");

        _logger.LogInformation("Fetching THUIS programma...");
        var thuisMatches = await _sportlinkClient.GetProgrammaAsync(tenant.SportlinkClientId, thuis: true);
        _logger.LogInformation("Received {Count} thuis matches", thuisMatches.Count);

        _logger.LogInformation("Fetching UIT programma...");
        var uitMatches = await _sportlinkClient.GetProgrammaAsync(tenant.SportlinkClientId, thuis: false);
        _logger.LogInformation("Received {Count} uit matches", uitMatches.Count);

        _logger.LogInformation("Fetching afgelastingen...");
        var afgelastingen = await _sportlinkClient.GetAfgelastingenAsync(tenant.SportlinkClientId);
        _logger.LogInformation("Received {Count} afgelastingen", afgelastingen.Count);
        var afgelastSet = afgelastingen.ToHashSet();

        var allIncoming = thuisMatches.Concat(uitMatches).ToList();
        _logger.LogInformation("Total incoming matches: {Count}", allIncoming.Count);

        var existingMatches = await _db.Matches
            .IgnoreQueryFilters()
            .Where(m => m.TenantId == tenantId)
            .ToListAsync();
        _logger.LogInformation("Existing matches in database: {Count}", existingMatches.Count);

        var existingByCode = existingMatches.ToDictionary(m => m.WedstrijdCode);

        int added = 0, updated = 0, afgelast = 0;

        foreach (var incoming in allIncoming)
        {
            var status = afgelastSet.Contains(incoming.WedstrijdCode)
                ? MatchStatus.Afgelast
                : MatchStatus.Gepland;

            if (status == MatchStatus.Afgelast) afgelast++;

            if (existingByCode.TryGetValue(incoming.WedstrijdCode, out var existing))
            {
                existing.WedstrijdDatum = incoming.WedstrijdDatum;
                existing.AanvangsTijd = incoming.AanvangsTijd;
                existing.ThuisTeam = incoming.ThuisTeam;
                existing.UitTeam = incoming.UitTeam;
                existing.Klasse = incoming.Klasse;
                existing.Competitie = incoming.Competitie;
                existing.Accommodatie = incoming.Accommodatie;
                existing.Veld = incoming.Veld;
                existing.Plaats = incoming.Plaats;
                existing.Scheidsrechters = incoming.Scheidsrechters;
                existing.Status = status;
                existing.IsThuis = incoming.IsThuis;
                updated++;
            }
            else
            {
                // Insert new match
                var match = new Match
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    WedstrijdCode = incoming.WedstrijdCode,
                    WedstrijdDatum = incoming.WedstrijdDatum,
                    AanvangsTijd = incoming.AanvangsTijd,
                    ThuisTeam = incoming.ThuisTeam,
                    UitTeam = incoming.UitTeam,
                    Klasse = incoming.Klasse,
                    Competitie = incoming.Competitie,
                    Accommodatie = incoming.Accommodatie,
                    Veld = incoming.Veld,
                    Plaats = incoming.Plaats,
                    Scheidsrechters = incoming.Scheidsrechters,
                    Status = status,
                    IsThuis = incoming.IsThuis
                };
                _db.Matches.Add(match);
                _logger.LogInformation("  + NEW match: {Datum} {Tijd} | {Thuis} vs {Uit} | {Klasse} | scheids: {Scheids}",
                    incoming.WedstrijdDatum.ToString("dd-MM-yyyy"), incoming.AanvangsTijd,
                    incoming.ThuisTeam, incoming.UitTeam, incoming.Klasse,
                    string.IsNullOrEmpty(incoming.Scheidsrechters) ? "(geen)" : incoming.Scheidsrechters);
                added++;
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("\u2713 Matches sync complete: {Added} added, {Updated} updated, {Afgelast} afgelast", added, updated, afgelast);
    }

    public async Task SyncMembersAsync(Guid tenantId)
    {
        _logger.LogInformation("───── STEP: Sync Members ─────");

        var tenant = await _db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId)
            ?? throw new InvalidOperationException($"Tenant {tenantId} not found");

        var teams = await _db.Teams
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();

        _logger.LogInformation("Fetching members for {Count} teams...", teams.Count);

        var existingMembers = await _db.Members
            .IgnoreQueryFilters()
            .Where(m => m.TenantId == tenantId)
            .ToListAsync();

        var existingByKey = existingMembers
            .ToDictionary(m => (m.RelatieCode, m.TeamId));

        int added = 0, updated = 0, totalMembers = 0;

        foreach (var team in teams)
        {
            var members = await _sportlinkClient.GetTeamIndelingAsync(
                tenant.SportlinkClientId, team.TeamCode, team.LokaleTeamCode);

            totalMembers += members.Count;

            foreach (var incoming in members)
            {
                var key = (incoming.RelatieCode, team.Id);

                if (existingByKey.TryGetValue(key, out var existing))
                {
                    existing.Naam = incoming.Naam;
                    existing.Voornaam = incoming.Voornaam;
                    existing.Achternaam = incoming.Achternaam;
                    existing.Tussenvoegsel = incoming.Tussenvoegsel;
                    existing.Geslacht = incoming.Geslacht;
                    existing.Rol = incoming.Rol;
                    updated++;
                }
                else
                {
                    var member = new Member
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        TeamId = team.Id,
                        RelatieCode = incoming.RelatieCode,
                        Naam = incoming.Naam,
                        Voornaam = incoming.Voornaam,
                        Achternaam = incoming.Achternaam,
                        Tussenvoegsel = incoming.Tussenvoegsel,
                        Geslacht = incoming.Geslacht,
                        Rol = incoming.Rol
                    };
                    _db.Members.Add(member);
                    added++;
                }
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("✓ Members sync complete: {Added} added, {Updated} updated (total from API: {Total})", added, updated, totalMembers);
    }
}
