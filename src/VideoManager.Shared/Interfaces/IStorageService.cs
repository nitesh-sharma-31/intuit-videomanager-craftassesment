namespace VideoManager.Shared.Interfaces
{
    /// <summary>
    /// Service interface for file storage operations
    /// </summary>
    public interface IStorageService
    {
        Task<string> SaveVideoAsync(Stream fileStream, string fileName, Guid videoId);
        Task<string> SaveVideoVersionAsync(Stream fileStream, string fileName, Guid videoId, int versionNumber);
        Task<Stream> GetVideoStreamAsync(string filePath);
        Task<bool> DeleteVideoAsync(string filePath);
        Task<string> SaveThumbnailAsync(Stream thumbnailStream, Guid videoId);
        Task<long> GetFileSizeAsync(string filePath);
        Task<string> CalculateFileHashAsync(Stream fileStream);
        Task<bool> FileExistsAsync(string filePath);
    }
}
