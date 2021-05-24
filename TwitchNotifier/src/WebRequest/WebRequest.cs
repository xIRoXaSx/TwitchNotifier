using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TwitchNotifier.src.config;

namespace TwitchNotifier.src.WebRequest {
    class WebRequest {

        public string webHookUrl { get; set; }
        public Embed embed { get; set; }


        ///// <summary>
        ///// Create a new webrequest / send data to Discord (WebHook)
        ///// </summary>
        ///// <param name="webHookUrlPassed"></param>
        ///// <param name="embedPassed"></param>
        //public WebRequest(string webHookUrlPassed, Embed embedPassed) {
        //    webHookUrl = webHookUrlPassed;
        //    embed = embedPassed;
        //}


        /// <summary>
        /// Send the webrequest / embedded message to Discord
        /// </summary>
        /// <returns></returns>
        public bool SendRequest() {
            bool returnValue = false;
            var request = System.Net.WebRequest.Create(webHookUrl);

            embed.Color = Convert.ToInt32(embed.Color.Replace("#", ""), 16).ToString();
            var embedJson = "{\"embeds\": [" +
                Parser.GetEmbedJson(embed) +
            "]}";

            File.WriteAllText("jsonfile.json", embedJson);

            request.ContentType = "application/json";
            request.Method = "POST";

            using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                streamWriter.Write(embedJson);
                streamWriter.Flush();
            }

            // === Thorws 400 Bad request
            var response = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream())) {
                var responseText = streamReader.ReadToEnd();
                Logging.Logging.Debug(responseText);
            }

            

            return returnValue;
        }



    }
}
