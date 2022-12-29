using System.ComponentModel.DataAnnotations;

using Identity.Domain.Dtos;

namespace Identity.Domain;

public class UserPreference
{
    [Key]
    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    public bool MarketingEmails { get; set; }

    public UserPreference() { }

    public UserPreference(Guid userId, bool marketing)
    {
        UserId = userId;
        MarketingEmails = marketing;
    }

    public UserPreference(Guid userId, UserPreferencesDto dto) : this(userId, dto.MarketingEmails)
    {
    }
}