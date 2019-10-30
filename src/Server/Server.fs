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
    [ "2.0.7.5",
      Parsers.BQv1.parser "./SampleData/DefaultQuests-2.0.7.5-cleaned-minified.json"

      "2.0.7.6c-dev",
      Parsers.BQv3.parser "./SampleData/DefaultQuests-2.0.7.6c-dev-cleaned-minified.json"

      "2.0.7.6d-dev",
      Parsers.BQv3.parser "./SampleData/DefaultQuests-2.0.7.6d-dev-cleaned-minified.json"

      "2.0.7.6e-dev",
      Parsers.BQv3.parser "./SampleData/DefaultQuests-2.0.7.6e-dev-cleaned-minified.json"  ]

let recipeSources =
    [ "2.0.7.5",
      Parsers.RecEx.parser "./SampleData/v2.0.7.5-gt-shaped-shapeless-cleaned-minified.json" ]

let parserBySourceId list (src:Source) =
    list
    |> List.where (fst >> ((=) src))
    |> List.exactlyOne
    |> snd

let rnd = System.Random()
let api : IApi =
    { questSources = fun () -> async { return questSources |> List.map fst }
      quests = fun src -> async { return (parserBySourceId questSources src).getQuests }
      questLines = fun src -> async { return (parserBySourceId questSources src).getQuestLines }
      questLineById = fun src id -> async { return (parserBySourceId questSources src).getQuestLineById id }
      recipeSources = fun () -> async { return recipeSources |> List.map fst }
      recipes = fun src -> async {
          let! parser = parserBySourceId recipeSources src
          return (parser.getRecipes |> List.choose (fun i -> if rnd.Next(0,10000) <= 1 then Some i else None))
      } }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue api
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
