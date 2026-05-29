using SRO.Domain.Entities;
using SRO.Domain.Engine;

namespace SRO.Domain.Interfaces;

public interface IPlanningService
{
    Task<AssignmentResult> GeneratePlanningAsync(SeasonPeriod period);
}
