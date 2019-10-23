module Quests.View

open Fable.React.Standard
open Fable.React.Helpers
open Fulma
open Shared

let navbarSource urlResolver s =
    Navbar.Item.a
        [ Navbar.Item.Props
            [ s
              |> Types.Page.SelectedSource
              |> urlResolver
              |> Fable.React.Props.Href ] ] [ str s ]

let navbarItem (model: Types.State) currentPage urlResolver =
    match model.Sources with
    | Empty
    | Loading -> [ Navbar.Item.a [] [ str "Quests" ] ]
    | LoadError e ->
        [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
              [ Navbar.Link.a [] [ str "Quests" ]
                Navbar.Dropdown.div [] [ str e ] ] ]
    | Body sources ->
        [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
              [ Navbar.Link.a [] [ str "Quests" ]
                Navbar.Dropdown.div [] (sources |> List.map (navbarSource urlResolver)) ] ]

let view (model: Types.State) (dispatch: Types.Msg -> unit) =
    [ Columns.columns []
          [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                [ Menu.menu []
                      [ Menu.label [] [ str "Placeholder" ]
                        Menu.list []
                            [ Menu.Item.li [] [ str "Test" ]
                              Menu.Item.li [] [ str "Test" ] ] ] ]
            Column.column [ Column.Width(Screen.All, Column.Is9) ]
                [ h2 [] [ str "Quests" ]
                  p []
                      [ model
                        |> sprintf "Model: %A"
                        |> str ] ] ] ]
