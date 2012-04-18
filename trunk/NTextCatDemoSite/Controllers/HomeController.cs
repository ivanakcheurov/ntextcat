using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IvanAkcheurov.NTextCat.Lib.Legacy;
using System.Collections;
using System.IO;
using System.Globalization;

namespace NTextCatDemoSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }
        [HttpPost]
        public ActionResult Index(string text)
        {
            var language = DetectLanguage(text);
            return Json(language);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your quintessential app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your quintessential contact page.";

            return View();
        }

        private string DetectLanguage(string Text)
        {
            var languageIdentifier = new LanguageIdentifier(Server.MapPath("/Wikipedia-Experimental-UTF8Only/"));
            var languageIdentifierSettings = new IvanAkcheurov.NTextCat.Lib.Legacy.LanguageIdentifier.LanguageIdentifierSettings(100);

            var languages = languageIdentifier.ClassifyText(Text, languageIdentifierSettings).ToList();
            var mostCertainLanguage = languages.FirstOrDefault();
            if (mostCertainLanguage != null)
                return GetCountryName(mostCertainLanguage.Item1);
            else
                return string.Empty;
        }

        public string GetCountryName(string code)
        {
            try
            {
                CultureInfo c = CultureInfo.CreateSpecificCulture(code);
                return c.DisplayName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
    }
}
