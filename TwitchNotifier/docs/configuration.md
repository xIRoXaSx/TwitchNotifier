# TwitchNotifier 💬
## 📝 Configuration
1. First of all grab the latest [release](https://github.com/xIRoXaSx/TwitchNotifier/releases) or build the program locally.  
2. Put the app somewhere on your client where you can easily access it later on.  
3. Start the application. It will generate the configuration file in the Folder `TwitchNotifier` in your ApplicationData directory:
    Operating System|Path|Environment Variable / Shortcut
    ----------------|----|-------------------------------
    Windows| `/Users/%USERNAME%/Library/Application Support/`|`%appdata%`
    OSX| `/Users/$USER/Library/Application Support/`|`~/Library/Application Support`
    Linux| `/home/$USER/.config`|`$HOME/.config`
4. Open the configuration (`config.yml`) and modify it to your needs
5. Grab yourself a new token from swiftyspiffys [website](https://twitchtokengenerator.com) (own generator is planned)
    1. Choose "Custom Scope Token" if you get asked what you want to get ![image](https://user-images.githubusercontent.com/38859398/119906078-ca28f180-bf4d-11eb-9567-b4781db2246d.png)
    2. Choose your scopes (for plain online and offline monitoring you <u>**don't need any**</u> scope)
    3. Scroll to the bottom and hit **Generate Token!**
    4. Copy the Client ID + the access token and paste it into the configuration at the bottom:
        ```yaml
        Settings:
          ClientID: <Place The Client ID Here>
          AccessToken: <Place The Access Token Here>
        ```
    5. Create a [Webhook](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) and paste its URL into each `Discord` section.
        ```yaml
        StreamerOption1:
          Username: '%Channel.Name%'
          AvatarUrl: '%Channel.User.Url%'
          Embed:
            Title: '%Channel.Name% went online!'
            [...] Trimmed for readability
        WebHookUrl: <Place The Discord Webhook URL Here>
        ```
    6. (Re-)Start the program and get notified! ☕

***
<br/>

## Multiple Channel Setup
You can set up multiple channels in different ways.  
If you'd like to use seperate embed formats, channels / Webhooks add multiple yml nodes underneath each event node like shown here:
```yaml
TwitchNotifier:
  OnStreamOnline:
    StreamerOption1:
      Twitch:
        Usernames:
        - Channel1
        - Channel2
        - Channel3
      Discord:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.Url%'
        Embed:
        [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
    AnotherOption:
      Twitch:
        Usernames:
        - ChannelA
        - ChannelB
        - ChannelC
      Discord:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.Url%'
        Embed:
        [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
```

If you like to use the same embed format, channel / Webhook just add the usernames underneath `Usernames:` like the following example:

```yaml
TwitchNotifier:
  OnStreamOnline:
    StreamerOption1:
      Twitch:
        Usernames:
        - Channel1
        - Channel2
        - Channel3
      Discord:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.Url%'
        Embed:
        [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
```

***
<br/>

## 📝 Default Configuration
```yaml
TwitchNotifier:
  OnStreamOnline:
    StreamerOption1:
      Twitch:
        Usernames:
        - Channelnames
      Discord:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.Url%'
        Embed:
          Title: '%Channel.Name% went online!'
          Url: '%Channel.User.Url%'
          Description: What are you waiting for?!\nGo check it out now!
          Color: '#5555FF'
          Timestamp: true
          Thumbnail:
            Url: '%Channel.User.Logo%'
          Image:
            Url: '%Stream.ThumbnailUrl%'
          Author:
            Name: "Stream Announcer \U0001F4E2"
            IconUrl: '%Channel.User.Logo%'
            Url: '%Channel.User.Url%'
          Fields:
          - Name: Unique Field Name 1
            Value: Value of field 1
            Inline: false
          - Name: Unique Field Name 2
            Value: Value of field 2
            Inline: false
          Footer:
            Text: The footer text (max 2048 chars)
            IconUrl: '%Channel.User.Logo%'
      WebHookUrl: The Discord Webhook URL
  OnStreamOffline:
    StreamerOption1:
      Twitch:
        Usernames:
        - Channelnames
      Discord:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.Url%'
        Embed:
          Title: '%Channel.Name% went online!'
          Url: '%Channel.User.Url%'
          Description: What are you waiting for?!\nGo check it out now!
          Color: '#5555FF'
          Timestamp: true
          Thumbnail:
            Url: '%Channel.User.Logo%'
          Image:
            Url: '%Stream.ThumbnailUrl%'
          Author:
            Name: "Stream Announcer \U0001F4E2"
            IconUrl: '%Channel.User.Logo%'
            Url: '%Channel.User.Url%'
          Fields:
          - Name: Unique Field Name 1
            Value: Value of field 1
            Inline: false
          - Name: Unique Field Name 2
            Value: Value of field 2
            Inline: false
          Footer:
            Text: The footer text (max 2048 chars)
            IconUrl: '%Channel.User.Logo%'
      WebHookUrl: The Discord Webhook URL
  OnFollow:
    StreamerOption1:
      Twitch:
        Usernames:
        - Channelnames
      Discord:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.Url%'
        Embed:
          Title: '%Channel.Name% went online!'
          Url: '%Channel.User.Url%'
          Description: What are you waiting for?!\nGo check it out now!
          Color: '#5555FF'
          Timestamp: true
          Thumbnail:
            Url: '%Channel.User.Logo%'
          Image:
            Url: '%Stream.ThumbnailUrl%'
          Author:
            Name: "Stream Announcer \U0001F4E2"
            IconUrl: '%Channel.User.Logo%'
            Url: '%Channel.User.Url%'
          Fields:
          - Name: Unique Field Name 1
            Value: Value of field 1
            Inline: false
          - Name: Unique Field Name 2
            Value: Value of field 2
            Inline: false
          Footer:
            Text: The footer text (max 2048 chars)
            IconUrl: '%Channel.User.Logo%'
      WebHookUrl: The Discord Webhook URL
Settings:
  ClientID: Your Client ID
  AccessToken: Your App Access Token

```