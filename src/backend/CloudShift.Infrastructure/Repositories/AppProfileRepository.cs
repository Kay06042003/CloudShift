using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudShift.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAppProfileRepository"/>.
/// Operates within the scoped <see cref="ApplicationDbContext"/>.
/// </summary>
public sealed class AppProfileRepository : IAppProfileRepository
{
    private readonly ApplicationDbContext _context;

    public AppProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppProfile> AddAsync(AppProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.AppProfiles.AddAsync(profile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<IReadOnlyList<AppProfile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AppProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AppProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AppProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
