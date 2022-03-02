﻿namespace TwitchNotifier.src.Helper {
    /// <summary>
    /// Helper class to use for repalcing placeholders
    /// </summary>
    public class PlaceholderHelper {
        internal const string TwitchBaseUrl = "https://www.twitch.tv/";
        public TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream Stream { get; set; }
        public PlaceHolderChannelHelper Channel { get; set; }
        public PlaceHolderClipHelper Clip { get; set; }
    }
}