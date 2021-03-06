﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KiteBotCore.Json.GiantBomb.Videos;
using Newtonsoft.Json;
using Serilog;

namespace KiteBotCore.Modules.Giantbomb
{
    public class VideoService
    {
        public Dictionary<int, Result> AllVideos { get; private set; }
        public bool IsReady { get; private set; }
        public static string JsonVideoFileLocation => Directory.GetCurrentDirectory() + "/Content/videos.json";
        private readonly string _apiCallUrl;

        public VideoService(string apiKey)
        {
            _apiCallUrl = $"http://www.giantbomb.com/api/videos/?api_key={apiKey}&format=json";
            var _ = Task.Run(InitializeTask);
        }

        private async Task InitializeTask()
        {
            if (File.Exists(JsonVideoFileLocation))
            {
                AllVideos = JsonConvert.DeserializeObject<Dictionary<int, Result>>(File.ReadAllText(JsonVideoFileLocation));
                Videos latest = await GetVideosEndpoint(0, 3);
                foreach (Result result in latest.Results.Where(x => AllVideos.All(y => y.Key != x.Id)))
                {
                    AllVideos[result.Id] = result;
                }
            }
            else
            {
                Log.Debug("Running full GB video list download");

                Videos latest = await GetVideosEndpoint(0, 3);
                AllVideos = new Dictionary<int, Result>(latest.NumberOfTotalResults + 250);
                for (int i = 0; i < latest.NumberOfPageResults; i++)
                {
                    AllVideos[latest.Results[i].Id] = latest.Results[i];
                }

                for (int offset = 100; offset < latest.NumberOfTotalResults; offset += 100)
                {
                    latest = await GetVideosEndpoint(offset, 3);
                    Log.Verbose("Queried GB videos API {one}/{two}", latest.NumberOfPageResults + offset * 100, latest.NumberOfTotalResults);
                    for (int i = 0; i < latest.NumberOfPageResults; i++)
                    {
                        AllVideos[latest.Results[i].Id] = latest.Results[i];
                    }
                }
            }
            IsReady = true;
            File.WriteAllText(JsonVideoFileLocation, JsonConvert.SerializeObject(AllVideos));
        }

        private async Task<Videos> GetVideosEndpoint(int offset, int retry)
        {
            await RateLimit.WaitAsync();
            var rateLimitTask = StartRatelimit();
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "KiteBotCore 1.1 GB Discord Bot for fetching wiki information");
                    return JsonConvert.DeserializeObject<Videos>(await client.GetStringAsync($@"{_apiCallUrl}&offset={offset}"));
                }
            }
            catch (Exception)
            {
                if (++retry < 3)
                {
                    await Task.Delay(5000);
                    await rateLimitTask;
                    return await GetVideosEndpoint(offset, retry);
                }
                throw new TimeoutException();
            }
        }

        private static readonly SemaphoreSlim RateLimit = new SemaphoreSlim(1, 1);

        private static async Task StartRatelimit()
        {
            await Task.Delay(1000);
            RateLimit.Release();
        }
    }
}
