# TwitchNotifier 💬
## 🔗 Placeholders
Most placeholders are parsed object oriented. All of them can either be found at the documentation of TwitchLib or down below.  
Each placeholder needs to be surrounded with `%` and can be used everwhere in embeds.

**User** specific placeholders are available under `%Channel.User%` or `%Clip.Creator%` (eg.: URL of the channel / user = `%Channel.User.Url%` / `%Clip.Creator.Url%`)  
**Stream** specific placeholders are available under `%Stream%` (eg.: Title of the stream = `%Stream.Title%`)

***
<br/>

### 📃 List Of Placeholders
| Placeholder                      | Type     | Description                                                              |
|----------------------------------|----------|--------------------------------------------------------------------------|
| `%Stream.Id%`                    | String   | The ID of the stream                                                     |
| `%Stream.UserId%`                | String   | The ID of the streamer                                                   |
| `%Stream.UserName%`              | String   | The user name of the streamer                                            |
| `%Stream.GameId%`                | String   | The ID of the current Game                                               |
| `%Stream.GameName%`              | String   | The name of the current Game                                             |
| `%Stream.IsMature%`              | Boolean  | Whether the stream is targeting adult audience or not                    |
| `%Stream.CommunityIds%`          | Array    | The IDs of the community                                                 |
| `%Stream.Type%`                  | String   | The type of the stream (eg.: `live`)                                     |
| `%Stream.Title%`                 | String   | The title of the stream                                                  |
| `%Stream.ViewerCount%`           | Int      | The title of the stream                                                  |
| `%Stream.StartedAt%`             | DateTime | The time the stream has started                                          |
| `%Stream.TagIds%`                | Array    | All used tag IDs (strings) of the stream.                                |
| `%Stream.Language%`              | String   | The language of the stream                                               |
| `%Stream.ThumbnailUrl%`          | String   | The URL of the streams thumbnail                                         |
||                                  |          |     |
| `%Channel.Name%`                 | String   | The name of the channel                                                  |
| `%Channel.Description%`          | String   | The description of the channel                                           |
| `%Channel.Url%`                  | String   | The URL to the channel.                                                  |
| `%Channel.User.BroadcasterType%` | String   | The type of the streamer (partner, affiliate, empty)                     |
| `%Channel.User.Id%`              | String   | The ID of the user                                                       |
| `%Channel.User.CreatedAt%`       | DateTime | The time when the channel has been created                               |
| `%Channel.User.DisplayName%`     | String   | The display name of the user                                             |
| `%Channel.User.ProfileImageUrl%` | String   | The URL of the users image                                               |
| `%Channel.User.OfflineImageUrl%` | String   | The URL of the users offline image                                       |
| `%Channel.User.ViewCount%`       | Long     | The amount of views the channel has                                      |
||                                  |          |     |
| `%Clip.CreatorChannelUrl%`       | String   | The URL to the clip creators channel.                                    |
| `%Clip.Creator.Name%`            | String   | The name of the clip creators channel                                    |
| `%Clip.Creator.Description%`     | String   | The description of the clip creators channel                             |
| `%Clip.Creator.BroadcasterType%` | String   | The type of the of the clip creators channel (partner, affiliate, empty) |
| `%Clip.Creator.Id%`              | String   | The ID of the clip creators channel                                      |
| `%Clip.Creator.CreatedAt%`       | DateTime | The time when the clip creators channel has been created                 |
| `%Clip.Creator.DisplayName%`     | String   | The display name of the clip creator                                     |
| `%Clip.Creator.ProfileImageUrl%` | String   | The URL of the clip creators image                                       |
| `%Clip.Creator.OfflineImageUrl%` | String   | The URL of the clip creators offline image                               |
| `%Clip.Creator.ViewCount%`       | Long     | The amount of views the of the clip creator has                          |
||                                  |          |     |
| `%Clip.BroadcasterId%`           | String   | The ID of the channel                                                    |
| `%Clip.BroadcasterName%`         | String   | The name of the channel                                                  |
| `%Clip.CreatedAt%`               | String   | The time when the clip has been created                                  |
| `%Clip.CreatorId%`               | String   | The ID of the channel which created the clip                             |
| `%Clip.CreatorName%`             | String   | The name of the channel which created the clip                           |
| `%Clip.Duration%`                | Float    | The duration of the clip                                                 |
| `%Clip.EmbedUrl%`                | String   | The embed url of the clip                                                |
| `%Clip.GameId%`                  | String   | The Game ID of played game in the clip                                   |
| `%Clip.Id%`                      | String   | The ID of the clip                                                       |
| `%Clip.Language%`                | String   | The Language of the clip                                                 |
| `%Clip.ThumbnailUrl%`            | String   | The URL of the clip's thumbnail                                          |
| `%Clip.Title%`                   | String   | The title of the clip                                                    |
| `%Clip.Url%`                     | String   | The clip's URL                                                           |
| `%Clip.VideoId%`                 | String   | The ID of the video                                                      |
| `%Clip.ViewCount%`               | Int      | The amount of views of the clip                                          |
