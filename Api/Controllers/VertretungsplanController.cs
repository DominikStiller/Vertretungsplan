using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Api.Helper;
using DominikStiller.VertretungsplanServer.Helper;

namespace DominikStiller.VertretungsplanServer.Api.Controllers
{
    [Route("dates")]
    public class VertretungsplanController : Controller
    {
        readonly VertretungsplanRepository vertretungsplanRepository;
        readonly DataLoader dataLoader;
        readonly ILogger logger;
        readonly VertretungsplanControllerOptions options;


        public VertretungsplanController(VertretungsplanRepository vertretungsplanRepository, DataLoader dataLoader, ILogger<VertretungsplanController> logger, IOptions<VertretungsplanControllerOptions> options)
        {
            this.vertretungsplanRepository = vertretungsplanRepository;
            this.dataLoader = dataLoader;
            this.logger = logger;
            this.options = options.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            IEnumerable<Vertretungsplan> vps = vertretungsplanRepository.GetAll();
            if (Request.Query.ContainsKey("hidepast"))
                vps = vps.Where(vp => vp.Date >= DateTime.Now.Date);

            // List dates
            if (Request.Query.ContainsKey("metadata"))
                return Ok(vps.Select(
                    vp => new { vp.Date, vp.Version, vp.LastUpdated }
                ));
            // Show data (all dates)
            else
                return Ok(vps);
        }

        // Show data (single date)
        [HttpGet("{date}")]
        public IActionResult Get(DateTime date)
        {
            var vertretungsplan = vertretungsplanRepository.Find(date);
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
            return BasicAuthentication.Auth("update", options.UpdatePassword, HttpContext, () => dataLoader.LoadDataFromS3());
        }
    }

    public class VertretungsplanControllerOptions
    {
        public string UpdatePassword { get; set; }
    }
}
