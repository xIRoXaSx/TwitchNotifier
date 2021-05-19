# TwitchNotifier 💬
## Description
Ever wanted to receive **customized** Discord messages when you or your favorite Twitch streamers went **live** without adding another bot to your **Discord** server?  
You came to the right place if this describes your problem!  
With TwitchNotifier you can customize which **events** should trigger messages for your Discord server, how they should look like!  
The setup is quickly done in a few minutes and the integrated **placeholders** will make your life even easier!  
Sounds too good to be true? Try it out and see! 😊
***
<br/>


### 📃 Configuration
For a detailed configuration please check out [<ARTIKLE>](<WIKI>)

***
<br/>

### 🔗 Placeholders
To view all available placeholders please check out [<ARTIKLE>](<WIKI>)
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
|Configurable threshold | Lets you configure a threshold which needs to be exceeded before posting another message (channel specifc) |⭕|
|Events | Adding multiple events like Follows, ect |⭕|
|Hotloading | Will hotload modified settings without restarting the app |📅|
|Implement custom token generation | Will give you the option to set up the Client ID and Token using the app |📅|
|Configurable Logging | Will give you the option to enable logging on specified events |📅|
|Local Webinterface | Will give you another way to set up the configuration and add a way to inspect the event history |📅|
|Grab a cup of coffee | Will give one clear sight |✅⭕📅|

<br/>

📅 => Planned  
✅ => Implemented  
⭕ => Currently being worked on  
❌ => Dropped
***
<br/>

### Used Libs
**`TwitchLib.Api`**: [API](https://github.com/TwitchLib/TwitchLib.Api) used to get information from Twitch. Owner: [swiftyspiffy](https://github.com/aaubry)  
**`YamlDotNet`**: [API](https://github.com/aaubry/YamlDotNet) used for writing and reading YAML files (config). Owner: [aaubry](https://github.com/aaubry)  
**`Newtonsoft.Json`**: [API](https://github.com/JamesNK/Newtonsoft.Json) used in YamlDotNet and to (de)-serialize data. Owner: [JamesNK](https://github.com/JamesNK)