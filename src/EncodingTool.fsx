#r "System.Xml.Linq"
#r "FSharp.PowerPack.dll"
#r "FSharp.PowerPack.Parallel.Seq.dll"
open System.Collections.Generic
open System.Linq
open System.Xml
open System.Xml.Linq
open System.Xml.XPath
open System.Text
open System.Globalization
open System.Text.RegularExpressions
open System
open System.IO
open Microsoft.FSharp.Collections


let characterRanges =
    [ (0x0001, 0xd7FF); (0xe000, 0xFFFF); // BMP excluding surrogate range
    (0x10000, 0x13FFF); (0x16000, 0x16FFF); 
    (0x1B000, 0x1BFFF); (0x1D000, 0x1DFFF); (0x1F000, 0x2BFFF); 
    (0x2F000, 0x2FFFF); (0xE0000, 0xE0FFF); 
    ]

let encodings =
    Encoding.GetEncodings()
    |> Seq.map (fun encInfo -> 
        let encoding =
            Encoding.GetEncoding(encInfo.CodePage)
        let failSequence =
            encoding.GetBytes("76D7EB7E23B5452F87297785AB106F31")
        encoding, failSequence
        )
let compare a b =
    if a < b then
        -1
    elif a = b then
        0
    else 1

let charEncodingMap =
    characterRanges
    |> Seq.collect (fun (from, to_) -> {from..to_})
    |> PSeq.ordered
    |> PSeq.map (fun char ->
        let str = Char.ConvertFromUtf32(char)
        let suitableEncodings =
            encodings
            |> Seq.filter (fun (enc, failSequence) -> 
                enc.GetString(enc.GetBytes(str)) = str)
            |> Seq.map fst
            |> Seq.toArray
        //printfn "%A" char//, (suitableEncodings |> Seq.map (fun e -> e.EncodingName))
        char, suitableEncodings
        )
    |> Map.ofSeq

let isLegalXmlChar character =
    match character with
    | 0x9 | 0xA -> 
        true
    | space when (space >= 0x20 && space <= 0xD7FF) ->
        true
    | plane when (plane >= 0xE000  && plane <= 0xFFFD) ->
        true
    | plane when (plane >= 0x10000 && plane <= 0x10FFFF) ->
        true
    | _ -> false

let charEncodingMapToXml (charEncodingMap:Map<int, Encoding array>) =
    new XStreamingElement(XName.Get("CharacterEncodingMap"),
        charEncodingMap
        |> Seq.map (fun entry -> 
            new XElement(XName.Get("Char"), 
                new XAttribute(XName.Get("utf32code"), entry.Key),
                new XAttribute(XName.Get("utf32char"), 
                    if isLegalXmlChar entry.Key then Char.ConvertFromUtf32(entry.Key) else "control"),
                entry.Value
                |> Seq.map (fun enc -> 
                    new XElement(XName.Get("enc"), 
                        new XAttribute(XName.Get("cp"), enc.CodePage),
                        enc.EncodingName)
                    )
                )
            )
        )

let saveXStream (filename:string) (xstream:XStreamingElement) =
    use xmlWriter = 
        XmlWriter.Create(filename, 
            new XmlWriterSettings(Indent = true))
    xstream.WriteTo(xmlWriter)

let itself item =
    item

//do
//    printfn "saving to xml"
//    charEncodingMap |> charEncodingMapToXml |> (saveXStream @"D:\Files\Projects\NLP\NTextCat\AllRep\trunk\charEncodingMap.xml")
    
let stringToUTF32 string =
    let enumerator = StringInfo.GetTextElementEnumerator(string)
    seq {
        while enumerator.MoveNext() do
            let charStr = enumerator.Current.ToString()
            yield Char.ConvertToUtf32(charStr, 0)
        }

module Seq =
    /// <summary>Groups items by key and value</summary>
    let groupValuesBy getKey getValue seq =
        seq
        |> Seq.groupBy getKey
        |> Seq.map (fun (key, items) -> key, items |> Seq.map getValue)

let mapFst transformFirst (first, second) =
        transformFirst first, second
let mapSnd transformSnd (first, second) =
    first, transformSnd second

let addFst generateFst second =
    second, generateFst second

