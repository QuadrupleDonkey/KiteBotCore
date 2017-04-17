﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using System.Linq;
using Discord;

namespace KiteBotCore.Modules
{
    public class MarkovChain : CleansingModuleBase
    {
        [Command("testMarkov", RunMode = RunMode.Mixed)]
        [Alias("tm")]
        [Summary("Creates a Markov Chain string based on user messages")]
        [RequireServer(Server.KiteCo)]
        public async Task MarkovChainCommand(string haiku = null)
        {
            if (haiku == "haiku")
            {
                await ReplyAsync(KiteChat.MultiDeepMarkovChains.GetSequence() + "\n" +
                                 KiteChat.MultiDeepMarkovChains.GetSequence() + "\n" +
                                 KiteChat.MultiDeepMarkovChains.GetSequence() + "\n").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(KiteChat.MultiDeepMarkovChains.GetSequence()).ConfigureAwait(false);
            }
        }

        [Command("feed", RunMode = RunMode.Async)]
        [Summary("Downloads and feeds the markovchain")]
        [RequireOwner, RequireServer(Server.KiteCo)]
        public async Task FeedCommand(int amount)
        {
            var messagesTask = Context.Channel.GetMessagesAsync(amount).Flatten().ConfigureAwait(false);
            int i = 0;
            List<Message> list = await KiteChat.MultiDeepMarkovChains.GetFullDatabase(Context.Channel.Id).ConfigureAwait(false);
            var messages = await messagesTask;
            foreach(var msg in messages)
            {
                i++;
                if (list.All(x => x.Id != msg.Id))
                {
                    await KiteChat.MultiDeepMarkovChains.Feed(msg).ConfigureAwait(false);
                }
            }

            await KiteChat.MultiDeepMarkovChains.SaveAsync().ConfigureAwait(false);//TODO: Stare at this some more http://stackoverflow.com/questions/1930982/when-should-i-call-savechanges-when-creating-1000s-of-entity-framework-object
            await ReplyAsync($"{i} messages downloaded.").ConfigureAwait(false);
        }


        [Command("setdepth", RunMode = RunMode.Mixed)]
        [Summary("Downloads and feeds the markovchain")]
        [RequireOwner, RequireServer(Server.KiteCo)]
        public async Task SetDepthCommand(int depth)
        {
            KiteChat.MultiDeepMarkovChains.SetDepth(depth);
            
            await ReplyAsync($"👌").ConfigureAwait(false);
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Removes a message from the remote database by messageId")]
        [RequireOwner, RequireServer(Server.KiteCo)]
        public async Task RemoveCommand(ulong messageId)
        {
            List<Message> list = await KiteChat.MultiDeepMarkovChains.GetFullDatabase(0).ConfigureAwait(false);

            foreach (var item in list.Where(x => x.Id == messageId))
                await KiteChat.MultiDeepMarkovChains.RemoveItemAsync(item).ConfigureAwait(false);

            await ReplyAsync("<:bop:230275292076179460>").ConfigureAwait(false);
        }
    }
}
