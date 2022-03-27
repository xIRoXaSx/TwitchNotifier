using System.Collections.Generic;

namespace TwitchNotifier.models {
    public class NotificationEvent {
        public string Condition { get; set; } = "";
        public List<string> Channels { get; set; } = new() { "Channel1", "Channel2" };
        public Embed Embed { get; set; } = new();
        public string WebHookUrl { get; set; } = "The Discord Webhook URL";
    }
}