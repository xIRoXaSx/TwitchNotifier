using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Clips.GetClips;
using TwitchNotifier.src.config;
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
        internal async void StartPollingForClips(string channelId) {
            var cancelSource = new CancellationTokenSource();
            var cancelToken = cancelSource.Token;

            await new Task(async () => {
                GetClipsResponse initialClips = null;
                GetClipsResponse recentClips = null;
                TwitchLib.Api.Helix.Models.Clips.GetClips.Clip latestClip = null;

                var deserializer = new DeserializerBuilder().Build();
                var config = deserializer.Deserialize<dynamic>(File.ReadAllText(Config.configFileLocation, Encoding.UTF8));
                var cancelTask = false;
                var callIsInitial = true;

                API = new TwitchAPI();
                API.Settings.ClientId = config["Settings"]["ClientID"];
                API.Settings.AccessToken = config["Settings"]["AccessToken"];
                
                if (await API.Auth.ValidateAccessTokenAsync() != null) {
                    // Get initial clips (max 1 day back) to sort and use its CreatedAt property
                    initialClips = await API.Helix.Clips.GetClipsAsync(broadcasterId: channelId, startedAt: DateTime.Now.AddDays(-1), endedAt: DateTime.Now);
                    latestClip = initialClips.Clips.OrderByDescending(x => x.CreatedAt).ToList().FirstOrDefault();
                    
                    GC.KeepAlive(latestClip);

                    while (!cancelToken.IsCancellationRequested && !cancelTask) {
                        recentClips = await API.Helix.Clips.GetClipsAsync(
                            broadcasterId: channelId, 
                            startedAt: Convert.ToDateTime(latestClip?.CreatedAt ?? DateTime.Now.AddDays(-1).ToString()), 
                            endedAt: DateTime.Now
                        );

                        if (recentClips.Clips.Length > 0) {
                            // Check if the newest clip does not have the same URL as the last clip
                            if ((recentClips.Clips.OrderByDescending(x => x.CreatedAt).ToList().FirstOrDefault().Url != latestClip?.Url) || callIsInitial) {
                                Log.Debug("Found new clip(s)!");

                                foreach (var recentClip in recentClips.Clips) {
                                    // Create and send embed
                                }

                                callIsInitial = false;
                            }

                            latestClip = recentClips.Clips.OrderByDescending(x => x.CreatedAt).ToList().FirstOrDefault();
                        }

                        await Task.Delay(10 * 1000);
                    }
                }
            });
        }
    }
}
