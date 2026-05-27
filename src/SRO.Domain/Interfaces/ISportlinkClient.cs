using SRO.Domain.Entities;

namespace SRO.Domain.Interfaces;

public interface ISportlinkClient
{
    Task<IReadOnlyList<Match>> GetProgrammaAsync(string clientId, bool thuis);
    Task<IReadOnlyList<Team>> GetTeamsAsync(string clientId);
    Task<IReadOnlyList<string>> GetAfgelastingenAsync(string clientId);
    Task<IReadOnlyList<Member>> GetTeamIndelingAsync(string clientId, int teamCode, int lokaleTeamCode);
}
