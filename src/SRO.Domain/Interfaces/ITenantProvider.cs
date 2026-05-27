namespace SRO.Domain.Interfaces;

public interface ITenantProvider
{
    Guid TenantId { get; }
}
