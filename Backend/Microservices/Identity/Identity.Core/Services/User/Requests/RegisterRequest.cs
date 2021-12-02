using Library.Core.Enums;

namespace Identity.Core.Services;

public record RegisterRequest(string Username, string Name, string Email, string Password, bool AllowMarketing, string Mobile = "", ExternalProviderRequest ExternalProvider = null);

public record ExternalProviderRequest(ExternalProvider Provider, string UserIdentifier);