using CloudShift.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudShift.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<AppProfile> AppProfiles { get; set; } = null!;
    public DbSet<ProjectMapping> ProjectMappings { get; set; } = null!;
    public DbSet<MigrationJob> MigrationJobs { get; set; } = null!;
    public DbSet<FileTransferLog> FileTransferLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProjectMapping relationships to prevent cascade delete cycles
        modelBuilder.Entity<ProjectMapping>()
            .HasOne(p => p.SourceProfile)
            .WithMany()
            .HasForeignKey(p => p.SourceProfileId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ProjectMapping>()
            .HasOne(p => p.DestProfile)
            .WithMany()
            .HasForeignKey(p => p.DestProfileId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<ProjectMapping>()
            .HasOne(p => p.User)
            .WithMany(u => u.ProjectMappings)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<AppProfile>()
            .HasOne(a => a.User)
            .WithMany(u => u.AppProfiles)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<MigrationJob>()
            .HasOne(m => m.ProjectMapping)
            .WithMany(p => p.MigrationJobs)
            .HasForeignKey(m => m.ProjectMappingId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<FileTransferLog>()
            .HasOne(f => f.MigrationJob)
            .WithMany(m => m.FileTransferLogs)
            .HasForeignKey(f => f.MigrationJobId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Performance Indexes ───────────────────────────────────────────────
        // FileTransferLog grows massively; index on (JobId, Status) is critical
        // for the worker to efficiently query "failed files for job X".
        modelBuilder.Entity<FileTransferLog>()
            .HasIndex(f => new { f.MigrationJobId, f.Status })
            .HasDatabaseName("IX_FileTransferLog_JobId_Status");

        // Allow the worker to poll for Queued/Processing jobs efficiently
        modelBuilder.Entity<MigrationJob>()
            .HasIndex(m => m.Status)
            .HasDatabaseName("IX_MigrationJob_Status");

        // Optimise per-user queries on mappings
        modelBuilder.Entity<ProjectMapping>()
            .HasIndex(m => m.UserId)
            .HasDatabaseName("IX_ProjectMapping_UserId");
    }
}

