using SRO.Domain.Entities;

namespace SRO.Domain.Constraints;

/// <summary>
/// Hard constraint: a team cannot referee a match if it plays (or already referees)
/// another match at the same time on the same day.
/// </summary>
public class TimeConflictConstraint : IAssignmentConstraint
{
    public bool IsSatisfied(Team team, Match match, IReadOnlyList<Match> allMatches)
    {
        // Find all matches where this team is already assigned, or is playing
        var teamName = team.TeamNaam;

        foreach (var other in allMatches)
        {
            if (other.Id == match.Id) continue;
            if (other.WedstrijdDatum.Date != match.WedstrijdDatum.Date) continue;

            // Check if team plays in this other match
            bool teamPlays = other.ThuisTeam == teamName || other.UitTeam == teamName;
            // Check if team is already assigned to referee this other match
            bool teamReferees = other.AssignedTeamId == team.Id;

            if (!teamPlays && !teamReferees) continue;

            // Check time overlap (same aanvangstijd = conflict)
            if (other.AanvangsTijd == match.AanvangsTijd)
                return false;
        }

        return true;
    }
}
