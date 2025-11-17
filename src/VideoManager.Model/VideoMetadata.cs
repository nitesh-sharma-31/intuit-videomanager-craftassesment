using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoManager.Model
{
    /// <summary>
    /// Extended metadata for videos including technical details
    /// </summary>
    public class VideoMetadata
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid VideoId { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        [MaxLength(50)]
        public string Resolution { get; set; } = string.Empty;

        public double? FrameRate { get; set; }

        [MaxLength(50)]
        public string VideoCodec { get; set; } = string.Empty;

        [MaxLength(50)]
        public string AudioCodec { get; set; } = string.Empty;

        public int? BitRate { get; set; }

        [MaxLength(20)]
        public string AspectRatio { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ColorSpace { get; set; } = string.Empty;

        public int AudioChannels { get; set; }

        public int AudioSampleRate { get; set; }

        [MaxLength(50)]
        public string Container { get; set; } = string.Empty;

        public string Tags { get; set; } = string.Empty; // JSON array of tags

        public long ViewCount { get; set; }

        public DateTime? LastViewedDate { get; set; }

        // Navigation property
        [ForeignKey(nameof(VideoId))]
        public virtual Video? Video { get; set; }
    }
}
