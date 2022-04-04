using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TwitchNotifier.twitch;

namespace TwitchNotifier;

internal class Program {
    internal static readonly string BinaryName = Assembly.GetExecutingAssembly().GetName().Name;
    internal static Config Conf { get; }
    internal static Core TwitchCore { get; private set; }
    private CancellationTokenSource _cancelSource = new();

    static Program() {
        Conf = new Config();
        Conf.CreateIfNotExisting();
        Conf.Load();
        if (Conf.GeneralSettings.EnableHotLoad)
            Conf.SetFileWatcher();
    }
        
    private static void Main(string[] args) {
        Task.Run(() => new Program().MainAsync()).GetAwaiter().GetResult();
    }

    private async Task MainAsync() {
        // Create a new TwitchCore instance and validate the data for it.
        TwitchCore = new Core();
        await TwitchCore.ValidateOrThrowAsync();
        void RunDispose(object sender, EventArgs args) {
            TwitchCoreOnDisposeRequested();
        }
            
        TwitchCore.DisposeRequested += RunDispose;
            
        // Start the stream, follower and clip monitor.
        TwitchCore.StreamMonitor.Start();
        TwitchCore.FollowerMonitor.Start();
        await TwitchCore.ClipMonitor.UpdateClipChannelsAsync();
        TwitchCore.ClipMonitor.Start();

        // Keep alive.
        Logging.Info("Setup finished.");
        try {
            await Task.Delay(-1, _cancelSource.Token);
        } catch (TaskCanceledException) {
            // Ignore cancelled tasks.
        }
        _cancelSource.Dispose();
        _cancelSource = new CancellationTokenSource();
        TwitchCore.DisposeRequested -= RunDispose;
        await MainAsync();
    }

    /// <summary>
    /// Stops and disposes all instances related to the core and creates a new instance of it.
    /// </summary>
    private void TwitchCoreOnDisposeRequested() {
        _cancelSource.Cancel();
        _cancelSource.Dispose();
        _cancelSource = new CancellationTokenSource();
        TwitchCore.StreamMonitor.Stop();
        TwitchCore.FollowerMonitor.Stop();
        TwitchCore.ClipMonitor.Stop();
    }
}