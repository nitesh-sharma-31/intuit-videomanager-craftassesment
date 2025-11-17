using Microsoft.EntityFrameworkCore;
using VideoManager.Shared.Interfaces;
using VideoManager.Model;

namespace VideoManager.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Video entities
    /// </summary>
    public class VideoRepository : Repository<Video>, IVideoRepository
    {
        public VideoRepository(VideoManagerDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Video>> GetVideosWithMetadataAsync()
        {
            return await _dbSet
                .Include(v => v.Metadata)
                .Include(v => v.Versions)
                .Where(v => !v.IsDeleted)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();
        }

        public async Task<Video?> GetVideoWithVersionsAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.Versions.OrderByDescending(ver => ver.VersionNumber))
                .Include(v => v.Metadata)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<IEnumerable<Video>> SearchVideosAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetVideosWithMetadataAsync();

            var lowerSearchTerm = searchTerm.ToLower();
            return await _dbSet
                .Include(v => v.Metadata)
                .Include(v => v.Versions)
                .Where(v => !v.IsDeleted &&
                           (v.Title.ToLower().Contains(lowerSearchTerm) ||
                            v.Description.ToLower().Contains(lowerSearchTerm) ||
                            v.OriginalFileName.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> GetVideosByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(v => v.Metadata)
                .Where(v => !v.IsDeleted &&
                           v.CreatedDate >= startDate &&
                           v.CreatedDate <= endDate)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Video>> GetRecentVideosAsync(int count)
        {
            return await _dbSet
                .Include(v => v.Metadata)
                .Include(v => v.Versions)
                .Where(v => !v.IsDeleted)
                .OrderByDescending(v => v.CreatedDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
