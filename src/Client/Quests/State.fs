module Quests.State

open Quests.Types
open Shared
open Elmish

let init route =
    { Sources = Shared.Remote.Empty
      QuestLines = Empty }, Cmd.ofMsg LoadSources

let update model =
    function
    | LoadSources ->
        { model with Sources = Loading },
        Cmd.OfAsync.either Server.questAPI.sources () LoadSourcesFinished LoadSourcesError
    | LoadSourcesFinished s -> { model with Sources = Body s }, Cmd.Empty
    | LoadSourcesError e ->
        { model with Sources = LoadError <| sprintf "%A" e },
        Cmd.OfAsync.either (fun _ ->
            async {
                do! Async.Sleep 5000
                return! Server.questAPI.sources()
            }) () LoadSourcesFinished LoadSourcesError
    | LoadQuestLines(_) -> failwith "Not Implemented"
    | LoadQuestLinesFinished(_) -> failwith "Not Implemented"
    | LoadQuestLinesError(_) -> failwith "Not Implemented"
