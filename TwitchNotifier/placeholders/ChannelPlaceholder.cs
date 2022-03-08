using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.placeholders;

internal class ChannelPlaceholder {
    internal readonly User User;
    internal readonly string Name;
    internal readonly string Url = Placeholder.TwitchBaseUrl;

    internal ChannelPlaceholder(User user, string channelName) {
        User = user;
        Name = channelName;
        Url += channelName;
    }
}