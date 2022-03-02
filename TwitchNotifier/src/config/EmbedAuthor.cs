namespace TwitchNotifier.src.config {
    /// <summary>
    /// The author of the embed
    /// </summary>
    public class EmbedAuthor {
        public string Name { get; set; } = "Stream Announcer 📢";
        public string IconUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
        public string Url { get; set; } = "%Channel.Url%";
    }
}
