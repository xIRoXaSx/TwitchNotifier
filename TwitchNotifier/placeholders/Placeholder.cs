using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TwitchNotifier.placeholders; 

public class Placeholder {
    internal const string TwitchBaseUrl = "https://www.twitch.tv/";
    private const char PlaceholderEnclosure = '%';
    private readonly string _source;
    
    [JsonProperty]
    private readonly TwitchPlaceholder _placeholderObj;

    internal Placeholder(string value, TwitchPlaceholder placeholderObj) {
        _source = value;
        _placeholderObj = placeholderObj;
    }
    
    /// <summary>
    /// Replaces every placeholder inside the provided string to their corresponding value.<br/>
    /// If a placeholder is not available or recognized, it will be set to an empty string.
    /// </summary>
    /// <returns><c>String</c> - The replaced string</returns>
    internal string Replace() {
        if (string.IsNullOrEmpty(_source))
            return _source;
        
        var serializerSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        // Convert placeholder object to a json string and match it via regular expression.
        var serialized = JsonConvert.SerializeObject(_placeholderObj, serializerSettings);
        var jObj = JObject.Parse(serialized);
        var matches = Regex.Matches(_source, $"{PlaceholderEnclosure}(.*?){PlaceholderEnclosure}", RegexOptions.IgnoreCase);
        var returnValue = _source;
        List<string> matched = new();
        
        for (var i = 0; i < matches.Count; i++) {
            var match = matches[i];
            if (matched.Contains(matches[i].Value.ToLower()))
                continue;
            
            matched.Add(match.Value.ToLower());
            var tokenValue = match.Groups[1].Value;
            var token = jObj.SelectToken(tokenValue.ToLower(), false) ?? 
                        jObj.SelectToken(tokenValue.ToSnakeCase(), false);
            
            // No corresponding token found.
            // Replace the placeholder with an empty string.
            if (token == null) {
                returnValue = returnValue.Replace(match.Groups[0].Value, "");
                continue;
            }
            
            // If token value is equal to the stream thumbnail url, replace the width and height placeholders from the API.
            tokenValue = tokenValue.ToLower() != "stream.thumbnailurl"
                ? token.ToString()
                : token.ToString().Replace("{width}", "1280").Replace("{height}", "720") +
                  $"?guid={Guid.NewGuid().ToString()}";
            returnValue = returnValue.Replace(match.Groups[0].Value, tokenValue);
        }
        
        // Use zero width space for the Discord embed line break translation.
        return returnValue.Replace(@"\\n", "\u200B");
    }
}
