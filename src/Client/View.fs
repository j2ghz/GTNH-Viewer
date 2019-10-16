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


let view (model : AppModel) (dispatch : AppMsg -> unit) =
    div []
        [

          Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                          [ model.CurrentPage |> sprintf "You are at %A" |> str ]

                      Column.column [ Column.Width(Screen.All, Column.Is9) ]
                          [ div [] [ model |> sprintf "%A" |> str] ] ] ] ]
