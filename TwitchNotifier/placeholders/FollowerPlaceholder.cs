using System;
using Newtonsoft.Json;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.placeholders; 

internal class FollowerPlaceholder {
    [JsonProperty]
    internal readonly User User;
    
    [JsonProperty]
    internal readonly string Name;
    
    [JsonProperty]
    internal readonly DateTime FollowedAt;
    
    [JsonProperty]
    internal readonly string FollowerChannelUrl = Placeholder.TwitchBaseUrl;

    internal FollowerPlaceholder(User follower, DateTime followedAt) {
        User = follower;
        Name = follower.DisplayName;
        FollowedAt = followedAt;
        FollowerChannelUrl += follower.DisplayName;
    }
}