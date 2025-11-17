using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using VideoManager.Model;

namespace VideoManager.ViewModel.Services
{
    /// <summary>
    /// HTTP client service for communicating with the API
    /// </summary>
    public class VideoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public VideoApiClient(string baseUrl = "https://localhost:5052")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<Result<PagedResult<VideoDto>>> GetVideosAsync(QueryParameters parameters)
        {
            try
            {
                var query = $"?PageNumber={parameters.PageNumber}&PageSize={parameters.PageSize}" +
                           $"&SearchTerm={Uri.EscapeDataString(parameters.SearchTerm)}" +
                           $"&SortBy={parameters.SortBy}&SortDescending={parameters.SortDescending}";

                var response = await _httpClient.GetAsync($"/api/videos{query}");
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<PagedResult<VideoDto>>();
                    return Result<PagedResult<VideoDto>>.Success(data!);
                }

                var error = await response.Content.ReadAsStringAsync();
                return Result<PagedResult<VideoDto>>.Failure($"API Error: {error}");
            }
            catch (Exception ex)
            {
                return Result<PagedResult<VideoDto>>.Failure($"Connection error: {ex.Message}");
            }
        }

        public async Task<Result<VideoDto>> GetVideoByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/videos/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<VideoDto>();
                    return Result<VideoDto>.Success(data!);
                }

                return Result<VideoDto>.Failure("Video not found");
            }
            catch (Exception ex)
            {
                return Result<VideoDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<VideoDto>> UploadVideoAsync(string filePath, string title, string description, List<string> tags)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(filePath);
                using var streamContent = new StreamContent(fileStream);
                
                content.Add(streamContent, "file", Path.GetFileName(filePath));
                content.Add(new StringContent(title), "title");
                content.Add(new StringContent(description), "description");
                content.Add(new StringContent(string.Join(",", tags)), "tags");

                var response = await _httpClient.PostAsync("/api/videos/upload", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<VideoDto>();
                    return Result<VideoDto>.Success(data!, "Video uploaded successfully");
                }

                var error = await response.Content.ReadAsStringAsync();
                return Result<VideoDto>.Failure($"Upload failed: {error}");
            }
            catch (Exception ex)
            {
                return Result<VideoDto>.Failure($"Upload error: {ex.Message}");
            }
        }

        public async Task<Result<VideoDto>> UpdateVideoAsync(Guid id, UpdateVideoDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/videos/{id}", dto);
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<VideoDto>();
                    return Result<VideoDto>.Success(data!, "Video updated successfully");
                }

                return Result<VideoDto>.Failure("Update failed");
            }
            catch (Exception ex)
            {
                return Result<VideoDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result> DeleteVideoAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/videos/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return Result.Success("Video deleted successfully");
                }

                return Result.Failure("Delete failed");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<string>> DownloadVideoAsync(Guid id, string savePath, int? version = null)
        {
            try
            {
                var url = version.HasValue 
                    ? $"/api/videos/{id}/download?version={version}" 
                    : $"/api/videos/{id}/download";

                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    using var fileStream = new FileStream(savePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                    return Result<string>.Success(savePath, "Video downloaded successfully");
                }

                return Result<string>.Failure("Download failed");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Download error: {ex.Message}");
            }
        }

        public async Task<Result<List<VideoVersionDto>>> GetVideoVersionsAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/videos/{id}/versions");
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<VideoVersionDto>>();
                    return Result<List<VideoVersionDto>>.Success(data!);
                }

                return Result<List<VideoVersionDto>>.Failure("Failed to retrieve versions");
            }
            catch (Exception ex)
            {
                return Result<List<VideoVersionDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<VideoVersionDto>> UploadVideoVersionAsync(Guid videoId, string filePath, string changeDescription)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(filePath);
                using var streamContent = new StreamContent(fileStream);
                
                content.Add(streamContent, "file", Path.GetFileName(filePath));
                content.Add(new StringContent(changeDescription), "changeDescription");

                var response = await _httpClient.PostAsync($"/api/videos/{videoId}/versions", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<VideoVersionDto>();
                    return Result<VideoVersionDto>.Success(data!, "Version uploaded successfully");
                }

                return Result<VideoVersionDto>.Failure("Version upload failed");
            }
            catch (Exception ex)
            {
                return Result<VideoVersionDto>.Failure($"Upload error: {ex.Message}");
            }
        }
    }
}
