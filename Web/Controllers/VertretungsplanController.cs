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
        readonly VertretungsplanRepository vertretungsplanRepository;

        public VertretungsplanController(VertretungsplanRepository vertretungsplanRepository)
        {
            this.vertretungsplanRepository = vertretungsplanRepository;
        }

        [Route("/")]
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

        [Route("/error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel() { StatusCode = HttpContext.Response.StatusCode });
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

    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
    }
}
