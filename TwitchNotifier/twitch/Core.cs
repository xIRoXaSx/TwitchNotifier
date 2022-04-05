#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.twitch; 

internal class Core {
    internal bool IsValid { get; private set; }
    internal TwitchAPI TwitchApi { get; }
    internal StreamMonitor StreamMonitor { get; }
    internal FollowerMonitor FollowerMonitor { get; }
    internal ClipMonitor ClipMonitor { get; }
    internal event EventHandler? DisposeRequested;

    /// <summary>
    /// The core to handle connections to Twitch's API.
    /// </summary>
    internal Core() {
        var config = Program.Conf;
        TwitchApi = new TwitchAPI {
            Settings = {
                ClientId = config.GeneralSettings.ClientId,
                AccessToken = config.GeneralSettings.AccessToken
            }
        };

        FollowerMonitor = new FollowerMonitor(TwitchApi);
        StreamMonitor = new StreamMonitor(TwitchApi);
        ClipMonitor = new ClipMonitor();
    }

    /// <summary>
    /// Validates the provided credentials and throws an <c>InvalidCredentialException</c> if
    /// the given information is incorrect.
    /// </summary>
    /// <exception cref="InvalidCredentialException">Credentials are invalid.</exception>
    internal async Task ValidateOrThrowAsync() {
        if (await TwitchApi.Auth.ValidateAccessTokenAsync() == null)
            throw new InvalidCredentialException("Provided credentials are invalid.");
        IsValid = true;
    }

    /// <summary>
    /// Dispose the Core instance.
    /// </summary>
    public void Dispose() {
        DisposeRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Get a single <c>User</c> with the corresponding name or id from the provided IEnumerable.
    /// </summary>
    /// <param name="channel"><c>IEnumerable&lt;string&gt;</c> - The IEnumerable containing the channel name / Id.</param>
    /// <param name="isName"><c>Bool</c> - Whether the given IEnumerable contains the name or the Id.</param>
    /// <returns><c>User?</c> - Either the matched User or null</returns>
    internal async Task<User?> GetUser(IEnumerable<string> channel, bool isName = true) {
        var userList = channel.ToList();
        if (userList.Count < 1)
            return null;
            
        GetUsersResponse? users;
        if (isName) {
            users = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(logins: userList);
        } else {
            users = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(ids: userList);
        }
        return users.Users.Length > 0 ? users.Users[0] : null;
    }
}