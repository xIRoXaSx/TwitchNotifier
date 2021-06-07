using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwitchNotifier.src.Helper;

namespace TwitchNotifier.src.Placeholders {
    class Placeholder {

        char placeholderEnclosure { get; } = '%';


        /// <summary>
        /// Replace placeholders with their actual value<br/>
        /// PlaceholderHelper gets serialized to a json string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacementObject"></param>
        /// <returns></returns>
        public string ReplacePlaceholders(string text, PlaceholderHelper replacementObject) {
            var returnValue = text;
            var jsonSerializerSettings = new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            var json = JsonConvert.SerializeObject(replacementObject, jsonSerializerSettings);
            var replacement = JObject.Parse(json);
            var matches = Regex.Matches(text, placeholderEnclosure + @"(.*?)" + placeholderEnclosure);
            List<Match> matchesDistinct = new List<Match>();

            // Create distinct List of Matches
            foreach (Match match in matches) {
                if (!matchesDistinct.Select(x => x.Groups[0].Value).Any(y => match.Groups[0].Value == y)) {
                    matchesDistinct.Add(match);
                }
            }

            // Replace each match
            foreach (Match match in matchesDistinct) {
                var selectedToken = replacement.SelectToken(match.Groups[1].Value.ToLower(), false) ?? replacement.SelectToken(match.Groups[1].Value.ToSnakeCase(), false);
                if (selectedToken != null) {
                    // Add more options to Stream.ThumbnailUrl in the future (eg.: change size)
                    selectedToken = match.Groups[1].Value.ToLower() == "stream.thumbnailurl" ? selectedToken.ToString().Replace("{width}", "1280").Replace("{height}", "720") : selectedToken;

                    // Check for matched placeholders which are empty / null (eg. unset game names)
                    if (string.IsNullOrEmpty(selectedToken.ToString())) {
                        returnValue = returnValue.Replace(match.Groups[0].Value, "-");
                    } else {
                        returnValue = returnValue.Replace(match.Groups[0].Value, selectedToken.ToString().Replace("\'", "\'\'"));
                    }
                } else {
                    returnValue = returnValue.Replace(match.Groups[0].Value, "-");
                }
            }

            returnValue = returnValue.Replace(@"\n", "\u200B");
            return returnValue;
        }
    }
}