let addSnd generateSnd first =
    first, generateSnd first

let toDistribution itemAndCount =
        let itemAndCount = Seq.cache itemAndCount
        let totalCharCount = itemAndCount |> (Seq.sumBy snd)
        itemAndCount
        |> Seq.map (fun (char, count) -> char, float count / float totalCharCount)

let getCharDistribution str =
    str
    |> stringToUTF32 
    |> Seq.countBy itself 
    |> Seq.map (mapSnd float) 
    |> toDistribution

let getEncodingDistribution charDistribution =
    let encodingDistribution =
        charDistribution
        |> Seq.collect (fun (char, freq) -> 
            if not (charEncodingMap.ContainsKey(char)) then
                printfn "Character is undefined: %d, frequency: %f" char freq
                Seq.empty
            else
                charEncodingMap.Item char 
                |> Seq.map (fun enc -> enc, freq)
            )
        |> Seq.groupValuesBy fst snd
        |> Seq.map (mapSnd Seq.sum)
        //|> toDistribution
    encodingDistribution

//do
//    let encodings = Encoding.GetEncodings()
//    use textWriter = new StreamWriter(@"d:\Files\Projects\NLP\NTextCat\AllRep\trunk\languageEncodingMatrix.csv")
//    textWriter.Write("language")
//    for enc in encodings do 
//        textWriter.Write(", {0} ({1})", enc.DisplayName.Replace(",", "`"), enc.CodePage)
//    textWriter.WriteLine()
//    Directory.GetFiles(@"d:\WikiDump\Wiki\extract\trainData\")
//    |> Seq.map (addSnd File.ReadAllText)
//    |> PSeq.ordered
//    |> PSeq.map (mapSnd (fun content -> 
//        let encodingDistributionMap =
//            content
//            |> getCharDistribution 
//            |> getEncodingDistribution
//            |> Seq.map (mapFst (fun enc -> enc.CodePage))
//            |> Map.ofSeq
//        encodingDistributionMap))
//    |> Seq.iter (fun (file, encodingDistributionMap) -> 
//        textWriter.Write(Path.GetFileNameWithoutExtension(file))
//        for enc in encodings do 
//            textWriter.Write(", {0}", if encodingDistributionMap.ContainsKey(enc.CodePage) then encodingDistributionMap.Item enc.CodePage else 0.0)
//        textWriter.WriteLine()
//        )

let streamLinesFromFile (filename:string) =
    seq {
        use textReader = new StreamReader(filename)
        for line in 
            (Seq.initInfinite (fun i -> textReader.ReadLine())
            |> Seq.takeWhile (fun line -> line <> null)) do
            yield line
    }

let loadLanguageEncodingMapping (filename:string) =
    let lines = streamLinesFromFile filename
    let codepages = 
        (lines |> Seq.head).Split([|", "|], StringSplitOptions.None)
        |> Seq.skip 1 
        |> Seq.map (fun header -> Int32.Parse(header.Substring(header.LastIndexOf('(')).Trim( [|'('; ')'|] )))
        |> Array.ofSeq
    lines 
    |> Seq.skip 1 
    |> Seq.map (fun line -> 
        let cells = line.Split([|", "|], StringSplitOptions.None)
        let language = cells.[0]
        let encodings = cells |> Seq.skip 1 |> Seq.map Double.Parse |> Seq.zip codepages
        language, encodings
        )

#r @"NTextCatLibLegacy\bin\Debug\IvanAkcheurov.NClassify.dll "
#r @"NTextCatLib\bin\Debug\IvanAkcheurov.NTextCat.Lib.dll"
#r @"NTextCatLibLegacy\bin\Debug\IvanAkcheurov.NTextCat.Lib.Legacy.dll "

