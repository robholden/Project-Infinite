namespace Identity.Core.Services;

public interface IPasswordService
{
    Task Change(Guid userId, string oldPassword, string newPassword);

    Task<bool> Check(Guid userId, string password);

    Task Set(Guid userId, string password);

    Task Verify(Guid userId, string password);
}