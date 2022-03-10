using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TwitchNotifier.models  {
    public class Embed {
        private static readonly string[] StaticEmbedFields = { "avatar_url", "username", "content" };
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
        
        /// <summary>
        /// Get the json string of the embed.
        /// </summary>
        /// <returns>Json formatted string of the embed.</returns>
        public override string ToString() {
            var jsonSerializerSettings = new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            // Add the current time as timestamp if property is set.
            // Remove the property otherwise.
            const string timeProp = "timestamp";
            var json = JsonConvert.SerializeObject(this, jsonSerializerSettings);
            var parsedJson = JObject.Parse(json);
            if (parsedJson.ContainsKey(timeProp) && (parsedJson.GetValue(timeProp)?.ToString().ParseToBoolean() ?? false)) {
                parsedJson[timeProp] = DateTime.Now;
            } else {
                parsedJson.Remove(timeProp);
            }
        
            // Remove unexpected fields for the request.
            parsedJson.Remove(StaticEmbedFields[0]);
            parsedJson.Remove(StaticEmbedFields[1]);
            parsedJson.Remove(StaticEmbedFields[2]);
            return parsedJson.ToString();
        }
    }

    /// <summary>
    /// Representing the thumbnail of the embed.
    /// </summary>
    public class EmbedThumbnail {
        public string Url = "%Channel.User.ProfileImageUrl%";
    }

    /// <summary>
    /// Representing the image of the embed.
    /// </summary>
    public class EmbedImage {
        public string Url = "%Stream.ThumbnailUrl%";
    }

    /// <summary>
    /// Represents the author of an embed.
    /// </summary>
    public class EmbedAuthor {
        public string Name { get; set; } = "Stream Announcer 📢";
        public string IconUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
        public string Url { get; set; } = "%Channel.Url%";
    }

    /// <summary>
    /// Representing a single field of an embed.
    /// </summary>
    public class EmbedField {
        public string Name { get; set; } = "Name of the field (max 256 chars)";
        public string Value { get; set; } = "Value of the field (max 1024 chars)";
        public bool Inline { get; set; }
    }

    public class EmbedFooter {
        public string Text { get; set; } = "The footer text (max 2048 chars)";
        public string IconUrl { get; set; } = "%Channel.User.ProfileImageUrl%";
    }
}