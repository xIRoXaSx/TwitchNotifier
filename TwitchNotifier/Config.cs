using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TwitchNotifier.models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TwitchNotifier; 

/// <summary>
/// The general configuration.
/// </summary>
internal class Config {
    /// <summary>
    /// The name of the config file.
    /// </summary>
    private const string Name = "config.yml";
    private FileSystemWatcher _fsWatcher;

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
    public GeneralSettings GeneralSettings { get; set; } = new();

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
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var config = deserializer.Deserialize<Config>(TryReadFile(FullPath));
        if (config == null)
            return;
        GeneralSettings = config.GeneralSettings;
        NotificationSettings = config.NotificationSettings;
    }

    /// <summary>
    /// Returns a distinct List of channels which should be monitored for live and offline events.
    /// </summary>
    /// <returns><c>List&lt;string&gt;</c> - List containing all channels to monitor.</returns>
    internal IEnumerable<string> GetMonitoredChannels() {
        var returnValue = new List<string>();
        for (var i = 0; i < NotificationSettings.OnLiveEvent.Count; i++) {
            returnValue.AddRange(NotificationSettings.OnLiveEvent[i].Channels);
        }
            
        for (var i = 0; i < NotificationSettings.OnOfflineEvent.Count; i++) {
            returnValue.AddRange(NotificationSettings.OnOfflineEvent[i].Channels);
        }
            
        return returnValue.Distinct(StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Returns a distinct List of channels which should be monitored for follow events.
    /// </summary>
    /// <returns><c>List&lt;string&gt;</c> - List containing all channels to monitor.</returns>
    internal IEnumerable<string> GetFollowMonitoredChannels() {
        var returnValue = new List<string>();
        for (var i = 0; i < NotificationSettings.OnFollowEvent.Count; i++) {
            returnValue.AddRange(NotificationSettings.OnFollowEvent[i].Channels);
        }
        return returnValue.Distinct(StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Adds the filesystem watcher to monitor config changes.
    /// </summary>
    internal void SetFileWatcher() {
        _fsWatcher = new FileSystemWatcher(DirPath, Name) {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite
        };
            
        _fsWatcher.Changed += FileSystemWatcherOnChanged;
    }

    /// <summary>
    /// Gets invoked whenever the config file has been modified.
    /// </summary>
    /// <param name="sender"><c>Object</c> - The sender object.</param>
    /// <param name="e"><c>FileSystemEventArgs</c> - The event args</param>
    private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e) {
        // Check if any property for the Twitch API has been changed.
        var entry = new CacheEntry {
            Key = "TN_Config",
            ExpirationTime = DateTime.Now.AddSeconds(1),
            Sha256HashKey = false
        };
            
        if (!Cache.IsCacheEntryExpired(entry))
            return;
            
        Cache.AddEntry(entry);
        Logging.Debug("Hot-loaded config");
        var oldConf = Program.Conf.GeneralSettings;
        Program.Conf.Load();
        Logging.IsDebugEnabled = Program.Conf.GeneralSettings.Debug;
        
        if (Program.Conf.GeneralSettings.ClientId.Create256Sha() != oldConf.ClientId.Create256Sha() ||
            Program.Conf.GeneralSettings.AccessToken.Create256Sha() != oldConf.AccessToken.Create256Sha()) {
            Program.TwitchCore.Dispose();
        }
    }
        
    /// <summary>
    /// Try to read a file from the given path.
    /// </summary>
    /// <param name="fullPath"><c>String</c> - The full path of the file to read.</param>
    /// <returns><c>String</c> - Either content of the config file or empty.</returns>
    private static string TryReadFile(string fullPath) {
        var res = Array.Empty<byte>();
        for (var numTries = 0; numTries < 10; numTries++) {
            try {
                res = File.ReadAllBytes(fullPath);
            } catch (IOException) {
                Thread.Sleep(50);
            }
        }
        return res.Length == 0 ? "" : System.Text.Encoding.UTF8.GetString(res);
    }
}

/// <summary>
/// Settings for the notifications.
/// </summary>
public class NotificationSettings {
    public List<NotificationEvent> OnLiveEvent { get; set; } = new(){ new NotificationEvent() };
    public List<NotificationEvent> OnOfflineEvent { get; set; } = new(){ new NotificationEvent() };
    public List<NotificationEvent> OnClipCreated { get; set; } = new(){ new NotificationEvent() };
    public List<NotificationEvent> OnFollowEvent { get; set; } = new(){ new NotificationEvent() };
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
    /// The interval to check the online and offline status of channels.
    /// </summary>
    public int LiveCheckIntervalInSeconds { get; set; } = 5;
    
    /// <summary>
    /// The interval to check for follower events.
    /// </summary>
    public int FollowerCheckIntervalInSeconds { get; set; } = 5; 
        
    /// <summary>
    /// The client Id for the Twitch API.
    /// </summary>
    public string ClientId { get; set; } = "Your Client ID";

    /// <summary>
    /// The access token for the Twitch API.
    /// </summary>
    public string AccessToken { get; set; } = "Your App Access Token";
}