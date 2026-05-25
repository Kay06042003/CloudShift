using CloudShift.Application.OAuthProviderApps.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudShift.Infrastructure.Repositories;

public sealed class OAuthProviderAppRepository : IOAuthProviderAppRepository
{
    private readonly ApplicationDbContext _context;

    public OAuthProviderAppRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OAuthProviderApp> AddAsync(OAuthProviderApp app, CancellationToken cancellationToken = default)
    {
        await _context.OAuthProviderApps.AddAsync(app, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return app;
    }

    public async Task<IReadOnlyList<OAuthProviderApp>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.OAuthProviderApps
            .AsNoTracking()
            .Include(app => app.AppProfiles)
            .Where(app => app.UserId == userId)
            .OrderByDescending(app => app.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<OAuthProviderApp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OAuthProviderApps
            .AsNoTracking()
            .FirstOrDefaultAsync(app => app.Id == id, cancellationToken);
    }

    public async Task<OAuthProviderApp?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OAuthProviderApps
            .Include(app => app.AppProfiles)
            .FirstOrDefaultAsync(app => app.Id == id, cancellationToken);
    }

    public async Task<int> CountLinkedProfilesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AppProfiles
            .CountAsync(profile => profile.ProviderAppId == id, cancellationToken);
    }

    public async Task<OAuthProviderApp> UpdateAsync(OAuthProviderApp app, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return app;
    }

    public async Task DeleteAsync(OAuthProviderApp app, CancellationToken cancellationToken = default)
    {
        _context.OAuthProviderApps.Remove(app);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
