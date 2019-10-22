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

let navBrand (model: AppModel) (dispatch: AppMsg -> unit) =
    Navbar.navbar [ Navbar.Color IsWhite ]
        [ Container.container []
              [ Navbar.Brand.div []
                    [ Navbar.Item.a
                        [ Navbar.Item.CustomClass "brand-text"
                          Navbar.Item.Props <| [ Href <| State.pageHash Page.Home ] ] [ str "SAFE Admin" ] ]

                Navbar.menu []
                    [ Navbar.Start.div []
                          (match model.Quests.Sources with
                           | Empty -> [ Navbar.Item.a [] [ str "Not requested" ] ]
                           | Loading -> [ Navbar.Item.a [] [ str "Loading" ] ]
                           | LoadError s -> [ Navbar.Item.a [] [ str <| sprintf "Error: %s" s ] ]
                           | Body sources -> sources |> List.map (navSource model dispatch)) ] ] ]

let view (model: AppModel) (dispatch: AppMsg -> unit) =
    div []
        [ navBrand model dispatch
          Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ] [ sprintf "State: %A" model |> str ]

                      Column.column [ Column.Width(Screen.All, Column.Is9) ]
                          (match model.CurrentPage with
                           | None -> [ str "404" ]
                           | Some Home -> [ str "You are home!" ]
                           | Some Recipes -> [ str "Recipes WIP!" ]
                           | Some(Quests _) -> [ Quests.View.view model.Quests (QuestsMsg >> dispatch) ]) ] ] ]
