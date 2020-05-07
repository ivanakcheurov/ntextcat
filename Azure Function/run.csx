#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NTextCat;
using System.IO;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    //string name = req.Query["name"];

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    string text = data?.text;

    if (string.IsNullOrWhiteSpace(text))
        return new BadRequestObjectResult("the body has to be json object with property text which is not empty or null or whitespace");

    var factory = new RankedLanguageIdentifierFactory();
    var location = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.Contains("NTextCat"))?.Location;
    var path = Path.Combine(Directory.GetParent(location).Parent.Parent.FullName, "content", "Core14.profile.xml");
    var identifier = factory.Load(path); // can be an absolute or relative path. Beware of 260 chars limitation of the path length in Windows. Linux allows 4096 chars.
    var languages = identifier.Identify(text);
    var mostCertainLanguage = languages.FirstOrDefault();

    return (ActionResult)new OkObjectResult(mostCertainLanguage?.Item1.Iso639_3);
}