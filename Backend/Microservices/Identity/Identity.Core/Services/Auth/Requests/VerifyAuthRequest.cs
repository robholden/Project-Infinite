namespace Identity.Core.Services;

public record VerifyAuthRequest(string Password, Guid? TouchIdKey);