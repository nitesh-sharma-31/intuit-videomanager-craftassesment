using Microsoft.AspNetCore.Mvc;
using VideoManager.Model;
using VideoManager.Shared.Interfaces;

namespace VideoManager.Api.Controllers
{
    [ApiController]
    [Route("api/videos/")]
    public class VideosController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly IStorageService _storageService;
        private readonly ILogger<VideosController> _logger;

        public VideosController(
            IVideoService videoService,
            IStorageService storageService,
            ILogger<VideosController> logger)
        {
            _videoService = videoService;
            _storageService = storageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<VideoDto>>> GetVideos([FromQuery] QueryParameters parameters)
        {
            var result = await _videoService.GetVideosAsync(parameters);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VideoDto>> GetVideo(Guid id)
        {
            var result = await _videoService.GetVideoByIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<VideoDto>> CreateVideo([FromBody] CreateVideoDto dto)
        {
            var createdBy = User.Identity?.Name ?? "System";
            var result = await _videoService.CreateVideoAsync(dto, createdBy);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetVideo), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VideoDto>> UpdateVideo(Guid id, [FromBody] UpdateVideoDto dto)
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var result = await _videoService.UpdateVideoAsync(id, dto, modifiedBy);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVideo(Guid id)
        {
            var result = await _videoService.DeleteVideoAsync(id);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return NoContent();
        }

        [HttpPost("upload")]
        public async Task<ActionResult<VideoDto>> UploadVideo([FromForm] IFormFile file, [FromForm] string title, 
            [FromForm] string description, [FromForm] string tags)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                var videoId = Guid.NewGuid();
                var createdBy = User.Identity?.Name ?? "System";

                // Save file
                using var fileStream = file.OpenReadStream();
                var filePath = await _storageService.SaveVideoAsync(fileStream, file.FileName, videoId);

                // Parse tags
                var tagList = string.IsNullOrEmpty(tags) 
                    ? new List<string>() 
                    : tags.Split(',').Select(t => t.Trim()).ToList();

                // Create video record
                var createDto = new CreateVideoDto
                {
                    Title = title,
                    Description = description,
                    OriginalFileName = file.FileName,
                    FileSizeBytes = file.Length,
                    FileFormat = Path.GetExtension(file.FileName),
                    DurationSeconds = 0, // Would need video processing to get actual duration
                    Tags = tagList
                };

                var result = await _videoService.CreateVideoAsync(createDto, createdBy);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                // Create initial version
                await _videoService.CreateVideoVersionAsync(videoId, "Initial upload", createdBy);

                return CreatedAtAction(nameof(GetVideo), new { id = result.Data!.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video");
                return StatusCode(500, "Error uploading video");
            }
        }

        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadVideo(Guid id, [FromQuery] int? version = null)
        {
            try
            {
                var filePathResult = await _videoService.GetVideoFilePathAsync(id, version);
                if (!filePathResult.IsSuccess)
                {
                    return NotFound(filePathResult.Message);
                }

                var stream = await _storageService.GetVideoStreamAsync(filePathResult.Data!);
                var videoResult = await _videoService.GetVideoByIdAsync(id);
                var fileName = videoResult.Data?.OriginalFileName ?? "video.mp4";

                await _videoService.IncrementViewCountAsync(id);

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading video");
                return StatusCode(500, "Error downloading video");
            }
        }

        [HttpGet("{id}/versions")]
        public async Task<ActionResult<List<VideoVersionDto>>> GetVideoVersions(Guid id)
        {
            var result = await _videoService.GetVideoVersionsAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result.Data);
        }

        [HttpPost("{id}/versions")]
        public async Task<ActionResult<VideoVersionDto>> CreateVideoVersion(Guid id, 
            [FromForm] IFormFile file, [FromForm] string changeDescription)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                var createdBy = User.Identity?.Name ?? "System";

                // Get version number
                var versionsResult = await _videoService.GetVideoVersionsAsync(id);
                if (!versionsResult.IsSuccess)
                {
                    return BadRequest(versionsResult);
                }

                var nextVersion = (versionsResult.Data != null && versionsResult.Data.Any()) 
                    ? versionsResult.Data.Max(v => v.VersionNumber) + 1 
                    : 1;

                // Save file
                using var fileStream = file.OpenReadStream();
                var filePath = await _storageService.SaveVideoVersionAsync(fileStream, file.FileName, id, nextVersion);

                // Create version record
                var result = await _videoService.CreateVideoVersionAsync(id, changeDescription, createdBy);

                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video version");
                return StatusCode(500, "Error creating video version");
            }
        }
    }
}
