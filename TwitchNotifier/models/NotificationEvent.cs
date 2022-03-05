using System.Collections.Generic;

namespace TwitchNotifier.models {
    internal class NotificationEvent {
        internal string Condition { get; set; } = "";
        internal List<string> Usernames { get; set; } = new() {"Channel1", "Channel2"};
        internal Embed Embed { get; set; } = new();
        internal string WebHookUrl { get; set; } = "The Discord Webhook URL";
    }
}