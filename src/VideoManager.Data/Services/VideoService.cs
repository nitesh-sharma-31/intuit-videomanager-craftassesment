using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VideoManager.Model;
using VideoManager.Shared.Interfaces;

namespace VideoManager.Data.Services
{
    /// <summary>
    /// Business logic service for video operations
    /// </summary>
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly VideoManagerDbContext _context;

        public VideoService(IVideoRepository videoRepository, VideoManagerDbContext context)
        {
            _videoRepository = videoRepository;
            _context = context;
        }

        public async Task<Result<PagedResult<VideoDto>>> GetVideosAsync(QueryParameters parameters)
        {
            try
            {
                var query = _context.Videos
                    .Include(v => v.Metadata)
                    .Include(v => v.Versions)
                    .Where(v => !v.IsDeleted)
                    .AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                {
                    var searchLower = parameters.SearchTerm.ToLower();
                    query = query.Where(v => 
                        v.Title.ToLower().Contains(searchLower) ||
                        v.Description.ToLower().Contains(searchLower) ||
                        v.OriginalFileName.ToLower().Contains(searchLower));
                }

                // Sorting
                query = parameters.SortBy.ToLower() switch
                {
                    "title" => parameters.SortDescending 
                        ? query.OrderByDescending(v => v.Title) 
                        : query.OrderBy(v => v.Title),
                    "size" => parameters.SortDescending 
                        ? query.OrderByDescending(v => v.FileSizeBytes) 
                        : query.OrderBy(v => v.FileSizeBytes),
                    "duration" => parameters.SortDescending 
                        ? query.OrderByDescending(v => v.DurationSeconds) 
                        : query.OrderBy(v => v.DurationSeconds),
                    _ => parameters.SortDescending 
                        ? query.OrderByDescending(v => v.CreatedDate) 
                        : query.OrderBy(v => v.CreatedDate)
                };

                // Count
                var totalCount = await query.CountAsync();

                // Pagination
                var videos = await query
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToListAsync();

                var videoDtos = videos.Select(v => MapToDto(v)).ToList();

                var pagedResult = new PagedResult<VideoDto>
                {
                    Items = videoDtos,
                    TotalCount = totalCount,
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };

                return Result<PagedResult<VideoDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return Result<PagedResult<VideoDto>>.Failure($"Error retrieving videos: {ex.Message}");
            }
        }

        public async Task<Result<VideoDto>> GetVideoByIdAsync(Guid id)
        {
            try
            {
                var video = await _videoRepository.GetVideoWithVersionsAsync(id);
                if (video == null)
                {
                    return Result<VideoDto>.Failure("Video not found");
                }

                return Result<VideoDto>.Success(MapToDto(video));
            }
            catch (Exception ex)
            {
                return Result<VideoDto>.Failure($"Error retrieving video: {ex.Message}");
            }
        }

