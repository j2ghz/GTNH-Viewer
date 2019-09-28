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

let tryGetEnv = System.Environment.GetEnvironmentVariable >> function null | "" -> None | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

// Replace by actual file
let questDB = Shared.BetterQuestingDB.GetSample()

let mapQuestLine (ql:Shared.BetterQuestingDB.QuestLine) = {
    Id=ql.LineId
    Name=ql.Properties.Betterquesting.Name
    Order=ql.Order
    Description=ql.Properties.Betterquesting.Desc
}

let questApi : IQuestApi = {
    quests = fun () -> async { return questDB.QuestDatabase }
    questLines = fun () -> async { return questDB.QuestLines |> Array.map mapQuestLine }
    questLineById = fun id -> async { return questDB.QuestLines |> Array.find (fun ql -> ql.LineId = id)}
}

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue questApi
    |> Remoting.buildHttpHandler

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_gzip
}

run app
