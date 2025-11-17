using VideoManager.Model;

namespace VideoManager.Shared.Interfaces
{
    /// <summary>
    /// Repository interface for Video entities with specific operations
    /// </summary>
    public interface IVideoRepository : IRepository<Video>
    {
        Task<IEnumerable<Video>> GetVideosWithMetadataAsync();
        Task<Video?> GetVideoWithVersionsAsync(Guid id);
        Task<IEnumerable<Video>> SearchVideosAsync(string searchTerm);
        Task<IEnumerable<Video>> GetVideosByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Video>> GetRecentVideosAsync(int count);
    }
}
