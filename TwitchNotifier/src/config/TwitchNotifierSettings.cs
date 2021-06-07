namespace TwitchNotifier.src.config {
    /// <summary>
    /// Contains the complete configuration
    /// </summary>
    public class TwitchNotifierSettings {
        public Event OnStreamOnline { get; set; } = new Event();
        public Event OnStreamOffline { get; set; } = new Event();
        public Event OnFollow { get; set; } = new Event();
    }
}
