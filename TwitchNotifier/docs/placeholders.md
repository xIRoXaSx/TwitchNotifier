# TwitchNotifier 💬
## 🔗 Placeholders
Most placeholders are parsed object oriented. All of them can either be found at the documentation of TwitchLib or down below.  
Each placeholder needs to be surrounded with `%` and can be used everwhere in embeds.  
**User** specific placeholders are available under `%Channel.User%` (eg.: URL of the channel / user = `%Channel.User.Url%`)  
**Stream** specific placeholders are available under `%Stream%` (eg.: Title of the stream = `%Stream.Title%`)


[**User**](https://swiftyspiffy.com/TwitchLib/Api/class_twitch_lib_1_1_api_1_1_v5_1_1_models_1_1_channels_1_1_channel.html) specific placeholders  
[**Stream**](https://swiftyspiffy.com/TwitchLib/Api/class_twitch_lib_1_1_api_1_1_helix_1_1_models_1_1_streams_1_1_stream.html) specific placeholders  

***
<br/>

### 📃 List Of Placeholders
Placeholder|Type|Description
-----------|----|-----------
`%Stream.Id%`|String|The ID of the stream
`%Stream.UserIdId%`|String|The ID of the streamer
`%Stream.GameId%`|String|The ID of the current Game
`%Stream.CommunityIds%`|Array|The IDs of the community
`%Stream.Type%`|String|The type of the stream (eg.: `live`)
`%Stream.Title%`|String|The title of the stream
`%Stream.ViewerCount%`|Int|The title of the stream
`%Stream.StartedAt%`|DateTime|The time the stream has started
`%Stream.Language%`|String|The language of the stream
`%Stream.ThumbnailUrl%`|String|The URL of the streams thumbnail
||
`%Channel.Name%`|String|The name of the channel
`%Channel.User.BroadcasterLanguage%`|String|The Language of the streamer
`%Channel.User.CreatedAt%`|DateTime|The time when the channel has been created
`%Channel.User.DisplayName%`|String|The display name of the user
`%Channel.User.Followers%`|Int|The amount of followers the channel has
`%Channel.User.BroadcasterType%`|String|The type of the streamer (partner, affiliate, empty)
`%Channel.User.Game%`|String|The currently played game
`%Channel.User.Language%`|String|The language of the channel
`%Channel.User.Logo%`|String|The URL of the users logo
`%Channel.User.Mature%`|Bool|Whether the channel is for mature audience or not
`%Channel.User.Name%`|String|The name of the user (same as `%Channel.Name%`)
`%Channel.User.Partner%`|Bool|Whether the channel is partnered or not
`%Channel.User.ProfileBanner%`|String|The URL of the users banner
`%Channel.User.ProfileBannerBackgroundColor%`|String|The profile banner background color
`%Channel.User.Status%`|String|The Status of the user
`%Channel.User.UpdatedAt%`|DateTime|The time of the last channel update
`%Channel.User.Url%`|String|The URL of the user
`%Channel.User.VideoBanner%`|String|The URL of the users video banner
`%Channel.User.Views%`|Int|The amount of views the channel has

