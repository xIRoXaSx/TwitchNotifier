namespace TwitchNotifier.src.config {
    /// <summary>
    /// The fields for the embed (max = 25)
    /// </summary>
    public class EmbedField {
        public string Name { get; set; } = "Name of the field (max 256 chars)";
        public string Value { get; set; } = "Value of the field (max 1024 chars)";
        public bool Inline { get; set; } = false;
    }
}
