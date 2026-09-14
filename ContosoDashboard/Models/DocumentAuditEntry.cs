using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentAuditEntry
{
    [Key]
    public int DocumentAuditEntryId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DocumentAuditAction Action { get; set; }

    [MaxLength(1000)]
    public string? Details { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}

public enum DocumentAuditAction
{
    Uploaded,
    Downloaded,
    Updated,
    Replaced,
    Shared,
    Deleted,
    Viewed,
    AccessDenied
}
