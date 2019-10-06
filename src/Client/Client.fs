module Client

open Domain
open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser

let init (route : Route option) : Model * Cmd<Msg> =
    let initialModel =
        { QuestSources = None
          SelectedSource = None
          QuestLines = None
          SelectedQuestLine = None }

    let loadCountCmd =
        Cmd.OfAsync.either Server.questAPI.sources () SourcesLoaded Error
    initialModel, loadCountCmd

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

let routeParser : Parser<Route option> = parseHash route

Program.mkProgram init update View.view
|> Program.withReactBatched "elmish-app"
|> Program.toNavigable routeParser urlUpdate
|> Program.run
