using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Linq;

using DominikStiller.VertretungsplanServer.Models;

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
            timer = new Timer((s) =>
            {
                try
                {
                    LoadDataFromApi();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
            }, null, 0, 2000);
        }

        void LoadDataFromApi()
        {
            var json = client.GetAsync("/dates?metadata").Result.Content.ReadAsStringAsync().Result;

            var dates = JsonConvert.DeserializeObject<List<Vertretungsplan>>(json);
            foreach (var e in dates)
            {
                var date = e.Date;
                var dateExists = vertretungsplanRepository.Contains(date);
                // New or more recent than existing version
                if (!dateExists || e.LastUpdated > vertretungsplanRepository.Find(date).LastUpdated)
                {
                    json = client.GetAsync("/dates/" + date.ToString("yyyy-MM-dd")).Result.Content.ReadAsStringAsync().Result;
                    var vp = JsonConvert.DeserializeObject<Vertretungsplan>(json);
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

    public class DataLoaderOptions
    {
        public string ApiHost { get; set; }
    }
}
