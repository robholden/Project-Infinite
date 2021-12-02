using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Domain;

public class PictureLocationRequest : Coords
{
    public PictureLocationRequest()
    {
    }

    public PictureLocationRequest(decimal lat, decimal lng)
    {
        Lat = lat;
        Lng = lng;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid RequestId { get; set; }

    public Guid? OwnedBy { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; }
}