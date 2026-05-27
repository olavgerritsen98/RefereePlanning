using System.Text.Json.Serialization;

namespace SRO.Infrastructure.Sportlink;

public record SportlinkMatchDto
{
    [JsonPropertyName("wedstrijddatum")]
    public string WedstrijdDatum { get; init; } = string.Empty;

    [JsonPropertyName("wedstrijdcode")]
    public int WedstrijdCode { get; init; }

    [JsonPropertyName("thuisteam")]
    public string ThuisTeam { get; init; } = string.Empty;

    [JsonPropertyName("uitteam")]
    public string UitTeam { get; init; } = string.Empty;

    [JsonPropertyName("aanvangstijd")]
    public string AanvangsTijd { get; init; } = string.Empty;

    [JsonPropertyName("klasse")]
    public string Klasse { get; init; } = string.Empty;

    [JsonPropertyName("competitie")]
    public string Competitie { get; init; } = string.Empty;

    [JsonPropertyName("accommodatie")]
    public string Accommodatie { get; init; } = string.Empty;

    [JsonPropertyName("veld")]
    public string Veld { get; init; } = string.Empty;

    [JsonPropertyName("plaats")]
    public string Plaats { get; init; } = string.Empty;

    [JsonPropertyName("scheidsrechters")]
    public string Scheidsrechters { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}

public record SportlinkTeamDto
{
    [JsonPropertyName("teamcode")]
    public int TeamCode { get; init; }

    [JsonPropertyName("lokaleteamcode")]
    public int LokaleTeamCode { get; init; }

    [JsonPropertyName("teamnaam")]
    public string TeamNaam { get; init; } = string.Empty;

    [JsonPropertyName("klasse")]
    public string Klasse { get; init; } = string.Empty;

    [JsonPropertyName("spelsoort")]
    public string SpelSoort { get; init; } = string.Empty;

    [JsonPropertyName("leeftijdscategorie")]
    public string LeeftijdsCategorie { get; init; } = string.Empty;

    [JsonPropertyName("competitienaam")]
    public string CompetitieNaam { get; init; } = string.Empty;
}

public record SportlinkAfgelastingDto
{
    [JsonPropertyName("wedstrijdcode")]
    public int WedstrijdCode { get; init; }
}

public record SportlinkMemberDto
{
    [JsonPropertyName("relatiecode")]
    public string RelatieCode { get; init; } = string.Empty;

    [JsonPropertyName("naam")]
    public string Naam { get; init; } = string.Empty;

    [JsonPropertyName("voornaam")]
    public string Voornaam { get; init; } = string.Empty;

    [JsonPropertyName("achternaam")]
    public string Achternaam { get; init; } = string.Empty;

    [JsonPropertyName("tussenvoegsel")]
    public string? Tussenvoegsel { get; init; }

    [JsonPropertyName("geslacht")]
    public string? Geslacht { get; init; }

    [JsonPropertyName("rol")]
    public string Rol { get; init; } = string.Empty;
}
