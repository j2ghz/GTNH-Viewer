module Quests.State

open Quests.Types
open Shared
open Elmish

let init route = { Sources = Shared.Remote.Empty }, Cmd.ofMsg LoadSources

let update model =
    function
    | LoadSources ->
        { model with Sources = Loading },
        Cmd.OfAsync.either Server.questAPI.sources () SourcesLoaded SourcesLoadingError
    | SourcesLoaded s -> { model with Sources = Body s }, Cmd.Empty
    | SourcesLoadingError e -> { model with Sources = LoadError <| sprintf "%A" e }, Cmd.Empty
    | LoadQuestLines _ -> failwith "NotImplementedException"
