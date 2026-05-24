using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Domain.Entities;
using CloudShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudShift.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IProjectMappingRepository"/>.
/// </summary>
public sealed class ProjectMappingRepository : IProjectMappingRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectMappingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMapping> AddAsync(
        ProjectMapping mapping,
        CancellationToken cancellationToken = default)
    {
        await _context.ProjectMappings.AddAsync(mapping, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return mapping;
    }

    public async Task<IReadOnlyList<ProjectMapping>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProjectMappings
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Include(m => m.SourceProfile)
            .Include(m => m.DestProfile)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectMapping?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProjectMappings
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Include(m => m.SourceProfile)
            .Include(m => m.DestProfile)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
