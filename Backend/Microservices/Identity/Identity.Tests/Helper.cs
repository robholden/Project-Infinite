
using Identity.Core;
using Identity.Core.Queries;
using Identity.Core.Services;
using Identity.Domain;

using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

using Moq;

namespace Identity.Tests;

public class Helper
{
    private static IdentitySettings _settings;

    public Helper()
    {
        // Add database context
        Context = new IdentityContext(
            new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase("IdentityDbTest")
                .UseLazyLoadingProxies(false)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options
        );

        var options = Options.Create(Settings);

        var mockCommEvents = new Mock<ICommsPubSub>().Object;
        var mockIdentityEvents = new Mock<IIdentityPubSub>().Object;
        var mockSocketEvents = new Mock<ISocketsPubSub>().Object;

        UserQueries = new UserQueries(Context);

        PasswordService = new PasswordService(Context);
        UserKeyService = new UserKeyService(Context);
        TwoFactorService = new TwoFactorService(Context, options, UserKeyService, mockCommEvents);
        UserService = new UserService(Context, options, UserKeyService, PasswordService, TwoFactorService, mockIdentityEvents, mockCommEvents, mockSocketEvents);
        AuthService = new AuthService(Context, UserService, PasswordService, TwoFactorService);
    }

    public static IdentitySettings Settings
    {
        get
        {
            if (_settings != null) return _settings;

            _settings = new IdentitySettings
            {
                FailedLoginAttempts = 5,
                FailedLoginDuration = 5,
                Totp = new TotpSettings
                {
                    Size = 6,
                    Step = 30,
                    Mode = OtpNet.OtpHashMode.Sha256,
                    ExpiresInSeconds = 60
                },
                ExpiryLength = 300
            };

            return _settings;
        }
    }

    public IdentityContext Context { get; }

    public IPasswordService PasswordService { get; }

    public IUserKeyService UserKeyService { get; }

    public ITwoFactorService TwoFactorService { get; }

    public IUserQueries UserQueries { get; }

    public IUserService UserService { get; }

    public IAuthService AuthService { get; }

    public User AddTestUser(User userModel) => AddTestUser(userModel, true);

    public User AddTestUser(bool withPassword) => AddTestUser(null, withPassword);

    public User AddTestUser(User userModel = null, bool withPassword = true)
    {
        userModel ??= BaseUserModel();
        var user = Context.Users.Add(userModel).Entity;
        if (withPassword) Context.Passwords.Add(new Password(user.UserId, "Test123$"));
        Context.SaveChanges();
        return user;
    }

    public User BaseUserModel()
    {
        var index = Context.Users.Count() + 1;
        return new User()
        {
            Name = "Unit Tester " + index,
            Username = "Tester " + index,
            Email = $"tester_{index}@email.com"
        };
    }
}