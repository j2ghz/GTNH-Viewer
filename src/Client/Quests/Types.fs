module Quests.Types

open Shared

type Page =
    | Home
    | SelectedSource of source: Source

type Msg =
    | LoadSources
    | SourcesLoaded of Source list
    | SourcesLoadingError of exn
    | LoadQuestLines of Source

type State =
    { Sources: Shared.Source list Remote }
