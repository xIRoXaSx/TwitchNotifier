using TwitchNotifier.models;

namespace TwitchNotifier {
    /// <summary>
    /// The general configuration.
    /// </summary>
    internal class Config {
        public NotificationSettings NotificationSettings { get; set; } = new();
        public GeneralSettings GeneralSettings { get; set; } = new();
    }

    /// <summary>
    /// Settings for the notifications.
    /// </summary>
    internal class NotificationSettings {
        internal NotificationEvent NotificationEvent { get; set; } = new();
    }

    /// <summary>
    /// General settings.
    /// </summary>
    internal class GeneralSettings {
        /// <summary>
        /// Whether debug logging is enabled or not.
        /// </summary>
        internal bool Debug { get; set; } = false;
        
        /// <summary>
        /// Whether the hot loading feature is enabled or not.
        /// </summary>
        internal bool EnableHotLoad { get; set; } = true;
        
        /// <summary>
        /// Whether the notifications on startup should be skipped or not.
        /// Notifications for channels which are currently live will be pushed
        /// regardless of previous detections.
        /// </summary>
        internal bool SkipNotificationsOnStartup { get; set; } = true;
        
        /// <summary>
        /// The threshold which needs to exceed before a new live notification will be sent.
        /// </summary>
        internal int LiveNotificationThresholdInSeconds { get; set; } = 120;

        /// <summary>
        /// The client Id for the Twitch API.
        /// </summary>
        internal string ClientId { get; set; } = "Your Client ID";

        /// <summary>
        /// The access token for the Twitch API.
        /// </summary>
        internal string AccessToken { get; set; } = "Your App Access Token";
    }
}