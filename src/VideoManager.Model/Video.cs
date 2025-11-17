using System.ComponentModel.DataAnnotations;

namespace VideoManager.Model
{
    /// <summary>
    /// Represents a video entity in the system
    /// </summary>
    public class Video
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        public long FileSizeBytes { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileFormat { get; set; } = string.Empty;

        public int DurationSeconds { get; set; }

        [MaxLength(500)]
        public string ThumbnailPath { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ModifiedBy { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        // Navigation properties
        public virtual ICollection<VideoVersion> Versions { get; set; } = new List<VideoVersion>();
        public virtual VideoMetadata? Metadata { get; set; }
    }
}
