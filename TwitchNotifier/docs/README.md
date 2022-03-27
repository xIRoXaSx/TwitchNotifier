# TwitchNotifier 💬
## Description
Ever wanted to receive **customized** Discord messages one certain Twitch events (eg: **live**, **offline**, ...) without adding another bot to your **Discord** server?  
You came to the right place if this describes your problem!  
With TwitchNotifier you can customize which events should trigger notifications for your Discord server and **how** they should look like!  
The setup is quickly done in a few minutes and the integrated **placeholders** will make your life even easier!  
Sounds too good to be true? Try it out and see! 😊

![DiscordEmbed](https://user-images.githubusercontent.com/38859398/122478113-1a352a00-cfc9-11eb-9d6d-8d2616876627.png)
***
<br/>

### 📃 Configuration
For a detailed configuration please check out the [Configuration](https://github.com/xIRoXaSx/TwitchNotifier/wiki/Configuration) guide.
***
<br/>

### 🔗 Placeholders
To view all available placeholders please check out the [Placeholders](./placeholders) guide.
***
<br/>

### 📅 Features

| Feature                           | Description                                                                                                 | Status |
|-----------------------------------|-------------------------------------------------------------------------------------------------------------|--------|
| Customizable message              | Lets you fully customize Discord embeds                                                                     | ✅      |
| Customizable Webhook user         | Lets you overwrite the default Discord Webhook names and images                                             | ✅      |
| Multiple channel listener         | Lets you select more than just one Twitch channel which will raise events                                   | ✅      |
| Multiple event nodes              | Lets you create different messages for the same event                                                       | ✅      |
| Customizable events               | Lets you choose which events should trigger messages                                                        | ✅      |
| Configurable threshold            | Lets you configure a threshold which needs to be exceeded before posting another message (channel specific) | ✅      |
| Hot-Loading                       | Will hot-load modified settings without restarting the app                                                  | ✅      |
| Implement custom token generation | Will give you the option to set up the Client ID and Token using the app                                    | 📅     |
| Grab a cup of coffee              | Will give one clear sight                                                                                   | ✅⭕📅   |
<br/>

📅 => Planned  
✅ => Implemented  
⭕ => Currently being worked on

To see which features are currently in the making, please head over to the [project boards](https://github.com/xIRoXaSx/TwitchNotifier/projects).
Current implemented events can be viewed [here](./configuration#event-nodes)
***
<br/>

### Used Libs
| Name                  | Description                                                                                     | Owner / Maintainer                                  |
|-----------------------|-------------------------------------------------------------------------------------------------|-----------------------------------------------------|
| **`TwitchLib.Api`**   | [Lib](https://github.com/TwitchLib/TwitchLib.Api) used to get information from Twitch           | [swiftyspiffy](https://github.com/swiftyspiffy)     |
| **`YamlDotNet`**      | [Lib](https://github.com/aaubry/YamlDotNet) used for writing and reading YAML files (config)    | [aaubry](https://github.com/aaubry)                 |
| **`Newtonsoft.Json`** | [Lib](https://github.com/JamesNK/Newtonsoft.Json) used in YamlDotNet and to (de)-serialize data | [JamesNK](https://github.com/JamesNK)               |
| **`BenchMarkDotNet`** | [Lib](https://github.com/dotnet/BenchmarkDotNet) used for benchmarking methods.                 | [AndreyAkinshin](https://github.com/AndreyAkinshin) |