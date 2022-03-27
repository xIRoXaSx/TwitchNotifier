# TwitchNotifier 💬
## 📝 Configuration
1. Grab the latest [release](https://github.com/xIRoXaSx/TwitchNotifier/releases) or build the program locally.  
2. Put the program somewhere on your client where you can easily access it later on.  
3. Start the application. On the initial start, it will generate the configuration file inside its dedicated folder under your default `ApplicationData` directory (see the [table of paths](#table-of-paths) down below).
4. Open the configuration (`config.yml`) and modify it to your needs (see down below).
5. Grab yourself a new token from swiftyspiffys [website](https://twitchtokengenerator.com).
    1. Choose "Custom Scope Token" if you get asked what you want to get ![image](https://user-images.githubusercontent.com/38859398/119906078-ca28f180-bf4d-11eb-9567-b4781db2246d.png).
    2. Choose your scopes (for plain online, offline and clip monitoring you <u>**don't need any**</u> scope).
    3. Scroll to the bottom and hit **Generate Token!**.
    4. Copy the Client ID & the access token and paste it into the configuration at the bottom:
        ```yaml
        GeneralSettings:
          # [...] Trimmed for readability
          ClientId: <Place The Client ID Here>
          AccessToken: <Place The Access Token Here>
        ```
       If you want to update your token later on, you can note down your refresh token and use it when the token expires.
    5. Create a [webhook](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) and paste its URL into each desired `Eventnode` section.
        ```yaml
        - Condition: ''
          Channels:
            - Channel1
            - Channel2
          Embed:
            Username: '%Channel.Name%'
            # [...] Trimmed for readability
          WebHookUrl: <Place The Discord Webhook URL Here>
        ```
    6. Restart the program and get notified! ☕ 
       Future modifications will get hot-loaded (if enabled) on the fly.

### Table of paths
| Operating System | Path                                            | Environment Variable / Shortcut |
|------------------|-------------------------------------------------|---------------------------------|
| Windows          | `%HomeDrive%\Users\%UserName%\AppData\Roaming\` | `%AppData%`                     |
| OSX              | `/Users/$USER/Library/Application Support/`     | `~/Library/Application Support` |
| Linux            | `/home/$USER/.config/`                          | `$HOME/.config`                 |
***
<br/>

## 📝 General settings
| Property                       | Description                                                                                               | Default value           |
|--------------------------------|-----------------------------------------------------------------------------------------------------------|-------------------------|
| EnableHotLoad                  | Whether configuration should be hot-loaded on change or not.                                              | `true`                  |
| SkipStartupNotifications       | Whether notifications should be sent at startup or not.                                                   | `true`                  |
| NotificationThresholdInSeconds | The threshold which needs to exceed in order for new notifications to be sent (channel & event dependant) | `120`                   |
| ClientId                       | The client id for the Twitch API. Have a look at the [configuration](#-configuration)                     | `Your Client ID`        |
| AccessToken                    | The access token for the Twitch API. Have a look at the [configuration](#-configuration)                  | `Your App Access Token` |

### Additional notes:
`SkipStartupNotifications`: When set to `false`, every start of the application will cause a new notification to be sent, even if the stream is already live for quite some time.  
`NotificationThresholdInSeconds`: If a stream gets interrupted (eg. streamer lost connection to the platform), a value gets cached. If the stream gets back online and the threshold got exceeded (cached item), a new notification will be sent.

### Config section
```yaml
GeneralSettings:
  EnableHotload: true
  SkipStartupNotifications: true
  NotificationThresholdInSeconds: 120
  ClientID: Your Client ID
  AccessToken: Your App Access Token
```
***
<br/>

## Event nodes
Each event has its very own yaml node but works the same way.  
Whenever one of the described events happen, the node with the corresponding channel will be evaluated. 
If the validation for that specific node passed and the given condition was satisfied, the notification will be sent.

`OnLiveEvent`: Describes the event whenever a given channel goes live / is currently live.
`OnOfflineEvent`: Describes the event whenever a given channel goes offline.
`OnClipCreated`: Describes the event whenever a clip has been created & picked up by the API (might take some time until Twitch's API discovers them).
***
<br/>

## 📝 Multiple Channel Setup
All events have the same structure.
Using the following example inside other [event nodes](#event-nodes) will work as well.
You can set up multiple channels in different ways.  
If you'd like to use separate embed formats and / or webhooks, add nodes underneath each event like shown here:
```yaml
NotificationSettings:
  OnLiveEvent:
    # Channel1 and Channel2 will share the same embed layout + webhook.
    - Condition: ''
      Channels:
        - Channel1
        - Channel2
      Embed:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.ProfileImageUrl%'
        Content: Content above the embed (max 2048 characters)
        # [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
    
    # Another node for separate notifications (different channels).
    # AnotherChannel1 and AnotherChannel2 will share the same embed layout + webhook.
    - Condition: ''
      Channels:
        - AnotherChannel1
        - AnotherChannel2
      Embed:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.ProfileImageUrl%'
        Content: Content above the embed (max 2048 characters)
        # [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
```

If you'd like to use the same embed format and webhook for multiple channels, just add the channel names to the `Channels` property like in the following example:
```yaml
NotificationSettings:
  OnLiveEvent:
    # Channel1, Channel2 and Channel3 will share the same embed layout + webhook.
    - Condition: ''
      Channels:
        - Channel1
        - Channel2
        - Channel3
      Embed:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.ProfileImageUrl%'
        Content: Content above the embed (max 2048 characters)
        # [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
```
***
<br/>

## 📝 Conditions
In each [event node](#event-nodes) you can define an optional condition which needs to be satisfied before the request will be sent.  
The syntax of those conditions are inspired by common if statement conditions.  
It can be as easy as `true` and `false` but can also be more specific and complex.  
If a condition is empty / not set, the statement will automatically be evaluated as `true` => request will be sent.  
In the condition statements [placeholders](./Placeholders) can be used as well!  
Conditions can help you to **customize** your embeds even more!  
Example of such a customization is shown below.  

```yaml
NotificationSettings:
  OnLiveEvent:
    # Only send a notification if the streamed game is Minecraft.
    - Condition: '%Stream.GameName% == Minecraft'
      Channels:
        - Channel1
      Embed:
        # Replace "Minecraft-RoleID" with the corresponding Discord role ID to notify
        # everyone having this role assigned.
        Title: "%Channel.Name% went live! Grab your pickaxes <@&Minecraft-RoleID> and let''s find some diamonds!"
        # [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
      
    # Only send a notification if the title contains "community event".
    - Condition: '%Stream.Title%.Contains(community event)'
      Channels:
        - Channel1
      Embed:
        Title: "%Channel.Name% went live! Come and join the new event!"
        # [...] Trimmed for readability
      WebHookUrl: The Discord Webhook URL
```

### 📃 List of comparison operators
| Operator      | Description                   | Example                              |
|---------------|-------------------------------|--------------------------------------|
| `.Contains()` | Case insensitive - Contains   | `%Stream.Title%.Contains(event)`     |
| `==`          | Case sensitive - Equal to     | `%Stream.GameName% == Just Chatting` |
| `!=`          | Case sensitive - Not equal to | `%Stream.GameName% != Just Chatting` |
| `>=`          | Greater than or equal to      | `%Stream.ViewerCount% >= 999`        |
| `<=`          | Less than or equal to         | `%Stream.ViewerCount% <= 999`        |
| `>`           | Greater than                  | `%Stream.ViewerCount% > 999`         |
| `<`           | Less than                     | `%Stream.ViewerCount% < 999`         |
<br/>

### 📃 List of logical operators
| Operator     | Description                                    | Example                                                                    |
|--------------|------------------------------------------------|----------------------------------------------------------------------------|
| &&           | And - Both statements must return `true`       | %Stream.GameName% == Just Chatting && %Stream.Title%.Contains(Educational) |
| &vert;&vert; | Or - One of both statements must return `true` | %Stream.GameName% == Just Chatting &vert;&vert; %Stream.GameName% == Art   |
***
<br/>

## Simple examples
### ❓ Comparison with `Contains`
In this specific example the **title** of the stream will be checked. If it **contains** `event`, it returns `true` and the request will be sent! **Case insensitive**:
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.Title%.Contains(educational)"
```

### ❓ Comparison with `==`
In this specific example the streamed **game** will be checked. If it is **equal** to `Just Chatting`, it returns `true` and the request will be sent! **Case sensitive**:
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.GameName% == Just Chatting"
```

### ❓ Comparison with `!=`
In this specific example the streamed **game** will be checked. If it is **not equal** to `Just Chatting`, it returns `true` and the request will be sent! **Case sensitive**:
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.GameName% != Just Chatting"
```

### ❓ Comparison with `>=`
In this specific example the **viewer count** of the stream will be checked. If it is **greater than or equal to** `999`, it returns `true` and the request will be sent!
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.ViewerCount% >= 999"
```

### ❓ Comparison with `<=`
In this specific example the **viewer count** of the stream will be checked. If it is **less than or equal to** `999`, it returns `true` and the request will be sent!
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.ViewerCount% <= 999"
```

### ❓ Comparison with `>`
In this specific example the **viewer count** of the stream will be checked. If it is **greater than** `999`, it returns `true` and the request will be sent!
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.ViewerCount% > 999"
```

### ❓ Comparison with `<`
In this specific example the **viewer count** of the stream will be checked. If it is **less than** `999`, it returns `true` and the request will be sent!
```yaml
TwitchNotifier:
OnStreamOnline:
  StreamerOption1:
    Condition: "%Stream.ViewerCount% < 999"
```
***
<br/>

## 📝 Advanced examples
Since placeholders are allowed, you can use more complex conditions.  
You can wrap your statements in `(` and `)` and concatenate them with [logical operators](#-list-of-logical-operators).  
If a condition is not valid (neither `true` or `false`) it will default to `false`!  
The algorithm will evaluate each statement first and replaces each part with the corresponding value.
Nested conditions (inside parentheses) are evaluated first like shown below: 

| Example stream title         | Example game name | Condition                                                                                                                                    |
|------------------------------|-------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| *Things I talk about a lot!* | *Just Chatting*   | %Stream.GameName% == Just Chatting && (%Stream.Title%.Contains(talk about) &vert;&vert; %Stream.Title%.Contains(When live gives you lemons)) |

Resolving step by step:
```
|     %Stream.GameName% == Just Chatting && (%Stream.Title%.Contains(talk about) || %Stream.Title%.Contains(When live gives you lemons))
|     true && (true || false)
|     true && true
v     true
```
Evaluation returned `true` => Request will be sent.
***
<br/>

## 💬 Mentions and emojis
If you want to mention users, channels, roles or if you want to use custom emojis, please use the following syntax in your embeds:

| Type                    | Format         | Example                      |
|-------------------------|----------------|------------------------------|
| User                    | `<@UserID>`    | `<@121212121212121212>`      |
| User Nickname           | `<@!UserID>`   | `<@!121212121212121212>`     |
| Channel                 | `<#ChannelID>` | `<#121212121212121212>`      |
| Role                    | `<@&RoleID>`   | `<@&121212121212121212>`     |
| Custom Emoji            | `<:NAME:ID>`   | `<:lul:121212121212121212>`  |
| Custom Emoji (Animated) | `<a:NAME:ID>`  | `<a:lul:121212121212121212>` |
***
<br/>

## 📝 Default Configuration with comments
```yaml
NotificationSettings:
  # Triggered whenever the given channel(s) go live.
  OnLiveEvent:
    - Condition: '' # The condition which needs to be satisfied.
      Channels:     # The channels for which the notification will be sent.
        - Channel1
        - Channel2
      Embed:
        # The name to use instead of the original webhook name (outside the embed). Keep empty to use the default one.
        Username: '%Channel.Name%'
        # The url of the webhook's avatar (outside the embed). Leave empty to use the default one.
        AvatarUrl: '%Channel.User.ProfileImageUrl%'
        # The text that should be put over the actual embed.
        Content: Content above the embed (max 2048 characters)
        # The title of the embed.
        Title: '%Channel.Name% went online!'
        # The url behind the title if clicked.
        Url: '%Channel.Url%'
        # The text inside the embed.
        Description: What are you waiting for?!\nGo check it out now!
        # The color of the embed (left hand side).
        Color: '#5555FF'
        # Whether to use time stamps for the embed (time whenever the event has been triggered, at bottom of the embed).
        Timestamp: true
        Thumbnail:
          # The thumbnail's url (at the top right of the embed).
          Url: '%Channel.User.ProfileImageUrl%'
        Image:
          # The image's url (big image right under the description and the embed's fields).
          Url: '%Stream.ThumbnailUrl%'
        # Settings for the embed author (inside the embed, above the embed's title).
        Author:
          # The embed's author name.
          Name: "Stream Announcer \U0001F4E2"
          # The avatar / icon url of the embed's author.
          IconUrl: '%Channel.User.ProfileImageUrl%'
          # The url behind the embed's author name when clicked.
          Url: '%Channel.Url%'
        # Settings for the embed's fields.
        Fields:
          # The name of the fields (bold above the value).
          - Name: Unique Field Name 1
            # The field's value.
            Value: Value of field 1
            # Whether or not to inline the field (enables multiple fields right next to each other). 
            Inline: false
          - Name: Unique Field Name 2
            Value: Value of field 2
            Inline: false
        # Settings for the embed's footer.
        Footer:
          # The text to use underneath the embed.
          Text: The footer text (max 2048 chars)
          # The icon's url left to the embed's footer text.
          IconUrl: '%Channel.User.ProfileImageUrl%'
      # The webhook of your Discord's channel, where the notification / embed should be sent to.
      WebHookUrl: The Discord Webhook URL
  # Triggered whenever the given channel(s) go offline.
  OnOfflineEvent:
    - Condition: ''
      Channels:
        - Channel1
        - Channel2
      Embed:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.ProfileImageUrl%'
        Content: Content above the embed (max 2048 characters)
        Title: '%Channel.Name% went online!'
        Url: '%Channel.Url%'
        Description: What are you waiting for?!\nGo check it out now!
        Color: '#5555FF'
        Timestamp: true
        Thumbnail:
          Url: '%Channel.User.ProfileImageUrl%'
        Image:
          Url: '%Stream.ThumbnailUrl%'
        Author:
          Name: "Stream Announcer \U0001F4E2"
          IconUrl: '%Channel.User.ProfileImageUrl%'
          Url: '%Channel.Url%'
        Fields:
          - Name: Unique Field Name 1
            Value: Value of field 1
            Inline: false
          - Name: Unique Field Name 2
            Value: Value of field 2
            Inline: false
        Footer:
          Text: The footer text (max 2048 chars)
          IconUrl: '%Channel.User.ProfileImageUrl%'
      WebHookUrl: The Discord Webhook URL
  # Triggered whenever a clip has been created on the given channel(s).
  OnClipCreated:
    - Condition: ''
      Channels:
        - Channel1
        - Channel2
      Embed:
        Username: '%Channel.Name%'
        AvatarUrl: '%Channel.User.ProfileImageUrl%'
        Content: Content above the embed (max 2048 characters)
        Title: '%Channel.Name% went online!'
        Url: '%Channel.Url%'
        Description: What are you waiting for?!\nGo check it out now!
        Color: '#5555FF'
        Timestamp: true
        Thumbnail:
          Url: '%Channel.User.ProfileImageUrl%'
        Image:
          Url: '%Stream.ThumbnailUrl%'
        Author:
          Name: "Stream Announcer \U0001F4E2"
          IconUrl: '%Channel.User.ProfileImageUrl%'
          Url: '%Channel.Url%'
        Fields:
          - Name: Unique Field Name 1
            Value: Value of field 1
            Inline: false
          - Name: Unique Field Name 2
            Value: Value of field 2
            Inline: false
        Footer:
          Text: The footer text (max 2048 chars)
          IconUrl: '%Channel.User.ProfileImageUrl%'
      WebHookUrl: The Discord Webhook URL
# Settings for the program itself.
GeneralSettings:
  # Whether or not to use the debug mode. SHOULD NOT BE USED IN PRODUCTION (more information will be printed).
  Debug: false
  # Whether or not to use the configuration's Hot-Load feature.
  EnableHotLoad: true
  # Whether or not to send notifications while program is starting.
  # This will send new notifications even if streamer went live long ago (will not send offline notifications again).
  SkipNotificationsOnStartup: true
  # The amount of seconds which needs to pass until a new Live / Offline notification will be sent.
  LiveNotificationThresholdInSeconds: 120
  # The interval in seconds to check for live channels.
  LiveCheckIntervalInSeconds: 5
  # Your Twitch's client Id.
  ClientId: Your Client ID
  # Your Twitch's access token.
  AccessToken: Your App Access Token
```