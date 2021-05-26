﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TwitchNotifier.src.config;

namespace TwitchNotifier.src.WebRequests {
    class WebRequest {

        public string webHookUrl { get; set; }
        public DiscordEmbed discordEmbed { get; set; }


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

            discordEmbed.Embed.Color = Convert.ToInt32(discordEmbed.Embed.Color.Replace("#", ""), 16).ToString();
            var embedJson = "{" +
                "\"avatar_url\":\"" + discordEmbed.AvatarUrl + "\"," +
                "\"username\":\"" + discordEmbed.Username + "\"," +
                "\"embeds\": [" +
                    Parser.GetEmbedJson(discordEmbed.Embed) +
                "]" +
            "}";

            // Enable for debugging purposes:
            //File.WriteAllText("jsonfile.json", embedJson);

            request.ContentType = "application/json";
            request.Method = "POST";

            using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                streamWriter.Write(embedJson);
                streamWriter.Flush();
            }

            var response = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream())) {
                var responseText = streamReader.ReadToEnd();
                Logging.Log.Debug(responseText);
            }

            return returnValue;
        }
    }
}