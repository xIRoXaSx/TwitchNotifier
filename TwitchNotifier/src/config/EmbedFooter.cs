namespace TwitchNotifier.src.config {
    /// <summary>
    /// The footer of the embed
    /// </summary>
    public class EmbedFooter {
        public string Text { get; set; } = "The footer text (max 2048 chars)";
        public string IconUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
    }
}