        public async Task<Result<VideoDto>> CreateVideoAsync(CreateVideoDto dto, string createdBy)
        {
            try
            {
                var video = new Video
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    OriginalFileName = dto.OriginalFileName,
                    FileSizeBytes = dto.FileSizeBytes,
                    FileFormat = dto.FileFormat,
                    DurationSeconds = dto.DurationSeconds,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsDeleted = false
                };

                var metadata = new VideoMetadata
                {
                    Id = Guid.NewGuid(),
                    VideoId = video.Id,
                    Tags = JsonSerializer.Serialize(dto.Tags),
                    ViewCount = 0
                };

                video.Metadata = metadata;

                await _videoRepository.AddAsync(video);

                return Result<VideoDto>.Success(MapToDto(video), "Video created successfully");
            }
            catch (Exception ex)
            {
                return Result<VideoDto>.Failure($"Error creating video: {ex.Message}");
            }
        }

        public async Task<Result<VideoDto>> UpdateVideoAsync(Guid id, UpdateVideoDto dto, string modifiedBy)
        {
            try
            {
                var video = await _videoRepository.GetVideoWithVersionsAsync(id);
                if (video == null)
                {
                    return Result<VideoDto>.Failure("Video not found");
                }

                video.Title = dto.Title;
                video.Description = dto.Description;
                video.ModifiedDate = DateTime.UtcNow;
                video.ModifiedBy = modifiedBy;

                if (video.Metadata != null)
                {
                    video.Metadata.Tags = JsonSerializer.Serialize(dto.Tags);
                }

                await _videoRepository.UpdateAsync(video);

                return Result<VideoDto>.Success(MapToDto(video), "Video updated successfully");
            }
            catch (Exception ex)
            {
                return Result<VideoDto>.Failure($"Error updating video: {ex.Message}");
            }
        }

        public async Task<Result> DeleteVideoAsync(Guid id)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(id);
                if (video == null)
                {
                    return Result.Failure("Video not found");
                }

                video.IsDeleted = true;
                video.DeletedDate = DateTime.UtcNow;

                await _videoRepository.UpdateAsync(video);

                return Result.Success("Video deleted successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error deleting video: {ex.Message}");
            }
        }

        public async Task<Result<List<VideoVersionDto>>> GetVideoVersionsAsync(Guid videoId)
        {
            try
            {
                var video = await _videoRepository.GetVideoWithVersionsAsync(videoId);
                if (video == null)
                {
                    return Result<List<VideoVersionDto>>.Failure("Video not found");
                }

                var versionDtos = video.Versions
                    .OrderByDescending(v => v.VersionNumber)
                    .Select(v => new VideoVersionDto
                    {
                        Id = v.Id,
                        VersionNumber = v.VersionNumber,
                        FileSizeBytes = v.FileSizeBytes,
                        ChangeDescription = v.ChangeDescription,
                        CreatedDate = v.CreatedDate,
                        CreatedBy = v.CreatedBy,
                        IsActive = v.IsActive
                    })
                    .ToList();

                return Result<List<VideoVersionDto>>.Success(versionDtos);
            }
            catch (Exception ex)
            {
                return Result<List<VideoVersionDto>>.Failure($"Error retrieving versions: {ex.Message}");
            }
        }

        public async Task<Result<VideoVersionDto>> CreateVideoVersionAsync(Guid videoId, string changeDescription, string createdBy)
        {
            try
            {
                var video = await _videoRepository.GetVideoWithVersionsAsync(videoId);
                if (video == null)
                {
                    return Result<VideoVersionDto>.Failure("Video not found");
                }

                var maxVersion = video.Versions.Any() 
                    ? video.Versions.Max(v => v.VersionNumber) 
                    : 0;

                var newVersion = new VideoVersion
                {
                    Id = Guid.NewGuid(),
                    VideoId = videoId,
                    VersionNumber = maxVersion + 1,
                    ChangeDescription = changeDescription,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsActive = true
                };

                // Deactivate previous versions
                foreach (var version in video.Versions)
                {
                    version.IsActive = false;
                }

                video.Versions.Add(newVersion);
                await _videoRepository.UpdateAsync(video);

                var versionDto = new VideoVersionDto
                {
                    Id = newVersion.Id,
                    VersionNumber = newVersion.VersionNumber,
                    FileSizeBytes = newVersion.FileSizeBytes,
                    ChangeDescription = newVersion.ChangeDescription,
                    CreatedDate = newVersion.CreatedDate,
                    CreatedBy = newVersion.CreatedBy,
                    IsActive = newVersion.IsActive
                };

                return Result<VideoVersionDto>.Success(versionDto, "Version created successfully");
            }
            catch (Exception ex)
            {
                return Result<VideoVersionDto>.Failure($"Error creating version: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetVideoFilePathAsync(Guid videoId, int? versionNumber = null)
        {
            try
            {
                var video = await _videoRepository.GetVideoWithVersionsAsync(videoId);
                if (video == null)
                {
                    return Result<string>.Failure("Video not found");
                }

                VideoVersion? version;
                if (versionNumber.HasValue)
                {
                    version = video.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber.Value);
                }
                else
                {
                    version = video.Versions.FirstOrDefault(v => v.IsActive);
                }

                if (version == null)
                {
                    return Result<string>.Failure("Version not found");
                }

                return Result<string>.Success(version.FilePath);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Error retrieving file path: {ex.Message}");
            }
        }

        public async Task<Result> IncrementViewCountAsync(Guid videoId)
        {
            try
            {
                var video = await _context.Videos
                    .Include(v => v.Metadata)
                    .FirstOrDefaultAsync(v => v.Id == videoId);

                if (video?.Metadata == null)
                {
                    return Result.Failure("Video not found");
                }

                video.Metadata.ViewCount++;
                video.Metadata.LastViewedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error incrementing view count: {ex.Message}");
            }
        }

        private VideoDto MapToDto(Video video)
        {
            var tags = new List<string>();
            if (video.Metadata != null && !string.IsNullOrEmpty(video.Metadata.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize<List<string>>(video.Metadata.Tags) ?? new List<string>();
                }
                catch { }
            }

            return new VideoDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                OriginalFileName = video.OriginalFileName,
                FileSizeBytes = video.FileSizeBytes,
                FileFormat = video.FileFormat,
                DurationSeconds = video.DurationSeconds,
                ThumbnailPath = video.ThumbnailPath,
                CreatedDate = video.CreatedDate,
                ModifiedDate = video.ModifiedDate,
                CreatedBy = video.CreatedBy,
                VersionCount = video.Versions?.Count ?? 0,
                CurrentVersion = video.Versions?.Where(v => v.IsActive).FirstOrDefault()?.VersionNumber ?? 1,
                Metadata = video.Metadata != null ? new VideoMetadataDto
                {
                    Resolution = video.Metadata.Resolution,
                    FrameRate = video.Metadata.FrameRate,
                    VideoCodec = video.Metadata.VideoCodec,
                    AudioCodec = video.Metadata.AudioCodec,
                    BitRate = video.Metadata.BitRate,
                    AspectRatio = video.Metadata.AspectRatio,
                    Tags = tags,
                    ViewCount = video.Metadata.ViewCount
                } : null
            };
        }
    }
}
