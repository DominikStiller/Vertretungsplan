﻿using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Helper;
using DominikStiller.VertretungsplanServer.Web.Helper;

namespace DominikStiller.VertretungsplanServer.Web.Controllers
{
    [NoCacheHeader()]
    public class VertretungsplanController : Controller
    {
        readonly VertretungsplanRepository cache;
        readonly VertretungsplanHelper helper;
        readonly ResponseCachingHelper cachingHelper;

        public VertretungsplanController(VertretungsplanRepository cache, VertretungsplanHelper helper, ResponseCachingHelper cachingHelper)
        {
            this.cache = cache;
            this.helper = helper;
            this.cachingHelper = cachingHelper;
        }

        [Route("/")]
        public IActionResult Students()
        {
            var result = cachingHelper.UseETag(helper.GenerateETag());
            if (result == null)
            {
                var nearestDate = cache.FindNearest(VertretungsplanTime.Now);
                result = View(helper.GenerateViewModel(VertretungsplanType.STUDENTS, nearestDate));
            }

            return result;
        }

        [Route("/ajax/students/{date}")]
        public IActionResult StudentsAjax(DateTime date)
        {
            if (cache.Contains(date))
            {
                var lastModified = helper.GenerateLastModified(date);
                return cachingHelper.UseLastModified(lastModified) ?? PartialView("Students", helper.GenerateViewModel(VertretungsplanType.STUDENTS, date));
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
        }

        [Route("/lehrer")]
        public IActionResult Teachers()
        {
            var result = cachingHelper.UseETag(helper.GenerateETag());
            if (result == null)
            {
                var nearestDate = cache.FindNearest(VertretungsplanTime.Now);
                result = View(helper.GenerateViewModel(VertretungsplanType.TEACHERS, nearestDate));
            }

            return result;
        }

        [Route("/ajax/teachers/{date}")]
        public IActionResult TeachersAjax(DateTime date)
        {
            if (cache.Contains(date))
            {

                return cachingHelper.UseLastModified(date) ?? PartialView("Teachers", helper.GenerateViewModel(VertretungsplanType.TEACHERS, date));
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
        }
    }

    public class VertretungsplanViewModel
    {
        public Vertretungsplan Vertretungsplan { get; set; }
        // Only used by Teachers because filtering and sorting Vertretungsplan.Entries directly would change all entries in VertretungsplanRepository
        public List<Entry> Entries { get; set; }

        public string LastUpdatedInWords { get; set; }

        public List<string> Dates { get; set; }
        public DateTime PreviousDate { get; set; }
        public DateTime NextDate { get; set; }
        public List<SelectListItem> DateSelectorItems { get; set; }

        public string DateformatInternal { get; set; }
        public string DateformatPublic { get; set; }
    }
}
