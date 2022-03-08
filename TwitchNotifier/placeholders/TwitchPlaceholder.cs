using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TwitchNotifier.placeholders; 

public class TwitchPlaceholder {
    internal readonly Stream Stream;
    internal ChannelPlaceholder Channel { get; set; }
    internal ClipPlaceholder Clip { get; set; }
        
    internal TwitchPlaceholder(Stream stream) {
        Stream = stream;
    }
}