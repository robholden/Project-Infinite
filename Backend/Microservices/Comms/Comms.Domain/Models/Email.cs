using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comms.Domain;

public class Email
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EmailId { get; set; }

    [Required]
    public string Body { get; set; }

    public bool Completed { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? DateSent { get; set; }

    [Required]
    public Guid EmailQueueId { get; set; }

    public virtual EmailQueue EmailQueue { get; set; }

    [MaxLength(500)]
    public string Errors { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string FromEmailAddress { get; set; }

    public int Attempts { get; set; }
}