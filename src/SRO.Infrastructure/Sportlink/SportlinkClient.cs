using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SRO.Domain.Interfaces;
using SRO.Domain.Entities;

namespace SRO.Infrastructure.Sportlink;

public class SportlinkClient : ISportlinkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SportlinkClient> _logger;

    public SportlinkClient(HttpClient httpClient, ILogger<SportlinkClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Match>> GetProgrammaAsync(string clientId, bool thuis)
    {
        var thuisParam = thuis ? "ja" : "nee";
        var uitParam = thuis ? "nee" : "ja";
        var url = $"programma?clientId={clientId}&thuis={thuisParam}&uit={uitParam}&aantaldagen=250&aantalregels=500";

        _logger.LogInformation("  GET {BaseUrl}{Url}", _httpClient.BaseAddress, url);

        var dtos = await _httpClient.GetFromJsonAsync<List<SportlinkMatchDto>>(url) ?? [];

        return dtos.Select(dto => MapToMatch(dto, thuis)).ToList();
    }

    public async Task<IReadOnlyList<Team>> GetTeamsAsync(string clientId)
    {
        var url = $"teams?clientId={clientId}";

        _logger.LogInformation("  GET {BaseUrl}{Url}", _httpClient.BaseAddress, url);

        var dtos = await _httpClient.GetFromJsonAsync<List<SportlinkTeamDto>>(url) ?? [];

        // Deduplicate: a team appears multiple times (different competitions/seasons)
        var uniqueTeams = dtos
            .GroupBy(t => t.TeamCode)
            .Select(g => MapToTeam(g.First(), g.Select(t => t.Klasse).FirstOrDefault(k => !string.IsNullOrEmpty(k)) ?? string.Empty))
            .ToList();

        return uniqueTeams;
    }

    public async Task<IReadOnlyList<string>> GetAfgelastingenAsync(string clientId)
    {
        var url = $"afgelastingen?clientId={clientId}&aantaldagen=250";

        _logger.LogInformation("  GET {BaseUrl}{Url}", _httpClient.BaseAddress, url);

        var dtos = await _httpClient.GetFromJsonAsync<List<SportlinkAfgelastingDto>>(url) ?? [];

        return dtos.Select(d => d.WedstrijdCode.ToString()).ToList();
    }

    public async Task<IReadOnlyList<Member>> GetTeamIndelingAsync(string clientId, int teamCode, int lokaleTeamCode)
    {
        var url = $"team-indeling?clientId={clientId}&teamcode={teamCode}&lokaleteamcode={lokaleTeamCode}";

        var dtos = await _httpClient.GetFromJsonAsync<List<SportlinkMemberDto>>(url) ?? [];

        return dtos.Select(dto => new Member
        {
            RelatieCode = dto.RelatieCode,
            Naam = dto.Naam,
            Voornaam = dto.Voornaam,
            Achternaam = dto.Achternaam,
            Tussenvoegsel = dto.Tussenvoegsel,
            Geslacht = dto.Geslacht,
            Rol = dto.Rol
        }).ToList();
    }

    private static Match MapToMatch(SportlinkMatchDto dto, bool isThuis)
    {
        DateTime.TryParse(dto.WedstrijdDatum, out var wedstrijdDatum);

        return new Match
        {
            WedstrijdCode = dto.WedstrijdCode.ToString(),
            WedstrijdDatum = wedstrijdDatum,
            AanvangsTijd = dto.AanvangsTijd,
            ThuisTeam = dto.ThuisTeam,
            UitTeam = dto.UitTeam,
            Klasse = dto.Klasse,
            Competitie = dto.Competitie,
            Accommodatie = dto.Accommodatie,
            Veld = dto.Veld,
            Plaats = dto.Plaats,
            Scheidsrechters = dto.Scheidsrechters,
            IsThuis = isThuis,
            Status = MatchStatus.Gepland
        };
    }

    private static Team MapToTeam(SportlinkTeamDto dto, string bestKlasse)
    {
        return new Team
        {
            TeamCode = dto.TeamCode,
            LokaleTeamCode = dto.LokaleTeamCode,
            TeamNaam = dto.TeamNaam,
            LeeftijdsCategorie = dto.LeeftijdsCategorie,
            Klasse = bestKlasse,
            SpelSoort = dto.SpelSoort
        };
    }
}
