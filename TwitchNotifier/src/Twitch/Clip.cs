using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Helper;
using TwitchNotifier.src.Logging;
using YamlDotNet.Serialization;

namespace TwitchNotifier.src.Twitch {
    class Clip {
        private TwitchAPI API;

        /// <summary>
        /// Will start monitoring clips of a channel<br/>
        /// It can take up to 10 minutes until the clip is available for the API!
        /// </summary>
        /// <param name="channelId">The channel ID to monitor clips for</param>
        internal async void StartListeneingForClips(string channelId) {
            var cancelSource = new CancellationTokenSource();
            var cancelToken = cancelSource.Token;
            var sendNotifications = false;

            await Task.Run(async () => {
                try {
                    GetClipsResponse recentClips = null;
                    TwitchLib.Api.Helix.Models.Clips.GetClips.Clip latestClip = null;

                    var deserializer = new DeserializerBuilder().Build();
                    var config = deserializer.Deserialize<dynamic>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));

                    API = new TwitchAPI();
                    API.Settings.ClientId = config["Settings"]["ClientID"];
                    API.Settings.AccessToken = config["Settings"]["AccessToken"];
                
                    if (await API.Auth.ValidateAccessTokenAsync() != null) {
                        while (!cancelToken.IsCancellationRequested) {
                            try {
                                recentClips = await API.Helix.Clips.GetClipsAsync(
                                    broadcasterId: channelId, 
                                    startedAt: DateTime.Now.AddSeconds(-21),
                                    endedAt: DateTime.Now
                                );

                                if (recentClips.Clips.Length > 0 && sendNotifications) {
                                    // Check if the newest clip does not have the same URL as the last clip
                                    if ((recentClips.Clips.OrderByDescending(x => x?.CreatedAt).ToList().FirstOrDefault() != latestClip)) {
                                        Log.Debug("Found new clip(s)!");

                                        foreach (TwitchLib.Api.Helix.Models.Clips.GetClips.Clip recentClip in recentClips.Clips) {
                                            // Create and send embed
                                            SendEmbeddedClip(recentClip);
                                        }

                                        latestClip = recentClips.Clips.OrderByDescending(x => x.CreatedAt).ToList().FirstOrDefault();
                                    }
                                }

                                sendNotifications = true;
                                await Task.Delay(10 * 1000);
                            } catch (Exception ex) {
                                Log.Error("Inner Exception: " + ex.Message);
                            }
                        }
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// Send the embed to the desired Webhook on Discord
        /// </summary>
        /// <param name="clip">The clip to send as an embed</param>
        internal async void SendEmbeddedClip(TwitchLib.Api.Helix.Models.Clips.GetClips.Clip clip) {
            var configEventName = "OnClipCreated";
            var cacheEntry = new CacheEntry() {
                Key = clip.Url,
                Value = string.Empty,
                ExpirationTime = DateTime.Now.AddHours(12)
            };
            
            if (!Cache.CheckCacheEntryExpiration(cacheEntry)) {
                var channel = await API.V5.Channels.GetChannelByIDAsync(clip.BroadcasterId);
                var channels = Config.GetEventObjectsByTwitchChannelName(configEventName, channel.Name);
                var placeholderHelper = new PlaceholderHelper() {
                    Channel = new PlaceHolderChannelHelper() {
                        Name = channel.Name,
                        User = channel
                    },
                    Clip = new PlaceHolderClipHelper(clip, await API.V5.Channels.GetChannelByIDAsync(clip.CreatorId))
                };

                LiveMonitoring.SendEmbed(placeholderHelper, channels);
            } else {
                var cachedChannelEntry = (CacheEntry)MemoryCache.Default.Get(Cache.HashString(clip.Url));
                Log.Debug("Event \"" + configEventName + "\" triggered multiple times! Still in cooldown!");

                if (cachedChannelEntry != null) {
                    Log.Debug("Cooldown: " + (cachedChannelEntry.ExpirationTime - DateTime.Now).TotalSeconds + " seconds");
                }
            }
        }
    }
}
