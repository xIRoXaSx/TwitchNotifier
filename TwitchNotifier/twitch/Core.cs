using TwitchLib.Api;

namespace TwitchNotifier.twitch {
    internal class Core {
        private TwitchAPI _twitchApi;

        internal Core() {
            var config = Program.Conf;
            _twitchApi = new TwitchAPI {
                Settings = {
                    ClientId = config.GeneralSettings.ClientId,
                    AccessToken = config.GeneralSettings.AccessToken
                }
            };
        }
    }
}