using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.Mvc;

public class MvcController : Controller {

   [OutputCache(Duration = 60, VaryByParam = "none", Location = OutputCacheLocation.ServerAndClient)]
   public ActionResult Index() {

      this.ViewData["generated-at"] = DateTime.Now;

      return View();
   }

   [OutputCache(Location = OutputCacheLocation.None)]
   public ActionResult Ping() {
      return Content("hello", "text/plain");
   }
}
