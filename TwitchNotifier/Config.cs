using System;
using System.Collections.Generic;
using System.IO;
using TwitchNotifier.models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TwitchNotifier {
    /// <summary>
    /// The general configuration.
    /// </summary>
    internal class Config {
        /// <summary>
        /// The name of the config file.
        /// </summary>
        private const string Name = "config.yml";

        /// <summary>
        /// The path to the binary's config directory.
        /// </summary>
        private static readonly string DirPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Program.BinaryName
        );
        
        /// <summary>
        /// The path to the binary's config directory.
        /// </summary>
        private static readonly string FullPath = Path.Combine(DirPath, Name);
        
        /// <summary>
        /// Settings for the notifications.
        /// </summary>
        public NotificationSettings NotificationSettings { get; set; } = new();
        
        /// <summary>
        /// Settings for the binary itself.
        /// </summary>
        public GeneralSettings GeneralSettings { get; private set; } = new();

        /// <summary>
        /// Create the configuration directory and file if they do not already exist.
        /// </summary>
        internal void CreateIfNotExisting() {
            // Check if config file does not already exist.
            if (File.Exists(FullPath))
                return;
            
            if (!Directory.Exists(DirPath)) {
                try {
                    Directory.CreateDirectory(DirPath);
                } catch (Exception ex) {
                    Console.Error.WriteLine("ERR: Could not create config directory: {0}", ex.Message);
                    throw;
                }
            }

            // Directory either exists or has been created.
            // Since config does not already exist, create a serializer instance and write config file.
            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            try {
                File.WriteAllText(FullPath, serializer.Serialize(this));
                Console.WriteLine("INF: Config has been written to: {0}", FullPath);
            } catch (Exception ex) {
                Console.Error.WriteLine("Err: Could not write config to file: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Load the config into the current instance.
        /// </summary>
        internal void Load() {
            var deserializer = new DeserializerBuilder().Build();
            var config = deserializer.Deserialize<Config>(File.ReadAllText(FullPath, System.Text.Encoding.UTF8));
            GeneralSettings = config.GeneralSettings;
            NotificationSettings = config.NotificationSettings;
        }
    }

    /// <summary>
    /// Settings for the notifications.
    /// </summary>
    public class NotificationSettings {
        public List<NotificationEvent> NotificationEvent { get; set; } = new(){ new NotificationEvent() };
    }

    /// <summary>
    /// General settings.
    /// </summary>
    public class GeneralSettings {
        /// <summary>
        /// Whether debug logging is enabled or not.
        /// </summary>
        public bool Debug { get; set; } = false;
        
        /// <summary>
        /// Whether the hot loading feature is enabled or not.
        /// </summary>
        public bool EnableHotLoad { get; set; } = true;
        
        /// <summary>
        /// Whether the notifications on startup should be skipped or not.
        /// Notifications for channels which are currently live will be pushed
        /// regardless of previous detections.
        /// </summary>
        public bool SkipNotificationsOnStartup { get; set; } = true;
        
        /// <summary>
        /// The threshold which needs to exceed before a new live notification will be sent.
        /// </summary>
        public int LiveNotificationThresholdInSeconds { get; set; } = 120;

        /// <summary>
        /// The client Id for the Twitch API.
        /// </summary>
        public string ClientId { get; set; } = "Your Client ID";

        /// <summary>
        /// The access token for the Twitch API.
        /// </summary>
        public string AccessToken { get; set; } = "Your App Access Token";
    }
}