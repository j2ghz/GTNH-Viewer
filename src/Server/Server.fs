open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let tryGetEnv =
    System.Environment.GetEnvironmentVariable
    >> function 
    | null | "" -> None
    | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv
    |> Option.map uint16
    |> Option.defaultValue 8085us

let questApi : IQuestApi =
    { quests = fun () -> async { return Parsers.BQv1.getQuests }
      questLines = fun () -> async { return Parsers.BQv1.getQuestLines }
      questLineById = fun id -> async { return Parsers.BQv1.getQuestLineById id } }

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
