using Newtonsoft.Json;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;

namespace TwitchNotifier.placeholders; 

public class TwitchPlaceholder {
    [JsonProperty]
    internal Stream Stream { get; set; }
    
    [JsonProperty]
    internal ChannelPlaceholder Channel { get; set; }
    
    [JsonProperty]
    internal ClipPlaceholder Clip { get; set; }
    
    internal TwitchPlaceholder() {}
}