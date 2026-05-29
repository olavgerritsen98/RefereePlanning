using Microsoft.AspNetCore.Mvc;
using SRO.Domain.Entities;
using SRO.Domain.Interfaces;

namespace SRO.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningController : ControllerBase
{
    private readonly IPlanningService _planningService;
    private readonly ILogger<PlanningController> _logger;

    public PlanningController(IPlanningService planningService, ILogger<PlanningController> logger)
    {
        _planningService = planningService;
        _logger = logger;
    }

    public record GenerateRequest(SeasonPeriod Period);

    /// <summary>
    /// Generate a planning for a given season period.
    /// Assigns supplier teams to all draft matches using the AssignmentEngine.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
    {
        _logger.LogInformation("Generating planning for period {Period}", request.Period);

        var result = await _planningService.GeneratePlanningAsync(request.Period);

        _logger.LogInformation("Planning complete: {Assigned} assigned, {Unassigned} unassigned",
            result.Assigned, result.Unassigned);

        return Ok(new
        {
            message = $"Er zijn {result.Assigned} wedstrijden ingepland en {result.Unassigned} drafts overgebleven.",
            assigned = result.Assigned,
            unassigned = result.Unassigned,
            period = request.Period.ToString()
        });
    }
}
