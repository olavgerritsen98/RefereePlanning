using System.Net.Http.Json;

namespace SRO.Web.Services;

public class MatchApiClient
{
    private readonly HttpClient _http;

    public MatchApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MatchDto>> GetMatchesAsync(SeasonPeriod? period = null)
    {
        var url = "api/matches";
        if (period.HasValue)
            url += $"?period={period.Value}";

        return await _http.GetFromJsonAsync<List<MatchDto>>(url) ?? [];
    }

    public async Task<PlanningResultDto> GeneratePlanningAsync(SeasonPeriod period)
    {
        var response = await _http.PostAsJsonAsync("api/planning/generate", new { period = period.ToString() });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlanningResultDto>() ?? new();
    }
}

public enum SeasonPeriod
{
    NajaarVeld,
    ZaalHelft1,
    ZaalHelft2,
    VoorjaarVeld
}

public class MatchDto
{
    public Guid Id { get; set; }
    public string WedstrijdCode { get; set; } = "";
    public string Datum { get; set; } = "";
    public string Tijd { get; set; } = "";
    public string ThuisTeam { get; set; } = "";
    public string UitTeam { get; set; } = "";
    public string Klasse { get; set; } = "";
    public string? AssignedTeam { get; set; }
    public string AssignmentStatus { get; set; } = "Draft";
    public bool IsKnkvMatch { get; set; }
    public bool IsReserveAssignment { get; set; }
}

public class PlanningResultDto
{
    public string Message { get; set; } = "";
    public int Assigned { get; set; }
    public int Unassigned { get; set; }
    public string Period { get; set; } = "";
}
