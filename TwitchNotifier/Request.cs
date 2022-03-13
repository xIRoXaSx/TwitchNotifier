using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace TwitchNotifier; 

internal class Request {
    private string Url { get; }
    private string Data { get; }

    private readonly CancellationTokenSource _cancelSource;

    internal Request(string url, string data) {
        Url = url;
        Data = data;
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
        var req = new HttpClient();
        
        // To keep it simple, use field names as is.
        // Since these fields should not change (in near future), assign them via DefaultInterpolatedStringHandler.
        try {
            var resp = await req.PostAsync(
                Url,
                new StringContent(Data, System.Text.Encoding.UTF8, "application/json"),
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
}