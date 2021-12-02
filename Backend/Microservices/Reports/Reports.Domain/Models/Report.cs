using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Library.Core.Models;

namespace Reports.Domain;

public abstract class Report<T> where T : ReportInstance, IUser
{
    protected Report()
    {
    }

    protected Report(IUser user)
    {
        UserId = user.UserId;
        Username = user.Username;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ReportId { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    public Guid? ActionId { get; set; }

    public virtual ReportAction Action { get; set; }

    public virtual ICollection<T> Reports { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public abstract class ReportInstance : IUser
{
    protected ReportInstance()
    {
    }

    protected ReportInstance(IUser user)
    {
        UserId = user.UserId;
        Username = user.Username;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid InstanceId { get; set; }

    public Guid UserId { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}