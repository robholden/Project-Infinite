
using Library.Core;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sockets;

[Authorize]
public class SocketHub : Hub
{
    public static readonly IList<string> GroupLevels = new List<string>() { nameof(UserLevel.Admin), nameof(UserLevel.Moderator) };

    public override async Task OnConnectedAsync()
    {
        foreach (var role in GroupLevels)
        {
            if (Context.User.IsInRole(role)) await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var role in GroupLevels)
        {
            if (Context.User.IsInRole(role)) await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
        }

        await base.OnDisconnectedAsync(exception);
    }

    [Authorize(Roles = nameof(UserLevel.Admin))]
    public Task SendMessageToAdmins(string message) => Clients.Group(nameof(UserLevel.Admin)).SendAsync("GroupValue", $"{Context.User.GetClaim(UserClaimsKeys.Username)} says {message}");
}