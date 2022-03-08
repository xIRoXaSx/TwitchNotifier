using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TwitchNotifier.placeholders; 

public class Placeholder {
    internal const string TwitchBaseUrl = "https://www.twitch.tv/";
    private const char PlaceholderEnclosure = '%';
    private readonly string _source;
    private readonly TwitchPlaceholder _placeholder;

    internal Placeholder(string value, TwitchPlaceholder placeholder) {
        _source = value;
        _placeholder = placeholder;
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

        // Keep the compiler happy until finished :)
        return "";
    }
}
