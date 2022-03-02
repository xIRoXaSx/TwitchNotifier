namespace TwitchNotifier.src.config {
    /// <summary>
    /// The webhook user that sends the embed (to overwrite the existing data from Discord)
    /// </summary>
    public class DiscordEmbed {
        public string Username { get; set; } = "%Channel.Name%";
        public string AvatarUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
        public string Content { get; set; } = "Content above the embed (max 2048 characters)";
        public Embed Embed { get; set; } = new Embed();
    }
}
