module Quests.View
open Fable.React.Standard
open Fable.React.Helpers

let view (model : Types.State) (dispatch : Types.Msg ->  unit ) =
    div [] [ model |> sprintf "%A" |> str ]