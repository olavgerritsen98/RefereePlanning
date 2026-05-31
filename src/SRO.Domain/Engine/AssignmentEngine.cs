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

        // Sort: wedstrijdsport reserve-matches eerst (harder te plannen, hoger niveau nodig),
        // dan chronologisch
        var sortedMatches = draftMatches
            .OrderByDescending(m => IsWedstrijdsportReserve(m))
            .ThenBy(m => m.WedstrijdDatum)
            .ThenBy(m => m.AanvangsTijd)
            .ToList();

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

                // Wedstrijdsport-wedstrijden van Wageningen 4/5 (niet 1-3, want die zijn IsKnkvMatch)
                // krijgen reserve-vinkje: KNKV kan later alsnog een scheids toewijzen.
                // Pas donderdag is duidelijk of het team echt moet fluiten.
                if (IsWedstrijdsportReserve(match))
                    match.IsReserveAssignment = true;
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

    /// <summary>
    /// Detecteert wedstrijdsport-wedstrijden die een reserve-toewijzing krijgen.
    /// Dit zijn thuiswedstrijden van Wageningen 4 en 5. 
    /// (Wageningen 1, 2, 3 + U19-1 zijn al uitgesloten via IsKnkvMatch.)
    /// 
    /// Reserve = team staat paraat als backup. Pas op donderdag is duidelijk of
    /// KNKV alsnog een scheidsrechter stuurt. Zo ja → reserve gedeactiveerd.
    /// </summary>
    private static bool IsWedstrijdsportReserve(Match match)
    {
        return IsWageningen4or5(match.ThuisTeam);
    }

    /// <summary>
    /// Checks if team name is Wageningen 4 or 5 (wedstrijdsport, geen gegarandeerde KNKV-scheids).
    /// </summary>
    private static bool IsWageningen4or5(string teamName)
    {
        if (string.IsNullOrEmpty(teamName)) return false;
        var lower = teamName.ToLowerInvariant();
        return lower.Contains("wageningen") &&
               (lower.EndsWith(" 4") || lower.EndsWith(" 5"));
    }
}