// train only compatible models
//do
//    let languageEncodingMapping =
//        loadLanguageEncodingMapping @"d:\Files\Projects\NLP\NTextCat\AllRep\trunk\languageEncodingMatrix.csv"
//        |> Seq.map (mapSnd (fun encoding2CompatibilitySeq -> encoding2CompatibilitySeq |> Seq.filter (fun (enc, compat) -> compat > 0.9)))
//        |> Map.ofSeq
//    Directory.EnumerateFiles(@"d:\WikiDump\Wiki\extract\trainData\")
//    |> Seq.map (fun file -> 
//        let lang = Path.GetFileNameWithoutExtension(file)
//        lang, file, languageEncodingMapping.Item lang)
//    |> PSeq.iter (fun (lang, file, encodings) -> 
//        let fileContents = File.ReadAllText(file)
//        encodings
//        |> Seq.map fst
//        |> Seq.iter (fun cp -> 
//            use input = new IvanAkcheurov.NTextCat.Lib.TextReaderStream(new StreamReader(file), Encoding.GetEncoding(cp));
//            let tokens = (new IvanAkcheurov.NTextCat.Lib.Legacy.ByteToUInt64NGramExtractor(5, Int64.MaxValue)).GetFeatures(input)
//            let langaugeModel = IvanAkcheurov.NTextCat.Lib.LanguageModelCreator<UInt64>.CreateLangaugeModel(tokens, 0, 400);
//            use output = new FileStream(Path.Combine(@"d:\WikiDump\Wiki\extract\trainData\lm_enc\", lang + "_cp" + cp.ToString() + ".txt"), FileMode.Create);
//            (new IvanAkcheurov.NTextCat.Lib.LanguageModelPersister()).Save(langaugeModel, output))
//        )

