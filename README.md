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

<h3>The MIT License (MIT)</h3>
Copyright (c) 2017 Ivan Akcheurov

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
