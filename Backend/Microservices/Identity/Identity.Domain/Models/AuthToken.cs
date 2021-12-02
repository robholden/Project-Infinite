using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Domain;

public class AuthToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AuthTokenId { get; set; }

    [NotMapped]
    public bool Active => Updated.HasValue && Updated > DateTime.UtcNow.AddDays(-1);

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public bool Deleted { get; set; }

    [Required, MaxLength(256)]
    public string IdentityKey { get; set; }

    [Required, MaxLength(50)]
    public string IpAddress { get; set; }

    [MaxLength(200)]
    public string PlatformRaw { get; set; }

    [MaxLength(50)]
    public string Platform { get; set; }

    public bool TouchIdEnabled { get; set; }

    public bool TwoFactorPassed { get; set; }

    public bool RememberIdentityForTwoFactor { get; set; }

    public DateTime? Updated { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(255)]
    public string RefreshToken { get; set; }

    public int Refreshes { get; set; }

    [Required]
    public DateTime RefreshedAt { get; set; } = DateTime.UtcNow;

    public void SetPlatform()
    {
        if (string.IsNullOrEmpty(PlatformRaw))
        {
            return;
        }

        var platform = PlatformRaw.ToLower();
        if (platform.StartsWith("ionic"))
        {
            if (platform.Contains("android"))
            {
                Platform = "Android";
            }
            else if (platform.Contains("ios"))
            {
                Platform = "IOS";
            }
            else
            {
                Platform = "Mobile";
            }

            if (platform.Contains("mobileweb"))
            {
                Platform += " [Web]";
            }
        }
        else if (platform.StartsWith("chrome"))
        {
            Platform = "Chrome";
        }
        else if (platform.StartsWith("edge-chromium"))
        {
            Platform = "Microsoft Edge";
        }
        else if (platform.StartsWith("firefox"))
        {
            Platform = "Firefox";
        }
        else if (platform.StartsWith("opera"))
        {
            Platform = "Opera";
        }
        else if (platform.StartsWith("safari"))
        {
            Platform = "Safari";
        }
        else
        {
            Platform = "Browser";
        }
    }
}