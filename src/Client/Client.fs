module Client

open Domain
open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser

let emptyModel =
    { QuestSources = None
      SelectedSource = None
      QuestLines = None
      SelectedQuestLine = None }

let init =
    let loadSources =
        Cmd.OfAsync.either Server.questAPI.sources () SourcesLoaded Error
    function
    | Some(Source s) -> { emptyModel with SelectedSource = Some s }, loadSources
    | Some(SourceQuestLine(s, ql)) ->
        { emptyModel with SelectedSource = Some s }, loadSources
    | Some(SourceQuestLineQuest(s, ql, q)) ->
        { emptyModel with SelectedSource = Some s }, loadSources
    | None -> emptyModel, loadSources

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | m, SourcesLoaded s ->
        { m with QuestSources = Some s
                 SelectedSource = None }, Cmd.none
    | m, SourceSelected s ->
        { m with SelectedSource = Some s
                 SelectedQuestLine = None },
        Cmd.OfAsync.either Server.questAPI.questLines s QuestLinesLoaded Error
    | m, QuestLinesLoaded quests ->
        { m with QuestLines = Some quests }, Cmd.none
    | m, QuestLineSelected i ->
        let req =
            Server.questAPI.questLineById
                (m.SelectedSource |> Option.defaultValue "")
        { m with SelectedQuestLine = None },
        Cmd.OfAsync.either req i QuestLineReceived Error
    | m, QuestLineReceived ql ->
        { m with SelectedQuestLine = (Some ql) }, Cmd.none
    | _, Error e ->
        printf "%O" e
        currentModel, Cmd.none

let curry f x y = f (x, y)
let curry3 f x y z = f (x, y, z)

let route =
    oneOf
        [ map Source (s "s" </> str)
          map (curry SourceQuestLine) (s "sql" </> str </> i32)
          map (curry3 SourceQuestLineQuest) (s "sqlq" </> str </> i32 </> i32) ]

let urlUpdate (result : Option<Route>) model : Model * Cmd<Msg> =
    match result with
    | Some(Source s) ->
        { model with SelectedSource = Some s
                     SelectedQuestLine = None }, []
    | Some(SourceQuestLine(s, ql)) ->
        { model with SelectedSource = Some s
                     SelectedQuestLine = None }, []
    | Some(SourceQuestLineQuest(s, ql, q)) ->
        { model with SelectedSource = Some s
                     SelectedQuestLine = None }, []
    | None -> (model, Navigation.modifyUrl "#")

//#if DEBUG for debugging

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif


Program.mkProgram init update View.view
#if DEBUG
|> Program.withConsoleTrace
#endif

|> Program.withReactBatched "elmish-app"
|> Program.toNavigable (parseHash route) urlUpdate
#if DEBUG
|> Program.withDebugger
#endif

|> Program.run
