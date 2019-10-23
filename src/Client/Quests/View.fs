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

let view (currentPage: Types.Page) (model: Types.State) (dispatch: Types.Msg -> unit) =
    [ Columns.columns []
          [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                [ Menu.menu []
                      (match currentPage with
                       | Types.Page.Home -> [ Menu.label [] [ str "Select a source" ] ]
                       | Types.Page.SelectedSource s ->
                           [ Menu.label [] [ str s ]
                             Menu.list []
                                 (match model.QuestLines with
                                  | Empty -> [ Menu.Item.li [] [ str "Empty" ] ]
                                  | Loading -> [ Menu.Item.li [] [ str "Loading" ] ]
                                  | LoadError e -> [ Menu.Item.li [] [ str e ] ]
                                  | Body qlis -> qlis |> List.map (fun qli -> Menu.Item.li [] [ str qli.Name ])) ]) ]
            Column.column [ Column.Width(Screen.All, Column.Is9) ]
                [ h2 [] [ str "Quests" ]
                  p [] [ "Placeholder" |> str ] ] ] ]
