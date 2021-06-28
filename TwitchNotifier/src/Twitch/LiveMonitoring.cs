using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Helper;
using TwitchNotifier.src.Logging;
using TwitchNotifier.src.Validation;
using TwitchNotifier.src.WebRequests;
using YamlDotNet.Serialization;

namespace TwitchNotifier.src.Twitch {
    /// <summary>
    /// Enum for disposing / stoping the LiveStreamMonitorService and TwitchAPI
    /// </summary>
    public enum DisposableInstance {
        Monitor = 0,
        API = 1,
        All = 2
    }

    /// <summary>
    /// Set up the live monitoring feature
    /// </summary>
    class LiveMonitoring {
        private FileSystemWatcher fileSystemWatcher;
        private LiveStreamMonitorService Monitor;
        private TwitchAPI API;
        private bool disposed = false;
        public bool sendNotifications = true;
        public static string defaultNotificationThresholdInSeconds = "NotificationThresholdInSeconds";
        public static string defaultSkipStartupNotifications = "SkipStartupNotifications";
        public static string defaultConfigCacheKey = "DefaultConfig";

        public LiveMonitoring() {
            Task.Run(() => ConfigureLiveMonitorAsync()).GetAwaiter().GetResult();
        }

        private async Task ConfigureLiveMonitorAsync() {
            var deserializer = new DeserializerBuilder().Build();
            
            try {
                var config = deserializer.Deserialize<dynamic>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));

                API = new TwitchAPI();
                API.Settings.ClientId = config["Settings"]["ClientID"];
                API.Settings.AccessToken = config["Settings"]["AccessToken"];

                Monitor = new LiveStreamMonitorService(API, 5);

                // Iterate through all event properties of the config
                var usernames = new List<string>();
                foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                    // Events like "OnStreamStart", "OnStreamEnd", "OnFollow", ...
                    var twitchEvent = config["TwitchNotifier"][property.Name];

                    // eventNode is the node for all settings below each event
                    foreach (var eventNode in twitchEvent) {
                        List<string> usernameList = ((List<object>)eventNode.Value["Twitch"]["Usernames"]).Select(x => (string)x).ToList();
                        usernames.AddRange(usernameList.Where(x => usernames.All(y => x.ToLower() != y.ToLower())));
                    }
                }

                Monitor.OnStreamOnline += Monitor_OnStreamOnline;
                Monitor.OnStreamOffline += Monitor_OnStreamOffline;
                Monitor.OnChannelsSet += Monitor_OnChannelsSet;
                Monitor.OnServiceStarted += Monitor_OnServiceStarted;
                Monitor.OnServiceStopped += Monitor_OnServiceStopped;

                Monitor.SetChannelsByName(usernames.Distinct().ToList());
                
                // As in TwitchLibs dev branch ExpiresIn is always set to 0.
                // When dev gets merged into main check if ExpiresIn passed threshold and refresh token
                var validAccessTokenResponse = await API.Auth.ValidateAccessTokenAsync();
                
