module Recipes.State

open Recipes.Types
open Shared
open Elmish

let init route =
    { Sources = Shared.Remote.Empty
      Recipes = Empty }, Cmd.ofMsg LoadSources

let urlUpdate page =
    match page with
    | Home -> Cmd.Empty
    | SelectedSource s -> Cmd.ofMsg (LoadRecipes s)

let update model =
    function
    | LoadSources ->
        { model with
              Sources = Loading
              Recipes = Empty }, Cmd.OfAsync.either Server.API.recipeSources () LoadSourcesFinished LoadSourcesError
    | LoadSourcesFinished s -> { model with Sources = Body s }, Cmd.Empty
    | LoadSourcesError e -> { model with Sources = LoadError <| sprintf "%A" e }, Cmd.Empty
    | LoadRecipes s ->
        { model with Recipes = Loading }, Cmd.OfAsync.either Server.API.recipes s LoadRecipesFinished LoadRecipesError
    | LoadRecipesFinished rs -> { model with Recipes = Body rs }, Cmd.Empty
    | LoadRecipesError e ->
        { model with
              Recipes =
                  sprintf "Error while loading Recipes:\n%A" e
                  |> LoadError }, Cmd.Empty
