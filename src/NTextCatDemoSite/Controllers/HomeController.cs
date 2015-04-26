using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Collections;
using System.IO;
using System.Globalization;
using NTextCat;

namespace NTextCatDemoSite.Controllers
{
    public class HomeController : Controller
    {
        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Lazy<Dictionary<string, string>> _code2LanguageName =
            new Lazy<Dictionary<string, string>>(LoadWikiCode2LanguageName, LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly static Lazy<RankedLanguageIdentifier> Identifier =
            new Lazy<RankedLanguageIdentifier>(CreateIdentifier, LazyThreadSafetyMode.ExecutionAndPublication);


        private static Dictionary<string, string> LoadWikiCode2LanguageName()
        {
            try
            {
                var languageMapPath = HostingEnvironment.MapPath("/ISO_639-2T.csv");
                if (languageMapPath == null)
                    throw new InvalidOperationException("HostingEnvironment.MapPath('Core14.profile.xml') has returned null!");
                var wikiCode2LanguageName =
                    System.IO.File.ReadLines(languageMapPath)
                    .Skip(1)
                    .Select(l => l.Split('\t'))
                    .ToDictionary(ar => ar[2], ar => ar[6]);
                return wikiCode2LanguageName;
            }
            catch (Exception ex)
            {
                // todo: log failure
                return new Dictionary<string, string>();
            }
        }

        private static RankedLanguageIdentifier CreateIdentifier()
        {

            var factory = new RankedLanguageIdentifierFactory();
            var identifierProfilePath =
                RankedLanguageIdentifierFactory.GetSetting("LanguageIdentificationProfileFilePath", (string)null);
            string mappedPath = null;
            if (identifierProfilePath != null && System.IO.File.Exists(identifierProfilePath) == false)
            {
                Log.DebugFormat("Cannot find a profile in the following path: '{0}'. Trying HostingEnvironment.MapPath", identifierProfilePath);
                mappedPath = HostingEnvironment.MapPath(identifierProfilePath);
            }
            var finalPath = mappedPath ?? identifierProfilePath;
            if (finalPath == null || System.IO.File.Exists(finalPath) == false)
            {
                Log.DebugFormat("Cannot find a profile in the following path: '{0}'.", finalPath);
                throw new InvalidOperationException(string.Format("Cannot find a profile in the following path: '{0}'.", finalPath));
            }
            var identifier = factory.Load(finalPath);
            return identifier;
        }

        public static string GetLanguages()
        {
            var dictionary = _code2LanguageName.Value;
            string name;
            var result =
                string.Join(", ",
                               Identifier.Value.LanguageModels
                               .Select(_ => _.Language.Iso639_2T)
                               .Select(
                                   code =>
                                       dictionary.TryGetValue(code, out name)
                                       ? string.Format("{0} ({1})", name, code.ToUpperInvariant())
                                       : code.ToUpperInvariant()));
            return result;
        }

        public static string GetAssemblyVersion()
        {
            var result = typeof(RankedLanguageIdentifier).Assembly.GetName().Version.ToString(3);
            return result;
        }

        public HomeController()
        {
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
            catch (Exception ex)
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

        private string DetectLanguage(string text)
        {
            //var languageIdentifier = new LanguageIdentifier(Server.MapPath("/Wikipedia-Experimental-UTF8Only/"));
            //var languageIdentifierSettings = new IvanAkcheurov.NTextCat.Lib.Legacy.LanguageIdentifier.LanguageIdentifierSettings(100);

            //var languages = languageIdentifier.ClassifyText(text, languageIdentifierSettings).ToList();
            //var mostCertainLanguage = languages.FirstOrDefault();

            var mostCertainLanguage = Identifier.Value.Identify(text).FirstOrDefault();

            if (mostCertainLanguage != null)
                return TryEnrichWithLanguageName(mostCertainLanguage.Item1.Iso639_2T);
            else
                return string.Empty;
        }

        public static string TryEnrichWithLanguageName(string code)
        {
            string languageName;
            if (_code2LanguageName.Value.TryGetValue(code, out languageName))
            {
                return string.Format("{0} ({1})", languageName, code.ToUpperInvariant());
            }
            return string.Format("\"{0}\" (ISO 639-2T)", code.ToUpperInvariant());
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
