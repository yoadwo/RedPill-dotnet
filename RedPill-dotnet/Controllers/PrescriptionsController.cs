using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedPill_dotnet.Controllers
{
    public class PrescriptionsController : Controller
    {
        // GET: Prescriptions
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read()
        {
            return View("ReadPres");
        }

        public ActionResult Write()
        {
            return View("WritePres");
        }
    }
}