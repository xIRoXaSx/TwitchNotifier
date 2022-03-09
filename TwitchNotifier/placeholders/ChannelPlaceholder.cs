using Newtonsoft.Json;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.placeholders;

internal class ChannelPlaceholder {
    [JsonProperty]
    internal readonly User User;
    
    [JsonProperty]
    internal readonly string Name;
    
    [JsonProperty]
    internal readonly string Url = Placeholder.TwitchBaseUrl;

    internal ChannelPlaceholder(User user, string channelName) {
        User = user;
        Name = channelName;
        Url += channelName;
    }
}