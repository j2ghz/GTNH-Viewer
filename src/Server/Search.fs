module Search

open Lucene.Net.Util
open Lucene.Net.Store
open System.IO
open Lucene.Net.Index
open Shared
open Lucene.Net.Documents
open Lucene.Net.Search
open Lucene.Net.QueryParsers.Simple
open System.Collections.Generic
open System.Linq

[<Literal>]
let VERSION = LuceneVersion.LUCENE_48

let questSearch quests =
    let storage = new RAMDirectory() //new MMapDirectory(DirectoryInfo("./lucene-index"))
    let analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(VERSION)
    let indexConfig = IndexWriterConfig(VERSION, analyzer)
    let writer = new IndexWriter(storage, indexConfig)

    let questToDoc (quest: Quest) =
        let doc = Document()
        doc.AddInt32Field("ID", quest.Id, Field.Store.YES) |> ignore
        doc.AddTextField("Name", quest.Name, Field.Store.YES) |> ignore
        doc.AddTextField("Description", quest.Description, Field.Store.YES) |> ignore
        doc :> IIndexableField seq

    quests
    |> List.map questToDoc
    |> writer.AddDocuments
    do writer.Flush(true, true)

    let reader = writer.GetReader(false)
    let searcher = IndexSearcher(reader)

    let parser =
        SimpleQueryParser
            (analyzer,
             dict
                 [ "Name", 0.9f
                   "Description", 0.1f ])

    let getDoc id =
        let doc = searcher.Doc(id)
        { Id = doc.GetField("ID").GetInt32Value().GetValueOrDefault()
          Name = doc.Get("Name")
          Description = doc.Get("Description") }

    fun searchText ->
        let query = parser.Parse(searchText)
        searcher.Search(query, 20).ScoreDocs
        |> Array.toList
        |> List.sortBy (fun d -> d.Score)
        |> List.map (fun d -> d.Doc |> getDoc)
