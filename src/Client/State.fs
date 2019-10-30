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
        | Quests.Types.Page.QuestLine(s, qli) -> sprintf "#/Quests/%s/%i" s qli
    | Recipes r ->
        match r with
        | Recipes.Types.Page.Home -> "#/Recipes"
        | Recipes.Types.Page.SelectedSource s -> sprintf "#/Recipes/%s" s

let urlUpdate (result: Page option) model: AppModel * Cmd<AppMsg> =
    match result with
    | Some(Quests p) ->
        let qCmd = Quests.State.urlUpdate p
        { model with CurrentPage = result }, Cmd.map QuestsMsg qCmd
    | Some(Recipes p) ->
        let rCmd = Recipes.State.urlUpdate p
        { model with CurrentPage = result }, Cmd.map RecipesMsg rCmd
    | Some p -> { model with CurrentPage = Some p }, Cmd.Empty
    | None -> (model, Cmd.ofMsg <| NavigateTo Home)

let init (route: Page option): AppModel * Cmd<AppMsg> =
    let quests, questsCmds =
        (match route with
         | Some(Quests p) -> Some p
         | _ -> None)
        |> Quests.State.init

    let recipes, recipesCmds =
        (match route with
         | Some(Recipes p) -> Some p
         | _ -> None)
        |> Recipes.State.init

    let model, cmd =
        (urlUpdate route
             { CurrentPage = route
               Quests = quests
               Recipes = recipes })

    model,
    Cmd.batch
        [ Cmd.map QuestsMsg questsCmds
          Cmd.map RecipesMsg recipesCmds
          cmd ]

let route state =
    oneOf
        [ map Home (s "Home")
          map (Recipes Recipes.Types.Page.Home) (s "Recipes")
          map (Recipes << Recipes.Types.Page.SelectedSource) (s "Recipes" </> str)
          map (Quests Quests.Types.Page.Home) (s "Quests")
          map (Quests << Quests.Types.Page.SelectedSource) (s "Quests" </> str)
          map (fun s i -> Quests.Types.Page.QuestLine(s, i) |> Quests) (s "Quests" </> str </> i32) ] state

let a loc = parseHash route loc

let update (msg: AppMsg) (state: AppModel): AppModel * Cmd<AppMsg> =
    match msg with
    | RecipesMsg rmsg ->
        let recipesState, recipesCmd = Recipes.State.update state.Recipes rmsg
        { state with Recipes = recipesState }, Cmd.map RecipesMsg recipesCmd
    | QuestsMsg qmsg ->
        let questsState, questsCmd = Quests.State.update state.Quests qmsg
        { state with Quests = questsState }, Cmd.map QuestsMsg questsCmd
    | NavigateTo r -> state, Navigation.newUrl <| pageHash r
