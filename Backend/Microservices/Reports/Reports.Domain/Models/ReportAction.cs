using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core.Enums;
using Library.Core.Models;

namespace Reports.Domain;

public class ReportAction : IUser
{
    public ReportAction()
    {
    }

    public ReportAction(IUser user, ReportedAction actionTaken, string notes)
    {
        UserId = user.UserId;
        Username = user.Username;
        ActionTaken = actionTaken;
        Notes = notes;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ActionId { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [Required]
    public ReportedAction ActionTaken { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}