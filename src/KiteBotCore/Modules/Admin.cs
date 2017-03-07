﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.IO;
using System.Text;
using Discord.Rest;
using Newtonsoft.Json;

namespace KiteBotCore.Modules
{
    public class Admin : CleansingModuleBase
    {
        private readonly CommandService _handler;
        private readonly IDependencyMap _map;

        public Admin(IDependencyMap map)
        {
            _handler = map.Get<CommandService>();
            _map = map;
        }

        [Command("archive")]
        [Summary("archives a channel and uploads a JSON")]
        [RequireOwner]
        public async Task ArchiveCommand(string guildName, string channelName, int amount = 10000)
        {
            var channelToArchive = (await
                (await Context.Client.GetGuildsAsync())
                .FirstOrDefault(x => x.Name == guildName)
                .GetTextChannelsAsync())
                .FirstOrDefault(x => x.Name == channelName);

            if (channelToArchive != null)
            {
                var listOfMessages = new List<IMessage>(await channelToArchive.GetMessagesAsync(amount).Flatten());
                List<Message> list = new List<Message>(listOfMessages.Capacity);
                foreach (var message in listOfMessages)
                    list.Add(new Message { Author = message.Author.Username, Content = message.Content, Timestamp = message.Timestamp });
                var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var json = JsonConvert.SerializeObject(list, Formatting.Indented, jsonSettings);
                await Context.Channel.SendFileAsync(GenerateStreamFromString(json), $"{channelName}.json");
            }
        }

