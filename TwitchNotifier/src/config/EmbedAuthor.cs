namespace TwitchNotifier.src.config {
    /// <summary>
    /// The author of the embed
    /// </summary>
    public class EmbedAuthor {
        public string Name { get; set; } = "Stream Announcer 📢";
        public string IconUrl { get; set; } = "%Channel.User.Logo%";
        public string Url { get; set; } = "%Channel.User.Url%";
    }
}
