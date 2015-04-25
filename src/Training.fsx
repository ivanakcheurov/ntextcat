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

//let characterRanges =
//    [ (0x0001, 0xd7FF); (0xe000, 0xFFFF); // BMP excluding surrogate range
//    (0x10000, 0x13FFF); (0x16000, 0x16FFF); 
//    (0x1B000, 0x1BFFF); (0x1D000, 0x1DFFF); (0x1F000, 0x2BFFF); 
//    (0x2F000, 0x2FFFF); (0xE0000, 0xE0FFF); 
//    ]

//let encodings =
//    Encoding.GetEncodings()
//    |> Seq.map (fun encInfo -> 
//        let encoding =
//            Encoding.GetEncoding(encInfo.CodePage)
//        let failSequence =
//            encoding.GetBytes("76D7EB7E23B5452F87297785AB106F31")
//        encoding, failSequence
//        )
let compare a b =
    if a < b then
        -1
    elif a = b then
        0
    else 1

//let charEncodingMap =
//    characterRanges
//    |> Seq.collect (fun (from, to_) -> {from..to_})
//    |> PSeq.ordered
//    |> PSeq.map (fun char ->
//        let str = Char.ConvertFromUtf32(char)
//        let suitableEncodings =
//            encodings
//            |> Seq.filter (fun (enc, failSequence) -> 
//                enc.GetString(enc.GetBytes(str)) = str)
//            |> Seq.map fst
//            |> Seq.toArray
//        //printfn "%A" char//, (suitableEncodings |> Seq.map (fun e -> e.EncodingName))
//        char, suitableEncodings
//        )
//    |> Map.ofSeq

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

//let charEncodingMapToXml (charEncodingMap:Map<int, Encoding array>) =
//    new XStreamingElement(XName.Get("CharacterEncodingMap"),
//        charEncodingMap
//        |> Seq.map (fun entry -> 
//            new XElement(XName.Get("Char"), 
//                new XAttribute(XName.Get("utf32code"), entry.Key),
//                new XAttribute(XName.Get("utf32char"), 
//                    if isLegalXmlChar entry.Key then Char.ConvertFromUtf32(entry.Key) else "control"),
//                entry.Value
//                |> Seq.map (fun enc -> 
//                    new XElement(XName.Get("enc"), 
//                        new XAttribute(XName.Get("cp"), enc.CodePage),
//                        enc.EncodingName)
//                    )
//                )
//            )
//        )

let saveXStream (filename:string) (xstream:XStreamingElement) =
    use xmlWriter = 
        XmlWriter.Create(filename, 
            new XmlWriterSettings(Indent = true))
    xstream.WriteTo(xmlWriter)

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
    |> Seq.countBy id 
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

#r @"NTextCatLibLegacy\bin\Debug\IvanAkcheurov.Commons.dll"
#r @"NTextCatLibLegacy\bin\Debug\IvanAkcheurov.NClassify.dll"
#r @"NTextCatLibLegacy\bin\Debug\IvanAkcheurov.NTextCat.Lib.dll"
#r @"NTextCatLibLegacy\bin\Debug\IvanAkcheurov.NTextCat.Lib.Legacy.dll"

