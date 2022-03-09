using System.Linq;
using Newtonsoft.Json;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.placeholders; 

internal class ClipPlaceholder {
    [JsonProperty]
    internal readonly User Creator;
    
    [JsonProperty]
    internal readonly string CreatorChannelUrl = Placeholder.TwitchBaseUrl;

    internal ClipPlaceholder(Clip clip, User creator) {
        Creator = creator;
        CreatorChannelUrl += clip.CreatorName;
        
        // To let users be able to use placeholders from inside the clip object
        // without using the same term twice (clip.clip.x), set fields dynamically inside this class.
        var props = clip.GetType().GetProperties().Where(x => x.CanRead && x.CanWrite);
        var context = this;
        foreach (var prop in props) {
            var value = prop.GetValue(clip, null);
            if (value == null)
                continue;
            prop.SetValue(context, value, null);
        }
    }
}