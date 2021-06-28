﻿# TwitchNotifier 💬
## 🔗 Placeholders
Most placeholders are parsed object oriented. All of them can either be found at the documentation of TwitchLib or down below.  
Each placeholder needs to be surrounded with `%` and can be used everwhere in embeds.  
**User** specific placeholders are available under `%Channel.User%` or `%Clip.Creator%` (eg.: URL of the channel / user = `%Channel.User.Url%` / `%Clip.Creator.Url%`)  
**Stream** specific placeholders are available under `%Stream%` (eg.: Title of the stream = `%Stream.Title%`)


[**User**](https://swiftyspiffy.com/TwitchLib/Api/class_twitch_lib_1_1_api_1_1_v5_1_1_models_1_1_channels_1_1_channel.html) specific placeholders  
[**Stream**](https://swiftyspiffy.com/TwitchLib/Api/class_twitch_lib_1_1_api_1_1_helix_1_1_models_1_1_streams_1_1_stream.html) specific placeholders  

***
<br/>

### 📃 List Of Placeholders
Placeholder|Type|Description
-----------|----|-----------
`%Stream.Id%` | String | The ID of the stream
`%Stream.UserIdId%` | String | The ID of the streamer
`%Stream.GameId%` | String | The ID of the current Game
`%Stream.CommunityIds%` | Array | The IDs of the community
`%Stream.Type%` | String | The type of the stream (eg.: `live`)
`%Stream.Title%` | String | The title of the stream
`%Stream.ViewerCount%` | Int | The title of the stream
`%Stream.StartedAt%` | DateTime | The time the stream has started
`%Stream.Language%` | String | The language of the stream
`%Stream.ThumbnailUrl%` | String | The URL of the streams thumbnail
||
`%Channel.Name%` | String | The name of the channel
`%Channel.User.BroadcasterLanguage%` | String | The Language of the streamer
`%Channel.User.BroadcasterType%` | String | The type of the streamer (partner, affiliate, empty)
`%Channel.User.CreatedAt%` | DateTime | The time when the channel has been created
`%Channel.User.DisplayName%` | String | The display name of the user
`%Channel.User.Followers%` | Int | The amount of followers the channel has
`%Channel.User.Game%` | String | The currently / last played game
`%Channel.User.Language%` | String | The language of the channel
`%Channel.User.Logo%` | String | The URL of the users logo
`%Channel.User.Mature%` | Bool | Whether the channel is for mature audience or not
`%Channel.User.Name%` | String | The name of the user (same as `%Channel.Name%`)
`%Channel.User.Partner%` | Bool | Whether the channel is partnered or not
`%Channel.User.ProfileBanner%` | String | The URL of the users banner
`%Channel.User.ProfileBannerBackgroundColor%` | String | The profile banner background color
`%Channel.User.Status%` | String | The Status of the user
`%Channel.User.UpdatedAt%` | DateTime | The time of the last channel update
`%Channel.User.Url%` | String | The URL of the users channel
`%Channel.User.VideoBanner%` | String | The URL of the users video banner
`%Channel.User.Views%` | Int | The amount of views the channel has
||
`%Clip.Creator.BroadcasterLanguage%` | String | The Language of the clip creator
`%Clip.Creator.BroadcasterType%` | String | The type of the clip creator (partner, affiliate, empty)
`%Clip.Creator.CreatedAt%` | DateTime | The time when the channel of the clip creator has been created
`%Clip.Creator.DisplayName%` | String | The display name of the clip creator
`%Clip.Creator.Followers%` | Int | The amount of followers the channel of the clip creator has
`%Clip.Creator.Game%` | String | The currently / last played game of the clip creator
`%Clip.Creator.Language%` | String | The language of the clip creators channel
`%Clip.Creator.Logo%` | String | The URL of the clip creators logo
`%Clip.Creator.Mature%` | Bool | Whether the clip creators channel is for mature audience or not
`%Clip.Creator.Name%` | String | The name of the clip creator
`%Clip.Creator.Partner%` | Bool | Whether the clip creators channel is partnered or not
`%Clip.Creator.ProfileBanner%` | String | The URL of the clip creators banner
`%Clip.Creator.ProfileBannerBackgroundColor%` | String | The profile banner background color of the clip creator
`%Clip.Creator.Status%` | String | The Status of the clip creator
`%Clip.Creator.UpdatedAt%` | DateTime | The time of the last clip creators channel update
`%Clip.Creator.Url%` | String | The URL of the clip creators channel
`%Clip.Creator.VideoBanner%` | String | The URL of the clip creators video banner
`%Clip.Creator.Views%` | Int | The amount of views the clip creators channel has
||
`%Clip.BroadcasterId%` | String | The ID of the channel
`%Clip.CreatedAt%` | String | The time when the clip has been created
`%Clip.CreatorId%` | String | The ID of the channel which created the clip
`%Clip.Duration%` | Float | The duration of the clip
`%Clip.EmbedUrl%` | String | The embed url of the clip
`%Clip.GameId%` | String | The Game ID of played game in the clip
`%Clip.Id%` | String | The ID of the clip
`%Clip.Language%` | String | The Language of the clip
`%Clip.ThumbnailUrl%` | String | The URL of the clip's thumbnail
`%Clip.Title%` | String | The title of the clip
`%Clip.Url%` | String | The clip's URL
`%Clip.VideoId%` | String | The ID of the video
`%Clip.ViewCount%` | Int | The amount of views of the clip