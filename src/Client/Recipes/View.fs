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
    let itemName (item: Item) =
        item.Name
        |> Option.orElse item.rawName
        |> Option.defaultValue "Unknown name"

    let itemText (i: ItemAmount) =
        span []
            [ (sprintf "%ix " i.Amount) |> str
              code [] [ str (itemName i.Item) ] ]

    match model.Recipes with
    | Empty -> []
    | Loading -> [ str "Loading (there's a lot of recipes)" ]
    | LoadError e -> [ str e ]
    | Body rs ->
        [ h1 [] [ str "Random recipes" ]
          yield! rs
                 |> List.map (fun r ->
                     div [ Class "box" ]
                         [ div [ Class "box" ]
                               [ r.Details
                                 |> sprintf "%A"
                                 |> str ]
                           div [ Class "box" ] (r.Input |> List.map itemText)
                           div [ Class "box" ] (r.Output |> List.map itemText) ]) ]