                if (validAccessTokenResponse != null) {
                    // Update live streams
                    await Monitor.UpdateLiveStreamersAsync();
                    
                    // Start the Monitor service
                    Monitor.Start();

                    // Start the clip listener
                    StartClipListener(config);

                    var enableHotload = true;

                    if (config["Settings"].ContainsKey("EnableHotload")) {
                        bool.TryParse(config["Settings"]["EnableHotload"], out enableHotload);
                    }

                    if (enableHotload) {
                        SetFileWatchers();
                    }
                } else {
                    Log.Error("The given credentials seem wrong! Please make sure the ClientID and token(s) are correct.");
                }

            } catch (Exception e) {
                Log.Error(e.Message);
            }

            await Task.Delay(-1);
        }

        /// <summary>
        /// Start the clip listener
        /// <param name="config">The configuration as dynamic</param>
        /// </summary>
        private async void StartClipListener(dynamic config) {
            Log.Info("Starting clip listener(s)");

            var onClipCreatedEvent = config["TwitchNotifier"]["OnClipCreated"];
            var channelClipMonitorUsers = new List<string>();

            // eventNode is the node for all settings below each event
            foreach (var eventNode in onClipCreatedEvent) {
                List<string> clipEventUsernameList = ((List<object>)eventNode.Value["Twitch"]["Usernames"]).Select(x => (string)x).ToList();
                channelClipMonitorUsers.AddRange(clipEventUsernameList);
            }

            var channelsToMonitorClips = await API.Helix.Users.GetUsersAsync(logins: channelClipMonitorUsers);

            Log.Debug("  > Channles:");
            
            foreach (var channel in channelsToMonitorClips.Users) {
                Log.Debug("    - " + channel.DisplayName);

                var clip = new Clip();
                clip.StartListeneingForClips(channel.Id);
            }
        }

        /// <summary>
        /// Dispose the Monitor nad / or API instance
        /// <param name="disposableInstance">The DisposableInstance which should be disposed</param>
        /// </summary>
        protected virtual void DisposeInstance(DisposableInstance disposableInstance) {
            if (!disposed) {
                // Called if the channels in the config file have been modified
                if (Monitor != null && (disposableInstance == DisposableInstance.Monitor || disposableInstance == DisposableInstance.All)) {
                    Monitor.ClearCache();
                    Monitor.Stop();
                    GC.SuppressFinalize(this);
                }

                // Called if the token has been changed
                if (API != null && (disposableInstance == DisposableInstance.API || disposableInstance == DisposableInstance.All)) {
                    API = null;
                    disposed = true;
                }

                disposed = true;
            }
        }

        /// <summary>
        /// The filewatcher to listen to changes of the config file
        /// </summary>
        public void SetFileWatchers() {
            fileSystemWatcher = new FileSystemWatcher() {
                Path = Config.configLocation,
                Filter = "config.yml",
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite
            };

            fileSystemWatcher.Changed += HotloadConfig;
            Log.Debug("FileSystemWatcher for the config file has been enabled");
        }

        /// <summary>
        /// Called if the config file has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotloadConfig(object sender, FileSystemEventArgs e) {
            var cachedConfig = MemoryCache.Default.Get(Cache.HashString(defaultConfigCacheKey));
            var needToDispose = false;
            var afterChange = new List<string>();
            var beforeChange = new List<string>();
            var configText = string.Empty;
            dynamic config = null;

            if (cachedConfig != null) {
                IEnumerable<string> configOld = ((CacheEntry)cachedConfig).Value.ToString().Split(Environment.NewLine).Distinct();

                try {
                    IEnumerable<string> configNew = Config.ReadConfigFile().Split(Environment.NewLine).Distinct();
                    beforeChange = configOld.Except(configNew).ToList();
                    afterChange = configNew.Except(configOld).ToList();
                } catch (Exception ex) {
                    Log.Error("Cannot read config file: " + ex.Message);
                }

                // If afterChange is in node "Usernames" or is value of "WebHookUrl", dispose Monitor instance
                var deserializer = new DeserializerBuilder().Build();
                configText = Config.ReadConfigFile();
                config = deserializer.Deserialize<dynamic>(configText);
                
                if (config == null) {
                    return;
                }
                
                var oldConfig = deserializer.Deserialize<dynamic>((((CacheEntry)cachedConfig).Value.ToString()));
                List<string> twitchChannelsAfterwards = GetAllTwitchUsernamesFromConfig(config);
                List<string> twitchChannelsBefore = GetAllTwitchUsernamesFromConfig(oldConfig);
                List<string> webhookUrlsAfterwards = new List<string>();
                List<string> webhookUrlsBefore = new List<string>();

                webhookUrlsAfterwards = GetWebhookProperties(config, "WebHookUrl");
                webhookUrlsBefore = GetWebhookProperties(oldConfig, "WebHookUrl");

                // Line removed from config file
                var twitchChannelsModified = beforeChange.Select(x => x.Trim()).Any(x => twitchChannelsBefore.Contains(x.Replace("- ", "")) && !twitchChannelsAfterwards.Contains(x.Replace("- ", "")));
                var webhooksModified = beforeChange.Select(x => x.Trim()).Any(x => webhookUrlsBefore.Contains(x.Replace("WebHookUrl: ", "")) && !webhookUrlsAfterwards.Contains(x.Replace("WebHookUrl: ", "")));
                needToDispose = twitchChannelsModified || webhooksModified;

                // Line added to config file
                if (!needToDispose) {
                    twitchChannelsModified = afterChange.Select(x => x.Trim()).Any(x => !twitchChannelsBefore.Contains(x.Replace("- ", "")) && twitchChannelsAfterwards.Contains(x.Replace("- ", "")));
                    webhooksModified = afterChange.Select(x => x.Trim()).Any(x => !webhookUrlsBefore.Contains(x.Replace("WebHookUrl: ", "")) && webhookUrlsAfterwards.Contains(x.Replace("WebHookUrl: ", "")));
                    needToDispose = twitchChannelsModified || webhooksModified;
                }
            }
            
            // CacheEntry to check if file has been saved multiple times
            var cacheEntry = new CacheEntry() {
                ExpirationTime = DateTime.Now.AddSeconds(2),
                Key = e.FullPath,
                Value = string.Empty
            };

            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                var changedDebugSetting = false;
                Log.Debug("Configuration file has been changed!");

                if (config["Settings"].ContainsKey("Debug")) {
                    bool.TryParse(config["Settings"]["Debug"], out changedDebugSetting);
                }

                var fileStreamText = Config.ReadConfigFile();
                var cachEntryList = new List<CacheEntry>() { 
                    // CacheEntry to update the debug status
                    new CacheEntry() {
                        Priority = CacheItemPriority.NotRemovable,
                        Key = Cache.debugConsole,
                        Value = changedDebugSetting
                    },

                    // CacheEntry to update the cached config
                    new CacheEntry() {
                        Priority = CacheItemPriority.NotRemovable,
                        Key = defaultConfigCacheKey,
                        Value = fileStreamText
                    }
                };

                foreach (var entry in cachEntryList) {
                    Cache.AddCacheEntry(entry);
                }

                if (needToDispose) {
                    Log.Debug("Disposing Monitor!");
                    DisposeInstance(DisposableInstance.Monitor);
                    new LiveMonitoring();
                    disposed = false;
                }
            } else {
                var cachedEntry = MemoryCache.Default.Get(cacheEntry.Key);
                
                try {
                    Log.Debug("Cooldown: " + (((CacheEntry)cachedEntry).ExpirationTime - DateTime.Now).TotalSeconds + " seconds");
                } catch {
                    Log.Debug("Cooldown is now over!");
                }
            }
        }

        /// <summary>
        /// Get all Twitch channels of the config file
        /// </summary>
        /// <param name="config">The deserialized config</param>
        /// <returns></returns>
        private List<string> GetAllTwitchUsernamesFromConfig(dynamic config) {
            var usernames = new List<string>();
            foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                // Events like "OnStreamStart", "OnStreamEnd", ...
                var twitchEvent = config["TwitchNotifier"][property.Name];

                // eventNode is the node for all settings below each event
                foreach (var eventNode in twitchEvent) {
                    List<string> usernameList = ((List<object>)eventNode.Value["Twitch"]["Usernames"]).Select(x => (string)x).ToList();
                    usernames.AddRange(usernameList);
                }
            }

            return usernames;
        }

        /// <summary>
        /// Get property of config file
        /// </summary>
        /// <returns><c>List&lt;string&gt;</c> containing the property of the config</returns>
        private List<string> GetWebhookProperties(dynamic config, dynamic configProperty) {
            var returnValue = new List<string>();
            
            foreach (var property in typeof(TwitchNotifierSettings).GetProperties()) {
                // Events like "OnStreamStart", "OnStreamEnd", ...
                var twitchEvent = config["TwitchNotifier"][property.Name];

                // eventNode is the node for all settings below each event
                foreach (var eventNode in twitchEvent) {
                    returnValue.Add((string)eventNode.Value[configProperty]);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Send the embed over the specified URL
        /// </summary>
        /// <param name="placeholderHelper">The PlaceholderHelper to replace placeholders for the embed</param>
        /// <param name="channels">The Dictionary to loop through all Twitch channels</param>
        internal static void SendEmbed(PlaceholderHelper placeholderHelper, Dictionary<string, object> channels) {
            if (channels.Count > 0) {
                foreach (var eventObject in channels) {
                    var condition = string.Empty;

                    try {
                        condition = new Placeholders.Placeholder().ReplacePlaceholders((string)((dynamic)eventObject.Value)["Condition"], placeholderHelper);
                    } catch {
                        Log.Warn("Node \"Condition\" could not be found on eventnode \"" + eventObject.Key + "\"... Defaulted to empty condition!");
                    }

                    //if (Parser.CheckEventCondition(condition)) {
                    if (Parser.GetBooleanOfParenthesesCondition(condition)) {
                        var embed = Parser.Deserialize(typeof(DiscordEmbed), ((dynamic)eventObject.Value)["Discord"], placeholderHelper);
                        var embedValidation = new EmbedValidation();
                        var result = embedValidation.ValidateEmbed((DiscordEmbed)embed);

                        if (result.Success) {
                            Log.Debug("+ Embed validation succeeded!");
                        } else {
                            Log.Warn("x Embed validation failed!");
                            Log.Warn(result.ErrorMessage);
                        }

                        new WebRequest() {
                            webHookUrl = ((dynamic)eventObject.Value)["WebHookUrl"],
                            discordEmbed = result.Embed
                        }.SendRequest();
                    } else {
                        Log.Debug("Condition returned false therefore not sending the notification!");
                        Log.Debug("   Condition used: " + (string)((dynamic)eventObject.Value)["Condition"]);
                        Log.Debug("   Condition replaced: " + condition);
                    }
                }
            }
        }

        #region Events
        /// <summary>
        /// Called when SetChannels() is being called
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e) {
            var cachedSkipStartupNotifications = (CacheEntry)MemoryCache.Default.Get(Cache.HashString(defaultSkipStartupNotifications));
            var skipInitialNotifications = true;
            bool.TryParse(cachedSkipStartupNotifications.Value.ToString(), out skipInitialNotifications);
            sendNotifications = !skipInitialNotifications;

            Log.Info("Channel list has been set!");
            Log.Debug("  > Channles: ");
            e.Channels.ForEach(x => Log.Debug("    - " + x));
        }

        /// <summary>
        /// Called when the LiveStreamMonitorService has started
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e) {
            sendNotifications = true;
            Log.Info("Service has started!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private void Monitor_OnServiceStopped(object sender, OnServiceStoppedArgs e) {
            sendNotifications = false;
            Log.Debug("Service has stopped!");
        }

        /// <summary>
        /// Called when stream went live
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private async void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e) {
            if (sendNotifications) {
                var configEventName = e.GetType().Name.Replace("args", "", StringComparison.OrdinalIgnoreCase);
                Log.Info(e.Channel + " went online!");

                var cacheEntry = new CacheEntry() {
                    Key = e.Stream.UserId,
                    Value = configEventName
                };

                var notificationThresholdInSeconds = (CacheEntry)MemoryCache.Default.Get(Cache.HashString(defaultNotificationThresholdInSeconds));
                int.TryParse(notificationThresholdInSeconds?.Value.ToString(), out int notificationThresholdInSecondsParsed);

                if (notificationThresholdInSeconds != null && (notificationThresholdInSecondsParsed) > -1) {
                    cacheEntry.ExpirationTime = DateTime.Now.AddSeconds(notificationThresholdInSecondsParsed);
                } else {
                    cacheEntry.ExpirationTime = DateTime.Now.AddMinutes(2);
                }

                if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                    var channels = Config.GetEventObjectsByTwitchChannelName(configEventName, e.Channel);
                    var placeholderHelper = new PlaceholderHelper() {
                        Stream = e.Stream,
                        Channel = new PlaceHolderChannelHelper() {
                            Name = e.Channel,
                            User = await API.V5.Channels.GetChannelByIDAsync(e.Stream.UserId)
                        }
                    };

                    SendEmbed(placeholderHelper, channels);
                } else {
                    var cachedChannelEntry = (CacheEntry)MemoryCache.Default.Get(Cache.HashString(e.Stream.UserId));
                    string cooldownMessage = "Event \"" + configEventName + "\" triggered multiple times... Still in cooldown!";

                    if (cachedChannelEntry != null) {
                        cooldownMessage += " (" + (cachedChannelEntry.ExpirationTime - DateTime.Now).TotalSeconds + " seconds)";
                    }

                    Log.Debug(cooldownMessage);
                }
            } else {
                Log.Debug(e.Channel + " is live right now but startup has not finished yet! " + "(\"" + defaultSkipStartupNotifications + "\" = " + !sendNotifications + ")");
            }
        }

        /// <summary>
        /// Called when stream went offlne<br/>
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The event args</param>
        private async void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e) {
            var configEventName = e.GetType().Name.Replace("args", "", StringComparison.OrdinalIgnoreCase);
            Log.Info(e.Channel + " went offline!");

            var cacheEntry = new CacheEntry() {
                Key = e.Stream.UserId,
                Value = configEventName
            };

            var notificationThresholdInSeconds = (CacheEntry)MemoryCache.Default.Get(Cache.HashString(defaultNotificationThresholdInSeconds));
            int.TryParse(notificationThresholdInSeconds?.Value.ToString(), out int notificationThresholdInSecondsParsed);

            if (notificationThresholdInSeconds != null && (notificationThresholdInSecondsParsed) > -1) {
                cacheEntry.ExpirationTime = DateTime.Now.AddSeconds(notificationThresholdInSecondsParsed);
            } else {
                cacheEntry.ExpirationTime = DateTime.Now.AddMinutes(2);
            }

            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                var channels = Config.GetEventObjectsByTwitchChannelName(configEventName, e.Channel);
                var placeholderHelper = new PlaceholderHelper() {
                    Stream = e.Stream,
                    Channel = new PlaceHolderChannelHelper() {
                        Name = e.Channel,
                        User = await API.V5.Channels.GetChannelByIDAsync(e.Stream.UserId)
                    }
                };

                SendEmbed(placeholderHelper, channels);
            } else {
                string cooldownMessage = "Event \"" + configEventName + "\" triggered multiple times... Still in cooldown!";

                if (cacheEntry != null) {
                    cooldownMessage += " (" + (cacheEntry.ExpirationTime - DateTime.Now).TotalSeconds + " seconds)";
                }

                Log.Debug(cooldownMessage);
            }
        }
        #endregion
    }
}