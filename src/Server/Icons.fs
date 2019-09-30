module Icons
open System.IO

let rec crawl dir =
    seq {
        yield! dir |> Directory.EnumerateFiles
        yield! dir |> Directory.EnumerateDirectories |> Seq.collect crawl
    }

let skipOrLast i arr =
    if arr |> Array.length >= i then
        arr
        |> Array.skip i
    else
        [| Seq.last arr |]
    

let icons = 
    @"C:\Users\j200g\source\GTNH-Viewer\icons\assets"   
    |> Directory.EnumerateDirectories
    |> Seq.map (fun modf ->
        (modf.Substring(47), crawl modf
        |> Seq.filter (fun s -> s.EndsWith(".png")) 
        |> Seq.map ((fun s -> modf.Substring(47) + ":" + ((s.Substring(47).Split(@"\") |> skipOrLast 3) |> String.concat ":")))
        |> Seq.toList) )
    

let levenshtein word1 word2 =
    let preprocess = fun (str : string) -> str.ToLower().ToCharArray()
    let chars1, chars2 = preprocess word1, preprocess word2
    let m, n = chars1.Length, chars2.Length
    let table : int[,] = Array2D.zeroCreate (m + 1) (n + 1)
    for i in 0..m do
        for j in 0..n do
            match i, j with
            | i, 0 -> table.[i, j] <- i
            | 0, j -> table.[i, j] <- j
            | _, _ ->
                let delete = table.[i-1, j] + 1
                let insert = table.[i, j-1] + 1
                //cost of substitution is 2
                let substitute = 
                    if chars1.[i - 1] = chars2.[j - 1] 
                        then table.[i-1, j-1] //same character
                        else table.[i-1, j-1] + 2
                table.[i, j] <- List.min [delete; insert; substitute]
    table.[m, n]//, table //return tuple of the table and distance

let getIcon (name:string) = let modf = (icons |> Seq.map (fun i -> (levenshtein name (fst i), i)) |> Seq.sortBy fst |> Seq.map snd |> Seq.head)
                            modf
                            |> snd
                            |> Seq.map (fun i -> ((levenshtein name i),i))
                            |> Seq.sortBy fst
                            |> Seq.map snd
                            |> Seq.take 15
                            |> String.concat "\n\t"           
    

Parsers.BQv3.getQuests
//|> List.take 10
|> List.iter (fun i -> 
    printfn "%s\n\t%s" i.Icon (getIcon i.Icon))