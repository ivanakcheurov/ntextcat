#r "System.Xml.Linq"
#r "FSharp.PowerPack.dll"
#r "FSharp.PowerPack.Parallel.Seq.dll"
open System.Collections.Generic
open System.Linq
open System.Xml
open System.Xml.Linq
open System.Xml.XPath
open System.Text
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

do
    printfn "saving to xml"
    use xmlWriter = 
        XmlWriter.Create(@"D:\Files\Projects\NLP\NTextCat\AllRep\trunk\charEncodingMap.xml", 
            new XmlWriterSettings(Indent = true))
    (charEncodingMapToXml charEncodingMap).WriteTo(xmlWriter)
    

//F0000–FFFFF
//100000–10FFFF