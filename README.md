# TwitchNotifier 💬
## Description
Ever wanted to receive **customized** Discord messages when certain events from Twitch have happened to your own or your favorite Twitch streamers channel (eg: **live**, **offline**, ...) without adding another bot to your **Discord** server?  
You came to the right place if this describes your problem!  
With TwitchNotifier you can customize which **events** should trigger messages for your Discord server and how they should look like!  
The setup is quickly done in a few minutes and the integrated **placeholders** will make your life even easier!  
Sounds too good to be true? Try it out and see! 😊
Example of an embed (stream went online) | Example of an embed (clip has been created)
-----------------------------------------|--------------------------------------------
![DiscordEmbed](https://user-images.githubusercontent.com/38859398/122478113-1a352a00-cfc9-11eb-9d6d-8d2616876627.png) | ![DiscordClipEmbed](https://user-images.githubusercontent.com/38859398/122477292-c37b2080-cfc7-11eb-91de-241fbcd33865.png)
***
<br/>


### 📃 Configuration
For a detailed configuration please check out [Configuration](https://github.com/xIRoXaSx/TwitchNotifier/wiki/Configuration)

***
<br/>

### 🔗 Placeholders
To view all available placeholders please check out [Placeholders](https://github.com/xIRoXaSx/TwitchNotifier/wiki/Placeholders)
***
<br/>

### 📅 Features

| Feature            | Description | Status |
|--------------------|-------------|--------|
|Customizable message  | Lets you fully controll Discord embeds |✅|
|Customizable Webhook user | Lets you overwrite the preset Discord Webhook names and images |✅|
|Multiple channel listener | Lets you select more than one Twitch channel which will raise events |✅|
|Multiple event nodes | Lets you create different messages for the same event listener |✅|
|Customizable events | Lets you choose which events should trigger messages |✅|
|Configurable threshold | Lets you configure a threshold which needs to be exceeded before posting another message (channel specifc) |✅|
|Hotloading | Will hotload modified settings without restarting the app |✅|
|Events | Adding multiple events like Follows, ect |⭕|
|Implement custom token generation | Will give you the option to set up the Client ID and Token using the app |📅|
|Local Webinterface | Will give you another way to set up the configuration and add a way to inspect the event history |❌|
|Grab a cup of coffee | Will give one clear sight |✅⭕📅|

<br/>

📅 => Planned  
✅ => Implemented  
⭕ => Currently being worked on  
 => Dropped

 To see which features are currently implemented please head over to the [project boards](https://github.com/xIRoXaSx/TwitchNotifier/projects)
***
<br/>

### Used Libs
| Name            | Description | Owner |
|--------------------|-------------|-------|
**`TwitchLib.Api`** | [Lib](https://github.com/TwitchLib/TwitchLib.Api) used to get information from Twitch | [swiftyspiffy](https://github.com/swiftyspiffy)  
**`YamlDotNet`** | [Lib](https://github.com/aaubry/YamlDotNet) used for writing and reading YAML files (config) | [aaubry](https://github.com/aaubry)  
**`Newtonsoft.Json`** | [Lib](https://github.com/JamesNK/Newtonsoft.Json) used in YamlDotNet and to (de)-serialize data | [JamesNK](https://github.com/JamesNK)
