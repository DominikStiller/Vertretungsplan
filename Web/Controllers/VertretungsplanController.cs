using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using DominikStiller.VertretungsplanServer.Models;

using static DominikStiller.VertretungsplanServer.Web.Helper.VertretungsplanHelper;

namespace DominikStiller.VertretungsplanServer.Web.Controllers
{
    public class VertretungsplanController : Controller
    {
        readonly VertretungsplanRepository cache;

        public VertretungsplanController(VertretungsplanRepository cache)
        {
            this.cache = cache;
        }

        [Route("/")]
        public IActionResult Students()
        {
            return View(GenerateViewModel(cache, VertretungsplanType.STUDENTS));
        }

        [Route("/ajax/students/{date}")]
        public IActionResult StudentsAjax(DateTime date)
        {
            return PartialView("Students", GenerateViewModel(cache, VertretungsplanType.STUDENTS, date));
        }

        [Route("/lehrer")]
        public IActionResult Teachers()
        {
            return View(GenerateViewModel(cache, VertretungsplanType.TEACHERS));
        }

        [Route("/ajax/teachers/{date}")]
        public IActionResult TeachersAjax(DateTime date)
        {
            return PartialView("Teachers", GenerateViewModel(cache, VertretungsplanType.TEACHERS, date));
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
