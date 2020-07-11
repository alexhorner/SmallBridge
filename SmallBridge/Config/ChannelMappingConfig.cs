using Newtonsoft.Json;

namespace SmallBridge.Config
{
    public class ChannelMappingConfig
    {
        [JsonProperty("irc_channel")] public string IrcChannel { get; set; }
        [JsonProperty("discord_channel")] public ulong DiscordChannel { get; set; }
    }
}
