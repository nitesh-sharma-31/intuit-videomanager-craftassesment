using Microsoft.EntityFrameworkCore;
using VideoManager.Model;

namespace VideoManager.Data
{
    /// <summary>
    /// Entity Framework DbContext for Video Manager
    /// </summary>
    public class VideoManagerDbContext : DbContext
    {
        public VideoManagerDbContext(DbContextOptions<VideoManagerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Video> Videos { get; set; } = null!;
        public DbSet<VideoVersion> VideoVersions { get; set; } = null!;
        public DbSet<VideoMetadata> VideoMetadata { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Video entity configuration
            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.CreatedDate);
                entity.HasIndex(e => e.IsDeleted);

                entity.HasMany(e => e.Versions)
                      .WithOne(v => v.Video)
                      .HasForeignKey(v => v.VideoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Metadata)
                      .WithOne(m => m.Video)
                      .HasForeignKey<VideoMetadata>(m => m.VideoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // VideoVersion entity configuration
            modelBuilder.Entity<VideoVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.VideoId, e.VersionNumber }).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // VideoMetadata entity configuration
            modelBuilder.Entity<VideoMetadata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VideoId).IsUnique();
            });
        }
    }
}
