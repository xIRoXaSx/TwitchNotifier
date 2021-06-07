namespace TwitchNotifier.src.Helper {
    /// <summary>
    /// Helper class to use for repalcing placeholders
    /// </summary>
    public class PlaceholderHelper {
        public TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream Stream { get; set; }
        public PlaceHolderChannelHelper Channel { get; set; }
    }
}