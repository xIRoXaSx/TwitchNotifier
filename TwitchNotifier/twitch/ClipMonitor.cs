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
    private IEnumerable<string> _channelIds;
    private List<string> _channelNames;
    private readonly List<Task> _clipListenerTasks = new();
    private const int TaskDelay = 10;

    internal async Task UpdateClipChannelsAsync() {
        if (_channelNames == null || _channelNames.Count < 1)
            return;
        
        Logging.Info("Channels to monitor for clips have been set.");
        Logging.Debug($"\t> Channel(s) to monitor: {string.Join(", ", _channelNames)}");
        var channelIds = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(logins: _channelNames);
        _channelNames = null;
        if (channelIds == null)
            return;
        
        var tmpIds = new string[channelIds.Users.Length];
        for (var i = 0; i < channelIds.Users.Length; i++) {
            tmpIds[i] = channelIds.Users[i].Id;
        }
        _channelIds = tmpIds;
    }

    internal ClipMonitor() {
        var channels = new List<string>();
        foreach (var clipEvent in Program.Conf.NotificationSettings.OnClipCreated) {
            foreach (var chan in clipEvent.Channels) {
                if (!channels.Contains(chan))
                    channels.Add(chan);
            }
        }
        _channelNames = channels;
    }

    internal async void Start() {
        if (_channelIds == null) {
            Logging.Error("Cannot start ClipMonitor, channels have not been set!");
            return;
        }
        
        foreach (var channelId in _channelIds) {
            _clipListenerTasks.Add(Task.Run(async () => {
                while (!_cancelSource.Token.IsCancellationRequested) {
                    try {
                        if (!Program.TwitchCore.IsValid)
                            return;

                        var recent = await Program.TwitchCore.TwitchApi.Helix.Clips.GetClipsAsync(
                            broadcasterId: channelId,
                            startedAt: DateTime.Now.AddSeconds(-TaskDelay-1),
                            endedAt: DateTime.Now
                        );

                        if (recent.Clips.Length < 1) {
                            await Task.Delay(TaskDelay * 1000, _cancelSource.Token);
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
                            // Streamer needs to be requested for each clip to ensure that the placeholders are up-to-date.
                            var clipper = await Program.TwitchCore.GetUser(new List<string> {clip.CreatorId}, false);
                            var streamer = await Program.TwitchCore.GetUser(new List<string> {channelId}, false);
                            var placeholder = new TwitchPlaceholder {
                                Channel = new ChannelPlaceholder(streamer, clip.BroadcasterName),
                                Clip = new ClipPlaceholder(clip, clipper)
                            };

                            // Get the first embed which contains the channel.
                            var notification = Program.Conf.NotificationSettings.OnClipCreated.GetFirstMatchOrNull(clip.BroadcasterName);
                            if (notification == null)
                                return;
                            
                            var cond = new Condition(new Placeholder(notification.Condition, placeholder).Replace());
                            if (!cond.Evaluate()) {
                                Logging.Debug("Notification withheld, condition evaluation returned false");
                                return;
                            }
                            
                            // Send clip embed and add it to the cache.
                            var json = notification.Embed.ToJson(placeholder);
                            await Task.Run(() => new Request(notification.WebHookUrl, json).SendAsync());
                            Cache.AddEntry(entry);
                        }
                        
                    } catch (Exception ex) {
                        Logging.Error($"ClipMonitor: caught an exception: {ex.Message}");
                    }
                    await Task.Delay(TaskDelay * 1000, _cancelSource.Token);
                }
            }, _cancelSource.Token));
        }

        try {
            await Task.WhenAll(_clipListenerTasks);
        } catch (TaskCanceledException) {
        } catch (Exception ex) {
            Logging.Error($"ClipMonitor: caught an exception: {ex.Message}");
        }
        
        DisposeSource();
        Logging.Info("Clip monitoring service has been stopped");
    }

    /// <summary>
    /// Wrapper to make code analysis happy.
    /// </summary>
    private void DisposeSource() {
        _cancelSource?.Dispose();
    }
    
    internal void Stop() {
        _cancelSource.Cancel();
        foreach (var clipListener in _clipListenerTasks) {
            clipListener.Dispose();
        }
    }
}