using System.Collections.Generic;

namespace TwitchNotifier.src.config {
    /// <summary>
    /// Settings for the Twitch channels which are creating events
    /// </summary>
    public class TwitchListenerSettings {
        public List<string> Usernames { get; set; } = new List<string>() { "Channelnames" };
    }
}
