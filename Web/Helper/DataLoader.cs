using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Linq;

using DominikStiller.VertretungsplanServer.Models;
using System.Threading.Tasks;

namespace DominikStiller.VertretungsplanServer.Web.Helper
{
    public class DataLoader
    {
        readonly VertretungsplanRepository vertretungsplanRepository;
        readonly ILogger logger;

        HttpClient client;
        // Keep reference to prevent GC
        Timer timer;

        public DataLoader(VertretungsplanRepository vertretungsplanRepository, ILogger<DataLoader> logger, IOptions<DataLoaderOptions> options)
        {
            this.vertretungsplanRepository = vertretungsplanRepository;
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

                var dates = JsonConvert.DeserializeObject<List<Vertretungsplan>>(json);
                foreach (var e in dates)
                {
                    var date = e.Date;
                    var dateExists = vertretungsplanRepository.Contains(date);
                    // New or more recent than existing version
                    if (!dateExists || e.LastUpdated > vertretungsplanRepository.Find(date).LastUpdated)
                    {
                        var dateResponse = await client.GetAsync("/dates/" + date.ToString("yyyy-MM-dd"));
                        var dateJson = await dateResponse.Content.ReadAsStringAsync();

                        var vp = JsonConvert.DeserializeObject<Vertretungsplan>(dateJson);
                        vertretungsplanRepository.Add(vp);
                    }
                }

                var oldDates = vertretungsplanRepository.GetAllDates().ToList().Except(dates.Select(vp => vp.Date));
                foreach (var vp in oldDates)
                {
                    vertretungsplanRepository.Remove(vp);
                }
            }
        }
    }

    public class DataLoaderOptions
    {
        public string ApiHost { get; set; }
    }
}
