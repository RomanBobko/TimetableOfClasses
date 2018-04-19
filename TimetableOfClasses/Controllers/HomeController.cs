using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimetableOfClasses.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Timetables";

            return View();
        }

        public ActionResult Vue()
        {
            ViewBag.Title = "Timetables Vue";

            return View();
        }

        public ActionResult React()
        {
            ViewBag.Title = "Timetables React";

            return View();
        }
    }
}
