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


let view (model : AppModel) (dispatch : Msg -> unit) =
    div []
        [

          Container.container []
              [ Columns.columns []
                    [ Column.column [ Column.Width(Screen.All, Column.Is3) ]
                          [ ]

                      Column.column [ Column.Width(Screen.All, Column.Is9) ]
                          [ div [] [] ] ] ] ]
