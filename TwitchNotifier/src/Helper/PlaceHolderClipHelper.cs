using System.Linq;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace TwitchNotifier.src.Helper {
    public class PlaceHolderClipHelper : TwitchLib.Api.Helix.Models.Clips.GetClips.Clip {
        public User Creator { get; set; }
        public string CreatorChannelUrl { get; set; }

        public PlaceHolderClipHelper(TwitchLib.Api.Helix.Models.Clips.GetClips.Clip clip, User user) {
            Creator = user;
            CreatorChannelUrl = string.Concat(PlaceholderHelper.TwitchBaseUrl, clip.CreatorName);
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