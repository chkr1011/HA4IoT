using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HA4IoT.Homepage.Controllers
{
    public class OthersController : Controller
    {
        // GET: Others
        public ActionResult Impressum()
        {
            return View();
        }

	    public ActionResult Contact()
	    {
		    return View();
	    }
    }
}