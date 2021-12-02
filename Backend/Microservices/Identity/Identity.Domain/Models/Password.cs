using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Domain;

public class Password
{
    public Password()
    {
    }

    public Password(Guid userId, string password)
    {
        UserId = userId;
        Set(password);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PasswordId { get; set; }

    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    [Required]
    public byte[] PasswordHash { get; set; }

    [Required]
    public byte[] PasswordSalt { get; set; }

    public void Set(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        using var hmac = new System.Security.Cryptography.HMACSHA512();
        PasswordSalt = hmac.Key;
        PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    }

    public bool Verify(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        if (PasswordHash?.Length != 64) return false;
        if (PasswordSalt?.Length != 128) return false;

        using (var hmac = new System.Security.Cryptography.HMACSHA512(PasswordSalt))
        {
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != PasswordHash[i]) return false;
            }
        }

        return true;
    }
}