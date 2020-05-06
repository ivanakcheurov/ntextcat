# ntextcat

| Appveyor | NuGet | License |
|----------|-------| --------|
|[![Build status](https://ci.appveyor.com/api/projects/status/github/ivanakcheurov/NTextCat?svg=true)](https://ci.appveyor.com/project/ivanakcheurov/NTextCat/branch/master) |[![NuGet](https://img.shields.io/nuget/v/NTextCat.svg)](https://www.nuget.org/packages/NTextCat/) [![Usage](https://img.shields.io/nuget/dt/NTextCat.svg)](https://www.nuget.org/stats/packages/NTextCat?groupby=Version) |[![License](https://img.shields.io/github/license/ivanakcheurov/NTextCat.svg)](https://github.com/ivanakcheurov/NTextCat/blob/master/license.MIT)|

## Why NTextCat?
- *NTextCat* helps to recognize (identify) the language of a given text (e.g. read a sentence and say it is *Italian*). 
- *NTextCat* can also be used for text classification (e.g. read a paragraph and say it belongs to *Sports* category).

Try it out yourself: [ONLINE DEMO](https://ivanakcheurov.github.io/ntextcat/).
Recommended input: a snippet of text with at least 5 words (though it works quite OK with just a couple of words). 

## How to use
*NTextCat* supports .NET Standard 2.0.
Just install the [NTextCat NuGet package](https://www.nuget.org/packages/NTextCat/):
```
dotnet add package NTextCat
```


Then we can use *NTextCat* to detect the language of a text.
```csharp
using NTextCat;
...
// Don't forget to deploy a language profile (e.g. Core14.profile.xml) with your application.
// (take a look at "content" folder inside of NTextCat nupkg and here: https://github.com/ivanakcheurov/ntextcat/tree/master/src/LanguageModels).
var factory = new RankedLanguageIdentifierFactory();
var identifier = factory.Load("Core14.profile.xml"); // can be an absolute or relative path. Beware of 260 chars limitation of the path length in Windows. Linux allows 4096 chars.
var languages = identifier.Identify("your text to get its language identified");
var mostCertainLanguage = languages.FirstOrDefault();
if (mostCertainLanguage != null)  
    Console.WriteLine("The language of the text is '{0}' (ISO639-3 code)", mostCertainLanguage.Item1.Iso639_3);  
else 
    Console.WriteLine("The language couldn’t be identified with an acceptable degree of certainty");

// outputs: The language of the text is 'eng' (ISO639-3 code)
```


