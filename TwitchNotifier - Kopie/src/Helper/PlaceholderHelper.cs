using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchNotifier.src.Helper {
    /// <summary>
    /// Helper class to use for repalcing placeholders
    /// </summary>
    public class PlaceholderHelper {
        public TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream Stream { get; set; }
        public Channel Channel { get; set; }
    }


    /// <summary>
    /// The Twitch channel of a specific user
    /// </summary>
    public class Channel {
        public string Name { get; set; }
        public TwitchLib.Api.V5.Models.Channels.Channel User { get; set; }
    }


    /// <summary>
    /// Extension methods for easier placeholder formatting
    /// </summary>
    public static class ExtensionMethods {
        public static string ToSnakeCase(this string str) {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) && str[i-1] != '.' ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
