module State

open Elmish.UrlParser
open Elmish
open Types
open Elmish.Navigation

let pageHash =
    function
    | Home -> "#/Home"
    | Quests q ->
        match q with
        | Quests.Types.Page.Home -> "#/Quests"
        | Quests.Types.Page.SelectedSource s -> sprintf "#/Quests/%s" s
    | Recipes -> "#/Recipes"

let urlUpdate (result: Page option) model: AppModel * Cmd<AppMsg> =
    match result with
    | Some p -> { model with CurrentPage = Some p }, Cmd.Empty
    | None -> (model, Cmd.ofMsg <| NavigateTo Home)

let init (route: Page option): AppModel * Cmd<AppMsg> =
    let quests, questsCmds =
        (match route with
         | Some(Quests p) -> Some p
         | _ -> None)
        |> Quests.State.init

    let model, cmd =
        (urlUpdate route
             { CurrentPage = route
               Quests = quests })

    model,
    Cmd.batch
        [ Cmd.map QuestsMsg questsCmds
          cmd ]

let route state =
    oneOf
        [ map Home (s "Home")
          map Recipes (s "Recipes")
          map (Quests Quests.Types.Page.Home) (s "Quests")
          map (Quests << Quests.Types.Page.SelectedSource) (s "Quests" </> str) ] state

let a loc = parseHash route loc

let update (msg: AppMsg) (state: AppModel): AppModel * Cmd<AppMsg> =
    match msg with
    | QuestsMsg qmsg ->
        let questsState, questsCmd = Quests.State.update state.Quests qmsg
        { state with Quests = questsState }, Cmd.map QuestsMsg questsCmd
    | NavigateTo r -> state, Navigation.newUrl <| pageHash r
