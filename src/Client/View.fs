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
                          [ navbarItem model.CurrentPage "Quests" (Page.Quests Quests.Types.Page.Home)
                            navbarItem model.CurrentPage "Recipes" (Page.Recipes) ] ] ] ]

let view (model: AppModel) (dispatch: AppMsg -> unit) =
    div []
        [ navBrand model dispatch
          Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ] []

                      Column.column [ Column.Width(Screen.All, Column.Is9) ]
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
                           | Some Recipes -> [ str "Recipes WIP!" ]
                           | Some(Quests _) -> [ Quests.View.view model.Quests (QuestsMsg >> dispatch) ]) ] ]
          Footer.footer []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
                    [ h6 [] [ str "Site debug info:" ]
                      p [] [ sprintf "State: %A" model |> str ] ] ] ]