// train profile
do
    let samples =
        Directory.EnumerateFiles(@"d:\WikiDump\Wiki\extract\trainData20M\")
        |> Seq.map (fun file -> 
            let lang = Path.GetFileNameWithoutExtension(file)
            lang, file)
        |> Seq.map (fun (lang, file) -> 
            printfn "lang: %s" lang
            let fileContents = File.ReadAllText(file)
            let input = new StreamReader(file);
            (new IvanAkcheurov.NTextCat.Lib.LanguageInfo(lang, null, null, null)), input :> TextReader) 
        |> Seq.cache
    use output = new FileStream(Path.Combine(@"d:\WikiDump\Wiki\extract\trainData20M\profile4000\", "profile.xml"), FileMode.Create);
    let factory = 
        new IvanAkcheurov.NTextCat.Lib.NaiveBayesLanguageIdentifierFactory(
            MaxNGramLength = 5, MaximumSizeOfDistribution = 4000, OccuranceNumberThreshold = 0, OnlyReadFirstNLines = Int32.MaxValue)
    factory.TrainAndSave(samples, output) |> ignore
    samples 
    |> Seq.iter (fun (_, reader) ->
        reader.Close()
        )


// train old models
do
    Directory.EnumerateFiles(@"d:\WikiDump\Wiki\extract\trainData20M\")
    |> Seq.map (fun file -> 
        let lang = Path.GetFileNameWithoutExtension(file)
        lang, file)
    |> PSeq.iter (fun (lang, file) -> 
        let fileContents = File.ReadAllText(file)
        use input = new StreamReader(file);
        let tokens = (new IvanAkcheurov.NTextCat.Lib.CharacterNgramsExtractor(5, Int64.MaxValue)).GetFeatures(input)
        let langaugeModel = 
            new IvanAkcheurov.NTextCat.Lib.LanguageModel<string>(
                new Dictionary<string, string>(),
                IvanAkcheurov.NTextCat.Lib.LanguageModelCreator.CreateLangaugeModel(tokens, 0, 4000));
        use output = new FileStream(Path.Combine(@"d:\WikiDump\Wiki\extract\trainData20M\lm_char4000\", lang + ".txt"), FileMode.Create);
        (new IvanAkcheurov.NTextCat.Lib.LanguageModelPersister<String>()).Save(langaugeModel, output))

let joinWith separator (items:seq<'T>) =
    String.Join(separator, items)

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

// copy minCharCount chars and remainder of the last word so that it is copied in its entirety
let copyAtLeast (inputFile:string) (outputFile:string) minCharCount =
    use reader = new StreamReader(inputFile)
    use writer = new StreamWriter(outputFile)
    let buffer : char array = Array.zeroCreate 4096
    let mutable further = true
    let mutable copied = 0L
    while further do 
        let read = reader.ReadBlock(buffer, 0, 4096)
        if read > 0 then
            let mutable toCopy = read
            if copied > minCharCount then
                let taleLength = buffer |> Seq.take read |> Seq.takeWhile (fun c -> not (Char.IsSeparator c)) |> Seq.length
                if taleLength < read then 
                    toCopy <- taleLength
                    further <- false
            writer.Write(buffer, 0, toCopy)
            copied <- copied + int64(read)
        else
            further <- false

// copy trainData with reduced size
let sourceDir = @"d:\WikiDump\Wiki\extract\trainData\"
let destDir = @"d:\WikiDump\Wiki\extract\trainData20M\"
Directory.GetFiles sourceDir
    |> Seq.iter (fun file ->
        let file = Path.GetFileName file
        copyAtLeast (Path.Combine(sourceDir, file)) (Path.Combine(destDir, file)) (20L*1000L*1000L)
        )

// play with identification
let getIdentifier modelDir =
    let factory = 
        new IvanAkcheurov.NTextCat.Lib.NaiveBayesLanguageIdentifierFactory(
            MaxNGramLength = 5, MaximumSizeOfDistribution = 4000, OccuranceNumberThreshold = 0, OnlyReadFirstNLines = Int32.MaxValue)
    use stream = File.OpenRead(modelDir)
    factory.Load(stream)
//    let classifier = new IvanAkcheurov.NTextCat.Lib.Legacy.RankedClassifier<string>(4000);
//    let persister = new IvanAkcheurov.NTextCat.Lib.LanguageModelPersister<String>()
//    for file in Directory.GetFiles(modelDir, "*.txt") do
//        use stream = File.OpenRead(file)
//        classifier.AddEtalonLanguageModel(Path.GetFileNameWithoutExtension(file), persister.Load(stream).Features);
//    classifier

let identifier = getIdentifier @"d:\WikiDump\Wiki\extract\trainData20M\profile4000\profile.xml"
let identifier = getIdentifier @"d:\WikiDump\Wiki\extract\trainData20M\lm_char4000\"

let identify (identifier:IvanAkcheurov.NTextCat.Lib.Legacy.RankedClassifier<string>) (text:string) =
    let extractor = new IvanAkcheurov.NTextCat.Lib.CharacterNgramsExtractor(5, Int64.MaxValue)
    let tokens = extractor.GetFeatures(text);
    let langaugeModel = IvanAkcheurov.NTextCat.Lib.LanguageModelCreator.CreateLangaugeModel(tokens, 0, 4000);
    let result = identifier.Classify(langaugeModel).ToList();
    result