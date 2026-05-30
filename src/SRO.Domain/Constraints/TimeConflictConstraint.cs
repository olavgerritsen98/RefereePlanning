using SRO.Domain.Entities;

namespace SRO.Domain.Constraints;

/// <summary>
/// Hard constraint: a team cannot referee a match if it plays (or already referees)
/// another match at overlapping times on the same day.
/// Uses TimeSpan calculations with travel/buffer time.
/// </summary>
public class TimeConflictConstraint : IAssignmentConstraint
{
    private static readonly TimeSpan MatchDuration = TimeSpan.FromMinutes(90);
    private static readonly TimeSpan AwayPreBuffer = TimeSpan.FromMinutes(90);  // 45 reistijd + 45 aanwezigheid
    private static readonly TimeSpan AwayPostBuffer = TimeSpan.FromMinutes(45); // 45 reistijd na einde
    private static readonly TimeSpan HomePreBuffer = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan HomePostBuffer = TimeSpan.FromMinutes(30);

    public bool IsSatisfied(Team team, Match match, IReadOnlyList<Match> allMatches)
    {
        if (!TryParseTime(match.AanvangsTijd, out var matchStart))
            return true;

        var matchDate = match.WedstrijdDatum.Date;
        var teamName = team.TeamNaam;

        foreach (var other in allMatches)
        {
            if (other.Id == match.Id) continue;
            if (other.WedstrijdDatum.Date != matchDate) continue;

            // Check if team plays in this other match
            bool teamPlays = other.ThuisTeam == teamName || other.UitTeam == teamName;
            // Check if team is already assigned to referee this other match
            bool teamReferees = other.AssignedTeamId == team.Id;

            if (!teamPlays && !teamReferees) continue;

            if (!TryParseTime(other.AanvangsTijd, out var otherStart))
                continue;

            // Determine blocked window for the 'other' match
            TimeSpan blockedStart;
            TimeSpan blockedEnd;

            if (teamPlays)
            {
                // Team speelt deze wedstrijd
                bool isHome = other.ThuisTeam == teamName;
                var preBuffer = isHome ? HomePreBuffer : AwayPreBuffer;
                var postBuffer = isHome ? HomePostBuffer : AwayPostBuffer;
                blockedStart = otherStart - preBuffer;
                blockedEnd = otherStart + MatchDuration + postBuffer;
            }
            else
            {
                // Team fluit deze wedstrijd al (al toegewezen)
                blockedStart = otherStart - HomePreBuffer;
                blockedEnd = otherStart + MatchDuration + HomePreBuffer;
            }

            // De te fluiten wedstrijd loopt van matchStart tot matchStart + 90 min
            var refStart = matchStart;
            var refEnd = matchStart + MatchDuration;

            // Overlap check: twee intervallen overlappen als start1 < end2 en start2 < end1
            if (refStart < blockedEnd && blockedStart < refEnd)
                return false;
        }

        return true;
    }

    private static bool TryParseTime(string time, out TimeSpan result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(time)) return false;

        var parts = time.Split(':');
        if (parts.Length >= 2
            && int.TryParse(parts[0], out var hours)
            && int.TryParse(parts[1], out var minutes))
        {
            result = new TimeSpan(hours, minutes, 0);
            return true;
        }
        return false;
    }
}
