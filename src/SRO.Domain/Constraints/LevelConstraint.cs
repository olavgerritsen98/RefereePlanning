using SRO.Domain.Entities;

namespace SRO.Domain.Constraints;

/// <summary>
/// Hard constraint: a team should only referee matches at or below its own level.
/// Senioren-supplier teams can referee senior matches, Jeugd only youth, etc.
/// Teams without SupplierCategorie cannot be assigned at all.
/// </summary>
public class LevelConstraint : IAssignmentConstraint
{
    public bool IsSatisfied(Team team, Match match, IReadOnlyList<Match> allMatches)
    {
        if (!team.IsSupplier)
            return false;

        // Map team's supplier category to allowed match categories
        return team.SupplierCategorie switch
        {
            SupplierCategorie.Senioren => IsSeniorenMatch(match),
            SupplierCategorie.Jeugd => IsJeugdMatch(match),
            SupplierCategorie.Wedstrijdsport => true, // Can referee any competitive match
            SupplierCategorie.Midweek => IsMidweekMatch(match),
            _ => false
        };
    }

    private static bool IsSeniorenMatch(Match match)
    {
        var klasse = match.Klasse.ToLowerInvariant();
        return klasse.Contains("senior") || klasse.Contains("hoofd") || klasse.Contains("over");
    }

    private static bool IsJeugdMatch(Match match)
    {
        var klasse = match.Klasse.ToLowerInvariant();
        return klasse.Contains("jeugd") || klasse.Contains("junior")
            || klasse.Contains("pupil") || klasse.Contains("aspi");
    }

    private static bool IsMidweekMatch(Match match)
    {
        return match.WedstrijdDatum.DayOfWeek != DayOfWeek.Saturday
            && match.WedstrijdDatum.DayOfWeek != DayOfWeek.Sunday;
    }
}
