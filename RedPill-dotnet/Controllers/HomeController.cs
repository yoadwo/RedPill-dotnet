using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedPill_dotnet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            System.Diagnostics.Trace.WriteLine("In Home Controller");
            return View();
        }
    }
}
