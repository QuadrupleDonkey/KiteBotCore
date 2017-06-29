﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;

namespace KiteBotCore.Modules
{
    public class Misc : ModuleBase
    {
        [Command("420")]
        [Alias("#420", "blaze", "waifu")]
        [Summary("Anime and weed, all you need.")]
        public async Task FourTwentyCommand()
        {
            await ReplyAsync("http://420.moe/").ConfigureAwait(false);
        }

        [Command("archive")]
        [Alias("gbarchive")]
        [Summary("Links you the GB livestream archive")]
        public async Task GBArchiveCommand()
        {
            await ReplyAsync("http://www.giantbomb.com/videos/embed/8635/?allow_gb=yes").ConfigureAwait(false);
        }

        [Command("hi", RunMode = RunMode.Async)]
        [Summary("Mentions you and says hello")]
        public async Task HiCommand()
        {
            var msg = await ReplyAsync($"{Context.User.Mention} Heyyo!").ConfigureAwait(false);

            try
            {
                ReactionCallbackBuilder rcb = new ReactionCallbackBuilder();
                await rcb
                    .WithClient(Context.Client)
                    .WithPrecondition(x => x.Id == Context.User.Id)
                    .WithReactions("📱")
                    .AddCallback("📱", async func =>
                    {
                        await msg.ModifyAsync(message => message.Content = "poop").ConfigureAwait(false);
                    })
                    .WithTimeout(120000)
                    .ExecuteAsync(msg)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [Command("randomql")]
        [Alias("randql")]
        [Summary("Posts a random quick look.")]
        public async Task Command()
        {
            await ReplyAsync(await GetResponseUriFromRandomQlCrew().ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task<string> GetResponseUriFromRandomQlCrew()
        {
            string url = "http://qlcrew.com/main.php?anyone=anyone&inc%5B0%5D=&p=999&exc%5B0%5D=&per_page=15&random";

            var request = (HttpWebRequest)WebRequest.Create(url);
            if (request != null)
            {
                HttpWebResponse response = await request.GetResponseAsync().ConfigureAwait(false) as HttpWebResponse;
                return response?.ResponseUri.AbsoluteUri;
            }
            return "Couldn't load QLcrew's Random Link.";
        }
    }
}