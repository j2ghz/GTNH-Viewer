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
    [ "BQv1 - 2.0.7.5",
      Parsers.BQv1.parser "./SampleData/DefaultQuests-2.0.7.5-cleaned.json"

      "BQv3 - 2.0.7.6c-dev",
      Parsers.BQv3.parser "./SampleData/DefaultQuests-2.0.7.6c-dev-cleaned.json" ]

let parserBySourceId src =
    questSources
    |> List.where (fst >> ((=) src))
    |> List.exactlyOne
    |> snd

let questApi : IQuestApi =
    { sources = fun () -> async { return questSources |> List.map fst }
      quests = fun src -> async { return (parserBySourceId src).getQuests }
      questLines =
          fun src -> async { return (parserBySourceId src).getQuestLines }
      questLineById =
          fun src id ->
              async { return (parserBySourceId src).getQuestLineById id } }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue questApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url ("http://0.0.0.0:" + port.ToString() + "/")
        use_router webApp
        memory_cache
        use_static publicPath
        use_gzip
    }

run app
