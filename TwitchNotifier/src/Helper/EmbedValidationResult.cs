using System.Collections.Generic;
using TwitchNotifier.src.config;

namespace TwitchNotifier.src.Helper {
    class EmbedValidationResult {
        /// <summary>
        /// The embed if validation was successfull else null
        /// </summary>
        public DiscordEmbed Embed { get; set; }

        /// <summary>
        /// Either <c>true</c> if the validation was successful or <c>false</c> if an error occurred
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The error message if validation was unsuccessful
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
