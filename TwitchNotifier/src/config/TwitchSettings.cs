namespace TwitchNotifier.src.config {
    /// <summary>
    /// Settings for the Twitch channel which are creating events
    /// </summary>
    public class TwitchSettings {
        public string ClientID { get; set; } = "Your Client ID";
        public string AccessToken { get; set; } = "Your App Access Token";
    }
}
