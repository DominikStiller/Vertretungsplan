using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Api.Helper;
using DominikStiller.VertretungsplanServer.Helper;

namespace DominikStiller.VertretungsplanServer.Api.Controllers
{
    [Route("dates")]
    public class VertretungsplanController : Controller
    {
        readonly VertretungsplanRepository cache;
        readonly DataLoader dataLoader;
        readonly Notifier notifier;
        readonly VertretungsplanControllerOptions options;


        public VertretungsplanController(VertretungsplanRepository cache, DataLoader dataLoader, Notifier notifier, IOptions<VertretungsplanControllerOptions> options)
        {
            this.cache = cache;
            this.dataLoader = dataLoader;
            this.notifier = notifier;
            this.options = options.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            IEnumerable<Vertretungsplan> vps = cache.GetAll();
            if (Request.Query.ContainsKey("hidepast"))
                vps = vps.Where(vp => vp.Date >= VertretungsplanTime.Now.Date);

            // List dates
            if (Request.Query.ContainsKey("metadata"))
                return Ok(vps.Select(vp => new VertretungsplanMetadata(vp)));
            // Show data (all dates)
            else
                return Ok(vps);
        }

        // Show data (single date)
        [HttpGet("{date}")]
        public IActionResult Get(DateTime date)
        {
            var vertretungsplan = cache.Find(date);
            if (vertretungsplan == null)
            {
                return NotFound();
            }
            return Ok(vertretungsplan);
        }

        // Update data
        [HttpPost]
        public IActionResult Post()
        {
            return BasicAuthentication.Auth("update", options.UpdatePassword, HttpContext, async () =>
            {
                var metadataBeforeUpdate = cache.GetAll().Select(vp => new VertretungsplanMetadata(vp)).ToList();
                await dataLoader.LoadDataFromS3();
                var metadataAfterUpdate = cache.GetAll().Select(vp => new VertretungsplanMetadata(vp)).ToList();

                // Only send notification if data has changed
                if (!metadataAfterUpdate.SequenceEqual(metadataBeforeUpdate))
                {
                    await notifier.NotifyFCM();
                }
            });
        }
    }

    public class VertretungsplanControllerOptions
    {
        public string UpdatePassword { get; set; }
    }
}
