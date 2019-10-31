module Recipes.View

open Fable.React.Standard
open Fable.React.Helpers
open Fable.React.Props
open Fulma
open Shared
open Fulma.Extensions.Wikiki

let navbarSource urlMaker s =
    Navbar.Item.a
        [ Navbar.Item.Props
            [ s
              |> Types.Page.SelectedSource
              |> urlMaker
              |> Fable.React.Props.Href ] ] [ str s ]

let navbarItem (model: Types.State) currentPage urlMaker =
    match model.Sources with
    | Empty
    | Loading -> [ Navbar.Item.a [] [ str "Recipes" ] ]
    | LoadError e ->
        [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
              [ Navbar.Link.a [] [ str "Recipes" ]
                Navbar.Dropdown.div [] [ str e ] ] ]
    | Body sources ->
        [ Navbar.Item.div [ Navbar.Item.HasDropdown; Navbar.Item.IsHoverable ]
              [ Navbar.Link.a [] [ str "Recipes" ]
                Navbar.Dropdown.div [] (sources |> List.map (navbarSource urlMaker)) ] ]

let view (currentPage: Types.Page) urlMaker (model: Types.State) (dispatch: Types.Msg -> unit) =
    match model.Items with
    | Empty -> []
    | Loading ->
        [ Heading.h1 [] [ str "Loading (there's so many items, your browser will stop responding for up to a minute)" ] ]
    | LoadError e -> [ str e ]
    | Body rs ->
        [ h1 [] [ str "Items" ]
          yield! rs
                 |> List.map (fun i ->
                     div [ Class "box" ]
                         [ i.Name
                           |> Option.defaultValue "Unknown"
                           |> str
                           code []
                               [ i.rawName
                                 |> Option.defaultValue "Unknown"
                                 |> str ] ]) ]
