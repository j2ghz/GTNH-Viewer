module Quests.View

open Fable.React.Standard
open Fable.React.Helpers
open Fulma

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
