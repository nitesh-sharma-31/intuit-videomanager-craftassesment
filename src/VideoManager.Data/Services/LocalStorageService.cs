using System.Security.Cryptography;
using VideoManager.Shared.Interfaces;

namespace VideoManager.Data.Services
{
    /// <summary>
    /// File storage service using local file system
    /// </summary>
    public class LocalStorageService : IStorageService
    {
        private readonly string _baseStoragePath;
        private readonly string _videosPath;
        private readonly string _thumbnailsPath;

        public LocalStorageService(string baseStoragePath)
        {
            _baseStoragePath = baseStoragePath;
            _videosPath = Path.Combine(_baseStoragePath, "Videos");
            _thumbnailsPath = Path.Combine(_baseStoragePath, "Thumbnails");

            // Ensure directories exist
            Directory.CreateDirectory(_videosPath);
            Directory.CreateDirectory(_thumbnailsPath);
        }

        public async Task<string> SaveVideoAsync(Stream fileStream, string fileName, Guid videoId)
        {
            var videoFolder = Path.Combine(_videosPath, videoId.ToString());
            Directory.CreateDirectory(videoFolder);

            var filePath = Path.Combine(videoFolder, fileName);
            
            using var fileStreamOut = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOut);

            return filePath;
        }

        public async Task<string> SaveVideoVersionAsync(Stream fileStream, string fileName, Guid videoId, int versionNumber)
        {
            var videoFolder = Path.Combine(_videosPath, videoId.ToString(), "Versions");
            Directory.CreateDirectory(videoFolder);

            var versionFileName = $"v{versionNumber}_{fileName}";
            var filePath = Path.Combine(videoFolder, versionFileName);
            
            using var fileStreamOut = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOut);

            return filePath;
        }

        public async Task<Stream> GetVideoStreamAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Video file not found", filePath);
            }

            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            return memoryStream;
        }

        public async Task<bool> DeleteVideoAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> SaveThumbnailAsync(Stream thumbnailStream, Guid videoId)
        {
            var thumbnailFileName = $"{videoId}.jpg";
            var filePath = Path.Combine(_thumbnailsPath, thumbnailFileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await thumbnailStream.CopyToAsync(fileStream);

            return filePath;
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            return await Task.Run(() => new FileInfo(filePath).Length);
        }

        public async Task<string> CalculateFileHashAsync(Stream fileStream)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            return await Task.Run(() => File.Exists(filePath));
        }
    }
}
