using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SmallBridge.Config
{
    public class MainConfig
    {
        [JsonProperty("discord_token")] public string DiscordToken { get; set; }


        [JsonProperty("irc_network_host")] public string IrcHost { get; set; }
        [JsonProperty("irc_network_port")] public int IrcPort { get; set; }
        [JsonProperty("irc_network_ssl")] public bool IrcSsl { get; set; }
        [JsonProperty("irc_nickname")] public string IrcNick { get; set; }
        [JsonProperty("irc_username")] public string IrcUserName { get; set; }
        [JsonProperty("irc_realname")] public string IrcRealName { get; set; }


        [JsonProperty("private_messages_discord_channel")] public ulong DiscordPmChannel { get; set; }
        [JsonProperty("private_messages_irc_channel")] public string IrcPmChannel { get; set; }


        [JsonProperty("channel_mapping")] public List<ChannelMappingConfig> ChannelMappings { get; set; }
        
        public static MainConfig LoadFromJsonFile(string path)
        {
            return JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(path));
        }
    }
}