let joinWith separator (items:seq<'T>) =
    String.Join(separator, items)

do
    
        loadLanguageEncodingMapping @"d:\Files\Projects\NLP\NTextCat\AllRep\trunk\languageEncodingMatrix.csv"
        |> Seq.map (
            mapSnd (
                fun encoding2CompatibilitySeq -> 
                    let singleByteAndUnicodeEncs =
                        encoding2CompatibilitySeq 
                        |> Seq.filter (
                            fun (enc, compat) -> 
                                let encoding = Encoding.GetEncoding(enc)
                                let isIbmIgnored = encoding.WebName.ToLowerInvariant().StartsWith("ibm") && not (encoding.EncodingName.ToLowerInvariant().Contains("dos"))
                                let isIa5Ignored = encoding.WebName.ToLowerInvariant().StartsWith("x-ia5")
                                compat >= 0.99 && encoding.IsSingleByte && not isIbmIgnored && not isIa5Ignored) 
                        |> Array.ofSeq
                    let nonUnicode =
                        if singleByteAndUnicodeEncs.Any() then
                            singleByteAndUnicodeEncs |> Seq.ofArray
                        else
                            encoding2CompatibilitySeq 
                            |> Seq.filter (
                                fun (enc, compat) -> 
                                    compat >= 0.99 && not ([|1200; 1201; 12000; 12001; 65000; 65001|].Contains(enc)))
                    let utf8_16 =
                        encoding2CompatibilitySeq |> Seq.filter (fun (enc, comp) -> enc = 1200 || enc = 65001)
                    Seq.append utf8_16 nonUnicode
                ))
        |> Seq.map (
            mapSnd (fun encoding2CompatibilitySeq -> encoding2CompatibilitySeq |> Seq.sortBy snd |> Array.ofSeq |> Array.rev |> Seq.ofArray))
        |> Seq.iter (fun (lang, enc2compSeq) -> printfn "%s:\t%s" lang (enc2compSeq |> Seq.map (fun (enc, comp) -> sprintf "%s: %f" (Encoding.GetEncoding(enc).WebName) comp) |> joinWith "; "))
//        |> Seq.iter (
//            fun (lang, enc2compat) ->
//                printfn "%s: %s" lang (String.Join(", ", enc2compat |> Seq.map fst |> Seq.map Encoding.GetEncoding |> Seq.map (fun enc -> sprintf "%s (%d)" enc.EncodingName enc.CodePage) )))
                    //encoding2CompatibilitySeq |> Seq.filter (fun (enc, compat) -> compat > 0.9)))
    //ignore 1

//F0000–FFFFF
//100000–10FFFF
//do
//    Directory.GetFiles(@"d:\WikiDump\Wiki\extract\trainData\")
//    |> Seq.map (fun file -> file, File.ReadAllText(file))
//    |> PSeq.iter (fun (file, content) ->
//        content
//        |> getCharDistribution
//        |> Seq.filter (fun (char, _) -> not(charEncodingMap.ContainsKey(char)))
//        |> Seq.iter (fun (char, freq) -> 
//            printfn "%s: Unknown char %d, freq: %f" (Path.GetFileNameWithoutExtension(file)) char freq)
//        )
//az: Unknown char 1049893, freq: 0.000000
//az: Unknown char 1048708, freq: 0.000000
//az: Unknown char 1048698, freq: 0.000000
//az: Unknown char 1048669, freq: 0.000000
//ar: Unknown char 1048661, freq: 0.000000
//bg: Unknown char 1113088, freq: 0.000000
//dz: Unknown char 1051555, freq: 0.000127
//dz: Unknown char 1051514, freq: 0.000018
//dz: Unknown char 1051422, freq: 0.000018
//dz: Unknown char 1051689, freq: 0.000018
//et: Unknown char 1050156, freq: 0.000000
//et: Unknown char 1051958, freq: 0.000000
//et: Unknown char 1052350, freq: 0.000000
//et: Unknown char 1054220, freq: 0.000000
//et: Unknown char 1053855, freq: 0.000000
//et: Unknown char 1049223, freq: 0.000000
//et: Unknown char 1061426, freq: 0.000000
//et: Unknown char 1049952, freq: 0.000000
//et: Unknown char 1051966, freq: 0.000000
//et: Unknown char 1053951, freq: 0.000000
//et: Unknown char 1051209, freq: 0.000000
//et: Unknown char 1057659, freq: 0.000000
//et: Unknown char 1049647, freq: 0.000000
//et: Unknown char 1052065, freq: 0.000000
//et: Unknown char 1052758, freq: 0.000000
//et: Unknown char 1050125, freq: 0.000000
//et: Unknown char 1053759, freq: 0.000000
//et: Unknown char 1049133, freq: 0.000000
//et: Unknown char 1050007, freq: 0.000000
//et: Unknown char 1050887, freq: 0.000000
//et: Unknown char 1050931, freq: 0.000000
//et: Unknown char 1052480, freq: 0.000000
//et: Unknown char 1054511, freq: 0.000000
//et: Unknown char 1054127, freq: 0.000000
//et: Unknown char 1051805, freq: 0.000000
//et: Unknown char 1057118, freq: 0.000000
//et: Unknown char 1049711, freq: 0.000000
//et: Unknown char 1057900, freq: 0.000000
//et: Unknown char 1049897, freq: 0.000000
//et: Unknown char 1051552, freq: 0.000000
//et: Unknown char 1050190, freq: 0.000000
//et: Unknown char 1049943, freq: 0.000000
//et: Unknown char 1051886, freq: 0.000000
//et: Unknown char 1051156, freq: 0.000000
//et: Unknown char 1049877, freq: 0.000000
//et: Unknown char 1050860, freq: 0.000000
//et: Unknown char 1058032, freq: 0.000000
//et: Unknown char 1052590, freq: 0.000000
//et: Unknown char 1049237, freq: 0.000000
//et: Unknown char 1051714, freq: 0.000000
//et: Unknown char 1049287, freq: 0.000000
//et: Unknown char 1049443, freq: 0.000000
//et: Unknown char 1049431, freq: 0.000000
//et: Unknown char 1054143, freq: 0.000000
//et: Unknown char 1049352, freq: 0.000000
//fa: Unknown char 1048598, freq: 0.000000
//fa: Unknown char 1048582, freq: 0.000000
//fa: Unknown char 1048586, freq: 0.000000
//fa: Unknown char 1048585, freq: 0.000000
//fa: Unknown char 210254, freq: 0.000000
//es: Unknown char 1048774, freq: 0.000000
//id: Unknown char 1048704, freq: 0.000000
//id: Unknown char 1048710, freq: 0.000000
//id: Unknown char 1048713, freq: 0.000000
//id: Unknown char 1048593, freq: 0.000000
//id: Unknown char 1048647, freq: 0.000000
//id: Unknown char 1048653, freq: 0.000000
//id: Unknown char 1048644, freq: 0.000000
//id: Unknown char 1048606, freq: 0.000000
//id: Unknown char 1048591, freq: 0.000000
//id: Unknown char 1048579, freq: 0.000000
//id: Unknown char 1048651, freq: 0.000000
//id: Unknown char 1048666, freq: 0.000000
//id: Unknown char 1048668, freq: 0.000000
//id: Unknown char 1048744, freq: 0.000000
//id: Unknown char 1048634, freq: 0.000000
//id: Unknown char 1048657, freq: 0.000000
//id: Unknown char 1048648, freq: 0.000000
//id: Unknown char 1048655, freq: 0.000000
//id: Unknown char 1048702, freq: 0.000000
//id: Unknown char 1048609, freq: 0.000000
//id: Unknown char 1048661, freq: 0.000000
//id: Unknown char 1048646, freq: 0.000000
//id: Unknown char 1048640, freq: 0.000000
//id: Unknown char 1048620, freq: 0.000000
//id: Unknown char 1048638, freq: 0.000000
//id: Unknown char 1048656, freq: 0.000000
//id: Unknown char 1048586, freq: 0.000000
//id: Unknown char 1048665, freq: 0.000000
//id: Unknown char 1048729, freq: 0.000000
//id: Unknown char 1048605, freq: 0.000000
//id: Unknown char 1048580, freq: 0.000000
//id: Unknown char 1048594, freq: 0.000000
//id: Unknown char 1048663, freq: 0.000000
//id: Unknown char 1048654, freq: 0.000000
//id: Unknown char 1048645, freq: 0.000000
//id: Unknown char 1048622, freq: 0.000000
//id: Unknown char 1048585, freq: 0.000000
//id: Unknown char 1048613, freq: 0.000000
//id: Unknown char 1048589, freq: 0.000000
//id: Unknown char 1048659, freq: 0.000000
//id: Unknown char 1048581, freq: 0.000000
//id: Unknown char 1048631, freq: 0.000000
//id: Unknown char 1048756, freq: 0.000000
//id: Unknown char 1048652, freq: 0.000000
//id: Unknown char 1048626, freq: 0.000000
//id: Unknown char 1048672, freq: 0.000000
//id: Unknown char 1048623, freq: 0.000000
//id: Unknown char 1048615, freq: 0.000000
//id: Unknown char 1048658, freq: 0.000000
//id: Unknown char 1048607, freq: 0.000000
//id: Unknown char 1048625, freq: 0.000000
//id: Unknown char 1048636, freq: 0.000000
//id: Unknown char 1048667, freq: 0.000000
//id: Unknown char 1048660, freq: 0.000000
//id: Unknown char 1048649, freq: 0.000000
//id: Unknown char 1048669, freq: 0.000000
//id: Unknown char 1048632, freq: 0.000000
//id: Unknown char 1048611, freq: 0.000000
//ko: Unknown char 394884, freq: 0.000000
//ko: Unknown char 985172, freq: 0.000000
//ko: Unknown char 985173, freq: 0.000000
//pa: Unknown char 1048798, freq: 0.000017
//pa: Unknown char 1048801, freq: 0.000005
//pa: Unknown char 1048808, freq: 0.000001
//pa: Unknown char 1048815, freq: 0.000003
//pa: Unknown char 1048800, freq: 0.000001
//pa: Unknown char 1048799, freq: 0.000003
//pa: Unknown char 1048817, freq: 0.000002
//pa: Unknown char 1048856, freq: 0.000001
//ps: Unknown char 1048707, freq: 0.000000
//si: Unknown char 1048577, freq: 0.000000
//si: Unknown char 1048578, freq: 0.000000
//simple: Unknown char 1114111, freq: 0.000000
//sr: Unknown char 1048708, freq: 0.000000
//th: Unknown char 1114111, freq: 0.000000
//uk: Unknown char 360036, freq: 0.000000
//uz: Unknown char 1048766, freq: 0.000002
//tr: Unknown char 1048702, freq: 0.000000
//vi: Unknown char 407050, freq: 0.000000

Encoding.GetEncodings()
|> Seq.map (fun ei -> Encoding.GetEncoding(ei.CodePage))
|> Seq.countBy (fun enc -> enc.WindowsCodePage)
|> Seq.iter (printfn "%A")