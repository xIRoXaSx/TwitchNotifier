using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchNotifier.src.config {
    /// <summary>
    /// The configuration of this program
    /// </summary>
    public class Config {
        public TwitchNotifier TwitchNotifier { get; set; } = new TwitchNotifier();
        public TwitchSettings Settings { get; set; } = new TwitchSettings();
    }


    /// <summary>
    /// Contains the complete configuration
    /// </summary>
    public class TwitchNotifier {
        public Event OnStreamStart { get; set; } = new Event();
        public Event OnStreamStop { get; set; } = new Event();
        public Event OnFollow { get; set; } = new Event();
    }


    /// <summary>
    /// Contains events from Twitch
    /// </summary>
    public class Event {
        public EventObject StreamerOption1 { get; set; } = new EventObject();
    }


    /// <summary>
    /// Contains events from Twitch
    /// </summary>
    public class EventObject {
        public TwitchSettings Twitch { get; set; } = new TwitchSettings();
        public Embed Embed { get; set; } = new Embed();
    }


    /// <summary>
    /// Settings for the Twitch channel which are creating events
    /// </summary>
    public class TwitchSettings {
        public string Username { get; set; } = "Channelname";
    }


    /// <summary>
    /// The author of the embed
    /// </summary>
    public class EmbedAuthor {
        public string Name { get; set; } = "The author's name for this message";
        public string IconUrl { get; set; } = "The URL of the author's image";
        public string Url { get; set; } = "The URL when the author's name is clicked";
    }


    /// <summary>
    /// The fields for the embed (max = 25)
    /// </summary>
    public class EmbedFields {
        public string Name { get; set; } = "Name of the field (max 256 chars)";
        public string Value { get; set; } = "Value of the field (max 1024 chars)";
        public bool Inline { get; set; } = false;
    }


    /// <summary>
    /// The footer of the embed
    /// </summary>
    public class EmbedFooter {
        public string Text { get; set; } = "The footer text (max 2048 chars)";
        public string IconUrl { get; set; } = "The URL of the footer's image";
    }


    /// <summary>
    /// Settings for the Discord embed
    /// </summary>
    public class Embed {
        public string Title { get; set; } = "%Username% went live!";
        public string Url { get; set; } = "The title's URL which can be clicked";
        public string Description { get; set; } = "The embeds text / description";
        public string Color { get; set; } = "The embeds hex color (like #5555FF)";
        public bool Timestamp { get; set; } = true;
        public EmbedAuthor Author { get; set; } = new EmbedAuthor();
        public EmbedFields Fields { get; set; } = new EmbedFields();
        public EmbedFooter Footer { get; set; } = new EmbedFooter();
    }
}
