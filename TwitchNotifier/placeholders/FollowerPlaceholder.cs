using Newtonsoft.Json;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.placeholders; 

internal class FollowerPlaceholder {
    [JsonProperty]
    internal readonly User User;
    
    [JsonProperty]
    internal readonly string FollowerChannelUrl = Placeholder.TwitchBaseUrl;

    internal FollowerPlaceholder(User follower) {
        User = follower;
        FollowerChannelUrl += follower.DisplayName;
    }
}