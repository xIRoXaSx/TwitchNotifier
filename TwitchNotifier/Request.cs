using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TwitchNotifier.models;
using TwitchNotifier.placeholders;

namespace TwitchNotifier; 

internal class Request {
    private string Url { get; }
    private Embed Embed { get; }
    private TwitchPlaceholder Placeholder { get; }

    private readonly CancellationTokenSource _cancelSource;
    private static readonly string[] Fields = { "avatar_url", "username", "content" };

    internal Request(string url, Embed embed, TwitchPlaceholder placeholder) {
        Url = url;
        Embed = embed;
        Placeholder = placeholder;
        _cancelSource = new CancellationTokenSource(3000);
    }

    /// <summary>
    /// Send the message via WebHook.
    /// </summary>
    /// <returns><c>Task&lt;Bool&gt;</c><br/>
    /// <c>True</c> - If the request has been successfully sent.<br/>
    /// <c>False</c> - If the request threw an exception or was unsuccessful.</returns>
    internal async Task<bool> SendAsync() {
        var returnValue = false;
        
        // Convert hex color to decimal.
        int.TryParse(Embed.Color.Replace("#", ""), System.Globalization.NumberStyles.HexNumber, null, out var embedColor);
        Embed.Color = embedColor.ToString();
        
        var req = new HttpClient();
        var jsonString = new Placeholder(GetEmbedJson(Embed), Placeholder).Replace();
        
        // Replace strings which are not inside the actual embed.
        // Testcases were somewhere in between 0 and 1ms for all 3 fields.
        Embed.AvatarUrl = new Placeholder(Embed.AvatarUrl, Placeholder).Replace();
        Embed.Username = new Placeholder(Embed.Username, Placeholder).Replace();
        Embed.Content = new Placeholder(Embed.Content, Placeholder).Replace();

        // To keep it simple, use field names as is.
        // Since these fields should not change (in near future), assign them via DefaultInterpolatedStringHandler.
        var json = $"{{\"avatar_url\": \"{Embed.AvatarUrl}\",\"username\": \"{Embed.Username}\",\"content\": \"{Embed.Content}\",\"embeds\": [{jsonString}]}}";
        try {
            var resp = await req.PostAsync(
                Url,
                new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
                _cancelSource.Token
            );
            
            // Check whether the response contains a success status code or not.
            if (resp.IsSuccessStatusCode) {
                returnValue = true;
            } else {
                // The request has been sent successfully but the response contained an error code.
                // This can possibly be due to an embed malformation.
                Logging.Error($"Request returned \"{resp.StatusCode}\", please check your settings!");
            }
        } catch (TaskCanceledException ex) {
            Logging.Error($"Timeout while sending request...: {ex.Message}");
        } catch (Exception ex) {
            Logging.Error($"{ex.GetType()}: {ex.Message}");
        }
        
        _cancelSource.Dispose();
        return returnValue;
    }
    
    /// <summary>
    /// Get a json string from the passed embed.
    /// </summary>
    /// <param name="embed"><c>Embed</c> - The embed that should be deserialized to a json string.</param>
    /// <returns>Json formatted string of the embed.</returns>
    private static string GetEmbedJson(Embed embed) {
        var jsonSerializerSettings = new JsonSerializerSettings() {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        // Add the current time as timestamp if property is set.
        // Remove the property otherwise.
        const string timeProp = "timestamp";
        var json = JsonConvert.SerializeObject(embed, jsonSerializerSettings);
        var parsedJson = JObject.Parse(json);
        if (parsedJson.ContainsKey(timeProp) && (parsedJson.GetValue(timeProp)?.ToString().ParseToBoolean() ?? false)) {
            parsedJson[timeProp] = DateTime.Now;
        } else {
            parsedJson.Remove(timeProp);
        }
        
        // Remove unexpected fields for the request.
        parsedJson.Remove(Fields[0]);
        parsedJson.Remove(Fields[1]);
        parsedJson.Remove(Fields[2]);
        return parsedJson.ToString();
    }
}