namespace TwitchNotifier.src.config {
    /// <summary>
    /// Contains events from Twitch
    /// </summary>
    public class EventObject {
        public string Condition { get; set; } = "";
        public TwitchListenerSettings Twitch { get; set; } = new TwitchListenerSettings();
        public DiscordEmbed Discord { get; set; } = new DiscordEmbed();
        public string WebHookUrl { get; set; } = "The Discord Webhook URL";
    }
}
