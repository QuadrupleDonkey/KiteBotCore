﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace KiteBotCore.Modules.Music
{
    public class SoundEffect : CleansingModuleBase
    {
        private readonly DiscordSocketClient _client;
        private readonly IDependencyMap _map;

        public SoundEffect(IDependencyMap map)
        {
           _client = map.Get<DiscordSocketClient>();
            _map = map;
        }

        [Command("play",RunMode = RunMode.Mixed)]
        [Summary("What is love")]
        [RequireOwner, RequireContext(ContextType.Guild)]
        public async Task ArchiveCommand()
        {
            var channel = (Context.User as SocketGuildUser).VoiceChannel;
            Debug.Assert(channel != null);
            const string path = "D:\\Users\\sindr\\Downloads\\MarkovChristmas.mp3";
            try
            {
                using (var audioClient = await channel.ConnectAsync())
                using (var stream = audioClient.CreatePCMStream(2880,bitrate: channel.Bitrate))
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{path}\" -f s16le -ar 48000 -ac 2 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                    process.EnableRaisingEvents = true;
                    
                    await process.StandardOutput.BaseStream.CopyToAsync(stream);
                    process.WaitForExit();
                    await ReplyAsync("OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + ex.Message);
            }
        }
    }
}