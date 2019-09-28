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

// Replace by actual file
let questDB = Shared.BetterQuestingDB.GetSample()

let mapQuestLine (ql : Shared.BetterQuestingDB.QuestLine) =
    { Id = ql.LineId
      Name = ql.Properties.Betterquesting.Name
      Order = ql.Order
      Description = ql.Properties.Betterquesting.Desc }

let mapQuest (q : Shared.BetterQuestingDB.QuestDatabase) =
    { Id = q.QuestId
      Name = q.Properties.Betterquesting.Name
      Description = q.Properties.Betterquesting.Desc }

let mapQuestLineQuest (q : Shared.BetterQuestingDB.Quest) =
    { Id = q.Id
      Location = (q.X, q.Y)
      Size = (q.SizeX, q.SizeY) }

let getQuestById id = questDB.QuestDatabase |> Array.find (fun q -> q.QuestId = id)

let questApi : IQuestApi =
    { quests =
          fun () -> async { return questDB.QuestDatabase |> Array.map mapQuest }
      questLines =
          fun () -> 
              async { return questDB.QuestLines |> Array.map mapQuestLine }
      questLineById =
          fun id -> 
              async { 
                  let ql =
                      questDB.QuestLines 
                      |> Array.find (fun ql -> ql.LineId = id)
                  let qlq = ql.Quests |> Array.map mapQuestLineQuest |> Array.toList
                  return { QuestLine =
                               { Id = ql.LineId
                                 Order = ql.Order
                                 Name = ql.Properties.Betterquesting.Name
                                 Description = ql.Properties.Betterquesting.Desc }
                           QuestLineQuests = qlq
                           Quests = qlq |> List.map (fun q -> q.Id) |> List.map (getQuestById >> mapQuest)  }
              } }

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
