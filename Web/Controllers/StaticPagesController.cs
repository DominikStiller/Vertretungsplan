﻿using Microsoft.AspNetCore.Mvc;

namespace DominikStiller.VertretungsplanServer.Web.Controllers
{
    public class StaticPagesController : Controller
    {
        [Route("/error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel() { StatusCode = HttpContext.Response.StatusCode });
        }
    }

    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
    }
}
