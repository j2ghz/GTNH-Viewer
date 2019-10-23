module Quests.State

open Quests.Types
open Shared
open Elmish

let init route =
    { Sources = Shared.Remote.Empty
      QuestLines = Empty
      QuestLine = Empty }, Cmd.ofMsg LoadSources

let urlUpdate page =
    match page with
    | Home -> Cmd.Empty
    | SelectedSource s -> Cmd.ofMsg (LoadQuestLines s)
    | QuestLine(s, i) -> Cmd.ofMsg (LoadQuestLine(s, i))

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
    | LoadQuestLines s ->
        { model with QuestLines = Loading },
        Cmd.OfAsync.either Server.questAPI.questLines s LoadQuestLinesFinished LoadQuestLinesError
    | LoadQuestLinesFinished qlis -> { model with QuestLines = Body qlis }, Cmd.Empty
    | LoadQuestLinesError e ->
        { model with
              QuestLines =
                  sprintf "Error while loading QuestLines:\n%A" e
                  |> LoadError }, Cmd.Empty
    | LoadQuestLine(s, i) ->
        { model with QuestLine = Loading },
        Cmd.OfAsync.either (Server.questAPI.questLineById s) i LoadQuestLineFinished LoadQuestLineError
    | LoadQuestLineFinished qli -> { model with QuestLine = Body qli }, Cmd.Empty
    | LoadQuestLineError(_) -> failwith "Not Implemented"
