module Client

open Domain
open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser

let urlUpdate (result : Option<Route>) model : Model * Cmd<Msg> =
    match result with
    | Some(Source s) -> { model with CurrentRoute = Some(Source s) }, []
    | None -> (model, Navigation.modifyUrl "#")

let emptyModel =
    { QuestSources = Empty
      CurrentRoute = None }

let init route =
    let loadSources =
        Cmd.OfAsync.either Server.questAPI.sources () SourcesLoaded Error
    let model, cmd = urlUpdate route emptyModel
    model, Cmd.batch [ loadSources; cmd ]

let curry f x y = f (x, y)
let curry3 f x y z = f (x, y, z)

let route =
    oneOf
        [ map Source (s "s" </> str)
          map (curry SourceQuestLine) (s "s" </> str </> s "ql" </> i32)

          map (curry3 SourceQuestLineQuest)
              (s "s" </> str </> s "ql" </> i32 </> s "q" </> i32) ]

let toHash = function
    | Source s -> sprintf "#/s/%s" s

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel, msg with
    | m, SourcesLoaded s -> { m with QuestSources = Body s }, Cmd.none
    // | m, SourceSelected s ->
    //     { m with SelectedSource = Some s
    //              SelectedQuestLine = None },
    //     Cmd.batch [ Cmd.OfAsync.either Server.questAPI.questLines s
    //                     QuestLinesLoaded Error
    //                 s
    //                 |> sprintf "#/s/%s"
    //                 |> Navigation.newUrl ]
    // | m, QuestLinesLoaded quests ->
    //     { m with QuestLines = Some quests }, Cmd.none
    // | { Model.SelectedSource = Some s }, QuestLineSelected i ->
    //     let req = Server.questAPI.questLineById s
    //     { currentModel with SelectedQuestLine = None },
    //     Cmd.batch [ Cmd.OfAsync.either req i QuestLineReceived Error
    //                 sprintf "#/s/%s/ql/%i" s i |> Navigation.newUrl ]
    // | m, QuestLineReceived ql ->
    //     { m with SelectedQuestLine = (Some ql) }, Cmd.none
    | m, NavigateTo r ->
        m,
        r
        |> toHash
        |> Navigation.newUrl
    | _, Error e ->
        eprintf "%O" e
        currentModel, Cmd.none

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
