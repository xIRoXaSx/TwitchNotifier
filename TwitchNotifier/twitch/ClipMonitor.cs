using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchNotifier.models;
using TwitchNotifier.placeholders;

namespace TwitchNotifier.twitch; 

internal class ClipMonitor {
    private readonly CancellationTokenSource _cancelSource = new();
    private bool notificationsActive = true;
    private readonly IEnumerable<string> _channelIds;
    private readonly List<Task> _clipListenerTasks = new();
    private readonly int _taskDelay = 10;
        
    public ClipMonitor(IEnumerable<string> channelIds) {
        _channelIds = channelIds;
    }
        
    internal async void Start() {
        foreach (var channelId in _channelIds) {
            _clipListenerTasks.Add(Task.Run(() => {
                Task.Run(async () => {
                    // Loop until cancellation has been requested.
                    while (!_cancelSource.Token.IsCancellationRequested) {
                        try {
                            if (!Program.TwitchCore.IsValid)
                                return;

                            var recent = await Program.TwitchCore.TwitchApi.Helix.Clips.GetClipsAsync(
                                broadcasterId: channelId,
                                startedAt: DateTime.Now.AddSeconds(-_taskDelay-1),
                                endedAt: DateTime.Now
                            );

                            if (recent.Clips.Length < 1 || !notificationsActive) {
                                await Task.Delay(_taskDelay * 1000, _cancelSource.Token);
                                continue;
                            }

                            // Check if there are any new clips.
                            var recentClips = recent.Clips.OrderByDescending(x => x.CreatedAt).ToList();
                            foreach (var clip in recentClips) {
                                // Create a new cache entry to check if it's already cached.
                                // Use "clip" prefix to ensure that clip and channel IDs aren't overlapping.
                                var entry = new CacheEntry {
                                    Key = $"clip_{clip.Id}",
                                    ExpirationTime = DateTime.Now.AddMinutes(5)
                                }.HashKey();
                                    
                                // Continue if clip ID is still cached.
                                if (!Cache.IsCacheEntryExpired(entry))
                                    continue;
                                
                                Logging.Debug($"Found a new clip @ channel {clip.BroadcasterName}");
                                
                                // Get the clip creator user.
                                var users = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(
                                    new List<string>{clip.BroadcasterId, clip.CreatorId}
                                );
                                var streamer = users.Users.Length > 0 ? users.Users[0] : null;
                                var clipper = users.Users.Length > 1 ? users.Users[1] : null;
                                var placeholder = new TwitchPlaceholder {
                                    Channel = new ChannelPlaceholder(streamer, clip.BroadcasterName),
                                    Clip = new ClipPlaceholder(clip, clipper)
                                };

                                // Get the first embed which contains the channel.
                                var notification = Program.Conf.NotificationSettings.OnClipCreated
                                    .FirstOrDefault(x => x.Channels.Select(y => y.ToLower()).Any(y=> y == clip.BroadcasterName.ToLower()));
                                
                                // Send clip embed and add it to the cache.
                                var json = notification.Embed.ToJson(placeholder);
                                await Task.Run(() => new Request(notification.WebHookUrl, json).SendAsync());
                                Cache.AddEntry(entry);
                            }
                            
                        } catch (Exception ex) {
                            Logging.Error($"ClipMonitor caught an exception: {ex.Message}");
                        }
                        
                        await Task.Delay(_taskDelay * 1000, _cancelSource.Token);
                    }
                }, _cancelSource.Token);
            }, _cancelSource.Token));
        }
            
        await Task.WhenAll(_clipListenerTasks);
    }
}