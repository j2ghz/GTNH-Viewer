open FSharp.Control.Tasks.V2
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Saturn
open Shared
open System.IO
open System.Threading.Tasks
open Giraffe.SerilogExtensions
open Serilog
open Serilog.Sinks
open Microsoft.Extensions.Logging

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function
    | null
    | "" -> None
    | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv
    |> Option.map uint16
    |> Option.defaultValue 8085us

let questSources =
    [ "2.0.7.5",
      Parsers.BQv1.parser "./SampleData/DefaultQuests-2.0.7.5-cleaned-minified.json"

      "2.0.7.7-dev",
      Parsers.BQv3.parser "./SampleData/DefaultQuests-2.0.7.7-dev-cleaned-minified.json"  ]

      |> List.map (fun (id,parser) -> (id,parser, Search.questSearch parser.getQuests))

let recipeSources =
    [ "2.0.7.5",
      Parsers.RecEx.parser "./SampleData/v2.0.7.5-gt-shaped-shapeless-cleaned-minified.json", "" ]

let fst3 (a,b,c) = a
let snd3 (a,b,c) = b
let trd3 (a,b,c) = c

let parserBySourceId list (src:Source) =
    list
    |> List.where (fst3 >> ((=) src))
    |> List.exactlyOne
    |> snd3

let searcherBySourceId list (src:Source) =
    list
    |> List.where (fst3 >> ((=) src))
    |> List.exactlyOne
    |> trd3

let api : IApi =
    { questSources = fun () -> async { return questSources |> List.map fst3 }
      quests = fun src -> async { return (parserBySourceId questSources src).getQuests }
      questLines = fun src -> async { return (parserBySourceId questSources src).getQuestLines }
      questLineById = fun src id -> async { return (parserBySourceId questSources src).getQuestLineById id }
      questSearch = fun (src,searchText) -> async { return searchText |> (searcherBySourceId questSources src) }
      recipeSources = fun () -> async { return recipeSources |> List.map fst3 }
      items = fun src -> async {
          let! parser = parserBySourceId recipeSources src
          return parser.getItems} }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue api
    |> Remoting.buildHttpHandler

Log.Logger <-
  LoggerConfiguration()
    .Destructure.FSharpTypes()
    .WriteTo.Console()
    .CreateLogger()

let app =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router (SerilogAdapter.Enable(webApp))
        memory_cache
        use_static publicPath
        use_gzip
        logging (fun (l:ILoggingBuilder) -> l.ClearProviders() |> ignore)
    }

run app
