namespace VideoManager.Model
{
    /// <summary>
    /// Data transfer object for Video
    /// </summary>
    public class VideoDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileFormat { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public string ThumbnailPath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int VersionCount { get; set; }
        public int CurrentVersion { get; set; }
        public VideoMetadataDto? Metadata { get; set; }
    }

    public class VideoMetadataDto
    {
        public string Resolution { get; set; } = string.Empty;
        public double? FrameRate { get; set; }
        public string VideoCodec { get; set; } = string.Empty;
        public string AudioCodec { get; set; } = string.Empty;
        public int? BitRate { get; set; }
        public string AspectRatio { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public long ViewCount { get; set; }
    }

    public class CreateVideoDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileFormat { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class UpdateVideoDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class VideoVersionDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public long FileSizeBytes { get; set; }
        public string ChangeDescription { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UploadVideoVersionDto
    {
        public Guid VideoId { get; set; }
        public string ChangeDescription { get; set; } = string.Empty;
    }
}
