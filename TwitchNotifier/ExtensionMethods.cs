﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TwitchNotifier.models;
using TwitchNotifier.placeholders;

namespace TwitchNotifier; 

public static class ExtensionMethods {
    /// <summary>
    /// <b>! Needs to be benchmarked against ToSnakeCase !</b>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string ToForSnakeCase(this string value) {
        var sb = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++) {
            var c = value[i];
            sb.Append(i > 0 && char.IsUpper(c) && value[i-1] != '.' ? "_" + c : c);
        }
        return sb.ToString().ToLower();
    }
    
    /// <summary>
    /// Get the string formatted via snake case.
    /// <param name="value"><c>String</c> - The value to format.</param>
    /// <returns><c>String</c> - The formatted value.</returns>
    /// </summary>    
    internal static string ToSnakeCase(this string value) {
        return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) && value[i - 1] != '.' ? "_" + x : x.ToString())).ToLower();
    }
    
    /// <summary>
    /// Try to parse a boolean from string.
    /// If parsing is not possible, use fallback value instead.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to parse.</param>
    /// <param name="fallback"><c>Bool</c> - The fallback if the parsing fails.</param>
    /// <returns><c>Bool</c> - The parsed value or its fallback.</returns>
    internal static bool ParseToBoolean(this string value, bool fallback = false) {
        return !bool.TryParse(value, out var returnValue) ? fallback : returnValue;
    }
    
    /// <summary>
    /// Truncate the given string if it is longer than <c>maxChars</c><br/>
    /// Adds <c>...</c> at the end of the string if truncated.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to truncate.</param>
    /// <param name="maxChars"><c>Int</c> - The amount of chars after which the string should be truncated.</param>
    /// <returns><c>String</c> - Truncated or original value.</returns>
    private static string Truncate(this string value, int maxChars) {
        return value.Length <= maxChars ? value : string.Concat(value.AsSpan(0, maxChars - 3), "...");
    }
    
    /// <summary>
    /// Hash the given value.
    /// </summary>
    /// <param name="value">The string that should be hashed.</param>
    /// <returns><c>string</c> - The hashed string.</returns>
    internal static string Create256Sha(this string value) {
        using var sha256 = SHA256.Create();
        var chars = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
        var stringBuilder = new StringBuilder();
        foreach (var c in chars) {
            stringBuilder.Append(c.ToString("x2"));
        }
        return stringBuilder.ToString();
    }
    
    /// <summary>
    /// Validate an embed and return truncated if field limits have exceeded.<br/>
    /// If the embed exceeds the total max length, it returns <c>null</c>.<br/>
    /// <a href="https://discord.com/developers/docs/resources/channel#embed-limits">Official docs</a>
    /// </summary>
    /// <param name="embed"><c>Embed</c> - The embed to validate</param>
    /// <returns><c>Embed</c> - Either truncated / original or null</returns>
    internal static Embed Validate(this Embed embed) {
        if (embed == null)
            return null;
        embed.Content = embed.Content.Truncate(4096);
        embed.Username = embed.Username.Truncate(80);
        embed.Author.Name = embed.Author.Name.Truncate(256);
        embed.Description = embed.Description.Truncate(2048);
        embed.Title = embed.Title.Truncate(256);
        embed.Footer.Text =  embed.Footer.Text.Truncate(2048);
        for (var i = 0; i < embed.Fields.Count; i++) {
            embed.Fields[i].Name = embed.Fields[i].Name.Truncate(256);
            embed.Fields[i].Value = embed.Fields[i].Value.Truncate(1024);
        }

        // Max fields currently: 25
        embed.Fields = embed.Fields.Count <= 25 ? embed.Fields : embed.Fields.GetRange(0, 25);
        
        // Total amount of chars needs to be lower or equal to 6000.
        return embed.Content.Length + embed.Username.Length + embed.Author.Name.Length + embed.Description.Length +
            embed.Title.Length + embed.Footer.Text.Length + embed.Footer.Text.Length +
            string.Join("", embed.Fields.Select(x => x.Name)).Length +
            string.Join("", embed.Fields.Select(x => x.Value)).Length <= 6000 ? embed : null;
    }

    /// <summary>
    /// Get the first <c>NotificationEvent</c> inside the given list which matches the provided value.
    /// </summary>
    /// <param name="events"><c>IEnumerable&lt;NotificationEvent&gt;</c> - The IEnumerable of NotificationEvents</param>
    /// <param name="value"><c>String</c> - The value to look for (case insensitive)</param>
    /// <returns><c>NotificationEvent</c> or <c>null</c></returns>
    internal static NotificationEvent GetFirstMatchOrNull(this IEnumerable<NotificationEvent> events, string value) {
        foreach (var notification in events) {
            value = value.ToLower();
            for (var i = 0; i < notification.Channels.Count; i++) {
                if (notification.Channels[i].ToLower() != value.ToLower()) 
                    continue;
                return notification.Clone();
            }
        }

        return null;
    }

    /// <summary>
    /// Convert the Embed into a json string. 
    /// </summary>
    /// <param name="embed"><c>Embed</c> - The Embed to serialize.</param>
    /// <param name="placeholder"><c>TwitchPlaceholder</c> - The placeholder used for the serialization.</param>
    /// <returns><c>String</c> - The serialized Embed</returns>
    internal static string ToJson(this Embed embed, TwitchPlaceholder placeholder) {
        // Convert Embed's color into an int.
        // Check if it matches the 3 and 6 digit syntax, afterwards try to parse it to an integer value.
        var match = Regex.Match(embed.Color.Trim(), @"[#]*([0-9a-f]{3}){1,2}", RegexOptions.IgnoreCase);
        if (match.Success) {
            embed.Color = int.TryParse(match.Groups[0].Value.Replace("#", ""), System.Globalization.NumberStyles.HexNumber, null,
                out var color) ? color.ToString() : "0";
        } else {
            Logging.Error($"Embed color {embed.Color} seems invalid and will be temporary set to 0!");
            embed.Color = "0";
        }

        // Get the json string of the Discord embed.
        var embedString = new Placeholder(embed.ToString(), placeholder).Replace();
        
        // Replace strings which are not inside the actual embed.
        // Testcases were somewhere in between 0 and 1ms for all 3 fields.
        embed.AvatarUrl = new Placeholder(embed.AvatarUrl, placeholder).Replace();
        embed.Username = new Placeholder(embed.Username, placeholder).Replace();
        embed.Content = new Placeholder(embed.Content, placeholder).Replace();

        // To keep it simple, use field names as is.
        // Since these fields should not change (in near future), assign them via DefaultInterpolatedStringHandler.
        var json = $"{{\"avatar_url\": \"{embed.AvatarUrl}\"," + 
                   $"\"username\": \"{embed.Username}\"," + 
                   $"\"content\": \"{embed.Content}\"," + 
                   $"\"embeds\": [{embedString}]}}";
        return json;
    }
    
    /// <summary>
    /// Clones the given NotificationEvent via reconstructing.
    /// </summary>
    /// <param name="notification"><c>NotificationEvent</c> - The notification to clone</param>
    /// <returns><c>NotificationEvent</c> - A clone of the NotificationEvent</returns>
    internal static NotificationEvent Clone(this NotificationEvent notification) {
        return new NotificationEvent {
            Channels = new List<string>(notification.Channels),
            Condition = notification.Condition,
            Embed = new Embed {
                Author = notification.Embed.Author,
                Color = notification.Embed.Color,
                Content = notification.Embed.Content,
                Description = notification.Embed.Description,
                Fields = new List<EmbedField>(notification.Embed.Fields),
                Footer = notification.Embed.Footer,
                Image = notification.Embed.Image,
                Thumbnail = notification.Embed.Thumbnail,
                Timestamp = notification.Embed.Timestamp,
                Title = notification.Embed.Title,
                Url = notification.Embed.Url,
                Username = notification.Embed.Username,
                AvatarUrl = notification.Embed.AvatarUrl
            },
            WebHookUrl = notification.WebHookUrl,
        };
    }
}
