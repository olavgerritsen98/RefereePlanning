using SRO.Domain.Constraints;
using SRO.Domain.Entities;

namespace SRO.Domain.Engine;

public class AssignmentResult
{
    public int Assigned { get; set; }
    public int Unassigned { get; set; }
}

/// <summary>
/// Core engine that assigns supplier teams to matches based on hard constraints.
/// Uses a greedy round-robin approach, distributing matches fairly across eligible teams.
/// </summary>
public class AssignmentEngine
{
    private readonly IEnumerable<IAssignmentConstraint> _constraints;

    public AssignmentEngine(IEnumerable<IAssignmentConstraint> constraints)
    {
        _constraints = constraints;
    }

    /// <summary>
    /// Assigns teams to draft matches. Mutates the Match entities in-place.
    /// </summary>
    /// <param name="draftMatches">Matches that need a team assigned (status = Draft).</param>
    /// <param name="supplierTeams">Teams eligible to referee.</param>
    /// <param name="allMatches">Full schedule for the period (used for conflict detection).</param>
    public AssignmentResult AssignMatches(
        IList<Match> draftMatches,
        IList<Team> supplierTeams,
        IReadOnlyList<Match> allMatches)
    {
        var result = new AssignmentResult();

        // Track assignment counts for fair distribution
        var assignmentCounts = new Dictionary<Guid, int>();
        foreach (var team in supplierTeams)
            assignmentCounts[team.Id] = allMatches.Count(m => m.AssignedTeamId == team.Id);

        // Sort draft matches by date to assign chronologically
        var sortedMatches = draftMatches.OrderBy(m => m.WedstrijdDatum).ThenBy(m => m.AanvangsTijd).ToList();

        foreach (var match in sortedMatches)
        {
            // Find eligible teams ordered by fewest assignments (round-robin fairness)
            var eligibleTeam = supplierTeams
                .Where(t => AllConstraintsSatisfied(t, match, allMatches))
                .OrderBy(t => assignmentCounts.GetValueOrDefault(t.Id, 0))
                .FirstOrDefault();

            if (eligibleTeam != null)
            {
                match.AssignedTeamId = eligibleTeam.Id;
                match.AssignedTeam = eligibleTeam;
                match.AssignmentStatus = AssignmentStatus.TeamAssigned;
                assignmentCounts[eligibleTeam.Id] = assignmentCounts.GetValueOrDefault(eligibleTeam.Id, 0) + 1;
                result.Assigned++;
            }
            else
            {
                result.Unassigned++;
            }
        }

        return result;
    }

    private bool AllConstraintsSatisfied(Team team, Match match, IReadOnlyList<Match> allMatches)
    {
        foreach (var constraint in _constraints)
        {
            if (!constraint.IsSatisfied(team, match, allMatches))
                return false;
        }
        return true;
    }
}
