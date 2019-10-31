module Recipes.State

open Recipes.Types
open Shared
open Elmish

let init route =
    { Sources = Shared.Remote.Empty
      Items = Empty }, Cmd.ofMsg LoadSources

let urlUpdate page =
    match page with
    | Home -> Cmd.Empty
    | SelectedSource s -> Cmd.ofMsg (LoadItems s)

let update model =
    function
    | LoadSources ->
        { model with
              Sources = Loading
              Items = Empty }, Cmd.OfAsync.either Server.API.recipeSources () LoadSourcesFinished LoadSourcesError
    | LoadSourcesFinished s -> { model with Sources = Body s }, Cmd.Empty
    | LoadSourcesError e -> { model with Sources = LoadError <| sprintf "%A" e }, Cmd.Empty
    | LoadItems s ->
        { model with Items = Loading }, Cmd.OfAsync.either Server.API.items s LoadItemsFinished LoadItemsError
    | LoadItemsFinished rs -> { model with Items = Body rs }, Cmd.Empty
    | LoadItemsError e ->
        { model with
              Items =
                  sprintf "Error while loading Recipes:\n%A" e
                  |> LoadError }, Cmd.Empty
