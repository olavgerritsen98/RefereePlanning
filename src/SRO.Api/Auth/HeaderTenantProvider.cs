using SRO.Domain.Interfaces;

namespace SRO.Api.Auth;

/// <summary>
/// Resolves tenant from the X-Tenant-Id header (MVP: simple API key auth).
/// </summary>
public class HeaderTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            return Guid.TryParse(header, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}
