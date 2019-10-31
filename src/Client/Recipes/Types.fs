module Recipes.Types

open Shared

type Page =
    | Home
    | SelectedSource of source: Source

type Msg =
    | LoadSources
    | LoadSourcesFinished of Source list
    | LoadSourcesError of exn
    | LoadItems of Source
    | LoadItemsFinished of Shared.Item list
    | LoadItemsError of exn

type State =
    { Sources: Shared.Source list Remote
      Items: Shared.Item list Remote }
