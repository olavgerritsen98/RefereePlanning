using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;

namespace SRO.Functions;

public class NightlySyncFunction
{
    private readonly ISyncService _syncService;
    private readonly SroDbContext _db;
    private readonly ILogger<NightlySyncFunction> _logger;

    public NightlySyncFunction(ISyncService syncService, SroDbContext db, ILogger<NightlySyncFunction> logger)
    {
        _syncService = syncService;
        _db = db;
        _logger = logger;
    }

    [Function("NightlySync")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("NightlySync triggered at {Time}", DateTime.UtcNow);

        var tenants = await _db.Tenants.IgnoreQueryFilters().ToListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                _logger.LogInformation("Syncing tenant {TenantName} ({TenantId})", tenant.Name, tenant.Id);
                await _syncService.SyncAllAsync(tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync tenant {TenantName} ({TenantId})", tenant.Name, tenant.Id);
            }
        }

        _logger.LogInformation("NightlySync completed. Next: {Next}", timerInfo.ScheduleStatus?.Next);
    }
}
