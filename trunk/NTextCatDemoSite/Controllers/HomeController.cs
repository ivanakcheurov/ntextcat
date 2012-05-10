using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public HomeController()
        {
            _wikiCode2LanguageName = new Lazy<Dictionary<string, string>>(LoadWikiCode2LanguageName, LazyThreadSafetyMode.ExecutionAndPublication); ;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }
        [HttpPost]
        public ActionResult Index(string text)
        {
            Log.DebugFormat("Language identification of text (ip: {0}): {1}", GetIpAddress(), text);
            try
            {
                var language = DetectLanguage(text);
                Log.DebugFormat("Language identification completed with result: {0}", language);
                return Json(language);
            }
            catch(Exception ex)
            {
                Log.Error("Language identification caused an error", ex);
                throw;
            }
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
                return TryEnrichWithLanguageName(mostCertainLanguage.Item1);
            else
                return string.Empty;
        }

        public string TryEnrichWithLanguageName(string code)
        {
            string languageName;
            if (_wikiCode2LanguageName.Value.TryGetValue(code, out languageName))
            {
                return string.Format("{0} ({1})", languageName, code);
            }
            return string.Format("Wikicode: {0}", code);
        }

        private Lazy<Dictionary<string, string>> _wikiCode2LanguageName;
            

        private Dictionary<string, string> LoadWikiCode2LanguageName()
        {
            try
            {
                var wikiCode2LanguageName = 
                    System.IO.File.ReadLines(Server.MapPath("/wiki_languages.txt"))
                    .Select(l => l.Split('\t'))
                    .ToDictionary(ar => ar[3], ar => ar[1]);
                return wikiCode2LanguageName;
            }
            catch(Exception ex)
            {
                // todo: log failure
                return new Dictionary<string, string>();
            }
        }

        private string GetIpAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;

            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}
