using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoManager.Model
{
    /// <summary>
    /// Represents a version of a video file
    /// </summary>
    public class VideoVersion
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid VideoId { get; set; }

        [Required]
        public int VersionNumber { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public long FileSizeBytes { get; set; }

        [MaxLength(64)]
        public string FileHash { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string ChangeDescription { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // Navigation property
        [ForeignKey(nameof(VideoId))]
        public virtual Video? Video { get; set; }
    }
}
