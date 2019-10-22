module Quests.View

open Fable.React.Standard
open Fable.React.Helpers

let view (model: Types.State) (dispatch: Types.Msg -> unit) =
    div []
        [ h2 [] [ str "Quests" ]
          p []
              [ model
                |> sprintf "Model: %A"
                |> str ] ]
