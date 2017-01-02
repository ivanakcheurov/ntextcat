# ntextcat
<img src=https://ci.appveyor.com/api/projects/status/2to4pti81u4lpj7q?retina=true>

    PM> Install-Package NTextCat
    
Please try it out: **<a href="http://ivanakcheurov.github.io/ntextcat/">Online DEMO</a>**

<a href="http://www.nuget.org/packages/NTextCat/">NuGet</a>

NTextCat 0.2.1
* Recommended length of a text snippet has been reduced to 5 (though mostly a single word is handled correctly).
* Simplified and made more consistent API.
* Fixed NaiveBayesLanguageIdentifier so that it performs as good as RankedLanguageIdentifier
* NTextCat.exe provides the main command line interface from now on (it's command line API may be changed in several subsequent releases).
* Much better support for asian languages.
* Based on the feedback, a set of 14 the most popular languages has been selected. It has become a default. The set: Chinese, Danish, Dutch, English, French, German, Italian, Japanese, Korean, Norwegian, Portugese, Russian, Spanish, Swedish
* SqlServerClrIntegration is not in the release yet. It will be reintroduced in one of the next releases recompiled and verified for SQL Server 2012.
* Fixed a bug in GaussianBag
* More rigid testing routines as preparations to produce a stable release.
