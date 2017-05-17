﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using DominikStiller.VertretungsplanServer.Models;

using static DominikStiller.VertretungsplanServer.Web.Helper.VertretungsplanHelper;
using Microsoft.Extensions.Logging;

namespace DominikStiller.VertretungsplanServer.Web.Controllers
{
    public class VertretungsplanController : Controller
    {
        readonly VertretungsplanRepository vertretungsplanRepository;
        readonly ILogger logger;

        public VertretungsplanController(VertretungsplanRepository vertretungsplanRepository, ILogger<VertretungsplanController> logger)
        {
            this.vertretungsplanRepository = vertretungsplanRepository;
            this.logger = logger;
        }


        [Route("/")]
        [Route("/error")]
        public IActionResult Students()
        {
            return View(GenerateViewModel(vertretungsplanRepository, VertretungsplanType.STUDENTS));
        }

        [Route("/ajax/students/{date}")]
        public IActionResult StudentsAjax(DateTime date)
        {
            return PartialView("Students", GenerateViewModel(vertretungsplanRepository, VertretungsplanType.STUDENTS, date));
        }

        [Route("/lehrer")]
        public IActionResult Teachers()
        {
            return View(GenerateViewModel(vertretungsplanRepository, VertretungsplanType.TEACHERS));
        }

        [Route("/ajax/teachers/{date}")]
        public IActionResult TeachersAjax(DateTime date)
        {
            return PartialView("Teachers", GenerateViewModel(vertretungsplanRepository, VertretungsplanType.TEACHERS, date));
        }
    }

    public class VertretungsplanViewModel
    {
        public Vertretungsplan Vertretungsplan { get; set; }
        // Only used by Teachers because filtering and sorting Vertretungsplan.Entries changes all entries
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
