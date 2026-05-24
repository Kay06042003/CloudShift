using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudShift.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IMigrationJobRepository"/>.
/// </summary>
public sealed class MigrationJobRepository : IMigrationJobRepository
{
    private readonly ApplicationDbContext _context;

    public MigrationJobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MigrationJob> AddAsync(
        MigrationJob job,
        CancellationToken cancellationToken = default)
    {
        await _context.MigrationJobs.AddAsync(job, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<MigrationJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.MigrationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MigrationJob>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MigrationJobs
            .AsNoTracking()
            .Include(j => j.ProjectMapping)
            .Where(j => j.ProjectMapping != null && j.ProjectMapping.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
