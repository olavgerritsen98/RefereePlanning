namespace SRO.Domain.Interfaces;

public interface ISyncService
{
    Task SyncTeamsAsync(Guid tenantId);
    Task SyncMatchesAsync(Guid tenantId);
    Task SyncAllAsync(Guid tenantId);
}
