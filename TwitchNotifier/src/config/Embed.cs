using System.Collections.Generic;

namespace TwitchNotifier.src.config {
    /// <summary>
    /// Settings for the Discord embed
    /// </summary>
    public class Embed {
        public string Title { get; set; } = "%Channel.Name% went online!";
        public string Url { get; set; } = "%Channel.Url%";
        public string Description { get; set; } = "What are you waiting for?!\\nGo check it out now!";
        public string Color { get; set; } = "#5555FF";
        public bool Timestamp { get; set; } = true;
        public Thumbnail Thumbnail { get; set; } //= new Thumbnail();
        public Image Image { get; set; } //= new Image();
        public EmbedAuthor Author { get; set; } = new EmbedAuthor();
        public List<EmbedField> Fields { get; set; } = new List<EmbedField>() {
            new EmbedField() {
                Name = "Unique Field Name 1",
                Value = "Value of field 1",
                Inline = false
            },
            new EmbedField() {
                Name = "Unique Field Name 2",
                Value = "Value of field 2",
                Inline = false
            },
        };
        public EmbedFooter Footer { get; set; } = new EmbedFooter();
    }
}
