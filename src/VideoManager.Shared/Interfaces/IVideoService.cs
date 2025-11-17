using VideoManager.Model;

namespace VideoManager.Shared.Interfaces
{
    /// <summary>
    /// Service interface for video operations
    /// </summary>
    public interface IVideoService
    {
        Task<Result<PagedResult<VideoDto>>> GetVideosAsync(QueryParameters parameters);
        Task<Result<VideoDto>> GetVideoByIdAsync(Guid id);
        Task<Result<VideoDto>> CreateVideoAsync(CreateVideoDto dto, string createdBy);
        Task<Result<VideoDto>> UpdateVideoAsync(Guid id, UpdateVideoDto dto, string modifiedBy);
        Task<Result> DeleteVideoAsync(Guid id);
        Task<Result<List<VideoVersionDto>>> GetVideoVersionsAsync(Guid videoId);
        Task<Result<VideoVersionDto>> CreateVideoVersionAsync(Guid videoId, string changeDescription, string createdBy);
        Task<Result<string>> GetVideoFilePathAsync(Guid videoId, int? versionNumber = null);
        Task<Result> IncrementViewCountAsync(Guid videoId);
    }
}
