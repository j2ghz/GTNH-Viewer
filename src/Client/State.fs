module State

open Elmish.UrlParser
open Elmish
open Types
open Elmish.Navigation

let pageHash =
    function
    | Types.Page.Home -> "#"
    | Types.Page.Quests _ -> "#/Quests"
    | Types.Page.Recipes -> "#/Recipes"


let init (route: Page option): AppModel * Cmd<AppMsg> =
    let quests, questsCmds =
        (match route with
         | Some (Quests p) -> Some p
         | _ -> None)
        |> Quests.State.init
    { CurrentPage = route
      Quests = quests }, Cmd.batch [ Cmd.map QuestsMsg questsCmds ]

let route state =
    oneOf
        [ map Home (s "Home" )
          map Recipes (s "Recipes")
          map (Quests Quests.Types.Page.Home) (s "Quests") ] state

let a loc = parseHash route loc

let urlUpdate (result: Option<Page>) model: AppModel * Cmd<AppMsg> =
    match result with
    | Some p -> { model with CurrentPage = Some p }, Cmd.Empty
    | None -> (model, Navigation.modifyUrl "#")

let update (msg: AppMsg) (state: AppModel): AppModel * Cmd<AppMsg> =
    match msg with
    | QuestsMsg qmsg ->
        let questsState, questsCmd = Quests.State.update state.Quests qmsg
        { state with Quests = questsState }, Cmd.map QuestsMsg questsCmd
    | NavigateTo r -> failwith "needed?"
