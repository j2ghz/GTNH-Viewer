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


let view (model: AppModel) (dispatch: AppMsg -> unit) =
    div []
        [ Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                          [ sprintf "State: %A" model
                            |> str ]

                      Column.column [ Column.Width(Screen.All, Column.Is9) ]
                          (match model.CurrentPage with
                           | Some Home -> [ str "You are home!" ]
                           | Some Recipes -> [ str "Recipes WIP!"]
                           | Some(Quests _) -> [ Quests.View.view model.Quests (QuestsMsg >> dispatch) ]) ] ] ]