        public class Message
        {
            public string Author;
            public string Content;
            public DateTimeOffset Timestamp;
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));
        }

        [Command("save")]
        [Summary("saves markov chain messages")]
        [RequireOwner]
        public async Task SaveCommand()
        {
            var message = await ReplyAsync("OK");
            var saveTask = KiteChat.MultiDeepMarkovChains.SaveAsync();
            await saveTask.ContinueWith(async (e) =>
            {
                if (e.IsCompleted) await message.ModifyAsync(x => x.Content += ", Saved.");
            });
        }

        [Command("saveexit")]
        [Alias("se")]
        [Summary("saves and exits")]
        [RequireOwner]
        public async Task SaveExitCommand()
        {
            var message = await ReplyAsync("OK");
            var saveTask = KiteChat.MultiDeepMarkovChains.SaveAsync();
            await saveTask.ContinueWith(async (e) =>
            {
                if (e.IsCompleted) await message.ModifyAsync(x => x.Content += ", Saved.");
            });
            Environment.Exit(0);
        }

        [Command("update")]
        [Alias("up")]
        [Summary("Updates the livestream channel, and probably crashes if there is no chat")]
        [RequireOwner]
        public async Task UpdateCommand()
        {
            await KiteChat.StreamChecker.ForceUpdateChannel();
            await ReplyAsync("updated?");
        }

        [Command("delete")]
        [Alias("del")]
        [Summary("Deletes the last message the bot has written")]
        [RequireOwner]
        public async Task DeleteCommand()
        {
            if (KiteChat.BotMessages.Any()) await ((IUserMessage)KiteChat.BotMessages.Last()).DeleteAsync();
        }

        [Command("restart")]
        [Alias("re")]
        [Summary("restarts the video and livestream checkers")]
        [RequireOwner]
        public async Task RestartCommand()
        {
            KiteChat.StreamChecker?.Restart();
            KiteChat.GbVideoChecker?.Restart();
            await ReplyAsync("It might have done something, who knows.");
        }

        [Command("ignore")]
        [Summary("ignore a gb chat channelname")]
        [RequireOwner]
        public async Task IgnoreCommand([Remainder] string input)
        {
            KiteChat.StreamChecker.IgnoreChannel(input);
            await ReplyAsync("Added to ignore list.");
        }

        [Command("listchannels")]
        [Summary("Lists names of GB chats")]
        [RequireOwner]
        public async Task ListChannelCommand()
        {

            await ReplyAsync("Current livestreams channels are:" + Environment.NewLine + (await KiteChat.StreamChecker.ListChannels()));
        }

        [Command("say")]
        [Alias("echo")]
        [Summary("Echos the provided input")]
        [RequireOwner]
        public async Task SayCommand([Remainder] string input)
        {
            await ReplyAsync(input);
        }

        [Command("embed")]
        [Summary("Echos the provided input")]
        [RequireOwner]
        public async Task EmbedCommand([Remainder] string input)
        {
            var embed = new EmbedBuilder
            {
                Title = "Test",
                Color = new Color(255, 0, 0),
                Description = input
            };
            await ReplyAsync("", false, embed);
        }

        [Command("setgame")]
        [Alias("playing")]
        [Summary("Sets a game in discord")]
        [RequireOwner]
        public async Task PlayingCommand([Remainder] string input)
        {
            var client = _map.Get<DiscordSocketClient>();
            await client.SetGameAsync(input);
        }

        [Command("setusername")]
        [Alias("username")]
        [Summary("Sets a new username for discord")]
        [RequireOwner]
        public async Task UsernameCommand([Remainder] string input)
        {
            var client = _map.Get<DiscordSocketClient>();
            await client.CurrentUser.ModifyAsync(x => x.Username = input);
        }

        [Command("setnickname")]
        [Alias("nickname")]
        [Summary("Sets a game in discord")]
        [RequireOwner, RequireContext(ContextType.Guild)]
        public async Task NicknameCommand([Remainder] string input)
        {
            await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = input);
        }

        [Command("setavatar", RunMode = RunMode.Sync)]
        [Alias("avatar")]
        [Summary("Sets a new avatar image for this bot")]
        [RequireOwner]
        public async Task AvatarCommand([Remainder] string input)
        {
            var avatarStream = await new HttpClient().GetByteArrayAsync(input);
            Stream stream = new MemoryStream(avatarStream);
            await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(stream));
            await ReplyAsync("👌");
        }

        [Command("help")]
        [Summary("Lists available commands")]
        public async Task Help(string optional = null)
        {
            string output = "";
            if (optional != null)
            {
                var command = _handler.Commands.FirstOrDefault(x => x.Aliases.Any(y => y.Equals(optional.ToLower())));
                if (command != null)
                {
                    output += $"Command: {string.Join(", ", command.Aliases)}: {Environment.NewLine}";
                    output += command.Summary;
                    await ReplyAsync(output + ".");
                    return;
                }
                output += "Couldn't find a command with that name, givng you the commandlist instead:" +
                          Environment.NewLine;
            }
            foreach (CommandInfo cmdInfo in _handler.Commands.OrderBy(x => x.Aliases[0]))
            {
                if ((await cmdInfo.CheckPreconditionsAsync(Context, _map)).IsSuccess)
                {
                    if (!string.IsNullOrWhiteSpace(output)) output += ",";
                    output += "`" + cmdInfo.Aliases[0] + "`";
                }
            }
            output += "." + Environment.NewLine;
            await ReplyAsync("These are the commands you can use: " + Environment.NewLine + output + "Run help <command> for more information.");
        }

        [Command("info")]
        [Summary("Contains info about the bot, such as owner, library, and runtime information")]
        public async Task Info()
        {
            string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

            string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);

            var application = await Context.Client.GetApplicationInfoAsync();

            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username}#{application.Owner.DiscriminatorValue} (ID {application.Owner.Id})\n" +
                $"- Source Code: <https://github.com/LassieME/KiteBotCore>\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- OS: {RuntimeInformation.OSDescription}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient)?.Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient)?.Guilds.Sum(g => g.Channels.Count)}" +
                $"- Users: {(Context.Client as DiscordSocketClient)?.Guilds.Sum(g => g.Users.Count)}"
            );
        }
    }
}