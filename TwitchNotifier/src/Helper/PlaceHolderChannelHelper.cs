namespace TwitchNotifier.src.Helper {
    /// <summary>
    /// The Twitch channel of a specific user
    /// </summary>
    public class PlaceHolderChannelHelper {
        public string Name { get; set; }
        public TwitchLib.Api.V5.Models.Channels.Channel User { get; set; }
    }
}