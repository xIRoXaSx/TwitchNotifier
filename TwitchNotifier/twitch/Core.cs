#nullable enable
using System;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;

namespace TwitchNotifier.twitch {
    internal class Core {
        internal bool IsValid { get; private set; }
        internal TwitchAPI TwitchApi { get; }
        internal StreamMonitor StreamMonitor { get; }
        internal ClipMonitor ClipMonitor { get; }
        internal event EventHandler? DisposeRequested;

        /// <summary>
        /// The core to handle connections to Twitch's API.
        /// </summary>
        internal Core() {
            var config = Program.Conf;
            TwitchApi = new TwitchAPI {
                Settings = {
                    ClientId = config.GeneralSettings.ClientId,
                    AccessToken = config.GeneralSettings.AccessToken
                }
            };

            StreamMonitor = new StreamMonitor(TwitchApi);
            ClipMonitor = new ClipMonitor();
        }

        /// <summary>
        /// Validates the provided credentials and throws an <c>InvalidCredentialException</c> if
        /// the given information is incorrect.
        /// </summary>
        /// <exception cref="InvalidCredentialException">Credentials are invalid.</exception>
        internal async Task ValidateOrThrowAsync() {
            if (await TwitchApi.Auth.ValidateAccessTokenAsync() == null)
                throw new InvalidCredentialException("Provided credentials are invalid.");
            IsValid = true;
        }

        /// <summary>
        /// Dispose the Core instance.
        /// </summary>
        public void Dispose() {
            DisposeRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}