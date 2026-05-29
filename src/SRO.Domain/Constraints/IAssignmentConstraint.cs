using SRO.Domain.Entities;

namespace SRO.Domain.Constraints;

public interface IAssignmentConstraint
{
    /// <summary>
    /// Returns true if the given team is allowed to referee the given match,
    /// given the full schedule context.
    /// </summary>
    bool IsSatisfied(Team team, Match match, IReadOnlyList<Match> allMatches);
}
