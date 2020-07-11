using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SmallBridge.Config;
using smIRCL.Config;
using smIRCL.Core;
using smIRCL.Extensions;
using smIRCL.ServerEntities;

namespace SmallBridge
{
    class Program
    {
        //Bot permissions 536988736

        private static DiscordClient _discord;
        private static IrcController _controller;
        private static MainConfig _config;

        static void Main(string[] args)
        {
            _config = MainConfig.LoadFromJsonFile("./config.json");

            List<string> autoJoinChannels = new List<string>();

            foreach (ChannelMappingConfig channelMapping in _config.ChannelMappings)
            {
                if (autoJoinChannels.Any(ch => ch.ToIrcLower() == channelMapping.IrcChannel.ToIrcLower())) continue;

                autoJoinChannels.Add(channelMapping.IrcChannel);
            }

            IrcConnector connector = new IrcConnector(new IrcConfig
            {
                ServerHostname = _config.IrcHost,
                ServerPort = _config.IrcPort,
                Nick = _config.IrcNick,
                UserName = _config.IrcUserName,
                RealName = _config.IrcRealName,
                UseSsl = _config.IrcSsl,
                AutoJoinChannels = autoJoinChannels
            });

            connector.MessageReceived += IrcControlMessageReceived;
            connector.MessageTransmitted += IrcControlMessageTransmitted;

            _controller = new IrcController(connector);

            _controller.PrivMsg += IrcIncomingMessage;

            DiscordAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task DiscordAsync(string[] args)
        {
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.DiscordToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            _discord.MessageCreated += DiscordIncomingMessage;

            await _discord.ConnectAsync();
            _controller.Connector.Connect();
            await Task.Delay(-1);
        }

        private static void IrcControlMessageTransmitted(string rawMessage)
        {
            _discord.DebugLogger.LogMessage(LogLevel.Debug, "smIRCL", $"[Rx] {rawMessage}", DateTime.Now);
        }

        private static void IrcControlMessageReceived(string rawMessage, IrcMessage message)
        {
            _discord.DebugLogger.LogMessage(LogLevel.Debug, "smIRCL", $"[Tx] {rawMessage}", DateTime.Now);
        }

        private static void IrcIncomingMessage(IrcController controller, IrcMessage message)
        {
            ChannelMappingConfig channelMapping = _config.ChannelMappings.FirstOrDefault(mapping => mapping.IrcChannel.ToIrcLower() == message.Parameters[0].ToIrcLower());

            if (channelMapping != null)
            {
                DiscordChannel channel = _discord.GetChannelAsync(channelMapping.DiscordChannel).GetAwaiter().GetResult();

                if (channel != null) channel.SendMessageAsync($"**[IRC]** <*{message.SourceNick}*> {message.Parameters[1]}");
            }
        }

        private static async Task DiscordIncomingMessage(MessageCreateEventArgs discordMessage)
        {
            if (discordMessage.Author.IsBot) return;

            ChannelMappingConfig channelMapping = _config.ChannelMappings.FirstOrDefault(mapping => mapping.DiscordChannel == discordMessage.Channel.Id);

            if (channelMapping != null && _controller.Channels.Any(ch => ch.Name.ToIrcLower() == channelMapping.IrcChannel.ToIrcLower()))
            {
                _controller.SendPrivMsg(channelMapping.IrcChannel, $"[Discord] <{discordMessage.Message.Author.Username}#{discordMessage.Message.Author.Discriminator}> {discordMessage.Message.Content}");
            }
        }
    }
}
