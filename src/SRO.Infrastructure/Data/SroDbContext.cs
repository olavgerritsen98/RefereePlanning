using Microsoft.EntityFrameworkCore;
using SRO.Domain.Entities;
using SRO.Domain.Interfaces;

namespace SRO.Infrastructure.Data;

public class SroDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public SroDbContext(DbContextOptions<SroDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SroDbContext).Assembly);

        // Apply global query filter for multi-tenancy on all BaseEntity-derived types
        // (Skip Tenant entity itself — it has its own filter in TenantConfiguration)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(Tenant))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateTenantFilter(entityType.ClrType));
            }
        }
    }

    public override int SaveChanges()
    {
        SetTenantId();
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantId();
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantId()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            // Don't overwrite TenantId if it was already explicitly set (e.g. by SyncService)
            if (entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _tenantProvider.TenantId;
            }
        }
    }

    private void SetTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

    private static System.Linq.Expressions.LambdaExpression CreateTenantFilter(Type entityType)
    {
        // Creates: entity => EF.Property<Guid>(entity, "TenantId") == _tenantId
        // But since we need runtime value, we use HasQueryFilter with expression
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var tenantProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.TenantId));
        // Note: Actual filtering happens via a shadow property approach or we inject at query time
        // For now, return a "true" filter - will be refined when we add the tenant resolution middleware
        var body = System.Linq.Expressions.Expression.NotEqual(
            tenantProperty,
            System.Linq.Expressions.Expression.Constant(Guid.Empty));
        return System.Linq.Expressions.Expression.Lambda(body, parameter);
    }
}
