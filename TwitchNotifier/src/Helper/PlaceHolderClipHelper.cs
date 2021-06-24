using System.Linq;
using TwitchLib.Api.V5.Models.Channels;

namespace TwitchNotifier.src.Helper {
    public class PlaceHolderClipHelper : TwitchLib.Api.Helix.Models.Clips.GetClips.Clip {
        public Channel Creator { get; set; }

        public PlaceHolderClipHelper(TwitchLib.Api.Helix.Models.Clips.GetClips.Clip clip, Channel channel) {
            Creator = channel;
            var properties = clip.GetType().GetProperties().Where(x => x.CanRead && x.CanWrite);
            var this_ = this;

            foreach (var property in properties) {
                var propertyValue = property.GetValue(clip, null);

                if (propertyValue != null) {
                    property.SetValue(this_, propertyValue, null);
                }
            }
        }
    }
}