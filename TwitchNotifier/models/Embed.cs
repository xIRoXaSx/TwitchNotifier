using System.Collections.Generic;

namespace TwitchNotifier.models  {
    internal class Embed {
        public string Username { get; set; } = "%Channel.Name%";
        public string AvatarUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
        public string Content { get; set; } = "Content above the embed (max 2048 characters)";
        public string Title { get; set; } = "%Channel.Name% went online!";
        public string Url { get; set; } = "%Channel.Url%";
        public string Description { get; set; } = "What are you waiting for?!\\nGo check it out now!";
        public string Color { get; set; } = "#5555FF";
        public bool Timestamp { get; set; } = true;
        public EmbedThumbnail Thumbnail { get; set; } = new();
        public EmbedImage Image { get; set; } = new();
        public EmbedAuthor Author { get; set; } = new();
        public List<EmbedField> Fields { get; set; } = new() {
            new EmbedField {
                Name = "Unique Field Name 1",
                Value = "Value of field 1",
                Inline = false
            },
            new EmbedField {
                Name = "Unique Field Name 2",
                Value = "Value of field 2",
                Inline = false
            },
        };
        public EmbedFooter Footer { get; set; } = new();
    }

    /// <summary>
    /// Representing the thumbnail of the embed.
    /// </summary>
    internal class EmbedThumbnail {
        public string Url = "%Channel.User.ProfileImageUrl%";
    }

    /// <summary>
    /// Representing the image of the embed.
    /// </summary>
    internal class EmbedImage {
        public string Url = "%Stream.ThumbnailUrl%";
    }

    /// <summary>
    /// Represents the author of an embed.
    /// </summary>
    internal class EmbedAuthor {
        public string Name { get; set; } = "Stream Announcer 📢";
        public string IconUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
        public string Url { get; set; } = "%Channel.Url%";
    }

    /// <summary>
    /// Representing a single field of an embed.
    /// </summary>
    internal class EmbedField {
        public string Name { get; set; } = "Name of the field (max 256 chars)";
        public string Value { get; set; } = "Value of the field (max 1024 chars)";
        public bool Inline { get; set; }
    }

    internal class EmbedFooter {
        public string Text { get; set; } = "The footer text (max 2048 chars)";
        public string IconUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
    }
}