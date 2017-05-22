using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using DominikStiller.VertretungsplanServer.Models;

namespace DominikStiller.VertretungsplanServer.Web.Helper
{
    public class DataLoader
    {
        readonly VertretungsplanRepository cache;
        readonly ILogger logger;

        HttpClient client;
        // Keep reference to prevent GC
        Timer timer;

        public DataLoader(VertretungsplanRepository cache, ILogger<DataLoader> logger, IOptions<DataLoaderOptions> options)
        {
            this.cache = cache;
            this.logger = logger;

            client = new HttpClient();
            client.BaseAddress = new Uri(options.Value.ApiHost);
        }

        public void Start()
        {
            timer = new Timer(async (s) =>
            {
                try
                {
                    await LoadDataFromApi();
                }
                catch (Exception e)
                {
                    logger.LogError($"ERROR while loading data\n{e}");
                }
            }, null, 0, 2000);
        }

        async Task LoadDataFromApi()
        {
            var response = await client.GetAsync("/dates?metadata");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var dates = JsonConvert.DeserializeObject<List<VertretungsplanMetadata>>(json);
                foreach (var loaded in dates)
                {
                    var cached = cache.Find(loaded.Date);
                    var dateExists = cached != null;

                    // New or more recent than existing version
                    if (!dateExists || loaded.LastUpdated > cached.LastUpdated || loaded.Version > cached.Version)
                    {
                        var dateResponse = await client.GetAsync("/dates/" + loaded.Date.ToString("yyyy-MM-dd"));
                        var dateJson = await dateResponse.Content.ReadAsStringAsync();

                        var vp = JsonConvert.DeserializeObject<Vertretungsplan>(dateJson);
                        cache.Add(vp);
                    }
                }

                var oldDates = cache.GetAllDates().Except(dates.Select(vp => vp.Date)).ToList();
                foreach (var old in oldDates)
                {
                    cache.Remove(old);
                }
            }
        }
    }

    public class DataLoaderOptions
    {
        public string ApiHost { get; set; }
    }
}
