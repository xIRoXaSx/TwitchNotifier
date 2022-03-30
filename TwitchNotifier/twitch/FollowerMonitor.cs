#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchNotifier.models;
using TwitchNotifier.placeholders;

namespace TwitchNotifier.twitch; 

internal class FollowerMonitor {
    private readonly FollowerService? _followerService;
    private readonly CancellationTokenSource _cancelSource;
    private readonly int _followerCacheExpiration = 3600;
    
    internal FollowerMonitor(ITwitchAPI twitchApi) {
        _cancelSource = new CancellationTokenSource();
        _followerService = new FollowerService(
            twitchApi, Program.Conf.GeneralSettings.FollowerCheckIntervalInSeconds
        );

        _followerService.OnServiceStarted += OnServiceStated;
        _followerService.OnServiceStopped += OnServiceStopped;
        _followerService.OnServiceStopped += OnServiceStopped;
        _followerService.OnChannelsSet += OnChannelsSet;
        _followerService.OnNewFollowersDetected += OnNewFollowersDetected;
    }

    /// <summary>
    /// Starts the follower monitoring.
    /// </summary>
    internal async void Start() {
        if (_followerService == null)
            return;
            
        var channelList = Program.Conf.GetFollowMonitoredChannels().Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
        if (channelList.Count < 1) {
            Logging.Info("No channels set to monitor follow activities.");
            return;
        }
        
        // Set channels of interest.
        _followerService.SetChannelsByName(channelList);
        await _followerService.UpdateLatestFollowersAsync(!Program.Conf.GeneralSettings.SkipNotificationsOnStartup);
            
        // Start the monitor.
        _followerService.Start();

        // Keep alive.
        try {
            await Task.Delay(-1, _cancelSource.Token);
        } catch (TaskCanceledException) {
            // Ignore cancelled tasks.
        } catch (Exception ex) {
            Logging.Debug($"FollowerMonitor caught an exception: {ex.Message}");
        }
            
        _cancelSource.Dispose();
    }
    
    
    /// <summary>
    /// Stop the follower service.
    /// </summary>
    internal void Stop() {
        _followerService?.ClearCache();
        _cancelSource.Cancel();
        if (_followerService?.Enabled ?? false)
            _followerService?.Stop();
    }
    
    private static void OnServiceStated(object? sender, OnServiceStartedArgs e) {
        Logging.Info("Follower monitoring service started.");
    }

    private static void OnServiceStopped(object? sender, OnServiceStoppedArgs e) {
        Logging.Info("Follower monitoring service stopped.");
    }

    private static void OnChannelsSet(object? sender, OnChannelsSetArgs e) {
        Logging.Info("Channels to monitor for follows have been set.");
        Logging.Debug($"\t> Channel(s) to monitor: {string.Join(", ", e.Channels)}");
    }
    
    private async void OnNewFollowersDetected(object? sender, OnNewFollowersDetectedArgs e) {
        var users = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(logins: new List<string>{e.Channel});
        var streamer = users.Users.Length > 0 ? users.Users[0] : null;
        if (streamer == null)
            return;
        var knownFollower = _followerService == null || !_followerService.KnownFollowers.ContainsKey(e.Channel)
            ? new List<Follow>()
            : _followerService.KnownFollowers[e.Channel];
        if (knownFollower.Count < 1)
            return;
        
        Logging.Debug($"{e.Channel} got new follower(s)!");
        foreach (var follower in e.NewFollowers) {
            if (follower == null)
                continue;
            
            // Dual layer cache.
            // First layer:  MemoryCache will hold item for the defined time in local memory.
            //               If first layer does not hold the follower, check the KnownFollowers.
            // Second layer: The follower service's KnownFollowers will hold the past view followers.
            var entry = new CacheEntry {
                Key = $"TN_F_{e.Channel}_{follower.FromUserName}",
                ExpirationTime = DateTime.Now.AddSeconds(_followerCacheExpiration)
            }.HashKey();
                
            // First layer cache.
            // Check if the same user had already followed the current streamer.
            if (!Cache.IsCacheEntryExpired(entry)) {
                var eventName = e.GetType().Name.Replace("args", "", StringComparison.OrdinalIgnoreCase);
                Logging.Debug($"{eventName} triggered while still in cooldown...");
                return;
            }

            Cache.AddEntry(entry);
            
            // Second layer cache.
            // Check if user has followed the channel before while the first layer cache wasn't aware of it.
            Follow? known = null;
            foreach (var f in knownFollower) {
                if (f.FromUserName == follower.FromUserName)
                    known = f;
            }

            // Since user is directly exposed to KnownFollowers after following,
            // check the time stamp.
            var check = Program.Conf.GeneralSettings.FollowerCheckIntervalInSeconds;
            if (known != null && DateTime.Now.AddSeconds(-check - check/2).ToUniversalTime() > known.FollowedAt.ToUniversalTime())
                continue;
            
            // Get the first embed which contains the channel.
            var notification = Program.Conf.NotificationSettings.OnFollowEvent
                .FirstOrDefault(x => x.Channels.Select(y => y.ToLower()).Any(y=> y == e.Channel.ToLower()));
                
            // Check if notification is null or invalid.
            if (notification == null)
                return;
            notification.Embed = notification.Embed.Validate();
            if (notification.Embed == null) {
                Logging.Error("Embed validation returned null!");
                return;
            }
            
            // Get User object of streamer for placeholders.
            users = await Program.TwitchCore.TwitchApi.Helix.Users.GetUsersAsync(new List<string>{follower.FromUserId});
            var user = users.Users.Length > 0 ? users.Users[0] : null;
            var placeholder = new TwitchPlaceholder {
                Channel = new ChannelPlaceholder(streamer, e.Channel),
                Follower = new FollowerPlaceholder(user, follower.FollowedAt)
            };

            var cond = new Condition(new Placeholder(notification.Condition, placeholder).Replace());
            if (!cond.Evaluate()) {
                Logging.Debug("Notification withheld, condition evaluation returned false");
                return;
            }
            
            var json = notification.Embed.ToJson(placeholder);
            await new Request(notification.WebHookUrl, json).SendAsync();
        }
    }
}