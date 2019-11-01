module Quests.State

open Quests.Types
open Shared
open Elmish

let init route =
    { Sources = Shared.Remote.Empty
      QuestLines = Empty
      QuestLine = Empty
      SearchResults = Empty }, Cmd.ofMsg LoadSources

let urlUpdate page =
    match page with
    | Home -> Cmd.Empty
    | SelectedSource s -> Cmd.ofMsg (LoadQuestLines s)
    | QuestLine(s, i) ->
        Cmd.batch
            [ Cmd.ofMsg (LoadQuestLine(s, i))
              //Cmd.ofMsg (LoadQuestLines s)
             ]
    | Search(s, st) -> Cmd.ofMsg (LoadSearchResults(s, st))

let update model =
    function
    | LoadSources ->
        { model with
              Sources = Loading
              QuestLines = Empty
              QuestLine = Empty }, Cmd.OfAsync.either Server.API.questSources () LoadSourcesFinished LoadSourcesError
    | LoadSourcesFinished s -> { model with Sources = Body s }, Cmd.Empty
    | LoadSourcesError e ->
        { model with Sources = LoadError <| sprintf "%A" e },
        Cmd.OfAsync.either (fun _ ->
            async {
                do! Async.Sleep 5000
                return! Server.API.questSources()
            }) () LoadSourcesFinished LoadSourcesError
    | LoadQuestLines s ->
        { model with
              QuestLines = Loading
              QuestLine = Empty }, Cmd.OfAsync.either Server.API.questLines s LoadQuestLinesFinished LoadQuestLinesError
    | LoadQuestLinesFinished qlis -> { model with QuestLines = Body qlis }, Cmd.Empty
    | LoadQuestLinesError e ->
        { model with
              QuestLines =
                  sprintf "Error while loading QuestLines:\n%A" e
                  |> LoadError }, Cmd.Empty
    | LoadQuestLine(s, i) ->
        { model with QuestLine = Loading },
        Cmd.OfAsync.either (Server.API.questLineById s) i LoadQuestLineFinished LoadQuestLineError
    | LoadQuestLineFinished qli -> { model with QuestLine = Body qli }, Cmd.Empty
    | LoadQuestLineError e -> { model with QuestLine = LoadError(e |> string) }, Cmd.Empty
    | LoadSearchResults(s, st) ->
        { model with SearchResults = Empty },
        Cmd.OfAsync.either Server.API.questSearch (s, st) LoadSearchResultsFinished LoadSearchResultsError
    | LoadSearchResultsFinished s -> { model with SearchResults = Body s }, Cmd.Empty
    | LoadSearchResultsError e -> { model with SearchResults = LoadError(e |> string) }, Cmd.Empty
