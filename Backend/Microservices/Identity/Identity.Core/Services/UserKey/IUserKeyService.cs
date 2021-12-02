
using Identity.Domain;

namespace Identity.Core.Services;

public interface IUserKeyService
{
    Task<UserKey> Create(Guid userId, UserKeyType type, DateTime? expires, string key = null);

    Task InvalidateKeys(Guid userId, UserKeyType type);

    Task<UserKey> UseKey(string key, UserKeyType type);

    Task<UserKey> ValidateAndUseKey(string key, UserKeyType type);

    Task ValidateKey(string key, UserKeyType type);

    Task<UserKey> ExistingKey(Guid userId, UserKeyType type);
}