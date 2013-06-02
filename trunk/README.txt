NTextCat 0.2.1
http://ntextcat.codeplex.com

NTextCat is a text classification utility (tool and API).
The primary target is language identification. So it helps you to recognize (identify) the language of a text snippet.

Languages available out of the box
-----------------------------------

Compatible with NTextCat.exe:
* Core14.profile.xml: 14 languages considered being most interesting: Chinese, Danish, Dutch, English, French, German, Italian, Japanese, Korean, Norwegian, Portugese, Russian, Spanish, Swedish. THE DEFAULT..
* Wikipedia82.profile.xml: 82 most popular languages extracted from *.Wikipedia.org. Warning: a wiki code is populated as ISO 639-2T and ISO 639-3 which is not correct! Will be fixed soon.
* Wikipedia280.profile.xml: 280+ languages (and flavors) extracted from *.Wikipedia.org. Warning: a wiki code is populated as ISO 639-2T and ISO 639-3 which is not correct! Will be fixed soon.

Compatible with NTextCatLegacy.exe (WILL NOT BE INCLUDED IN RELEASES IN THE FUTURE):
* Wikipedia-MostCommon-Legacy__All-Utf8: 280+ languages (and flavors) extracted from *.Wikipedia.org and encoded in UTF-8. Additionally 83 most popular lanugages encoded in their respective "legacy" encodings (e.g. 1252, Big5, etc.). THE DEFAULT.
* Wikipedia-All-Utf8: 280+ languages (and flavors) extracted from *.Wikipedia.org and encoded in UTF-8 only.
* Wikipedia-MostCommon-LegacyAndUtf8: 83 most popular languages extracted from *.Wikipedia.org and encoded in UTF-8 and their respective "legacy" encodings (e.g. 1252, Big5, etc.).
* Wikipedia-MostCommon-Utf8: 83 most popular languages extracted from *.Wikipedia.org and encoded in UTF-8 only. Will be removed from releases in the future.
* TextCat: 74 languages from original TextCat tool.

Recommended input: snippet of text with at least 5 words.

Features:
---------
* .Net Framework 4.0 support
* Should be compatible with Mono 2.10 but hasn't been tested in this release.
* Pure .Net application (C#).
* SQL-CLR integration (temporarily unavailable, expecting to restore it in 0.2.2)

Example of usage for NTextCat.exe (default settings used):
-----------------------------------------
	NTextCat.exe -e=UTF-8 -noprompt < Evaluation\russian-utf8.txt

First result returned is considered the best. The language is specified as ISO 639-3. E.g. "eng"

How to identify language using command line interface
-----------------------------------------------------
With Use of NTextCat as console application which is capable of training (creating language models) and classifying new snippet of text into one or more classes of known languages.
http://ntextcat.codeplex.com/wikipage?title=How%20to%20identify%20language%20from%20command%20line&referringTitle=Home

How to identify language using managed API
------------------------------------------
With Use of NTextCat as library that you can reference from your application to empower it with language identification capabilities.
http://ntextcat.codeplex.com/wikipage?title=NTextCat.Lib.Legacy%20samples&referringTitle=Home
For always the latest and working API reference, please use unit-tests in the solution. e.g. https://ntextcat.svn.codeplex.com/svn/trunk/NTextCatLib.Test/LanguageIdentificationTest.cs