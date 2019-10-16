module Quests.State
open Quests.Types
open Shared
open Elmish

let init route =
    let source = match route with
                    | Some (SelectedSource s) -> Some s
                    | None -> None
    {Source = source
     Sources = Shared.Remote.Empty}, Cmd.Empty

let update model = function
    | LoadSources -> { model with Sources = Loading }, Cmd.Empty