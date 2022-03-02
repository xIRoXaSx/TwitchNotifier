using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.src.Helper {
    /// <summary>
    /// The Twitch channel of a specific user
    /// </summary>
    public class PlaceHolderChannelHelper {
        public string Name { get; }
        public string Url { get; }
        public User User { get; }

        public PlaceHolderChannelHelper(User user, string channel) {
            Name = channel;
            Url = string.Concat(PlaceholderHelper.TwitchBaseUrl, channel);
            User = user;
        }
    }
}