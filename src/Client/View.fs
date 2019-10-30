module View

open Types
open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser
open Fable.FontAwesome
open Fable.FontAwesome.Free
open Fable.React
open Fable.React.Props
open Fulma
open Shared
open Thoth.Json

let navSource (model: AppModel) (dispatch: AppMsg -> unit) source =
    let page =
        source
        |> Quests.Types.Page.SelectedSource
        |> Page.Quests
    Navbar.Item.a
        [ Navbar.Item.Props
            [ OnClick(fun _ ->
                page
                |> NavigateTo
                |> dispatch) ]
          (model.CurrentPage
           |> Option.defaultValue Page.Home
           |> (=) page
           |> Navbar.Item.IsActive) ] [ str source ]

// (match model.Quests.Sources with
//  | Empty -> [ Navbar.Item.a [] [ str "Not requested" ] ]
//  | Loading -> [ Navbar.Item.a [] [ str "Loading" ] ]
//  | LoadError s -> [ Navbar.Item.a [] [ str <| sprintf "Error: %s" s ] ]
//  | Body sources -> sources |> List.map (navSource model dispatch))

let navbarItem currentPage name link =
    Navbar.Item.a
        [ currentPage
          |> Option.defaultValue Home
          |> (=) link
          |> Navbar.Item.IsActive
          Navbar.Item.Props [ Href <| (State.pageHash link) ] ] [ str name ]

let navBrand (model: AppModel) (dispatch: AppMsg -> unit) =
    let expanded = true //Model?
    Navbar.navbar []
        [ Container.container []
              [ Navbar.Brand.div []
                    [ Navbar.Item.a
                        [ Navbar.Item.CustomClass "brand-text"
                          Navbar.Item.Props <| [ Href <| State.pageHash Page.Home ] ] [ str "GTNH-Viewer" ]
                      Navbar.burger [ if expanded then yield CustomClass "is-active" ]
                          [ span [] []
                            span [] []
                            span [] [] ] ]

                Navbar.menu [ Navbar.Menu.IsActive expanded ]
                    [ Navbar.Start.div []
                          [ yield! Quests.View.navbarItem model.Quests model.CurrentPage
                                       (Types.Page.Quests >> State.pageHash)
                            yield! Recipes.View.navbarItem model.Recipes model.CurrentPage
                                       (Types.Page.Recipes >> State.pageHash) ] ] ] ]

let view (model: AppModel) (dispatch: AppMsg -> unit) =
    div []
        [ navBrand model dispatch
          Container.container []

              (match model.CurrentPage with
               | None -> [ str "404" ]
               | Some Home ->
                   [ Hero.hero []
                         [ Hero.body []
                               [ Container.container [ Container.IsFluid ]
                                     [ Heading.h1 [] [ str "Welcome to GTNH-Viewer" ]
                                       Heading.h2 [ Heading.IsSubtitle ]
                                           [ str
                                               "Use the navbar on top and menu on the left to explore the site and find the info you need" ] ] ] ] ]
               | Some(Recipes r) ->
                   Recipes.View.view r (Recipes >> State.pageHash) model.Recipes (RecipesMsg >> dispatch)
               | Some(Quests q) -> Quests.View.view q (Quests >> State.pageHash) model.Quests (QuestsMsg >> dispatch))
          Footer.footer []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
                    [ p []
                          [ a [ Href "https://github.com/j2ghz/GTNH-Viewer" ] [ str "GTNH-Viewer" ]
                            str " by "
                            a [ Href "https://github.com/j2ghz" ] [ str "J2ghz" ]
                            str ". Licensed under "
                            a [ Href "https://github.com/j2ghz/GTNH-Viewer/blob/master/LICENSE" ]
                                [ str "GNU AGPL v3.0" ] ] ] ] ]
