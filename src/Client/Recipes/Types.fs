module Recipes.Types

open Shared

type Page =
    | Home
    | SelectedSource of source: Source

type Msg =
    | LoadSources
    | LoadSourcesFinished of Source list
    | LoadSourcesError of exn
    | LoadRecipes of Source
    | LoadRecipesFinished of Shared.Recipe list
    | LoadRecipesError of exn

type State =
    { Sources: Shared.Source list Remote
      Recipes: Shared.Recipe list Remote }
